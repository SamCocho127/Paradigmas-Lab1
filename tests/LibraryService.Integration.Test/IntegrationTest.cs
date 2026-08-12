
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryService.Api;
using LibraryService.Modules.Auth;
using LibraryService.Modules.Books;
using LibraryService.SharedKernel.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Xunit;

namespace LibraryService.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly WebApplicationFactory<Program> _testFactory;
        private readonly LibraryContext context;

        public HttpClient Client { get; private set; }

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            context = new LibraryContext(new DbContextOptionsBuilder<LibraryContext>()
                        .UseSqlite("DataSource=:memory:")
                        .EnableSensitiveDataLogging()
                        .Options);
            _testFactory = _factory.WithWebHostBuilder(builder =>
                builder
                .ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(LibraryContext));
                    services.AddSingleton(context);

                    context.Database.OpenConnection();
                    context.Database.Migrate();

                    // Clear local context cache
                    foreach (var entity in context.ChangeTracker.Entries().ToList())
                    {
                        entity.State = EntityState.Detached;
                    }
                })
            );

            Client = _testFactory.CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetTokenAsync().GetAwaiter().GetResult());
        }

        private async Task<string> GetTokenAsync()
        {
            var user = new User { Email = "admin", Password = "1234" };
            var response = await Client.PostAsync("/login",
                new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(body);
            tokenResponse!.token.Should().NotBeNullOrEmpty();
            return tokenResponse.token;
        }

        private async Task SeedLibrary()
        {
            var libraries = new List<Library>
            {
                new Library { Name = "Library Name 1", Location = "Location 1" },
                new Library { Name = "Library Name 2", Location = "Location 2" },
                new Library { Name = "Library Name 3", Location = "Location 3" },
                new Library { Name = "Library Name 4", Location = "Location 4" }
            };

            await context.Libraries.AddRangeAsync(libraries);
            await context.SaveChangesAsync();  // Save to the database
        }

        private async Task SeedBook(string bookName, int libraryId)
        {
            var bookForm = new BookForm
            {
                Name = bookName
            };
            var response1 = await Client.PostAsync($"/api/libraries/{libraryId}/books",
                new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));
        }

        // TEST NAME - addBookToLibrary
        // TEST DESCRIPTION - It adds book to a library
        [Fact]
        public async Task TestAddBook_Ok_GetBook_NotFound()
        {
            await SeedLibrary();

            var bookForm = new BookForm
            {
                Name = "Test book 1",
            };

            var response1 = await Client.PostAsync($"/api/libraries/1/books",
                new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));

            response1.StatusCode.Should().Be(HttpStatusCode.Created);

            bookForm = new BookForm
            {
                Name = "Test book 2",
            };

            var response2 = await Client.PostAsync($"/api/libraries/100/books",
                new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));

            response2.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // TEST NAME - getBooksInALibrary
        // TEST DESCRIPTION - It finds all books in a library by ID
        [Fact]
        public async Task TestGetBooks_Ok_NotFound()
        {
            await SeedLibrary();

            await SeedBook("test book 1", 1);
            await SeedBook("test book 2", 1);

            var response1 = await Client.GetAsync($"/api/libraries/2/books");
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            var content1 = await response1.Content.ReadAsStringAsync();
            var books = JsonConvert.DeserializeObject<IEnumerable<Book>>(content1)!.ToList();
            books.Count.Should().Be(0);

            var response2 = await Client.GetAsync($"/api/libraries/1/books");
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
            var content2 = await response2.Content.ReadAsStringAsync();
            var books2 = JsonConvert.DeserializeObject<IEnumerable<Book>>(content2)!.ToList();
            books2.Count.Should().Be(2);

            var response3 = await Client.GetAsync($"/api/libraries/31232/books");
            response3.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // TEST NAME - booksRequireAuth
        // TEST DESCRIPTION - Books endpoints reject requests without a valid JWT
        [Fact]
        public async Task TestBooks_WithoutToken_Unauthorized()
        {
            var anonClient = _testFactory.CreateClient();

            var response = await anonClient.GetAsync("/api/libraries/1/books");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // TEST NAME - swaggerAvailable
        // TEST DESCRIPTION - Swagger JSON endpoint is served in the development pipeline
        [Fact]
        public async Task TestSwaggerJson_Ok()
        {
            var response = await Client.GetAsync("/swagger/v1/swagger.json");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // TEST NAME - deleteLibraryById
        // TEST DESCRIPTION - Check delete library web api end point
        [Fact]
        public async Task TestDeleteLibrary()
        {
            await SeedLibrary();

            var bookForm = new BookForm
            {
                Name = "test book 1",
            };

            // add book to library
            var response0 = await Client.PostAsync("/api/libraries/1/books",
                new StringContent(JsonConvert.SerializeObject(bookForm), Encoding.UTF8, "application/json"));
            response0.StatusCode.Should().Be(HttpStatusCode.Created);

            // delete library
            var response1 = await Client.DeleteAsync("/api/libraries/1");
            response1.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify that delete is successful
            var response2 = await Client.GetAsync("/api/libraries/1/books");
            response2.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var response3 = await Client.DeleteAsync("/api/libraries/1");
            response3.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
