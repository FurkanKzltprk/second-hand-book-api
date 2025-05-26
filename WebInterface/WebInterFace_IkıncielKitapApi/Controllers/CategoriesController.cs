using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using WebInterFace_IkıncielKitapApi.Models;

public class CategoriesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CategoriesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /Categories
    public async Task<IActionResult> Index()
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync("https://localhost:7078/api/Categories");

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var categories = JsonSerializer.Deserialize<List<Category>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(categories);
        }
        else
        {
            return View(new List<Category>());
        }
    }

    // GET: /Categories/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Categories/Create
    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(category);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://localhost:7078/api/Categories", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var errorContent = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError("", $"Kategori eklenirken hata oluştu: {errorContent}");
        return View(category);
    }

    // GET: /Categories/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"https://localhost:7078/api/Categories/{id}");

        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var jsonString = await response.Content.ReadAsStringAsync();
        var category = JsonSerializer.Deserialize<Category>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(category);
    }

    // POST: /Categories/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id)
            return BadRequest();

        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(category);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PutAsync($"https://localhost:7078/api/Categories/{id}", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        ModelState.AddModelError("", "Kategori güncellenirken hata oluştu.");
        return View(category);
    }

    // GET: /Categories/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"https://localhost:7078/api/Categories/{id}");

        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var jsonString = await response.Content.ReadAsStringAsync();
        var category = JsonSerializer.Deserialize<Category>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(category);
    }

    // POST: /Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.DeleteAsync($"https://localhost:7078/api/Categories/{id}");

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        ModelState.AddModelError("", "Kategori silinirken hata oluştu.");
        return RedirectToAction("Delete", new { id });
    }
}
