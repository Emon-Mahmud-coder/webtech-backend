using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Dtos;

public class BookRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 5)]
    public string Isbn { get; set; } = string.Empty;

    [Range(1000, 2100)]
    public int Year { get; set; }

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int AuthorId { get; set; }
}

public class BookResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
}
