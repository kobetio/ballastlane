# My Library — Development Plan & Checklist

> Derived from `Specification.md` and `SRS AI Development Guide.md`.
> This document breaks the project into small, reviewable phases. Each phase ends with a **Checkpoint** you can run/test yourself before we move to the next one.

------------------------------------------------------------------------

## 0. Verified Local Environment

Checked on this machine on 2026-07-09:

| Tool | Version found | Required by spec |
|---|---|---|
| PostgreSQL (`psql`) | **16.13** | PostgreSQL |
| .NET SDK | **10.0.202** | .NET 10 |
| Node.js | **v22.22.2** | Angular 21 (needs Node ≥ 20.19 or ≥ 22.12) |
| npm | **10.9.7** | — |
| Angular CLI | not installed yet | Angular 21 — installed in Phase 6 |

All backend connection strings / docs in this plan will target **PostgreSQL 16.13**.

------------------------------------------------------------------------

## How We'll Work

- Each phase = one or more small commits/PRs of work.
- After each phase I implement it, you review/test using the **Checkpoint** instructions, then we tick the boxes and move on.
- Checkboxes track progress — mark `[x]` as we complete/confirm each item.

------------------------------------------------------------------------

## Phase 1 — Repository & Solution Skeleton ✅

- [x] Create solution `MyLibrary.slnx` (backend/) — .NET 10 uses the new XML solution format
- [x] Create empty projects with correct dependency flow (Api → Application → Domain, Infrastructure → Application/Domain):
  - [x] `MyLibrary.Domain` (class library)
  - [x] `MyLibrary.Application` (class library)
  - [x] `MyLibrary.Infrastructure` (class library)
  - [x] `MyLibrary.Api` (ASP.NET Core Web API, Controllers-based, Swagger via Swashbuckle)
  - [x] `MyLibrary.Tests` (xUnit test project)
- [x] Add project references matching Clean Architecture rules (Domain has zero dependencies)
- [x] Add `.gitignore` for .NET + Angular + PostgreSQL secrets
- [x] Add root `README.md` with run instructions (backend + frontend + DB)

**Checkpoint:** `dotnet build` succeeds with 0 warnings / 0 errors across all 5 projects. Confirmed the API starts (`dotnet run --project src/MyLibrary.Api`) and serves `/swagger/v1/swagger.json`.

**Ready for your review:**
```
backend/
  MyLibrary.slnx
  src/
    MyLibrary.Domain/MyLibrary.Domain.csproj
    MyLibrary.Application/MyLibrary.Application.csproj
    MyLibrary.Infrastructure/MyLibrary.Infrastructure.csproj
    MyLibrary.Api/MyLibrary.Api.csproj (+ Program.cs, Controllers/)
  tests/
    MyLibrary.Tests/MyLibrary.Tests.csproj
```
Try it yourself: `cd backend && dotnet run --project src/MyLibrary.Api`, then open `http://localhost:<port>/swagger` in your browser.

------------------------------------------------------------------------

## Phase 2 — PostgreSQL Setup ✅

- [x] Create local database `mylibrary_db` (PostgreSQL 16.13)
- [x] Decide/confirm local connection credentials — dedicated role `mylibrary_user` (not the `postgres` superuser), port 5432
- [x] Connection string stored via `dotnet user-secrets` (kept out of git); `appsettings.Development.json` only documents the key name, not real credentials
- [x] Document exact `psql`/SQL commands to create the DB in `README.md`

**Checkpoint:** Verified `mylibrary_user` can connect to `mylibrary_db` via `psql` with password auth (`scram-sha-256`), and that the `postgres` superuser still requires its own password (i.e. no auth was left weakened).

**Note:** while diagnosing the `postgres` superuser password, we temporarily set local auth to `trust` (with the original `pg_hba.conf` backed up) to create the new role, then restored `scram-sha-256` and restarted the service — confirmed working correctly afterward.

------------------------------------------------------------------------

## Phase 3 — Domain Layer (TDD) ✅

- [x] `User` entity: Id (Guid), Name, Email, PasswordHash, `Books` collection
- [x] `Book` entity: Id (Guid), Title, Author, Genre, PublicationYear, ReadingStatus (enum), Rating, Notes, UserId
- [x] Value/enum types — `ReadingStatus`: `WantToRead`, `Reading`, `Read`
- [x] Domain-level invariants: constructors guard required fields (`Title`, `Author`, `Name`, `Email`, `PasswordHash`, `UserId`) and throw `DomainException` on violation; `Book.BelongsTo(userId)` helper for the ownership rule; `Book.Update(...)` for mutating an existing book
- [x] Added FluentAssertions + Moq to `MyLibrary.Tests` (Moq will be used starting Phase 4)
- [x] Wrote unit tests **first** (TDD), then implemented entities to satisfy them

**Checkpoint:** `dotnet test` — **25/25 tests pass**, full solution build has 0 warnings / 0 errors.

**Design notes for your review:**
- Entities use `Guid` ids (maps to PostgreSQL `uuid`), private setters, and a private parameterless constructor reserved for EF Core.
- Only *required-field* invariants live in the Domain (per SRS §7: "Business rules only inside Application"). Things like max length (Title ≤150) or numeric ranges (Rating 1–5) are intentionally **not** enforced here — they'll be enforced by FluentValidation in the Application layer (Phase 4), with domain guards acting as a defense-in-depth backstop.
- Files: `backend/src/MyLibrary.Domain/Entities/{User,Book}.cs`, `Enums/ReadingStatus.cs`, `Exceptions/DomainException.cs`, `Common/Guard.cs`. Tests: `backend/tests/MyLibrary.Tests/Domain/{UserTests,BookTests}.cs`.

------------------------------------------------------------------------

## Phase 4 — Application Layer ✅

- [x] DTOs: `RegisterRequest`, `LoginRequest`, `AuthResponse`, `BookCreateRequest`, `BookUpdateRequest`, `BookResponse` (records, sharing an `IBookRequest` shape for Create/Update)
- [x] Interfaces: `IBookRepository`, `IUserRepository`, `IPasswordHasher`, `ITokenService`, `ICurrentUserService`, plus `IAuthService`/`IBookService` for the services themselves
- [x] Application services: `AuthService`, `BookService` (business rules live here, not in controllers)
- [x] FluentValidation validators for all input DTOs, enforcing spec rules:
  - Title ≤150, Author ≤100, Genre ≤50, PublicationYear 1450–current year, Rating 1–5, Notes ≤1000
- [x] Ownership rule: `BookService` verifies `UserId` match before Update/Delete/GetById → throws `ForbiddenAccessException` (maps to 403 in Phase 6); missing books throw `NotFoundException` (maps to 404)
- [x] Unit tests (Moq for repository/dependency mocking) for services and validators, written TDD-style (tests written before the implementation)

**Checkpoint:** `dotnet test` — **79/79 tests pass** (25 Domain + 38 validators + 16 services), including explicit tests proving cross-user access to a book throws `ForbiddenAccessException` on Get/Update/Delete. `dotnet build` — 0 warnings, 0 errors.

**Design notes for your review:**
- **Exception → status code mapping** (wired up for real in Phase 6): `NotFoundException` → 404, `ForbiddenAccessException` → 403, `AuthenticationException` → 401, `AppValidationException` / FluentValidation's `ValidationException` → 400.
- **Duplicate email on register** is handled inside `AuthService` (throws `AppValidationException` on the `Email` field) rather than as an async FluentValidation rule — keeps validators stateless/fast and puts the state-dependent "must be unique" rule where business logic belongs.
- **Assumption:** the spec doesn't define rules for `Name`/`Email`/`Password` on registration (only Book fields are specified). I applied sensible defaults — Name required ≤100 chars, Email required + valid format, Password required ≥8 chars — happy to adjust if you have different requirements.
- Files: `backend/src/MyLibrary.Application/{DTOs,Interfaces,Services,Validators,Common}/**`. Tests: `backend/tests/MyLibrary.Tests/Application/{Validators,Services}/**`.

------------------------------------------------------------------------

## Phase 5 — Infrastructure Layer ✅

- [x] Installed EF Core 10.0.4 + `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.2 (pinned to matching versions — see note below)
- [x] `AppDbContext` with `User` and `Book` entity configurations (Fluent API), 1:N relationship (`Book.UserId` FK, cascade delete)
- [x] Repository implementations (`BookRepository`, `UserRepository`) implementing the Application interfaces
- [x] `PasswordHasher` — wraps ASP.NET Core Identity's `PasswordHasher<T>` (PBKDF2-based, battle-tested, no hand-rolled crypto)
- [x] `JwtTokenService` (JWT) implementation, `JwtOptions` bound via Options Pattern from a `Jwt` config section
- [x] Initial EF Core Migration (`InitialCreate`) generated and **applied to `mylibrary_db`** — `Users` and `Books` tables confirmed present
- [x] Repository/integration tests using **in-memory SQLite** (see note below) — 20 tests covering both repositories, the password hasher, and the JWT service
- [x] `DependencyInjection.AddInfrastructure(...)` extension registers everything; wired into `Program.cs`
- [x] JWT signing secret generated and stored via `dotnet user-secrets` (never committed); non-secret JWT settings (Issuer/Audience/ExpiryMinutes) in `appsettings.json`

**Checkpoint:** `dotnet ef database update` ran cleanly; confirmed via `psql \dt` that `Users`, `Books`, and `__EFMigrationsHistory` exist in `mylibrary_db`, owned by `mylibrary_user`. `dotnet test` — **96/96 tests pass**. `dotnet build` — 0 warnings, 0 errors. API still starts and serves Swagger correctly with the DbContext wired in.

**Design notes for your review:**
- **Version pin:** `Microsoft.EntityFrameworkCore`/`.Design` were pinned to `10.0.4` to match what the latest stable Npgsql provider (`10.0.2`) depends on — the newer `10.0.9` caused an unresolvable assembly version conflict at build time.
- **Repository tests use in-memory SQLite, not real PostgreSQL** — this was explicitly allowed by the plan ("in-memory/SQLite fallback if agreed"). It exercises the same EF Core model/configuration and catches real mapping/FK issues (it already caught one — the first draft of the Book tests didn't seed an owning `User` row, which correctly failed on the FK constraint). If you'd prefer true Postgres-backed integration tests later (e.g. via Testcontainers, which needs Docker), let me know and we can add that as a follow-up.
- Also swapped the vulnerable `SQLitePCLRaw.lib.e_sqlite3 2.1.11` transitive dependency for a patched `3.0.3` bundle to keep the build warning-free.
- Files: `backend/src/MyLibrary.Infrastructure/{Persistence,Security}/**`, `DependencyInjection.cs`. Tests: `backend/tests/MyLibrary.Tests/Infrastructure/**`.

------------------------------------------------------------------------

## Phase 6 — API Layer: Auth & Cross-Cutting Concerns ✅

- [x] Dependency Injection wiring (Program.cs) for all services/repositories — new `MyLibrary.Application.DependencyInjection.AddApplication()` registers `AuthService`/`BookService` plus every FluentValidation validator; `CurrentUserService` registered in the API layer (needs `HttpContext`)
- [x] JWT Authentication configured (issuer, audience, signing key via `IConfiguration`/Options pattern) — `AddAuthentication().AddJwtBearer(...)` in `Program.cs`, reading the same `Jwt` section/secret used to *issue* tokens in Phase 5
- [x] Global Exception Middleware → consistent error responses (`ProblemDetails`/`ValidationProblemDetails`), maps `NotFoundException`→404, `ForbiddenAccessException`→403, `AuthenticationException`→401, `AppValidationException`/FluentValidation `ValidationException`→400, anything else→500 (logged, generic message returned)
- [x] Global validation pipeline — `ValidationActionFilter` (registered once as a global MVC filter) resolves the matching `IValidator<T>` for every action argument via DI and throws on failure, so controllers never call validators manually
- [x] `AuthController`: `POST /api/auth/register` (201 + JWT), `POST /api/auth/login` (200 + JWT)
- [x] Swagger configured: XML comments enabled and included, JWT "Authorize" button (bearer security scheme + global security requirement), `[ProducesResponseType]` documented per endpoint
- [x] Controller tests (Moq-based, `AuthController` delegates correctly) **and** full HTTP integration tests (`WebApplicationFactory`) covering register/login happy paths, validation failures (400 with field-level `errors`), duplicate email (400), and bad credentials (401)

**Checkpoint:** `dotnet test` — **105/105 tests pass**. `dotnet build` — 0 warnings, 0 errors. Manually verified end-to-end: registered a user via `POST /api/auth/register` → got back a JWT (201), logged in with the same credentials (200), wrong password → 401, invalid payload → 400 with per-field `errors`, duplicate email → 400. Swagger UI (`/swagger`) shows both endpoints with a working "Authorize" button.

**Design notes for your review:**
- **Global validation pipeline vs. manual calls:** rather than each controller action calling `validator.ValidateAsync(...)` explicitly, a single `ValidationActionFilter` inspects every action argument, looks up a registered `IValidator<T>` for its runtime type, and validates automatically. New endpoints get validation "for free" just by having a validator registered — nothing to remember in the controller.
- **Response shape:** validation failures (`FluentValidation.ValidationException` and our own `AppValidationException`, e.g. "email already registered") both render as an ASP.NET Core-standard `ValidationProblemDetails` (`{ title, status, errors: { field: [messages] } }`), matching what a typical ASP.NET Core client already expects from model-binding validation.
- **Gotcha found & fixed:** the exception middleware originally declared the response variable as `ProblemDetails` (the base type) and called `WriteAsJsonAsync(response)` — System.Text.Json used that *declared* type for serialization and silently dropped `ValidationProblemDetails.Errors`. Fixed by serializing with the *runtime* type (`WriteAsJsonAsync(response, response.GetType())`); verified the `errors` object now appears correctly for both validator- and business-rule-triggered 400s.
- **Testing the DbContext without touching your real Postgres data:** `Program.cs` now takes a `registerDbContext` flag (`AddInfrastructure(config, registerDbContext: !Environment.IsEnvironment("Testing"))`); the new `CustomWebApplicationFactory` runs the API in a `"Testing"` environment and registers its own in-memory SQLite `AppDbContext` instead — avoids a "two database providers registered" EF Core conflict that occurs if both the real Npgsql registration and a test one are added to the same container.
- **Swashbuckle v10 breaking change:** upgrading pulled in `Microsoft.OpenApi` v2, which removed `OpenApiSecurityScheme.Reference`/`OpenApiReference` entirely. The current pattern for wiring the Swagger "Authorize" button is `options.AddSecurityRequirement(document => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] })`.
- Files: `backend/src/MyLibrary.Api/{Program.cs,Controllers/AuthController.cs,Middleware/ExceptionHandlingMiddleware.cs,Filters/ValidationActionFilter.cs,Services/CurrentUserService.cs}`, `backend/src/MyLibrary.Application/DependencyInjection.cs`. Tests: `backend/tests/MyLibrary.Tests/Api/**`.

------------------------------------------------------------------------

## Phase 7 — API Layer: Books CRUD ✅

- [x] `BooksController` (thin — delegates to `BookService`):
  - [x] `GET /api/books` (only current user's books)
  - [x] `GET /api/books/{id}` (403 if not owner, 404 if it doesn't exist)
  - [x] `POST /api/books` (201 + `Location` header via `CreatedAtAction`)
  - [x] `PUT /api/books/{id}` (403 if not owner)
  - [x] `DELETE /api/books/{id}` (403 if not owner, 204 on success)
- [x] `[Authorize]` applied at the controller level; `ICurrentUserService` extracts the user id from the JWT `NameIdentifier` claim (throws `AuthenticationException` → 401 if somehow missing)
- [x] Proper status codes: 200/201/204/400/401/403/404 (all exercised by tests below)
- [x] Swagger docs for every endpoint (summaries, `[ProducesResponseType]` per response code, XML `<param>`/`<response>` comments)
- [x] Controller unit tests (Moq) + full HTTP integration tests covering the ownership rule (403 explicitly tested both for `GET`/`PUT`/`DELETE`), 404 for unknown ids, validation (400), and that `GET /api/books` only returns the caller's own books

**Checkpoint:** `dotnet test` — **123/123 tests pass**. Manually verified in a running instance: registered a user, created a book with `readingStatus: "Read"` (as a friendly string, not a raw number — see design note below), fetched it back, and confirmed `GET /api/books` without a token returns 401.

**Design notes for your review:**
- **Enum serialization gotcha found & fixed:** `ReadingStatus` was serializing as a raw integer (`0`/`1`/`2`) in JSON responses and required a raw integer on input, which is unfriendly for API consumers and doesn't show named values in Swagger. Added a global `JsonStringEnumConverter` in `Program.cs` so the API now accepts/returns `"WantToRead"`/`"Reading"`/`"Read"` — confirmed the Swagger schema now shows the enum as a string with the three allowed values, and both directions (create with a string, response has a string) work end-to-end.
- Files: `backend/src/MyLibrary.Api/Controllers/BooksController.cs`, `Program.cs` (enum converter). Tests: `backend/tests/MyLibrary.Tests/Api/{Controllers/BooksControllerTests.cs,BooksEndpointsTests.cs}`.

------------------------------------------------------------------------

## Phase 8 — Backend Test Suite Completion ✅

- [x] Reviewed coverage against SRS §11: Domain ✅, Application/Services ✅, Validators ✅, Controllers ✅, Authentication ✅, Repository ✅, Integration ✅ — every category already had tests from prior phases
- [x] Filled two small gaps found during review:
  - `ExceptionHandlingMiddleware` was previously only exercised indirectly (via HTTP integration tests); added direct unit tests for every branch, including the unhandled/500 case which is impractical to trigger from a real endpoint, and an explicit regression test for the "declared type drops derived properties" bug found in Phase 6
  - `CurrentUserService` had no direct unit test; added one covering the happy path plus missing-context/missing-claim/invalid-claim edge cases
- [x] Ensured no build warnings

**Checkpoint:** `dotnet test` full run — **133/133 tests pass**, 0 warnings / 0 errors on `dotnet build`.

**Test suite composition:** 133 tests across Domain (25), Application validators (38+), Application services (Auth/Book, ~20), Infrastructure/Repository (20), and API (Controllers + HTTP integration + Middleware + Services, ~30).

------------------------------------------------------------------------

## Phase 9 — Frontend Bootstrap ✅

- [x] Installed Angular CLI **21.2.19** globally (`npm i -g @angular/cli@21`) — confirmed via `ng version`
- [x] Generated a standalone Angular 21 app (`frontend/`) with routing, SCSS styles, no SSR (`ng new frontend --routing --style=scss --ssr=false`)
- [x] Added Angular Material **21.2.14** (`ng add @angular/material`) — azure-blue theme, Material typography, animations enabled (`provideAnimationsAsync()`)
- [x] Feature-based folder structure created under `frontend/src/app/`: `core/{interceptors,guards,services}`, `shared/components`, `layout`, `features/{auth,books}`, `models`
- [x] Environment files (`environments/environment.ts` for production, `environment.development.ts` for local dev pointing at `http://localhost:5073/api`), wired via `fileReplacements` in `angular.json`'s `development` build configuration

**Checkpoint:** `ng build` and `ng test` both succeed (1/1 test passing). `ng serve` starts cleanly on `http://localhost:4200` — confirmed via HTTP that it serves `index.html` with the Material font/theme `<link>` tags and the `<app-root>` root component mounting correctly.

**Design notes for your review:**
- Angular 21's `ng new` no longer scaffolds `environments/` by default (nor Karma/Jasmine — it now ships **Vitest** as the default test runner); both were added/configured manually to match the plan.
- Cleared out the default Angular welcome-page boilerplate from `app.html`/`app.ts` (kept just `<router-outlet>`) so Phase 10/11 start from a clean shell instead of fighting generated markup.
- `provideAnimationsAsync()` requires the separate `@angular/animations` package (not installed by `ng add @angular/material` automatically) — added it explicitly after hitting a build-time module resolution error.
- Files: `frontend/` (new Angular workspace), `frontend/src/environments/**`, `frontend/angular.json` (`fileReplacements`).

------------------------------------------------------------------------

## Phase 10 — Frontend Auth Feature ✅

- [x] Typed models for `RegisterRequest`, `LoginRequest`, `AuthResponse`, `CurrentUser` (`models/auth.model.ts`) and a `ApiProblemDetails` model matching the API's error shape
- [x] `AuthService` (`core/services/auth.service.ts`) — signals (`currentUser`, `isAuthenticated`) backed by a session persisted to `localStorage`, so a refresh doesn't log the user out; `login()`/`register()`/`logout()`
- [x] `authInterceptor` (`core/interceptors/auth.interceptor.ts`) — attaches `Authorization: Bearer {token}` to every outgoing request; also catches a `401` from any *non*-auth endpoint (i.e. an expired/invalid token) and logs the user out + redirects to `/login`
- [x] `authGuard` (protects `books`/layout routes) + `guestGuard` (keeps logged-in users off `/login`/`/register`) in `core/guards/auth.guard.ts`
- [x] `Login` page and `Register` page — standalone, `OnPush`, typed reactive forms via `FormBuilder.nonNullable`, Angular Material (`mat-form-field`, password visibility toggle, spinner while submitting)
- [x] Server-side validation errors mapped onto the exact form field via `applyServerValidationErrors()` (shows the API's own message, e.g. "Email is already registered."); non-field errors (e.g. wrong password) shown as-is via `MatSnackBar`
- [x] `MainLayout` (toolbar shell with app title, current user's name, logout button) wrapping the authenticated route tree
- [x] Routing wired in `app.routes.ts`: `/login`, `/register` (both lazy-loaded, `guestGuard`), and a `authGuard`-protected layout with `/books` as its only child so far (Phase 11 fleshes this out)

**Checkpoint:** `ng build` (dev + prod configs) and `ng test` both succeed — **22/22 tests pass** across `AuthService`, the interceptor (token attachment + 401 handling, including *not* logging out on a failed login attempt itself), both guards, and the `Login`/`Register`/`MainLayout` components.

**Design notes for your review:**
- **I could not perform a live, click-through browser walkthrough** — this environment has no browser-automation tool available, only HTTP requests and the Angular test runner. I verified the pieces individually instead: the backend register/login endpoints were manually exercised end-to-end back in Phase 6/7, and every piece of frontend auth logic (token storage/persistence, header injection, 401 handling, guard redirects, form validation) has a dedicated automated test. **Please do a quick manual pass** in the browser (`npm start` in `frontend/`, `dotnet run` in `backend/src/MyLibrary.Api`) to confirm the actual UI/UX feels right — that's the one thing I can't self-certify here.
- Files: `frontend/src/app/{models/auth.model.ts,models/api-error.model.ts,core/**,layout/main-layout/**,features/auth/**,shared/utils/api-error.util.ts,app.routes.ts,app.config.ts}`.

------------------------------------------------------------------------

## Phase 11 — Frontend Books Feature ✅

- [x] `Book`/`BookRequest`/`ReadingStatus` models (`models/book.model.ts`) + `BookService` (`core/services/book.service.ts`, thin `HttpClient` wrapper for the 5 CRUD calls)
- [x] `BookList` (smart, `/books`) + `BookItem` (dumb, presentational card) — Signals (`books`, `isLoading`, `loadError`), a `sortedBooks`/`isEmpty` **Computed Signal**, `OnPush` on both
- [x] `@if`/`@for` control-flow syntax throughout; explicit **loading**, **error-with-retry**, and **empty** states (with a call-to-action to add the first book) alongside the normal grid
- [x] `BookForm` — a Material Dialog shared by Create and Edit, typed `FormBuilder.nonNullable` reactive form matching every validation rule from Specification.md §4 (Title ≤150, Author ≤100, Genre ≤50, PublicationYear 1450–current year, Rating 1–5, Notes ≤1000); performs the actual `POST`/`PUT` itself and maps field-level server errors back onto the form (same pattern as Login/Register)
- [x] `BookDetail` (`/books/:id`) — full details page with Edit/Delete actions, loading/error states
- [x] Delete flow uses the shared `ConfirmDialog` (`shared/components/confirm-dialog`) before calling `DELETE`, from both the list and the details page
- [x] `MatSnackBar` notifications for create/update/delete success and failure, using the API's own error message where available
- [x] Responsive layout: CSS grid (`auto-fill, minmax(280px, 1fr)`) for the book cards, stacking header/actions on narrow viewports

**Checkpoint:** `ng build` (dev + prod) and `ng test` both succeed — **49/49 tests pass** across 13 files, covering `BookService`, `BookList` (loading/empty/error/sort/navigation), `BookItem`, `BookForm` (create vs. edit pre-fill, client-side validation boundaries, POST vs. PUT, error handling), `BookDetail`, and `ConfirmDialog`.

**Design notes for your review:**
- Same caveat as Phase 10: **no browser-automation tool is available here**, so I could not click through the actual CRUD flow myself end-to-end in a real browser. Every piece is unit/HTTP-tested in isolation instead (including the exact request/response shapes verified against the real API in Phases 6–7). **Please do a manual pass**: `dotnet run` the API, `npm start` the frontend, register, then add/edit/delete a few books and confirm the empty state, validation messages, and delete confirmation feel right.
- `BookForm` intentionally performs its own HTTP call (rather than the parent doing it) so it can map server validation errors back onto the exact field and keep the dialog open on failure — consistent with how `Login`/`Register` behave.
- Files: `frontend/src/app/{models/book.model.ts,core/services/book.service.ts,features/books/**,shared/components/confirm-dialog/**,app.routes.ts}`.

------------------------------------------------------------------------

## Phase 12 — End-to-End Polish & Review ✅

- [x] Cross-checked every rule in `Specification.md` §4 and §6/§8 against the codebase (see table below)
- [x] Verified 403 handling is surfaced sensibly in the UI: `BookDetail` shows the API's own message (e.g. *"You do not have permission to access this book."*) in its error state if a user manually navigates to another user's book id; the Books list itself can never show another user's books in the first place (the API only ever returns the caller's own)
- [x] Cleaned up: no `TODO`/`FIXME` left in either codebase; the only `console.*` call in the frontend is the standard Angular bootstrap failure handler in `main.ts`; added the one missing `OnPush` (root `App` component)
- [x] Finalized `README.md` — tech stack, repo structure, prerequisites, backend build/run/test/migrations, full Books + Auth API reference (incl. error shapes/status codes), frontend setup/run/test, and a "running the whole app locally" walkthrough
- [x] Final full regression pass — **ran directly against the live API** (register → login → CRUD as two separate users), see results below

**Checkpoint:**
- Backend: `dotnet build` → **0 warnings, 0 errors**. `dotnet test` → **133/133 passing**.
- Frontend: `ng build` (dev + prod) → clean. `ng test` → **49/49 passing** (13 files).
- Live regression pass against a running instance of the API — **15/15 checks passed**: register ×2 users, login, create books (incl. a create with every optional field `null`), list isolation (user A sees only their 2 books, user B only their 1), `GetById`/`Update`/`Delete` as owner (200/200/204) vs. as a different user (403/403/403), invalid create → 400 with the exact expected field errors (`Title`, `Author`, `PublicationYear`, `Rating`), delete → subsequent `GetById` → 404, no token → 401.

**Specification cross-check:**

| Area | Status | Notes |
|---|---|---|
| §4 Business rules (required/optional fields, max lengths, year/rating ranges) | ✅ | Enforced in `BookRequestValidatorBase` (server) and mirrored client-side in `BookForm` |
| §4 Ownership / 403 Forbidden | ✅ | `BookService.GetOwnedBookOrThrowAsync`, verified live above |
| §5/§7 Backend requirements (.NET 10, Clean Architecture, SOLID, PostgreSQL, EF Core Code First, FluentValidation, Global Exception Middleware, DI, async/await, JWT, Swagger w/ XML comments) | ✅ | All phases 1–8 |
| §6/§8 Frontend requirements (Angular 21, Material, Standalone, Signals, Computed Signals, typed Reactive Forms + FormBuilder, OnPush, `@if`/`@for`, `inject()`, Guards, Interceptor, Smart/Dumb, feature folders, no template logic, getters, SnackBar, delete confirmation Dialog, loading/empty states, responsive) | ✅ | All phases 9–11; `@switch` wasn't needed anywhere (no natural discrete-value branch beyond what `@if`/`@else if` already expresses clearly) |
| §7 API guidelines (RESTful, verbs, status codes, friendly validation, consistent errors, 403 ownership) | ✅ | Verified live above |
| §8 Testing (xUnit/FluentAssertions/Moq covering Domain/Application/Validators/Controllers/Auth/Repository/Integration) | ✅ | 133 backend tests; frontend has its own Vitest suite (49 tests) even though not explicitly required by §8 |

**One honest limitation to flag:** I don't have a browser-automation tool in this environment, so all frontend UI verification was done via the Vitest suite + direct API regression testing rather than an actual click-through in a live browser. I'd still recommend you do a quick manual pass through the UI (register → add a few books → edit → delete → log out → log back in) before considering this fully signed off.

------------------------------------------------------------------------

## Legend

- `[ ]` Not started
- `[x]` Done & confirmed by you at the phase Checkpoint

We'll proceed phase by phase — nothing in a later phase starts until the current phase's checkpoint is confirmed.
