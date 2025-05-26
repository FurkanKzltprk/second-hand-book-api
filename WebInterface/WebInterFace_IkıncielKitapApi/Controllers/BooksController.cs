using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using WebInterFace_IkıncielKitapApi.Models;
public class BooksController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BooksController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var httpClient = _httpClientFactory.CreateClient();

        // Session'dan token al
        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Account");
        }

        // Authorization header ekle
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // API URL'sini kendi backend API adresinle değiştir
        var response = await httpClient.GetAsync("https://localhost:7078/api/Books");

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            // JSON'u deserialize et (kitap listesi)
            var books = JsonSerializer.Deserialize<List<Book>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(books);
        }
        else
        {
            // Hata durumunda boş liste veya hata mesajı dönebilirsin
            return View(new List<Book>());
        }
    }
    public async Task<IActionResult> Details(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        // Session'dan token al
        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
        {
            // Eğer token yoksa login sayfasına yönlendir
            return RedirectToAction("Login", "Account");
        }

        // Authorization header'a token ekle
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // API'den tek kitap bilgisi al
        var response = await httpClient.GetAsync($"https://localhost:7078/api/Books/{id}");

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            // Kitap detayını deserialize et
            var book = JsonSerializer.Deserialize<Book>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(book);
        }
        else
        {
            // Hata durumunda HTTP durumu ve mesajı konsola yazdır
            var statusCode = response.StatusCode;
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status Code: {statusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            // Hata durumunda anasayfaya yönlendirebilirsin veya hata sayfası gösterebilirsin
            return RedirectToAction("Index");
        }


    }
    // GET: /Books/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Books/Create
    [HttpPost]
    public async Task<IActionResult> Create(Book book)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Account");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(book);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://localhost:7078/api/Books", content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Kitap eklenirken hata oluştu: {errorContent}");
            return View(book);
        }
    }

    // GET: /Books/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"https://localhost:7078/api/Books/{id}");
        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var jsonString = await response.Content.ReadAsStringAsync();
        var book = JsonSerializer.Deserialize<Book>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(book);
    }

    // POST: /Books/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.Id)
            return BadRequest();

        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(book);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PutAsync($"https://localhost:7078/api/Books/{id}", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        ModelState.AddModelError("", "Kitap güncellenirken hata oluştu.");
        return View(book);
    }

    // GET: /Books/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"https://localhost:7078/api/Books/{id}");
        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var jsonString = await response.Content.ReadAsStringAsync();
        var book = JsonSerializer.Deserialize<Book>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(book);
    }

    // POST: /Books/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.DeleteAsync($"https://localhost:7078/api/Books/{id}");

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        ModelState.AddModelError("", "Kitap silinirken hata oluştu.");
        return RedirectToAction("Delete", new { id });
    }







}


