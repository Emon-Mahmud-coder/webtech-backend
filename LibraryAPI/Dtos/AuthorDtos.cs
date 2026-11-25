using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Dtos;

public class AuthorRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Bio { get; set; }
}

public class AuthorResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int BookCount { get; set; }
}
