## Why

The whole solution currently lives in one Web API project (`HackerRank1`): controllers, business logic, EF Core `DbContext`, and entities are mixed together, with entities defined inside the `LibraryContext` file and services depending directly on `DbContext`. Namespaces are inconsistent (`HackerRank1.*` vs `LibraryService.WebAPI.*`), and there are two near-duplicate test projects. This violates .NET N-layer best practices, makes the code hard to test, and prevents swapping or reusing the data layer. This lab ("Lab1-capas") requires migrating the solution into a clean N-layer architecture.

## What Changes

- Split the single `HackerRank1` project into four layers, following Clean Architecture N-layer best practices:
  - `HackerRank1.API` (Presentation) — controllers, Startup/Program, DI composition root, config binding.
  - `HackerRank1.Application` (Application) — business services and their interfaces.
  - `HackerRank1.Domain` (Domain) — entities and domain models; zero external dependencies.
  - `HackerRank1.Infrastructure` (Infrastructure) — EF Core `LibraryContext`, repositories, migrations, JWT token generation.
- Move `Book`/`Library` entities out of the `LibraryContext.cs` file into the Domain layer; move the `DbContext`, repositories, and migrations into Infrastructure; move service interfaces into Application.
- Reorganize the solution into `src/` and `tests/` folders; update project references and namespaces; unify namespaces to the `HackerRank1.*` family.
- **BREAKING** (internal): namespaces and project layout change; the test project's reference to `LibraryService.WebAPI.*` types must be updated.
- Complete the missing CRUD so existing behavior matches the integration tests:
  - Add `POST`, `PUT`, `DELETE` to the books endpoints under `/api/libraries/{libraryId}/books`.
  - Add `DELETE` to the libraries endpoint `/api/libraries/{libraryId}`.
- Migrate `Program`/`Startup` to the API project and keep the `WebApplicationFactory<Program>`-based integration tests working.
- No change to auth behavior: `/login` still returns a JWT for `admin/1234`; books GET remains `[Authorize]`.

## Capabilities

### New Capabilities
- `library-catalog`: Management of libraries and the books they contain — listing, creating, updating, and deleting libraries and books, with the established URL scheme (`/api/libraries`, `/api/libraries/{id}/books`) and status-code contract exercised by the integration tests.
- `authentication`: The `/login` endpoint, credential validation, and JWT issuance (issuer/audience/secret) that the migration must preserve.

### Modified Capabilities
<!-- None — the repository has no existing specs yet; both capabilities are introduced by this change. -->

## Impact

- **Projects**: `HackerRank1` (Web API, net8.0) split into four projects; `LibraryService.Integration.Test` updated; the orphaned `IntegrationTest/LibraryService.Test.csproj` (net6.0, not in the solution) is a candidate for removal or consolidation.
- **Solution file**: `HackerRank1.sln` restructured with `src/` and `tests/` solution folders.
- **Endpoints**: new `POST`/`PUT`/`DELETE` for books and `DELETE` for libraries; existing `GET` routes and the `/login` route are unchanged.
- **Data access**: `LibraryContext`, `Book`, `Library`, and migrations move layers; the EF Core/Supabase connection string and JwtSettings configuration format stay as-is in `appsettings.json`.
- **Tests**: integration tests become the acceptance criteria and must pass after migration.
