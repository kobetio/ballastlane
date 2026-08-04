# My Library — Architecture Overview

This document explains how the **My Library** challenge was designed and built: a full-stack personal book collection where each registered user manages only their own books.

It is intended for interviewers reviewing the submission. It covers the architecture of both projects, the main decisions behind them, and deliberate next steps that were left out of scope.

For setup and run instructions, see [`README.md`](./README.md). For the phased build checklist, see [`Development Plan.md`](./Development%20Plan.md).

---

## 1. What the app does

| Capability | Behavior |
|---|---|
| Register / Login | Issues a JWT; password is never stored in plain text |
| List / Create / View / Edit / Delete books | All operations are scoped to the authenticated user |
| Ownership | Accessing another user's book returns **403 Forbidden**, not 404 |
| Validation | Client-side (UX) + server-side (source of truth), with field-level errors |

**Stack at a glance**

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API (Controllers), EF Core, PostgreSQL, JWT, FluentValidation, Swagger |
| Frontend | Angular 21 (standalone), Angular Material, Signals, Reactive Forms |
| Tests | xUnit + Moq + FluentAssertions (backend) · Vitest (frontend) |

---

## 2. High-level system view

```
┌─────────────────────┐         JWT Bearer          ┌──────────────────────┐
│  Angular (4200)     │ ──────────────────────────► │  ASP.NET Core API    │
│  features / core    │         HTTP JSON           │  Controllers         │
│  AuthService        │ ◄────────────────────────── │  Application         │
│  BookService        │     ProblemDetails          │  Infrastructure      │
└─────────────────────┘                             └──────────┬───────────┘
                                                               │ EF Core
                                                               ▼
                                                    ┌──────────────────────┐
                                                    │  PostgreSQL          │
                                                    │  Users / Books       │
                                                    └──────────────────────┘
```

The frontend is a thin client: it handles UX, form validation, and session storage. Business rules, ownership, and persistence live on the backend.

---

## 3. Backend architecture

### 3.1 Clean Architecture layout

```
backend/
  MyLibrary.slnx
  src/
    MyLibrary.Domain/          # Entities, enums, domain guards — no external deps
    MyLibrary.Application/     # Use cases, DTOs, ports (interfaces), FluentValidation
    MyLibrary.Infrastructure/  # EF Core, repositories, JWT, password hashing
    MyLibrary.Api/             # HTTP host, DI, middleware, filters, Swagger
  tests/
    MyLibrary.Tests/           # Unit + HTTP integration tests
```

**Dependency rule (dependencies point inward):**

```
Api ──► Application ◄── Infrastructure
              │
              ▼
           Domain
```

| Project | Responsibility |
|---|---|
| **Domain** | Rich entities (`User`, `Book`), `ReadingStatus` enum, `Guard` helpers, `DomainException`. Zero NuGet packages. |
| **Application** | `AuthService` / `BookService`, DTOs, repository/security interfaces, FluentValidation validators, application exceptions. |
| **Infrastructure** | `AppDbContext`, EF configurations, repositories, `PasswordHasher`, `JwtTokenService`. |
| **Api** | Thin controllers, composition root (`Program.cs`), `ExceptionHandlingMiddleware`, `ValidationActionFilter`, `CurrentUserService`. |

`ICurrentUserService` is defined in Application but implemented in Api, because it depends on `HttpContext`.

### 3.2 Domain model

- **`User`**: `Id`, `Name`, `Email`, `PasswordHash`, collection of books. Private setters; private parameterless ctor for EF Core.
- **`Book`**: `Title`, `Author`, optional `Genre` / `PublicationYear` / `ReadingStatus` / `Rating` / `Notes`, plus `UserId`.
- **`Book.BelongsTo(userId)`** encodes the ownership rule used by the application layer.
- **`Book.Update(...)`** mutates an existing book through the same construction guards.

**Validation split (intentional):**

| Layer | What it enforces |
|---|---|
| Domain | Hard invariants (required title/author/userId, non-empty Guids) |
| Application (FluentValidation) | Spec rules: max lengths, year range (1450–current), rating 1–5, notes ≤1000 |

Domain guards are a defense-in-depth backstop; input shape and business ranges are validated at the Application boundary before entities are created/updated.

### 3.3 Application layer (use cases)

| Service | Responsibility |
|---|---|
| `AuthService` | Register (unique email + hash + JWT), Login (verify + JWT) |
| `BookService` | User-scoped CRUD; ownership checks before get/update/delete |

**Exception → HTTP mapping** (handled centrally by middleware):

| Exception | Status |
|---|---|
| FluentValidation `ValidationException` / `AppValidationException` | 400 |
| `AuthenticationException` | 401 |
| `ForbiddenAccessException` | 403 |
| `NotFoundException` | 404 |
| Unhandled | 500 (logged; generic message to client) |

Duplicate email is checked inside `AuthService` (stateful rule), not as an async FluentValidation rule — validators stay fast and side-effect free.

### 3.4 Infrastructure

- **EF Core + PostgreSQL** via Npgsql; Code First migrations.
- **Repositories** implement Application ports; each mutating call persists (`SaveChanges`). Adequate for the current single-aggregate flows.
- **Password hashing** wraps ASP.NET Identity's `PasswordHasher<T>` (PBKDF2) — no custom crypto.
- **JWT** issued by `JwtTokenService` (HS256); options bound via the Options pattern. Signing secret and DB connection string live in **`dotnet user-secrets`**, not in git.

### 3.5 API / cross-cutting

- Controllers are thin: authorize, resolve current user id, delegate to services.
- **`ValidationActionFilter`**: globally resolves `IValidator<T>` for action arguments so new endpoints get validation automatically.
- **`ExceptionHandlingMiddleware`**: maps known exceptions to RFC 7807 `ProblemDetails` / `ValidationProblemDetails`.
- Enums serialize as **strings** (`"WantToRead"`, `"Reading"`, `"Read"`) via `JsonStringEnumConverter`.
- Swagger (Development): XML comments + Bearer Authorize button.
- CORS allowed for `http://localhost:4200` in Development only.

### 3.6 Request flow examples

**Register / Login**

```
Client → AuthController
      → ValidationActionFilter
      → AuthService (hash / verify)
      → JwtTokenService
      → AuthResponse { userId, name, email, token, expiresAtUtc }
```

**Book CRUD (authenticated)**

```
Client + Authorization: Bearer <token>
  → JWT middleware
  → BooksController [Authorize]
  → CurrentUserService (NameIdentifier claim)
  → BookService(userId, …)
       list: filter by user
       get/update/delete: load → BelongsTo? → 403 / 404 / proceed
  → BookResponse | 204
```

---

## 4. Frontend architecture

### 4.1 Feature-based structure

```
frontend/src/app/
  core/                 # AuthService, BookService, authInterceptor, guards
  shared/               # ConfirmDialog, API error helpers
  layout/               # MainLayout (toolbar + outlet)
  features/
    auth/               # Login, Register
    books/              # BookList, BookItem, BookForm (dialog), BookDetail
  models/               # TypeScript interfaces mirroring API DTOs
```

Standalone components throughout (no NgModules). Routes use **lazy `loadComponent`**. Change detection is **OnPush**; UI state uses **signals**.

### 4.2 Routing and access control

| Route | Guard | Purpose |
|---|---|---|
| `/login`, `/register` | `guestGuard` | Redirect authenticated users to `/books` |
| `/books`, `/books/:id` (under `MainLayout`) | `authGuard` | Redirect anonymous users to `/login?returnUrl=…` |

### 4.3 Auth on the client

- **`AuthService`**: session held in a signal; JWT + user persisted in `localStorage` so refresh keeps the user logged in.
- **`authInterceptor`**: attaches `Authorization: Bearer …`; on **401** from non-auth endpoints, logs out and navigates to `/login`.
- Login/Register use Reactive Forms; server field errors are mapped onto the matching controls (`Email` → `email`).

### 4.4 Books feature

| Piece | Role |
|---|---|
| `BookService` | Thin HTTP façade (`getAll` / `getById` / `create` / `update` / `delete`) — no local cache |
| `BookList` | Smart component: load states, grid, create/edit/delete orchestration |
| `BookItem` | Presentational card (`input` / outputs) |
| `BookForm` | Shared Material dialog for create **and** edit; owns the POST/PUT call so validation errors keep the dialog open |
| `BookDetail` | Full-page view with edit/delete |

Destructive actions always go through **`ConfirmDialog`**. Outcomes surface via **`MatSnackBar`**, preferring the API error message when present.

### 4.5 State management choice

No NgRx / global store.

| Concern | Approach |
|---|---|
| Auth session | Shared signals in `AuthService` + `localStorage` |
| Books list / detail | Component-local signals, updated after successful API calls |
| HTTP | RxJS Observables from services |

This keeps the frontend simple and makes the API the source of truth — appropriate for the size of the challenge.

### 4.6 API configuration

| Environment | `apiBaseUrl` |
|---|---|
| Development | `http://localhost:5073/api` |
| Production | `/api` (expects reverse proxy) |

Client validators mirror backend rules for fast feedback; the server remains authoritative.

---

## 5. Key decisions (and why)

| Decision | Rationale |
|---|---|
| **Clean Architecture on the backend** | Clear separation of concerns, testable use cases, Domain free of frameworks. Easy to explain ownership and dependency flow in an interview. |
| **Controllers + services (not MediatR/CQRS)** | Two aggregates and a handful of endpoints do not justify the ceremony of CQRS. Services keep the use-case surface obvious. |
| **JWT Bearer, no refresh tokens** | Stateless auth matching a SPA. Refresh tokens, revocation, and cookie-based sessions are production hardening left as follow-ups. |
| **Ownership → 403 (not 404)** | Distinguishes “does not exist” from “exists but not yours”, matching the challenge ownership rule explicitly. |
| **FluentValidation + global filter** | Spec rules live in one place; controllers stay free of `ValidateAsync` boilerplate. |
| **ProblemDetails for errors** | Standard ASP.NET Core contract; frontend can map `errors` onto form controls consistently. |
| **Secrets via user-secrets** | Connection string and JWT signing key never committed. |
| **Identity password hasher** | Battle-tested PBKDF2 instead of hand-rolled hashing. |
| **Same error message on bad login** | Avoids user/email enumeration (“Invalid email or password”). |
| **SQLite in-memory for tests** | Exercises the real EF model/FKs without requiring PostgreSQL in CI; production still uses Postgres. |
| **Angular standalone + feature folders** | Matches modern Angular 21 practice; lazy routes keep the initial bundle small. |
| **Signals for UI/session state** | Lightweight reactivity with OnPush; no global store for a CRUD app of this size. |
| **Dialog owns create/edit HTTP** | Keeps the form open on validation failure and avoids leaking dialog concerns into the list/detail pages. |
| **TDD-style backend phases** | Domain → Application → Infrastructure → API, with tests written ahead of (or alongside) implementation — see Development Plan. |

**Intentionally not used:** MediatR, AutoMapper, Unit of Work, full ASP.NET Identity UI, NgRx, proxy.conf (CORS used in Development instead).

---

## 6. Testing strategy

### Backend (~133 tests)

| Area | What is covered |
|---|---|
| Domain | Entity invariants, `BelongsTo`, `DomainException` |
| Application | Service rules (incl. cross-user 403), FluentValidation |
| Infrastructure | Repositories on SQLite, password hasher, JWT service |
| API | Controller unit tests + `WebApplicationFactory` HTTP integration tests (auth, ownership, validation, status codes) |
| Middleware / CurrentUser | Direct unit tests for mapping branches and claim edge cases |

Integration tests run in a `"Testing"` environment with an in-memory SQLite `AppDbContext`, so they never touch the developer’s real PostgreSQL database.

### Frontend (~49 tests across 13 specs)

Covers `AuthService`, `BookService`, interceptor, guards, Login/Register, BookList/Item/Form/Detail, ConfirmDialog, and MainLayout — focused on the critical auth and CRUD paths.

---

## 7. Possible improvements

These are conscious omissions or natural next steps — useful discussion points rather than bugs in the delivered scope.

### Backend / security

1. **Map `DomainException` in middleware** — today an unexpected domain guard failure would fall through to 500; mapping it to 400 would be more precise.
2. **Normalize emails** (trim + lowercase) before uniqueness checks — avoids case-sensitive duplicates depending on collation.
3. **Stronger password policy** and/or **rate limiting / lockout** on login and register.
4. **Refresh tokens** (or short-lived access + revocation list) if longer sessions or logout-everywhere are required.
5. **Pagination, filtering, and search** on `GET /api/books` before collections grow large.
6. **Health checks** and production CORS/Swagger policy when deploying.
7. **Unit of Work / shared DbContext transactions** if multi-aggregate writes appear later.
8. **Testcontainers + real PostgreSQL** for a second integration tier closer to production.

### Frontend

1. **Honor `expiresAtUtc`** — currently modeled on the auth response but not stored/checked; expiry is handled reactively via 401. Proactive logout or silent refresh would improve UX.
2. **Shared books state** (e.g. a small signal store) so list and detail stay in sync without reloading.
3. **`takeUntilDestroyed`** (or equivalent) on long-lived subscriptions for hardening.
4. **E2E suite** (Playwright/Cypress) covering register → CRUD → logout.
5. **Dev proxy** (`proxy.conf.json`) as an alternative to CORS for local API calls.
6. **Accessibility and i18n** polish (ARIA labels, screen-reader announcements for snackbars, translated status labels).

### Product / ops

1. Soft-delete or audit fields on books/users.
2. Structured logging + correlation IDs across API requests.
3. CI pipeline running `dotnet test` + `npm test` + build on every PR.
4. Containerization (API + Postgres + optional nginx for the SPA).

---

## 8. How to navigate the code in a review

| If you want to see… | Start here |
|---|---|
| Composition root / auth pipeline | `backend/src/MyLibrary.Api/Program.cs` |
| Ownership rule | `backend/src/MyLibrary.Application/Services/BookService.cs` + `Domain/Entities/Book.cs` |
| Validation rules | `backend/src/MyLibrary.Application/Validators/` |
| Error contract | `backend/src/MyLibrary.Api/Middleware/ExceptionHandlingMiddleware.cs` |
| HTTP integration tests | `backend/tests/MyLibrary.Tests/Api/*EndpointsTests.cs` |
| Angular routes / guards | `frontend/src/app/app.routes.ts`, `core/guards/auth.guard.ts` |
| Session + JWT attachment | `frontend/src/app/core/services/auth.service.ts`, `core/interceptors/auth.interceptor.ts` |
| Books UX | `frontend/src/app/features/books/` |

---

## 9. Summary

The solution is a deliberately straightforward Clean Architecture API paired with a modern Angular SPA:

- **Clear boundaries** between Domain, Application, Infrastructure, and Api.
- **Security basics done properly** for the challenge: hashed passwords, JWT, user-scoped data, secrets out of source control, consistent error responses.
- **Tests at every layer**, including full HTTP integration tests for ownership and validation.
- **Frontend that stays thin**: feature folders, signals, Material + Reactive Forms, and the API as the source of truth.

Trade-offs (no CQRS, no refresh tokens, no global frontend store, SQLite for tests) favor clarity and delivery for an interview challenge, while leaving an obvious path to production hardening as outlined in section 7.
`)