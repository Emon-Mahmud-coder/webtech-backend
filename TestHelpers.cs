using System.Net.Http.Json;
using LibraryApi.Dtos;

namespace LibraryApi.Tests;

public static class TestHelpers
{
    public static async Task<AuthorResponse> CreateAuthor(HttpClient c, string name = "Humayun Ahmed")
    {
        var res = await c.PostAsJsonAsync("/api/authors", new AuthorRequest { Name = name, Bio = "Bangladeshi writer" });
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<AuthorResponse>())!;
    }

    public static BookRequest NewBook(int authorId, string? isbn = null) => new()
    {
        Title = "Misir Ali",
        Isbn = isbn ?? "ISBN-" + Guid.NewGuid().ToString("N")[..10],
        Year = 1990,
        Price = 250.50m,
        AuthorId = authorId
    };
}
