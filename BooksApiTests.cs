using System.Net;
using System.Net.Http.Json;
using LibraryApi.Dtos;
using static LibraryApi.Tests.TestHelpers;

namespace LibraryApi.Tests;

public class BooksApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Book_FullCrudLifecycle()
    {
        var author = await CreateAuthor(_client);

        // CREATE
        var createRes = await _client.PostAsJsonAsync("/api/books", NewBook(author.Id));
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var created = (await createRes.Content.ReadFromJsonAsync<BookResponse>())!;
        Assert.Equal(author.Name, created.AuthorName);

        // READ one
        var got = await _client.GetFromJsonAsync<BookResponse>($"/api/books/{created.Id}");
        Assert.Equal(250.50m, got!.Price);

        // READ list
        var list = await _client.GetFromJsonAsync<List<BookResponse>>("/api/books");
        Assert.Contains(list!, b => b.Id == created.Id);

        // UPDATE
        var update = NewBook(author.Id, created.Isbn);
        update.Title = "Himu";
        update.Price = 300m;
        var upd = await _client.PutAsJsonAsync($"/api/books/{created.Id}", update);
        Assert.Equal(HttpStatusCode.NoContent, upd.StatusCode);
        var after = await _client.GetFromJsonAsync<BookResponse>($"/api/books/{created.Id}");
        Assert.Equal("Himu", after!.Title);
        Assert.Equal(300m, after.Price);

        // DELETE
        var del = await _client.DeleteAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/books/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Create_WithUnknownAuthor_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/api/books", NewBook(99999));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateIsbn_Returns409()
    {
        var author = await CreateAuthor(_client);
        var book = NewBook(author.Id, "DUP-ISBN-001");
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/books", book)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/books", book)).StatusCode);
    }

    [Fact]
    public async Task Create_InvalidPrice_Returns400()
    {
        var author = await CreateAuthor(_client);
        var book = NewBook(author.Id);
        book.Price = -5;
        var res = await _client.PostAsJsonAsync("/api/books", book);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Create_MissingTitle_Returns400()
    {
        var author = await CreateAuthor(_client);
        var book = NewBook(author.Id);
        book.Title = "";
        var res = await _client.PostAsJsonAsync("/api/books", book);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var author = await CreateAuthor(_client);
        var res = await _client.PutAsJsonAsync("/api/books/99999", NewBook(author.Id));
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var res = await _client.DeleteAsync("/api/books/99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GetAll_FiltersByAuthorId()
    {
        var a1 = await CreateAuthor(_client, "Author One");
        var a2 = await CreateAuthor(_client, "Author Two");
        await _client.PostAsJsonAsync("/api/books", NewBook(a1.Id));
        await _client.PostAsJsonAsync("/api/books", NewBook(a2.Id));

        var list = await _client.GetFromJsonAsync<List<BookResponse>>($"/api/books?authorId={a1.Id}");
        Assert.Single(list!);
        Assert.All(list!, b => Assert.Equal(a1.Id, b.AuthorId));
    }
}
