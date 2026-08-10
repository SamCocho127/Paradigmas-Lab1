## 1. Baseline

- [x] 1.1 Build the solution (`dotnet build HackerRank1.sln`) and confirm it compiles
- [x] 1.2 Run `dotnet test` and record the current failing integration tests (unimplemented CRUD endpoints, auth mismatch on books GET) as the pre-change baseline

## 2. Domain layer

- [x] 2.1 Create `src/HackerRank1.Domain` class library (net8.0) with namespace `HackerRank1.Domain`
- [x] 2.2 Move `Book` and `Library` entities out of `LibraryContext.cs` into `HackerRank1.Domain` as POCOs (namespace `HackerRank1.Domain`)
- [x] 2.3 Move `User` and `JwtSettings` into `HackerRank1.Domain` (namespace `HackerRank1.Domain`)

## 3. Application layer

- [x] 3.1 Create `src/HackerRank1.Application` class library (net8.0) referencing `HackerRank1.Domain`, namespace `HackerRank1.Application`
- [x] 3.2 Define repository interfaces `ILibraryRepository` and `IBookRepository` in Application (namespaced `HackerRank1.Application`) covering get-by-id(s), add, update, delete, and books-by-library queries
- [x] 3.3 Move `ILibrariesService`/`LibrariesService` and `IBooksService`/`BooksService` into Application, replacing direct `LibraryContext` usage with repository dependency injection
- [x] 3.4 Move `IAuthenticationService`/`AuthenticationService` into Application (validation logic unchanged)
- [x] 3.5 Add `ITokenGenerator` interface in Application: `string Generate(User user, JwtSettings settings)`

## 4. Infrastructure layer

- [x] 4.1 Create `src/HackerRank1.Infrastructure` class library (net8.0) referencing `HackerRank1.Application` and `HackerRank1.Domain`, namespace `HackerRank1.Infrastructure`
- [x] 4.2 Move `LibraryContext` into Infrastructure (namespace `HackerRank1.Infrastructure`) with `DbSet<Book>`, `DbSet<Library>`
- [x] 4.3 Implement `EfLibraryRepository` and `EfBookRepository` in Infrastructure wrapping `LibraryContext` with the Application repository interfaces
- [x] 4.4 Move `TokenGenerator` into Infrastructure as the implementation of `ITokenGenerator` (JWT logic unchanged)
- [x] 4.5 Move the `Migrations` folder into `HackerRank1.Infrastructure.Migrations`; keep migration id `20260528004745_InitialCreate` and snapshot unchanged

## 5. API layer

- [x] 5.1 Create `src/HackerRank1.API` Web project (net8.0) referencing Application, Domain, and Infrastructure; namespace `HackerRank1.API`
- [x] 5.2 Move `Program.cs` (kept `public`) and `Startup.cs` into the API project
- [x] 5.3 Update `Startup.ConfigureServices`: register `LibraryContext` via `AddDbContextPool` with Npgsql and `MigrationsAssembly(typeof(LibraryContext).Assembly)`; register repositories, services, and `ITokenGenerator`; keep JWT, CORS (`localhost:5173`), Swagger, and controller setup as-is
- [x] 5.4 Move request DTOs (`BookForm`, `LibraryForm`, `User`) into the API project with their existing `[JsonProperty]` attributes
- [x] 5.5 Move `AuthController`, `BooksController`, `LibrariesController` into the API project; controllers now depend on Application interfaces (`ILibrariesService`, `IBooksService`, `IAuthenticationService`, `ITokenGenerator`) and DTOs instead of entities for request binding
- [x] 5.6 Delete the old `HackerRank1` project directory

## 6. CRUD completion and contract alignment

- [x] 6.1 Libraries `POST`: return 201 Created with the created library (spec: Create a library)
- [x] 6.2 Libraries `DELETE`: implement `LibrariesService.Delete` via repository; return 204, or 404 when the library does not exist (spec: Delete a library)
- [x] 6.3 Books `GET`: return 404 when the library does not exist, 200 with an (possibly empty) book list otherwise; remove `[Authorize]` so the endpoint is public (spec: List books in a library)
- [x] 6.4 Books `POST`: create the book and return 201, or 404 when the library does not exist (spec: Add a book to a library)
- [x] 6.5 Books `PUT`: update and return 204, or 404 when the book is missing (spec: Update a book)
- [x] 6.6 Books `DELETE`: implement `BooksService.Delete` via repository; return 204, or 404 when the book is missing (spec: Delete a book)
- [x] 6.7 Keep `PUT /api/libraries/{libraryId}` at 204/404 and `GET` endpoints at 200/404 per the existing contract

## 7. Solution and tests

- [x] 7.1 Restructure `HackerRank1.sln`: add `src/` and `tests/` solution folders with the four new projects and the test project
- [x] 7.2 Update `LibraryService.Integration.Test`: retarget references to `HackerRank1.API.Program`/`Startup`, `HackerRank1.Domain.Book`/`Library`, and the API `BookForm`; keep test logic unchanged
- [x] 7.3 Delete the orphaned `IntegrationTest/` project (duplicate, not referenced by the solution)

## 8. Verification

- [x] 8.1 `dotnet build` the solution with no warnings-as-errors and confirm all projects compile
- [x] 8.2 `dotnet test` — all integration tests pass (`TestAddBook_Ok_GetBook_NotFound`, `TestGetBooks_Ok_NotFound`, `TestDeleteLibrary`)
- [x] 8.3 Run the API (`dotnet run --project src/HackerRank1.API`) and smoke-test `/login`, libraries CRUD, and books CRUD against the local environment; confirm Supabase migration history is untouched
