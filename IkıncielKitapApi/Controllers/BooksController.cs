using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IkincielKitapApi.Data;
using IkıncielKitapApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace IkıncielKitapApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books.ToListAsync();
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // PUT: api/Books/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, [FromBody] Book updatedBook)
        {
            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            // ID kontrolü yok, güncellenecek alanları ata
            existingBook.Title = updatedBook.Title;
            existingBook.Author = updatedBook.Author;
            existingBook.Description = updatedBook.Description;
            existingBook.Price = updatedBook.Price;
            existingBook.CategoryId = updatedBook.CategoryId;
            existingBook.IsSold = updatedBook.IsSold;
            existingBook.PostedDate = updatedBook.PostedDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // POST: api/Books

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            // Token'dan gelen kullanıcıyı kitaba ata
            book.UserId = int.Parse(userId);
            book.PostedDate = DateTime.Now;
            book.IsSold = false;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBook", new { id = book.Id }, book);
        }


        // DELETE: api/Books/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks(
       string? title,
       string? author,
       decimal? minPrice,
       decimal? maxPrice,
       int? categoryId,
       string? sortBy = "postedDate",
       int page = 1,
       int pageSize = 20) //page size varsayılan olarak 20 ayarlandı , 20 tanesini getrir 20 den fazlası için
                          // benim gidip HTTP isteğime &pagesize=25 , tarzında bir şey demem gerekiyor 25 oldu
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(title))
                query = query.Where(b => b.Title.Contains(title));

            if (!string.IsNullOrEmpty(author))
                query = query.Where(b => b.Author.Contains(author));

            if (minPrice.HasValue)
                query = query.Where(b => b.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(b => b.Price <= maxPrice.Value);

            if (categoryId.HasValue)
                query = query.Where(b => b.CategoryId == categoryId.Value);

            query = sortBy?.ToLower() switch
            {
                "title" => query.OrderBy(b => b.Title),
                "price" => query.OrderBy(b => b.Price),
                _ => query.OrderByDescending(b => b.PostedDate)
            };

            var totalItems = await query.CountAsync();
            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                books
            });
        }

        [Authorize]
        [HttpGet("mybooks")]
        public IActionResult GetMyBooks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var books = _context.Books
                .Where(b => b.UserId.ToString() == userId)
                .ToList();

            return Ok(books);
        }
        [HttpGet("available")]
        public IActionResult GetAvailableBooks()
        {
            var availableBooks = _context.Books
                .Where(b => !b.IsSold)
                .ToList();

            return Ok(availableBooks);
        }

    }
}
