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
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }


        //Sadece Admin erişebilir. Kullanıcılar erişemez.
        // GET: api/Orders
        [Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Book)
                    .ThenInclude(b => b.User)   // Kitabın sahibi bilgisi için
                .Include(o => o.Buyer)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                o.Id,
                BookTitle = o.Book != null ? o.Book.Title : "Bilinmiyor",
                BookAuthor = o.Book != null ? o.Book.Author : "Bilinmiyor",
                BuyerUsername = o.Buyer != null ? o.Buyer.Username : "Bilinmiyor",
                SellerUsername = o.Book?.User != null ? o.Book.User.Username : "Bilinmiyor",  // Satıcı
                o.OrderDate
            });

            return Ok(result);
        }


        //Sadece Admin erişebilir
        
        [Authorize(Roles ="Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Book)
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var result = new
            {
                order.Id,
                BookTitle = order.Book != null ? order.Book.Title : "Bilinmiyor",
                BookAuthor = order.Book != null ? order.Book.Author : "Bilinmiyor",
                BuyerUsername = order.Buyer != null ? order.Buyer.Username : "Bilinmiyor",
                order.OrderDate
            };

            return Ok(result);
        }


        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //Sadece Admin erişebilir.
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, [FromBody] Order updatedOrder)
        {
            var existingOrder = await _context.Orders
                .Include(o => o.Book)
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existingOrder == null)
                return NotFound();

            // Sadece güncellenebilir alanları değiştiriyoruz
            existingOrder.BookId = updatedOrder.BookId;
            existingOrder.BuyerId = updatedOrder.BuyerId;
            existingOrder.OrderDate = updatedOrder.OrderDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }



        // Not!!! : Bu endpoint sadece Admin tarafından manuel sipariş girişi için kullanılabilir.
        // Artık kullanıcılar siparişlerini Cart üzerinden verir.

        // POST: api/Orders
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            // Sipariş verilen kitap
            var book = await _context.Books.FindAsync(order.BookId);

            if (book == null)
                return BadRequest("Kitap bulunamadı.");

            if (book.IsSold)
                return BadRequest("Bu kitap zaten satılmış.");

            // Kitabı satılmış olarak işaretle
            book.IsSold = true;

            // Siparişi ekle
            order.OrderDate = DateTime.Now;
            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            return Ok("Sipariş başarıyla oluşturuldu ve kitap satıldı olarak işaretlendi.");
        }

        // DELETE: api/Orders/5
        //Sadece Admin erişebilir.

        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //Kullanıcıya açık kendi siparişlerini görebilir !!

        [Authorize]
        [HttpGet("myorders")]
        public IActionResult GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var orders = _context.Orders
                .Where(o => o.BuyerId.ToString() == userId)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.BookId,  //bunu sonradan ekledim
                    o.Book.Title,
                    o.Book.Author,
                    o.Book.Price
                })
                .ToList();

            return Ok(orders);
        }


        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
