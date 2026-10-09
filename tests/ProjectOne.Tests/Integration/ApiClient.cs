using System.Net.Http.Json;
using System.Text.Json;

namespace ProjectOne.Tests.Integration;

/// <summary>Тонкая обёртка над HttpClient: один экземпляр = одна "сессия" (свои куки).</summary>
public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    public string? Email { get; set; }

    // ---------- auth ----------

    public Task<HttpResponseMessage> RegisterAsync(string email, string password, string fullName)
        => _http.PostAsJsonAsync("/Auth/register", new { email, password, fullName });

    public Task<HttpResponseMessage> LoginAsync(string email, string password)
        => _http.PostAsJsonAsync("/Auth/login", new { email, password });

    public Task<HttpResponseMessage> LogoutAsync()
        => _http.PostAsync("/Auth/logout", content: null);

    public Task<HttpResponseMessage> MeAsync()
        => _http.GetAsync("/Auth/me");

    // ---------- warehouses / cells ----------

    public Task<HttpResponseMessage> CreateWarehouseRawAsync(string name = "Склад тест", int floor = 1)
        => _http.PostAsJsonAsync("/WareHouse", new
        {
            name,
            description = "integration test",
            address = "ул. Тестовая, 1",
            floor
        });

    public async Task<Guid> CreateWarehouseAsync()
    {
        var response = await CreateWarehouseRawAsync($"Склад {Guid.NewGuid():N}");
        response.EnsureSuccessStatusCode();
        return await ReadResultGuidAsync(response);
    }

    public Task<HttpResponseMessage> CreateCellRawAsync(Guid warehouseId, decimal price, string? number = null)
        => _http.PostAsJsonAsync("/StorageCell", new
        {
            wareHouseId = warehouseId,
            numberStorageCalls = number ?? $"C-{Guid.NewGuid():N}"[..8],
            sizeOfStorage = new { width = 1, height = 1, depth = 1 },
            price,
            floor = 1,
            size = 0
        });

    public async Task<Guid> CreateCellAsync(Guid warehouseId, decimal price = 100m)
    {
        var response = await CreateCellRawAsync(warehouseId, price);
        response.EnsureSuccessStatusCode();
        return await ReadResultGuidAsync(response);
    }

    /// <summary>Создаёт склад и свободную ячейку (под админом) и возвращает id ячейки.</summary>
    public async Task<Guid> CreateFreshCellAsync(decimal price = 100m)
        => await CreateCellAsync(await CreateWarehouseAsync(), price);

    public Task<HttpResponseMessage> GetCellsAsync() => _http.GetAsync("/StorageCell");

    public Task<HttpResponseMessage> GetCellAsync(Guid id) => _http.GetAsync($"/StorageCell/{id}");

    public Task<HttpResponseMessage> GetWarehousesAsync() => _http.GetAsync("/WareHouse");

    public Task<HttpResponseMessage> DeleteCellAsync(Guid id) => _http.DeleteAsync($"/StorageCell/{id}");

    public Task<HttpResponseMessage> DeleteWarehouseAsync(Guid id) => _http.DeleteAsync($"/WareHouse/{id}");

    // ---------- reservation / rentals ----------

    public Task<HttpResponseMessage> ReserveAsync(Guid cellId, DateTime start, DateTime end, decimal totalPrice = 0m)
        => _http.PostAsJsonAsync($"/StorageCell/{cellId}/reserve", new
        {
            startDate = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc),
            endDate = DateTime.SpecifyKind(end.Date, DateTimeKind.Utc),
            totalPrice
        });

    public Task<HttpResponseMessage> GetRentalsAsync() => _http.GetAsync("/api/Rentalagreement");

    public Task<HttpResponseMessage> GetRentalAsync(Guid id) => _http.GetAsync($"/api/Rentalagreement/{id}");

    public Task<HttpResponseMessage> DeleteRentalAsync(Guid id) => _http.DeleteAsync($"/api/Rentalagreement/{id}");

    public Task<HttpResponseMessage> UpdateRentalAsync(Guid id, DateTime start, DateTime end, decimal totalPrice)
        => _http.PutAsJsonAsync($"/api/Rentalagreement/{id}", new
        {
            startDate = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc),
            endDate = DateTime.SpecifyKind(end.Date, DateTimeKind.Utc),
            totalPrice
        });

    public Task<HttpResponseMessage> CreateRentalRawAsync(Guid cellId, DateTime start, DateTime end, decimal totalPrice = 0m)
        => _http.PostAsJsonAsync("/api/Rentalagreement", new
        {
            storageId = cellId,
            startDate = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc),
            endDate = DateTime.SpecifyKind(end.Date, DateTimeKind.Utc),
            totalPrice
        });

    // ---------- JSON helpers ----------

    /// <summary>Читает { "result": "<guid>" } из Envelope.</summary>
    public static async Task<Guid> ReadResultGuidAsync(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("result").GetGuid();
    }

    /// <summary>Читает JSON-ответ целиком (корень документа).</summary>
    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.Clone();
    }

    /// <summary>
    /// Для одиночных объектов: если ответ в обёртке Envelope - берёт поле "result",
    /// иначе (например /Auth/me) возвращает объект как есть.
    /// </summary>
    public static async Task<JsonElement> ReadResultAsync(HttpResponseMessage response)
    {
        var root = await ReadJsonAsync(response);

        return root.ValueKind == JsonValueKind.Object
               && root.TryGetProperty("result", out var result)
               && root.TryGetProperty("timestamp", out _)
            ? result
            : root;
    }

    /// <summary>Статус ячейки: 0 = Free, 1 = Booked (enum сериализуется числом).</summary>
    public async Task<int> GetCellStatusAsync(Guid cellId)
    {
        var response = await GetCellAsync(cellId);
        response.EnsureSuccessStatusCode();
        return (await ReadResultAsync(response)).GetProperty("status").GetInt32();
    }

    /// <summary>Id всех договоров, которые видит текущий пользователь.</summary>
    public async Task<List<Guid>> GetRentalIdsAsync()
    {
        var response = await GetRentalsAsync();
        response.EnsureSuccessStatusCode();
        var root = await ReadJsonAsync(response);
        return root.EnumerateArray().Select(e => e.GetProperty("id").GetGuid()).ToList();
    }

    public static DateTime Today => DateTime.UtcNow.Date;
}
