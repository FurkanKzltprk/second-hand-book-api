using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using WebInterFace_IkıncielKitapApi.Models;

public class OrdersController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OrdersController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /Orders
    public async Task<IActionResult> Index()
    {
        var httpClient = _httpClientFactory.CreateClient();
        var token = HttpContext.Session.GetString("JWToken");
        var role = HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;

        if (role == "Admin")
        {
            response = await httpClient.GetAsync("https://localhost:7078/api/Orders");
        }
        else
        {
            response = await httpClient.GetAsync("https://localhost:7078/api/Orders/myorders");
        }

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var orders = JsonSerializer.Deserialize<List<Order>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(orders);
        }

        return View(new List<Order>());
    }



    // GET: /Orders/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"https://localhost:7078/api/Orders/{id}");

        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var jsonString = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<Order>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(order);
    }


    // GET: /Orders/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Orders/Create
    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(order);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://localhost:7078/api/Orders", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var errorContent = await response.Content.ReadAsStringAsync();

        // Sadece "Bu kitap zaten satılmış." mesajını al, diğerlerini yok say
        if (errorContent.Contains("Bu kitap zaten satılmış."))
        {
            ModelState.Clear(); // Önce diğer hataları temizle
            ModelState.AddModelError("", "Bu kitap zaten satılmış.");
        }
        else
        {
            ModelState.Clear();
            ModelState.AddModelError("", "Sipariş oluşturulurken hata oluştu.");
        }

        return View(order);
    }


    // GET: /Orders/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"https://localhost:7078/api/Orders/{id}");

        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var jsonString = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<Order>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(order);
    }

    // POST: /Orders/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Order order)
    {
        if (id != order.Id)
            return BadRequest();

        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var json = JsonSerializer.Serialize(order);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PutAsync($"https://localhost:7078/api/Orders/{id}", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        ModelState.AddModelError("", "Sipariş güncellenirken hata oluştu.");
        return View(order);
    }

    // GET: /Orders/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync($"https://localhost:7078/api/Orders/{id}");
        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var jsonString = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<Order>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(order);
    }

    // POST: /Orders/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.DeleteAsync($"https://localhost:7078/api/Orders/{id}");

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        ModelState.AddModelError("", "Sipariş silinirken hata oluştu.");
        return RedirectToAction("Delete", new { id });
    }

    // GET: /Orders/MyOrders
    public async Task<IActionResult> MyOrders()
    {
        var httpClient = _httpClientFactory.CreateClient();

        var token = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.GetAsync("https://localhost:7078/api/Orders/myorders");

        if (!response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var jsonString = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<Order>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(orders);
    }

}
