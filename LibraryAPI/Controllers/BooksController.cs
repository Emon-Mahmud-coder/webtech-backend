using LibraryApi.Data;
using LibraryApi.Dtos;
using LibraryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(AppDbContext db) : ControllerBase
{
    private IQueryable<BookResponse> Projection() => db.Books.Select(b => new BookResponse
    {
        Id = b.Id, Title = b.Title, Isbn = b.Isbn, Year = b.Year, Price = b.Price,
        AuthorId = b.AuthorId, AuthorName = b.Author!.Name
    });

    // READ (list) with optional filters
    [HttpGet]
    public async Task<ActionResult<List<BookResponse>>> GetAll([FromQuery] int? authorId, [FromQuery] string? search)
    {
        var q = Projection();
        if (authorId.HasValue) q = q.Where(b => b.AuthorId == authorId);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(b => b.Title.ToLower().Contains(search.ToLower()));
        return await q.OrderBy(b => b.Id).ToListAsync();
    }

    // READ (single)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id)
    {
        var book = await Projection().FirstOrDefaultAsync(b => b.Id == id);
        return book is null ? NotFound() : book;
    }

    // CREATE
    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(BookRequest req)
    {
        if (!await db.Authors.AnyAsync(a => a.Id == req.AuthorId))
            return BadRequest(new ProblemDetails { Title = "Author does not exist", Status = 400 });
        if (await db.Books.AnyAsync(b => b.Isbn == req.Isbn))
            return Conflict(new ProblemDetails { Title = "ISBN already exists", Status = 409 });

        var entity = new Book
        {
            Title = req.Title.Trim(), Isbn = req.Isbn, Year = req.Year,
            Price = req.Price, AuthorId = req.AuthorId
        };
        db.Books.Add(entity);
        await db.SaveChangesAsync();
        var res = await Projection().FirstAsync(b => b.Id == entity.Id);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, res);
    }

    // UPDATE
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BookRequest req)
    {
        var entity = await db.Books.FindAsync(id);
        if (entity is null) return NotFound();
        if (!await db.Authors.AnyAsync(a => a.Id == req.AuthorId))
            return BadRequest(new ProblemDetails { Title = "Author does not exist", Status = 400 });
        if (await db.Books.AnyAsync(b => b.Isbn == req.Isbn && b.Id != id))
            return Conflict(new ProblemDetails { Title = "ISBN already exists", Status = 409 });

        entity.Title = req.Title.Trim();
        entity.Isbn = req.Isbn;
        entity.Year = req.Year;
        entity.Price = req.Price;
        entity.AuthorId = req.AuthorId;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Books.FindAsync(id);
        if (entity is null) return NotFound();
        db.Books.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
