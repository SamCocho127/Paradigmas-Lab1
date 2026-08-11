## Why

The entire solution currently lives in a single `HackerRank1` project: controllers, services, entities, `DbContext`, EF migrations, DTOs, JWT auth, and configuration are all mixed together with mixed namespaces (`LibraryService.WebAPI.*` and `HackerRank1.*`). This makes the code hard to test, evolve, and reason about, and it fails the .NET Clean Architecture best-practice layering expected for this lab. The goal is to restructure the solution into the standard Clean Architecture four-layer layout without changing any externally observable behavior.

## What Changes

- Split the monolith into four projects under `src/`:
  - `HackerRank1.Domain` — entities only, no dependencies.
  - `HackerRank1.Application` — service/use-case interfaces and implementations, DTO/contract types used by the app layer.
  - `HackerRank1.Infrastructure` — EF Core `DbContext`, Fluent-API entity configuration, repositories, and migrations.
  - `HackerRank1.API` — controllers, request DTOs, JWT auth wiring, Swagger/CORS, composition root, appsettings.
- Update `HackerRank1.sln` to reference the new projects and the reorganized test project.
- Move EF migrations from `HackerRank1/Migrations` into `HackerRank1.Infrastructure`.
- Re-point the integration test project (`LibraryService.Integration.Test`) at the new API project and namespaces; align its NuGet package versions to net8.0 (currently references EF 6.0 packages on a net8.0 target).
- Delete the orphaned net6.0 duplicate test project `IntegrationTest/` and the empty `tests/` folder.
- **BREAKING** (internal only): all namespaces change to the `HackerRank1.<Layer>` scheme (`HackerRank1.Domain.Entities.Library`, `HackerRank1.Infrastructure.Persistence.LibraryContext`, `HackerRank1.API.Controllers.*`, etc.). External HTTP contract is unchanged.

## Capabilities

### New Capabilities

None. This is a pure structural refactor: no endpoints, status codes, auth rules, or request/response shapes change. Per the spec-driven schema, a change with zero behavior change opts out of specs (`skip_specs: true` in `.openspec.yaml`) rather than inventing requirements.

### Modified Capabilities

None.

## Impact

- **Affected projects**: `HackerRank1` (replaced by the four new projects), `LibraryService.Integration.Test` (updated), `IntegrationTest/` (deleted, orphan), `tests/` (deleted, empty).
- **Moved code**: entities → Domain; `LibraryContext` + migrations + repositories → Infrastructure; service interfaces/implementations + auth service + token helper → Application; controllers + DTOs + composition root + settings → API.
- **Dependencies**: EF Core + Npgsql move to Infrastructure only; ASP.NET Core packages stay in the API project; Domain has zero package dependencies.
- **Behavior preserved**: all endpoints (`GET/POST/DELETE api/libraries...`, `GET api/libraries/{id}/books`, `POST /login`), response codes, JWT auth, CORS, Swagger, and the integration test suite remain functionally identical.
- **Known deferred work**: the `NotImplementedException` stubs in the services move as-is to their new homes; completing the missing endpoint implementations is a separate change and out of scope here.
