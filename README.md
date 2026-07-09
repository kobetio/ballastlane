# My Library

Full-stack personal book collection manager. Each registered user manages their own independent
collection of books (create, list, view, edit, delete) — nobody can see or touch another user's
books. See `Specification.md` and `SRS AI Development Guide.md` for the full requirements, and
`Development Plan.md` for the phased build checklist (with design notes for every phase).

## Tech Stack

- **Backend:** .NET 10, ASP.NET Core Web API (Controllers), Entity Framework Core (Code First + Migrations), PostgreSQL, JWT Authentication, Swagger (Swashbuckle)
- **Frontend:** Angular 21, Angular Material, Standalone Components, Signals, Reactive Forms
- **Testing:** xUnit, FluentAssertions, Moq (backend) · Vitest (frontend)

## Repository Structure

```
backend/
  MyLibrary.slnx
  src/
    MyLibrary.Domain/         # Entities, enums, guard clauses — no external dependencies
    MyLibrary.Application/    # DTOs, service interfaces/implementations, FluentValidation validators
    MyLibrary.Infrastructure/ # EF Core DbContext, repositories, JWT, password hashing
    MyLibrary.Api/            # Controllers, DI wiring, Swagger, middleware, filters
  tests/
    MyLibrary.Tests/          # xUnit + FluentAssertions + Moq — Domain/Application/Infrastructure/Api

frontend/
  src/app/
    core/                     # interceptors, guards, services (AuthService, BookService)
    shared/                   # reusable components (ConfirmDialog) and utils
    layout/                   # MainLayout (toolbar shell for authenticated routes)
    features/
      auth/                   # Login, Register
      books/                  # BookList, BookItem, BookForm (dialog), BookDetail
    models/                   # TypeScript interfaces mirroring the API's DTOs
  src/environments/           # environment.ts (prod) / environment.development.ts (local dev)
```

## Prerequisites

| Tool | Version used to build this repo |
|---|---|
| PostgreSQL | 16.13 |
| .NET SDK | 10.0.202 |
| Node.js | v22.22.2 (≥ 20.19 or ≥ 22.12 required by Angular 21) |
| npm | 10.9.7 |
| Angular CLI | 21.2.19 (`npm i -g @angular/cli@21`) |

---

## Backend

### 1. Database setup (PostgreSQL 16.13) — do this first

The app connects to a local PostgreSQL 16.13 instance using a dedicated role (not the `postgres` superuser).
`dotnet run`/`dotnet ef database update` below will fail until this is done.

**One-time local setup** — connect as the `postgres` superuser and run:

```sql
CREATE ROLE mylibrary_user WITH LOGIN PASSWORD '<choose-a-strong-password>';
CREATE DATABASE mylibrary_db OWNER mylibrary_user;
GRANT ALL PRIVILEGES ON DATABASE mylibrary_db TO mylibrary_user;
```

**Configure the connection string** (kept out of git via `dotnet user-secrets`, never committed to `appsettings*.json`):

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=mylibrary_db;Username=mylibrary_user;Password=<your-password>" --project src/MyLibrary.Api
```

**Configure the JWT signing secret** (also via `dotnet user-secrets`, key `Jwt:Secret`, never committed).
Non-secret settings (`Issuer`, `Audience`, `ExpiryMinutes`) live in `appsettings.json` under the `Jwt` section:

```bash
dotnet user-secrets set "Jwt:Secret" "<a long random string>" --project backend/src/MyLibrary.Api
```

**Apply migrations** to create the `Users`/`Books` tables:

```bash
cd backend
dotnet ef database update --project src/MyLibrary.Infrastructure --startup-project src/MyLibrary.Api
```

To create a *new* migration later, after changing an entity/configuration:

```bash
dotnet ef migrations add <MigrationName> --project src/MyLibrary.Infrastructure --startup-project src/MyLibrary.Api --output-dir Persistence/Migrations
```

### 2. Build, run, test

```bash
cd backend
dotnet build
dotnet run --project src/MyLibrary.Api
```

- Swagger UI (Development environment): `http://localhost:5073/swagger` (see `src/MyLibrary.Api/Properties/launchSettings.json` for the exact port)
- Click **Authorize** (top right of Swagger) and paste a JWT (with or without the `Bearer ` prefix) to call protected endpoints from the docs page.
- `src/MyLibrary.Api/MyLibrary.Api.http` has ready-to-run sample requests (register/login/CRUD) if your editor supports `.http` files (e.g. VS Code REST Client, Visual Studio, Rider).

```bash
cd backend
dotnet test
```

Tests never touch the real PostgreSQL database: repository tests and the HTTP integration tests
(`tests/MyLibrary.Tests/Api/**`) run against an in-memory SQLite database, so `dotnet test` works
even without PostgreSQL running or configured. As of the last full run: **133/133 tests pass, 0 build warnings.**

### API Reference

**Authentication** (public, no token required):

```
POST /api/auth/register   { "name", "email", "password" } -> 201 { userId, name, email, token, expiresAtUtc }
POST /api/auth/login      { "email", "password" }          -> 200 { userId, name, email, token, expiresAtUtc }
```

Use the returned `token` as a bearer token on every subsequent request: `Authorization: Bearer <token>`.

**Books** (require `Authorization: Bearer <token>`; every endpoint only ever operates on the caller's own books):

```
GET    /api/books        -> 200 [ Book, ... ]
GET    /api/books/{id}   -> 200 Book | 403 (not the owner) | 404 (doesn't exist)
POST   /api/books        { title, author, genre?, publicationYear?, readingStatus?, rating?, notes? } -> 201 Book
PUT    /api/books/{id}   { ...same shape... }                                                          -> 200 Book | 403 | 404
DELETE /api/books/{id}   -> 204 | 403 | 404
```

`readingStatus` is one of `"WantToRead"`, `"Reading"`, `"Read"` (serialized as its string name, not a raw number).
Business rules (Specification.md §4): `title` ≤150 chars, `author` ≤100 chars, `genre` ≤50 chars,
`publicationYear` between 1450 and the current year, `rating` between 1 and 5, `notes` ≤1000 chars.

**Errors:** every error response is a `ProblemDetails`/`ValidationProblemDetails` JSON body:

```json
{ "title": "One or more validation errors occurred.", "status": 400, "errors": { "Title": ["Title is required."] } }
```

| Status | Meaning |
|---|---|
| 400 | Validation failure (field-level `errors`) or business-rule failure (e.g. duplicate email) |
| 401 | Missing/invalid/expired token, or wrong login credentials |
| 403 | Authenticated, but the resource belongs to another user |
| 404 | Resource doesn't exist |
| 500 | Unexpected server error (generic message; details are logged server-side, never leaked to the client) |

---

## Frontend

### Setup, run, test

```bash
cd frontend
npm install
npm start          # ng serve -> http://localhost:4200, expects the API at http://localhost:5073
npm test           # ng test  -> Vitest, headless, single run
npm run build      # ng build -> production bundle in dist/frontend
```

The API base URL is configured per-environment in `src/environments/` (`environment.development.ts`
points at `http://localhost:5073/api`; swap `environment.ts` for a deployed backend). As of the last
full run: **49/49 tests pass** across 13 files.

### Feature overview

- **Auth** (`features/auth/`): Register/Login with typed Reactive Forms, client + server-side
  validation (server field errors are shown on the exact matching control), JWT persisted to
  `localStorage` via `AuthService` so a refresh doesn't log you out. `authInterceptor` attaches the
  token to every request and force-logs-out on a `401` from an expired/invalid session.
  `authGuard`/`guestGuard` protect the `/books` routes and keep logged-in users off `/login`/`/register`.
- **Books** (`features/books/`): `BookList` (grid with loading/empty/error states) → `BookItem` cards
  → `BookForm` (a Material Dialog shared by Create/Edit, matching every backend validation rule) and
  `BookDetail` (`/books/:id`, full page). Deleting always goes through a shared `ConfirmDialog` first.
  Every outcome (create/update/delete, success or failure) surfaces via `MatSnackBar`, using the API's
  own error message where available.

## Running the whole app locally

1. Start PostgreSQL, then `dotnet run --project backend/src/MyLibrary.Api` (defaults to `http://localhost:5073`).
2. In another terminal: `cd frontend && npm start` (defaults to `http://localhost:4200`).
3. Open `http://localhost:4200`, register a new account, and start managing your library.
