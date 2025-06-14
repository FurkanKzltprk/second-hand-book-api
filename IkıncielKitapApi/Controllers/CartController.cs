using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IkincielKitapApi.Data;
using IkıncielKitapApi.Models;
using System.Security.Claims;

namespace IkincielKitapApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/cart
        [HttpGet]
        public IActionResult GetCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cartItems = _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.Id,
                    c.BookId,
                    c.Book!.Title,
                    c.Book.Author,
                    c.Book.Price,
                    c.AddedDate
                })
                .ToList();

            return Ok(cartItems);
        }

        // POST: api/cart/{bookId}
        [HttpPost("{bookId}")]
        public IActionResult AddToCart(int bookId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Kitap var mı kontrol
            var book = _context.Books.Find(bookId);
            if (book == null || book.IsSold)
                return BadRequest("Kitap bulunamadı veya zaten satılmış.");

            // Zaten sepette var mı kontrol
            bool alreadyInCart = _context.CartItems.Any(c => c.BookId == bookId && c.UserId == userId);
            if (alreadyInCart)
                return BadRequest("Bu kitap zaten sepette.");

            var cartItem = new CartItem
            {
                UserId = userId,
                BookId = bookId
            };

            _context.CartItems.Add(cartItem);
            _context.SaveChanges();

            return Ok("Kitap sepete eklendi.");
        }

        // DELETE: api/cart/{bookId}
        [HttpDelete("{bookId}")]
        public IActionResult RemoveFromCart(int bookId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var item = _context.CartItems.FirstOrDefault(c => c.BookId == bookId && c.UserId == userId);
            if (item == null)
                return NotFound("Bu kitap sepette yok.");

            _context.CartItems.Remove(item);
            _context.SaveChanges();

            return Ok("Kitap sepetten çıkarıldı.");
        }

        // POST: api/cart/checkout
        [HttpPost("checkout")]
        public IActionResult Checkout()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cartItems = _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToList();

            if (!cartItems.Any())
                return BadRequest("Sepetiniz boş.");

            foreach (var item in cartItems)
            {
                if (item.Book == null || item.Book.IsSold)
                    continue;

                var order = new Order
                {
                    BookId = item.BookId,
                    BuyerId = userId,
                    OrderDate = DateTime.Now
                };

                item.Book.IsSold = true;
                _context.Orders.Add(order);
            }

            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();

            return Ok("Satın alma işlemi tamamlandı. Sepet temizlendi.");
        }
    }
}
