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
using IkincielKitapApi.Models;

namespace IkıncielKitapApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserUpdateDto updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            var changedFields = new List<string>();
            var unchangedFields = new List<string>();

            if (user.Username != updatedUser.Username)
            {
                user.Username = updatedUser.Username;
                changedFields.Add("username");
            }
            else
            {
                unchangedFields.Add("username");
            }

            if (user.Email != updatedUser.Email)
            {
                user.Email = updatedUser.Email;
                changedFields.Add("email");
            }
            else
            {
                unchangedFields.Add("email");
            }

            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
                changedFields.Add("password");
            }
            else
            {
                unchangedFields.Add("password");
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Kullanıcı başarıyla güncellendi.",
                changedFields,
                unchangedFields
            });
        }


        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /*Bu metodu ↓↓↓ kaldırıyorum çünkü Kullanıcı kaydını sadece auth/register kısmında yapacağım
         bu kısımda da sadece Admin yetkisi olacak daha güvenli bir sistem olacak.*/
        #region Kaldırılmış Http isteği

        
        //[Authorize(Roles = "Admin")]
        //[HttpPost]
        //public async Task<ActionResult<User>> PostUser(User user)
        //{
        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetUser", new { id = user.Id }, user);
        //}
        #endregion


        // DELETE: api/Users/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            bool hasOrders = _context.Orders.Any(o => o.BuyerId == id);
            if (hasOrders)
            {
                return BadRequest("Bu kullanıcıya ait siparişler olduğu için silinemez.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("me")]
        public IActionResult GetMyInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Role
            });
        }
        [Authorize]
        [HttpPut("me")]
        public IActionResult UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var updateMessages = new List<string>();

            // Kullanıcı adı güncelleme kontrolü
            if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != user.Username)
            {
                user.Username = request.Username;
                updateMessages.Add("Kullanıcı adı güncellendi.");
            }
            else if (request.Username == user.Username)
            {
                updateMessages.Add("Kullanıcı adı zaten aynı, güncellenmedi.");
            }

            // Email güncelleme kontrolü
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                bool emailExists = _context.Users.Any(u => u.Email == request.Email && u.Id != user.Id);
                if (emailExists)
                {
                    updateMessages.Add("Bu e-posta başka bir kullanıcı tarafından kullanılıyor, güncellenmedi.");
                }
                else
                {
                    user.Email = request.Email;
                    updateMessages.Add("E-posta adresi güncellendi.");
                }
            }
            else if (request.Email == user.Email)
            {
                updateMessages.Add("E-posta zaten aynı, güncellenmedi.");
            }

            // Şifre güncelleme kontrolü
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                updateMessages.Add("Şifre güncellendi.");
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "Profil güncelleme tamamlandı.",
                details = updateMessages
            });
        }


        public class UpdateProfileRequest
        {
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? Password { get; set; }
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
