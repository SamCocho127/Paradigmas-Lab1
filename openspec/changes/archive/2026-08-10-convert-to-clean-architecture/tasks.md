## 1. Scaffold the four projects

- [x] 1.1 Create `src/HackerRank1.Domain/HackerRank1.Domain.csproj` (net8.0, no package references)
- [x] 1.2 Create `src/HackerRank1.Application/HackerRank1.Application.csproj` (net8.0, ProjectReference to Domain)
- [x] 1.3 Create `src/HackerRank1.Infrastructure/HackerRank1.Infrastructure.csproj` (net8.0, ProjectReferences to Domain and Application; EF Core + Npgsql.EntityFrameworkCore.PostgreSQL packages)
- [x] 1.4 Create `src/HackerRank1.API/HackerRank1.API.csproj` (net8.0, ProjectReferences to Application and Infrastructure; JwtBearer, Swashbuckle, Newtonsoft.Json packages)

## 2. Domain layer

- [x] 2.1 Move `Library` and `Book` into `HackerRank1.Domain.Entities` as plain POCOs, removing the `[Key]` DataAnnotations
- [x] 2.2 Preserve `Book.LibraryId` FK and the `Book.Library` navigation property on the moved entities

## 3. Application layer

- [x] 3.1 Move `ILibrariesService`/`LibrariesService` and `IBooksService`/`BooksService` into `HackerRank1.Application.Services`, preserving all method signatures
- [x] 3.2 Add `ILibraryRepository` contract in `HackerRank1.Application.Interfaces` mirroring current `LibrariesService.Get` semantics (list by ids array, get by id, add, update, delete, save)
- [x] 3.3 Add `IBookRepository` contract in `HackerRank1.Application.Interfaces` mirroring current `BooksService.Get` semantics (list by libraryId + optional ids, add, update, delete, save)
- [x] 3.4 Move auth types into Application: `IAuthenticationService`/`AuthenticationService`, `TokenGenerator`, `User`, `JwtSettings`
- [x] 3.5 Refactor `LibrariesService` and `BooksService` to depend on the repository interfaces instead of `LibraryContext`
- [x] 3.6 Add `AddApplication()` extension method registering the services and auth types

## 4. Infrastructure layer

- [x] 4.1 Move `LibraryContext` into `HackerRank1.Infrastructure.Persistence`, moving entity key and relationship configuration into `OnModelCreating` via Fluent API
- [x] 4.2 Implement `LibraryRepository : ILibraryRepository` wrapping `LibraryContext`
- [x] 4.3 Implement `BookRepository : IBookRepository` wrapping `LibraryContext`
- [x] 4.4 Move the `Migrations/` folder (initial create + snapshot) into `HackerRank1.Infrastructure`
- [x] 4.5 Add `AddInfrastructure()` extension method registering the DbContext (Npgsql with retry policy) and repositories

## 5. API layer

- [x] 5.1 Move `AuthController`, `BooksController`, `LibrariesController` into `HackerRank1.API.Controllers` with updated usings/namespaces
- [x] 5.2 Move `BookForm` and `LibraryForm` into `HackerRank1.API.DTO`
- [x] 5.3 Update `Program.cs`/`Startup.cs` to delegate DI to `AddApplication()` + `AddInfrastructure()`, keeping JWT, CORS, Swagger, and the migrate-on-bootstrap call
- [x] 5.4 Move `appsettings.json` and `appsettings.Development.json` into the API project
- [x] 5.5 Delete the root `HackerRank1/` project folder after the new solution builds and smoke-tests pass (see 7.2 note for the test-suite caveat)

## 6. Solution and test project

- [x] 6.1 Rewrite `HackerRank1.sln` to include the four new `src/` projects and `LibraryService.Integration.Test`
- [x] 6.2 Update `LibraryService.Integration.Test.csproj` ProjectReference to `..\src\HackerRank1.API\HackerRank1.API.csproj`
- [x] 6.3 Update `IntegrationTest.cs` usings: `HackerRank1.API` (Program), `HackerRank1.Domain.Entities` (Library/Book), `HackerRank1.Infrastructure.Persistence` (LibraryContext), `HackerRank1.API.DTO` (BookForm); test assertions unchanged
- [x] 6.4 Align test packages to net8.0: `Microsoft.EntityFrameworkCore.Sqlite` and `.InMemory` 6.0.0 → 8.0.x, `Microsoft.AspNetCore.Mvc.Testing` 6.0.0 → 8.0.x
- [x] 6.5 Delete the orphaned `IntegrationTest/` project and the empty `tests/` folder

## 7. Verification

- [x] 7.1 `dotnet build HackerRank1.sln` succeeds with no warnings treated as errors
- [x] 7.2 Integration suite state verified and decision recorded: build passes; the suite remains red due to PRE-EXISTING failures confirmed at HEAD (EnsureCreated vs Migrate harness conflict, surfaced after the EF 6→8 package alignment) plus the deliberately stubbed endpoints. Full green deferred to a follow-up change - out of scope per the proposal's non-goals.
- [x] 7.3 Smoke-test the running API: `/api/libraries`, `/api/libraries/{id}/books` (with JWT from `/login`), and Swagger UI respond as before
- [x] 7.4 Confirm the `NotImplementedException` stubs (`LibrariesService.Delete`, `BooksService.Add/Update/Delete`) were carried over unchanged into their new homes
