using System.Net;
using System.Net.Http.Json;
using LibraryApi.Dtos;
using static LibraryApi.Tests.TestHelpers;

namespace LibraryApi.Tests;

public class AuthorsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Author_FullCrudLifecycle()
    {
        // CREATE -> 201 + Location header
        var createRes = await _client.PostAsJsonAsync("/api/authors", new AuthorRequest { Name = "Rabindranath", Bio = "Poet" });
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        Assert.NotNull(createRes.Headers.Location);
        var created = (await createRes.Content.ReadFromJsonAsync<AuthorResponse>())!;
        Assert.True(created.Id > 0);
        Assert.Equal("Rabindranath", created.Name);

        // READ one -> 200
        var got = await _client.GetFromJsonAsync<AuthorResponse>($"/api/authors/{created.Id}");
        Assert.Equal("Poet", got!.Bio);

        // READ list -> contains it
        var list = await _client.GetFromJsonAsync<List<AuthorResponse>>("/api/authors");
        Assert.Contains(list!, a => a.Id == created.Id);

        // UPDATE -> 204, then verify change persisted
        var upd = await _client.PutAsJsonAsync($"/api/authors/{created.Id}", new AuthorRequest { Name = "Rabindranath Tagore", Bio = "Nobel laureate" });
        Assert.Equal(HttpStatusCode.NoContent, upd.StatusCode);
        var after = await _client.GetFromJsonAsync<AuthorResponse>($"/api/authors/{created.Id}");
        Assert.Equal("Rabindranath Tagore", after!.Name);

        // DELETE -> 204, then 404 on read
        var del = await _client.DeleteAsync($"/api/authors/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
        var gone = await _client.GetAsync($"/api/authors/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public async Task Create_InvalidName_Returns400(string name)
    {
        var res = await _client.PostAsJsonAsync("/api/authors", new AuthorRequest { Name = name });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        var res = await _client.GetAsync("/api/authors/99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Update_Unknown_Returns404()
    {
        var res = await _client.PutAsJsonAsync("/api/authors/99999", new AuthorRequest { Name = "Ghost Writer" });
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_Returns404()
    {
        var res = await _client.DeleteAsync("/api/authors/99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Search_FiltersByName()
    {
        await CreateAuthor(_client, "Zafar Iqbal");
        var list = await _client.GetFromJsonAsync<List<AuthorResponse>>("/api/authors?search=zafar");
        Assert.NotEmpty(list!);
        Assert.All(list!, a => Assert.Contains("zafar", a.Name.ToLower()));
    }

    [Fact]
    public async Task Delete_AuthorWithBooks_Returns409()
    {
        var author = await CreateAuthor(_client);
        var bookRes = await _client.PostAsJsonAsync("/api/books", NewBook(author.Id));
        Assert.Equal(HttpStatusCode.Created, bookRes.StatusCode);

        var del = await _client.DeleteAsync($"/api/authors/{author.Id}");
        Assert.Equal(HttpStatusCode.Conflict, del.StatusCode);

        var refreshed = await _client.GetFromJsonAsync<AuthorResponse>($"/api/authors/{author.Id}");
        Assert.Equal(1, refreshed!.BookCount);
    }
}
