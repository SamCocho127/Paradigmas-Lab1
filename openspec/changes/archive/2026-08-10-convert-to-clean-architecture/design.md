## Context

Current state (see proposal.md for the why): everything lives in one `HackerRank1` project — controllers, services, `LibraryContext` + entities, EF migrations, DTOs, JWT auth, and configuration — with mixed `LibraryService.WebAPI.*` and `HackerRank1.*` namespaces. net8.0 targets throughout. A prior abandoned attempt already created `src/HackerRank1.{API,Application,Domain,Infrastructure}` folders (only stale bin/obj today); its compiled artifacts confirm the intended project/namespace naming (`HackerRank1.API.Controllers`, `HackerRank1.API.DTO`).

Constraints that shape the design:
- The integration test project (`LibraryService.Integration.Test`, net8.0) drives acceptance via `WebApplicationFactory<Program>` + `UseStartup<Startup>` and directly constructs `LibraryContext` with SQLite. The refactor must keep this harness functional.
- The `NotImplementedException` stubs (`LibrariesService.Delete`, `BooksService.Add/Update/Delete`, missing `DELETE api/libraries/{id}`) are deferred to a separate change and move as-is.

## Goals / Non-Goals

**Goals:**
- Four layers with the Clean Architecture dependency rule: API → Application/Infrastructure → Domain (Infrastructure → Application for repository contracts). Nothing depends on the API.
- Persistence-ignorant domain: entities are plain POCOs; EF concerns live only in Infrastructure.
- Composition root in the API layer; DI registration organized per layer via extension methods.
- Zero external behavior change: endpoints, status codes, auth rules, JSON shapes, CORS, Swagger, and the integration test suite behave identically.

**Non-Goals:**
- Implementing the missing endpoints (`NotImplementedException` stubs) — separate change.
- Migrating to minimal hosting (`WebApplication.CreateBuilder`) — Startup pattern is kept for test-harness compatibility; a later change can migrate both.
- Rotating the leaked Supabase credential or moving secrets to user-secrets (flagged as a risk; remediation is a separate change).
- Fixing pre-existing `[Authorize]` inconsistencies (Books endpoints authorized, Libraries endpoints anonymous) — preserved as-is.

## Decisions

### 1. Project layout and namespaces
```
src/
├── HackerRank1.Domain            net8.0  (no package refs)
│   └── Entities/          Library, Book
├── HackerRank1.Application       net8.0  (refs Domain)
│   ├── Interfaces/       ILibraryRepository, IBookRepository
│   ├── Services/         ILibrariesService, LibrariesService,
│   │                     IBooksService, BooksService,
│   │                     IAuthenticationService, AuthenticationService
│   └── Contracts/        JwtSettings, User, TokenGenerator
├── HackerRank1.Infrastructure    net8.0  (refs Domain, Application; EF Core + Npgsql)
│   ├── Persistence/      LibraryContext (Fluent-API config)
│   ├── Repositories/     LibraryRepository, BookRepository
│   ├── Migrations/
│   └── DependencyInjection.cs    (AddInfrastructure)
└── HackerRank1.API               net8.0  (refs Application, Infrastructure)
    ├── Controllers/      AuthController, BooksController, LibrariesController
    ├── DTO/              BookForm, LibraryForm
    ├── Extensions/       AddApplication/AddInfrastructure/AddPresentation
    ├── Program.cs, Startup.cs
    └── appsettings*.json
```
Root `HackerRank1/` project is deleted once the new API compiles and tests pass.
**Rationale:** `HackerRank1.<Layer>` matches the solution name, the existing project name, and the abandoned `src/` attempt's DLLs.
**Alternative considered:** `LibraryService.*` namespaces (dominant in today's source) — rejected; the project/solution names and ghost artifacts all say `HackerRank1`.

### 2. Domain entities are plain POCOs
`Library` and `Book` move to `HackerRank1.Domain.Entities` as persistence-ignorant classes. Drop the `[Key]` DataAnnotations; configure keys and the `Book → Library` relationship via Fluent API in `LibraryContext.OnModelCreating`. Keep `Book.LibraryId` and the `Book.Library` navigation property.
**Rationale:** standard Clean Architecture practice; keeps Domain free of any framework hints.
**Alternative:** keep `[Key]` (it's BCL `System.ComponentModel.DataAnnotations`, so no EF package needed in Domain) — rejected for cleanliness; Fluent config is the canonical EF approach and consolidates mapping in Infrastructure.

### 3. Repositories bridge Application services to EF
Application defines thin contracts that preserve today's query semantics exactly:
- `ILibraryRepository.ListAsync(int[] ids)` — filters by ids when non-empty (mirrors current `LibrariesService.Get`), plus `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `SaveChangesAsync`.
- `IBookRepository.ListAsync(int libraryId, int[] ids)`, plus Add/Update/Delete/SaveChanges.
Implementations in Infrastructure wrap `LibraryContext` and return domain entities.
**Rationale:** the Dependency Rule — Application services (`LibrariesService`, `BooksService`) keep their current method signatures and orchestration, but depend on repository interfaces instead of `LibraryContext`. This is what makes Infrastructure swappable.
**Alternative:** skip repositories and let Application depend on an `IRepository<T>` generic — rejected; a generic with the same filtering semantics ends up as a leaky `IQueryable` abstraction. Per-aggregate contracts are simpler to keep behavior-preserving.

### 4. Auth stays in Application, wiring in API
`IAuthenticationService`/`AuthenticationService` (hardcoded `admin`/`1234`), `TokenGenerator` (JWT), `User`, and `JwtSettings` move to Application as contracts/helpers. `AuthController` and JWT/Swagger/CORS configuration stay in the API composition root, which binds `JwtSettings` from appsettings.
**Rationale:** token generation is pure logic (no I/O) so it needs no Infrastructure dependency; the API owns middleware/config concerns.
**Alternative:** put `TokenGenerator` in Infrastructure — rejected, would add an unnecessary framework seam to a pure function.

### 5. Composition root keeps the Startup pattern
`Program` + `Startup` remain, but `ConfigureServices` delegates to per-layer extension methods: `services.AddApplication()` (services + auth), `services.AddInfrastructure()` (DbContext + repositories, Npgsql with retry policy, migration-on-bootstrap call), and API-level registration (JWT, CORS, Swagger, controllers). `appsettings.json`/`appsettings.Development.json` stay in the API project.
**Rationale:** preserves the test harness (`UseStartup<Startup>()` + `ConfigureServices` override for the SQLite `LibraryContext`) with zero test-harness churn.
**Alternative:** minimal hosting — better net8 idiom but forces the test factory rewrite; deferred to keep the refactor behavior-preserving and reviewable.

### 6. Solution, test project, and repo hygiene
- Rewrite `HackerRank1.sln`: the four new `src/` projects + `LibraryService.Integration.Test`.
- `IntegrationTest.cs` updates: `using HackerRank1.API;` (Program), `HackerRank1.Domain.Entities` (Library/Book), `HackerRank1.Infrastructure.Persistence` (LibraryContext), `HackerRank1.API.DTO` (BookForm). Behavior of every `[Fact]` is unchanged.
- Bump test project packages to net8.0-aligned versions: `Microsoft.EntityFrameworkCore.Sqlite`/`InMemory` 6.0.0 → 8.0.x, `Microsoft.AspNetCore.Mvc.Testing` 6.0.0 → 8.0.x.
- Delete orphaned `IntegrationTest/` (net6.0 duplicate, tracked but not in the solution) and empty `tests/` (only stale bin/obj).
- Stale bin/obj under `src/` from the abandoned attempt are overwritten by the new builds; no manual cleanup needed (untracked anyway).

## Risks / Trade-offs

- **Namespace churn breaks test usings** → the test project is updated in the same change; `dotnet test` is the acceptance gate. The external contract is exercised verbatim by the unchanged test cases.
- **Moving EF migrations can break the bootstrap migrate call** → the `DbContext` type name (`LibraryContext`) and its `DesignTime`/snapshot wiring carry over; verify `Startup` bootstrap and run `dotnet ef migrations list` (or a build + run) before deleting the old project.
- **Credential in `appsettings.Development.json`** (Supabase password committed in `HackerRank1/appsettings.Development.json:3`) → out of scope to fix here, but the file moves verbatim to the API project; strongly recommend a follow-up to rotate the password and use env vars/user-secrets. This is a standing risk regardless of the refactor.
- **Keeping the Startup pattern** trades modern minimal hosting for zero test-harness churn → acceptable; documented as a Non-Goal to revisit later.
- **Deleting the root project loses the small git history on those files** → history is only 2 commits and `git` retains it; the move is additive for review (`git diff` of new projects + sln).

## Migration Plan

1. Scaffold the four projects and move code layer by layer (Domain → Application → Infrastructure → API).
2. Update the solution and the test project; delete orphans.
3. Verify: `dotnet build HackerRank1.sln`, then `dotnet test`, then `dotnet run --project src/HackerRank1.API` and smoke-test `/api/libraries`, `/api/libraries/{id}/books` (with JWT), `/login`, Swagger UI.
4. Delete the root `HackerRank1/` project only after the new solution builds and tests pass.
5. Rollback: `git revert` of the change commit (2-commit history makes this trivial).

## Open Questions

None — decisions above resolve the namespace scheme, repository strategy, composition root, and test-project handling. Any unknowns (e.g., completing the stubbed endpoints, rotating the credential) are explicitly scoped to separate changes.
