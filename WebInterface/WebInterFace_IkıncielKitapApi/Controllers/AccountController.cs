using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;  // Session için gerekli

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var loginData = new
        {
            email = username,
            password = password
        };

        var json = JsonSerializer.Serialize(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://localhost:7078/api/Auth/login", content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            var token = root.GetProperty("token").GetString();

            // Role bilgisi varsa al, yoksa "User" kabul et
            var role = root.TryGetProperty("role", out var roleElement)
                ? roleElement.GetString()
                : "User";

            // Session'a token ve rol bilgisi kaydedilir
            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("UserRole", role);

            return RedirectToAction("Index", "Books");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre yanlış.");
            return View();
        }
    }


    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(string email, string username, string password)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var registerData = new
        {
            email = email,
            username = username,
            password = password
        };

        var json = JsonSerializer.Serialize(registerData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://localhost:7078/api/Auth/register", content);

        if (response.IsSuccessStatusCode)
        {
            // Kayıt başarılı ise login sayfasına yönlendir
            return RedirectToAction("Login");
        }
        else
        {
            ModelState.AddModelError("", "Kayıt sırasında hata oluştu.");
            return View();
        }
    }

}
