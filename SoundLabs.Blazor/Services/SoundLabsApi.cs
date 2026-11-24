using System.Net.Http.Headers;
using System.Net.Http.Json;
using SoundLabs.Blazor.Models;

namespace SoundLabs.Blazor.Services;
public sealed class SoundLabsApi(HttpClient http, AuthStore auth)
{
    private HttpRequestMessage Request(HttpMethod method, string url)
    {
        var r = new HttpRequestMessage(method, url);
        if (!string.IsNullOrWhiteSpace(auth.Current?.Token)) r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Current.Token);
        return r;
    }
    private async Task<T> Send<T>(HttpRequestMessage request)
    {
        using var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new Exception(await Error(response));
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
    private static async Task<string> Error(HttpResponseMessage r)
    {
        try { var body = await r.Content.ReadFromJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>(); if (body?.TryGetValue("message", out var m) == true) return m.ToString(); } catch { }
        return $"Ошибка запроса: {(int)r.StatusCode} {r.ReasonPhrase}";
    }
    public Task<List<Product>> Products(string? search = null, int? category = null) => Send<List<Product>>(Request(HttpMethod.Get, "api/products" + Query(search, category)));
    public Task<Product> Product(int id) => Send<Product>(Request(HttpMethod.Get, $"api/products/{id}"));
    public Task<List<Category>> Categories() => Send<List<Category>>(Request(HttpMethod.Get, "api/categories"));
    public Task<List<ShopService>> Services() => Send<List<ShopService>>(Request(HttpMethod.Get, "api/services"));
    public Task<AuthResponse> Login(string login, string password) => Send<AuthResponse>(Json(HttpMethod.Post, "api/auth/login", new LoginRequest(login,password), false));
    public Task<AuthResponse> Register(RegisterRequest data) => Send<AuthResponse>(Json(HttpMethod.Post, "api/auth/register", data, false));
    public Task<ProfileDto> Profile() => Send<ProfileDto>(Request(HttpMethod.Get, "api/auth/profile"));
    public async Task SaveProfile(UpdateProfileRequest data)
    {
        using var r = await http.SendAsync(Json(HttpMethod.Put, "api/auth/profile", data));
        if (!r.IsSuccessStatusCode) throw new Exception(await Error(r));
    }
    public Task<List<Employee>> Employees() => Send<List<Employee>>(Request(HttpMethod.Get, "api/employees"));
    public async Task SaveEmployee(EmployeeSave data, int? id = null) { using var r = await http.SendAsync(Json(id is null ? HttpMethod.Post : HttpMethod.Put, id is null ? "api/employees" : $"api/employees/{id}", data)); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public async Task DeactivateEmployee(int id) { using var r = await http.SendAsync(Request(HttpMethod.Delete, $"api/employees/{id}")); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public Task<List<ShopOrder>> Orders() => Send<List<ShopOrder>>(Request(HttpMethod.Get, "api/orders"));
    public Task<List<OrderNotification>> OrderNotifications() => Send<List<OrderNotification>>(Request(HttpMethod.Get, "api/orders/notifications"));
    public Task<ShopOrder> CreateOrder(CreateOrderRequest data) => Send<ShopOrder>(Json(HttpMethod.Post, "api/orders", data));
    public async Task ChangeOrder(int id, ChangeOrderStatus data) { using var r = await http.SendAsync(Json(HttpMethod.Patch, $"api/orders/{id}/status", data)); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public Task<List<Repair>> Repairs() => Send<List<Repair>>(Request(HttpMethod.Get, "api/repairs"));
    public Task<List<RepairMaster>> RepairMasters() => Send<List<RepairMaster>>(Request(HttpMethod.Get, "api/repairs/masters"));
    public Task<Repair> CreateRepair(CreateRepairRequest data) => Send<Repair>(Json(HttpMethod.Post, "api/repairs", data));
    public async Task AssignRepair(int id, AssignRepairRequest data) { using var r = await http.SendAsync(Json(HttpMethod.Patch, $"api/repairs/{id}/assign", data)); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public async Task ChangeRepair(int id, string status) { using var r = await http.SendAsync(Json(HttpMethod.Patch, $"api/repairs/{id}/status", new { Status = status })); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public async Task<int> SaveProduct(ProductSave data, int? id = null)
    {
        using var response = await http.SendAsync(Json(id is null ? HttpMethod.Post : HttpMethod.Put,
            id is null ? "api/products" : $"api/products/{id}", data));
        if (!response.IsSuccessStatusCode) throw new Exception(await Error(response));
        if (id.HasValue) return id.Value;
        var created = await response.Content.ReadFromJsonAsync<Product>();
        return created?.Id ?? throw new Exception("Сервер не вернул идентификатор созданного товара.");
    }
    public string ProductImageUrl(string? path) => string.IsNullOrWhiteSpace(path)
        ? "images/placeholder.jpg"
        : new Uri(http.BaseAddress!, path.TrimStart('/')).ToString();
    public async Task UploadProductImage(int id, byte[] bytes, string fileName, string contentType)
    {
        using var request = Request(HttpMethod.Post, $"api/products/{id}/image");
        using var form = new MultipartFormDataContent();
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(content, "file", fileName);
        request.Content = form;
        using var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new Exception(await Error(response));
    }
    public async Task DeleteProduct(int id) { using var r = await http.SendAsync(Request(HttpMethod.Delete, $"api/products/{id}")); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }

    public Task<List<SystemSettingView>> AdminSettings() => Send<List<SystemSettingView>>(Request(HttpMethod.Get, "api/admin/settings"));
    public async Task CreateAdminSetting(string name, string value, string? description) { using var r = await http.SendAsync(Json(HttpMethod.Post, "api/admin/settings", new { Name = name, Value = value, Description = description })); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public async Task UpdateAdminSetting(int id, string value) { using var r = await http.SendAsync(Json(HttpMethod.Put, $"api/admin/settings/{id}", new { Value = value })); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public Task<List<AdminLog>> AdminLogs() => Send<List<AdminLog>>(Request(HttpMethod.Get, "api/admin/logs?take=300"));
    public async Task<byte[]> AdminBackup() { using var r = await http.SendAsync(Request(HttpMethod.Get, "api/admin/backup")); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); return await r.Content.ReadAsByteArrayAsync(); }
    public Task<List<SparePartView>> SpareParts() => Send<List<SparePartView>>(Request(HttpMethod.Get, "api/inventory/parts"));
    public async Task SaveSparePart(SparePartSave data, int? id = null) { using var r = await http.SendAsync(Json(id is null ? HttpMethod.Post : HttpMethod.Put, id is null ? "api/inventory/parts" : $"api/inventory/parts/{id}", data)); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public Task<List<SupplierView>> Suppliers() => Send<List<SupplierView>>(Request(HttpMethod.Get, "api/inventory/suppliers"));
    public async Task SaveSupplier(SupplierSave data, int? id = null) { using var r = await http.SendAsync(Json(id is null ? HttpMethod.Post : HttpMethod.Put, id is null ? "api/inventory/suppliers" : $"api/inventory/suppliers/{id}", data)); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public async Task DeleteSupplier(int id) { using var r = await http.SendAsync(Request(HttpMethod.Delete, $"api/inventory/suppliers/{id}")); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public Task<List<SupplierPurchaseView>> SupplierPurchases() => Send<List<SupplierPurchaseView>>(Request(HttpMethod.Get, "api/inventory/purchases"));
    public async Task CreateSupplierPurchase(PurchaseSave data) { using var r = await http.SendAsync(Json(HttpMethod.Post, "api/inventory/purchases", data)); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public async Task ReceiveSupplierPart(int orderId, int partId, int quantity) { using var r = await http.SendAsync(Json(HttpMethod.Post, $"api/inventory/purchases/{orderId}/receive/{partId}", quantity)); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    public Task<List<SparePartView>> MasterParts() => Send<List<SparePartView>>(Request(HttpMethod.Get, "api/repairs/parts"));
    public async Task AddRepairPart(int repairId, int partId, int quantity) { using var r = await http.SendAsync(Json(HttpMethod.Post, $"api/repairs/{repairId}/parts", new { PartId = partId, Quantity = quantity })); if (!r.IsSuccessStatusCode) throw new Exception(await Error(r)); }
    private HttpRequestMessage Json(HttpMethod method, string url, object data, bool bearer = true)
    { var r = new HttpRequestMessage(method, url) { Content = JsonContent.Create(data) }; if (bearer && !string.IsNullOrWhiteSpace(auth.Current?.Token)) r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Current.Token); return r; }
    private static string Query(string? s, int? c) { var p = new List<string>(); if (!string.IsNullOrWhiteSpace(s)) p.Add("search=" + Uri.EscapeDataString(s)); if (c.HasValue) p.Add("categoryId=" + c); return p.Count == 0 ? "" : "?" + string.Join("&", p); }
}
