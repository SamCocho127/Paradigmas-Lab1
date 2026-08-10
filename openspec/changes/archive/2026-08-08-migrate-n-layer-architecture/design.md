## Context

Today the entire system lives in one Web API project (`HackerRank1`): controllers, services, EF Core `DbContext`, entities (defined inside `LibraryContext.cs`), and static helpers are all mixed together. Services depend directly on `LibraryContext`, namespaces are inconsistent (`HackerRank1.*` vs `LibraryService.WebAPI.*`), and there is an orphaned `IntegrationTest/` project not referenced by the solution. The integration tests reference `Program`/`Startup`, the `Book`/`Library` entities, and `BookForm` from the old namespaces, and expect CRUD endpoints that are not yet implemented (`POST`/`PUT`/`DELETE` books, `DELETE` libraries). See proposal.md - Why for motivation and specs/ for the required behavior contract.

Key constraints shaping the approach:
- Tests use `WebApplicationFactory<Program>` + `UseStartup<Startup>()`, so classic hosting must be preserved and `Program`/`Startup` must stay `public` in the API project.
- The Postgres database is already migrated in Supabase; the migration id `20260528004745_InitialCreate` must survive the move so EF does not try to re-create the schema.
- The `Book` ↔ `Library` FK already cascades (`onDelete: ReferentialAction.Cascade`).

## Goals / Non-Goals

**Goals:**
- Four clean layers with one-way dependencies: API → Application → Domain and Infrastructure → Application → Domain.
- Entities and DbContext separated; services depend on repository interfaces, not EF Core.
- Zero change to the external wire contract beyond the specified additions (new CRUD endpoints, books GET becomes public).
- Existing integration tests become green.

**Non-Goals:**
- No database schema changes and no new migration.
- No change to JWT config format (`JwtSettings` in `appsettings.json`) or the Supabase connection string.
- No change to the CORS policy or frontend integration.
- Not introducing an external DI container, mapper library (AutoMapper, etc.), or a new test framework.

## Decisions

### Decision 1: Project layout

```
Paradigmas-Lab1/
├─ src/
│  ├─ HackerRank1.Domain/          (classlib, net8.0) — Book, Library, User, JwtSettings (POCOs only)
│  ├─ HackerRank1.Application/     (classlib, net8.0) — service + repository interfaces and implementations
│  ├─ HackerRank1.Infrastructure/  (classlib, net8.0) — LibraryContext, EF repositories, TokenGenerator, Migrations
│  └─ HackerRank1.API/             (Web SDK, net8.0) — Controllers, Program, Startup, request DTOs, config binding
└─ tests/
   └─ LibraryService.Integration.Test/  (net8.0, updated references)
```

Dependency direction (enforced by project references):

```
        ┌─────────────┐
        │ HackerRank1.API │
        └──────┬──────┘
        ┌──────┴──────┐
   ┌────▼────┐   ┌────▼────────────┐
   │Application│   │Infrastructure    │
   └────┬────┘   └────┬────────────┘
        └──────┬──────┘
        ┌──────▼──────┐
        │   Domain    │
        └─────────────┘
```

Alternatives considered: a flat three-project split (API/Business/Data). Chosen four-layer Clean split because the user selected it and it is the standard .NET N-layer best practice; it keeps Domain dependency-free so entities can never leak persistence concerns.

### Decision 2: Repository pattern

- `ILibraryRepository` and `IBookRepository` interfaces live in **Application** (the abstraction of persistence the services consume).
- Implementations (`EfLibraryRepository`, `EfBookRepository`) live in **Infrastructure** and wrap `LibraryContext`.
- `LibrariesService`/`BooksService` move to **Application** and take the repositories via constructor injection; they no longer reference EF Core or `LibraryContext`.

Rationale: this is the core of "N-layer best practice" — business logic is decoupled from the ORM and testable against fakes. Alternative considered: keep services in Infrastructure next to DbContext. Rejected, as that keeps persistence leaking into business logic.

### Decision 3: Token generation becomes a service

- `ITokenGenerator` interface (in **Application**) with `string Generate(User user, JwtSettings settings)`.
- `TokenGenerator` implementation moves to **Infrastructure** (JWT is infrastructure).
- `AuthController` depends on `IAuthenticationService` + `ITokenGenerator` instead of calling a static helper.

Rationale: keeps the API layer thin and makes the token behavior replaceable/testable. `AuthenticationService` (Application) keeps the existing `admin`/`1234` validation; no credential behavior changes.

### Decision 4: Preserve classic hosting

`Program.cs`/`Startup.cs` move to `HackerRank1.API` unchanged in structure (`CreateHostBuilder` + `UseStartup`). `Program` and `Startup` remain `public` so `WebApplicationFactory<Program>` and `UseStartup<Startup>()` in the test project keep working with only a namespace change.

Rationale: the test harness depends on this hosting shape. Alternative considered: migrate to minimal hosting (`.WithWebApplicationBuilder`). Rejected — higher churn and risks the test harness.

### Decision 5: Namespaces and JSON contract

- Unify namespaces: `HackerRank1.Domain`, `HackerRank1.Application`, `HackerRank1.Infrastructure`, `HackerRank1.API`. `LibraryService.WebAPI.*` is retired.
- Responses continue to serialize via System.Text.Json camelCase (current behavior). Do **not** add `AddNewtonsoftJson()` — it would change the wire format to PascalCase.
- Request DTOs (`BookForm`, `LibraryForm`, `User`) keep their Newtonsoft `[JsonProperty]` attributes; tests serialize them with `JsonConvert`, which honors those attributes. `User` keeps `Email`/`Password`/`Role` PascalCase properties (System.Text.Json binding is case-insensitive).

### Decision 6: Books GET becomes public

Per the user decision, the `[Authorize]` attribute is removed from `GET /api/libraries/{libraryId}/books`. The integration tests call it without a token, and it is part of the acceptance contract. No other endpoints gained or lost auth (`/login` stays anonymous; libraries endpoints were already public).

### Decision 7: Migrations move to Infrastructure

Migrations and `LibraryContextModelSnapshot` move into `HackerRank1.Infrastructure.Migrations`. Because EF tracks applied migrations by migration id (`20260528004745_InitialCreate`), which does not change, the Supabase database's `__EFMigrationsHistory` remains valid and `db.Database.Migrate()` will see the migration as already applied.

The DbContext registration in `Startup` sets `MigrationsAssembly` to the Infrastructure assembly so runtime tooling finds the moved migrations.

### Decision 8: CRUD completion in the layers

- **Libraries**: `Delete` implemented in `LibrariesService` via `ILibraryRepository.Delete`; FK cascade removes books. Controller gains `[HttpDelete("{libraryId}")]` returning 204/404, and `POST` returns 201 Created with the created library.
- **Books**: `Add`/`Update`/`Delete` implemented in `BooksService`. Controller gains `POST` (201 / 404 when library missing), `PUT` (204/404), `DELETE` (204/404). `GET` additionally returns 404 when the library does not exist (the existing test asserts this).
- Library-existence checks happen in the controller path by querying the library repository, preserving the spec's status-code contract.

### Decision 9: Test project consolidation

- Keep the net8.0 `LibraryService.Integration.Test` and update it to the new namespaces (`Program`, `Startup`, `Book`, `BookForm`).
- Delete the orphaned `IntegrationTest/` project (net6.0, byte-for-byte duplicate of the kept project, not referenced by `HackerRank1.sln`).

Rationale: two identical test projects serve no purpose. Alternative considered: keep both. Rejected as pure duplication.

## Risks / Trade-offs

- **Migrations assembly move** → EF could try to create a new schema if it can't match applied migration ids. → Set `MigrationsAssembly` explicitly; keep the migration id unchanged; verify against Supabase before/after.
- **Removing `[Authorize]` on books GET** reduces API security. → Deliberate acceptance-criteria decision (tests call it anonymously); `/login` and token issuance are unchanged and auth remains available for future endpoints.
- **Cascade delete in SQLite in-memory tests** may not be FK-enforced by the provider. → EF Core client-side cascade deletes dependent books tracked in the change tracker, and the spec only requires 404 from the controller afterward; the controller checks library existence rather than trusting DB cascade.
- **Deleting the orphaned test project** discards net6-specific scaffolding. → It is a duplicate not referenced by the solution; kept project covers the same scenarios.
- **JSON nuance** (`[JsonProperty]` on DTOs only affecting Newtonsoft serialization) is easy to break by adding Newtonsoft to the pipeline. → Documented in Decision 5; no pipeline change.

## Migration Plan

1. Confirm baseline: solution builds; integration tests fail only on the unimplemented CRUD endpoints and the auth mismatch (record the current failure list).
2. Create `HackerRank1.Domain`, `HackerRank1.Application`, `HackerRank1.Infrastructure`; move entities, services, DbContext, repositories, `TokenGenerator`, and migrations into them with new namespaces.
3. Create `HackerRank1.API` from the current `HackerRank1` project; rewire `Startup` DI (repositories, services, token generator, `MigrationsAssembly`); delete the old project.
4. Implement missing CRUD (Decision 8) and the library-existence checks.
5. Update `HackerRank1.sln` with `src/`/`tests/` solution folders and new project references; update the integration test project namespaces; delete `IntegrationTest/`.
6. Run `dotnet build` and `dotnet test` until green.
7. Manual smoke check: `dotnet run`, hit `/login`, `GET`/`POST`/`PUT`/`DELETE` on libraries and books.

Rollback: `git revert` of the migration commit. No schema change was made, so rollback is safe.
