using LibraryApi.Data;
using LibraryApi.Dtos;
using LibraryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController(AppDbContext db) : ControllerBase
{
    private IQueryable<AuthorResponse> Projection() => db.Authors.Select(a => new AuthorResponse
    {
        Id = a.Id, Name = a.Name, Bio = a.Bio, BookCount = a.Books.Count
    });

    // READ (list)
    [HttpGet]
    public async Task<ActionResult<List<AuthorResponse>>> GetAll([FromQuery] string? search)
    {
        var q = Projection();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(a => a.Name.ToLower().Contains(search.ToLower()));
        return await q.OrderBy(a => a.Id).ToListAsync();
    }

    // READ (single)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorResponse>> GetById(int id)
    {
        var author = await Projection().FirstOrDefaultAsync(a => a.Id == id);
        return author is null ? NotFound() : author;
    }

    // CREATE
    [HttpPost]
    public async Task<ActionResult<AuthorResponse>> Create(AuthorRequest req)
    {
        var entity = new Author { Name = req.Name.Trim(), Bio = req.Bio };
        db.Authors.Add(entity);
        await db.SaveChangesAsync();
        var res = new AuthorResponse { Id = entity.Id, Name = entity.Name, Bio = entity.Bio, BookCount = 0 };
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, res);
    }

    // UPDATE
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AuthorRequest req)
    {
        var entity = await db.Authors.FindAsync(id);
        if (entity is null) return NotFound();
        entity.Name = req.Name.Trim();
        entity.Bio = req.Bio;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);
        if (entity is null) return NotFound();
        if (entity.Books.Count > 0)
            return Conflict(new ProblemDetails { Title = "Author still has books", Status = 409 });
        db.Authors.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
