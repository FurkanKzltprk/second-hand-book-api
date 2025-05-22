using Microsoft.AspNetCore.Mvc;
using IkincielKitapApi.Data;
using Microsoft.AspNetCore.Authorization;
using IkıncielKitapApi.Models;

namespace IkincielKitapApi.Controllers
{
    [Authorize]  // İstersen roller ekleyebilirsin, örn [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public IActionResult GetCategories()
        {
            var categories = _context.Categories.ToList();
            return Ok(categories);
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        public IActionResult GetCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        public IActionResult PostCategory([FromBody] Category category)
        {
            if (_context.Categories.Any(c => c.Name == category.Name))
                return BadRequest("Bu kategori zaten mevcut.");

            _context.Categories.Add(category);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        public IActionResult PutCategory(int id, [FromBody] Category updatedCategory)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            if (_context.Categories.Any(c => c.Name == updatedCategory.Name && c.Id != id))
                return BadRequest("Bu kategori adı zaten kullanılıyor.");

            category.Name = updatedCategory.Name;
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();

            // Eğer kategoriye bağlı kitap varsa silmeyi engelle
            bool hasBooks = _context.Books.Any(b => b.CategoryId == id);
            if (hasBooks)
                return BadRequest("Bu kategoriye bağlı kitaplar var, silinemez.");

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
