using System.Net;

namespace ProjectOne.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AuthAndAccessTests
{
    private readonly ApiFixture _api;

    public AuthAndAccessTests(ApiFixture api) => _api = api;

    // ------------------------------------------------------------------
    // Аутентификация (кука access_token + /Auth/me)
    // ------------------------------------------------------------------

    [Fact]
    public async Task Register_sets_session_and_me_returns_client_role()
    {
        var client = await _api.CreateRegisteredClientAsync("reg");

        var me = await client.MeAsync();

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var body = await ApiClient.ReadResultAsync(me);
        Assert.Equal(client.Email, body.GetProperty("email").GetString());
        Assert.Equal("Client", body.GetProperty("role").GetString());
        Assert.Equal("Test User", body.GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task Login_as_seeded_admin_returns_administrator_role()
    {
        var admin = await _api.CreateAdminAsync();

        var me = await admin.MeAsync();

        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var body = await ApiClient.ReadResultAsync(me);
        Assert.Equal("Administrator", body.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Me_without_session_returns_401()
    {
        var guest = _api.CreateClient();

        var me = await guest.MeAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task Login_with_wrong_password_is_rejected_without_server_error()
    {
        var client = await _api.CreateRegisteredClientAsync("wrongpass");
        var other = _api.CreateClient();

        var response = await other.LoginAsync(client.Email!, "definitely-wrong");

        Assert.False(response.IsSuccessStatusCode);
        Assert.True((int)response.StatusCode < 500);
        Assert.Equal(HttpStatusCode.Unauthorized, (await other.MeAsync()).StatusCode);
    }

    [Fact]
    public async Task Registering_the_same_email_twice_fails()
    {
        var first = await _api.CreateRegisteredClientAsync("dup");

        var second = await _api.CreateClient().RegisterAsync(first.Email!, "Passw0rd!", "Someone Else");

        Assert.False(second.IsSuccessStatusCode);
        Assert.True((int)second.StatusCode < 500);
    }

    [Fact]
    public async Task Logout_ends_the_session()
    {
        var client = await _api.CreateRegisteredClientAsync("logout");
        Assert.Equal(HttpStatusCode.OK, (await client.MeAsync()).StatusCode);

        var logout = await client.LogoutAsync();

        Assert.True(logout.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.MeAsync()).StatusCode);
    }

    // ------------------------------------------------------------------
    // Права доступа
    // ------------------------------------------------------------------

    [Fact]
    public async Task Guest_can_read_cells_and_warehouses()
    {
        var guest = _api.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await guest.GetCellsAsync()).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await guest.GetWarehousesAsync()).StatusCode);
    }

    [Fact]
    public async Task Guest_cannot_see_rentals_reserve_or_create()
    {
        var admin = await _api.CreateAdminAsync();
        var cellId = await admin.CreateFreshCellAsync();
        var guest = _api.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await guest.GetRentalsAsync()).StatusCode);

        var reserve = await guest.ReserveAsync(cellId, ApiClient.Today.AddDays(1), ApiClient.Today.AddDays(2));
        Assert.Equal(HttpStatusCode.Unauthorized, reserve.StatusCode);

        var create = await guest.CreateWarehouseRawAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, create.StatusCode);
    }

    [Fact]
    public async Task Client_cannot_create_or_delete_warehouses_and_cells()
    {
        var admin = await _api.CreateAdminAsync();
        var warehouseId = await admin.CreateWarehouseAsync();
        var cellId = await admin.CreateCellAsync(warehouseId);
        var client = await _api.CreateRegisteredClientAsync("perm");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.CreateWarehouseRawAsync()).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.CreateCellRawAsync(warehouseId, 10m)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteCellAsync(cellId)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteWarehouseAsync(warehouseId)).StatusCode);

        // ничего не удалилось
        Assert.Equal(0, await admin.GetCellStatusAsync(cellId));
    }

    // ------------------------------------------------------------------
    // Контракт ответов, на который опирается фронтенд
    // ------------------------------------------------------------------

    [Fact]
    public async Task Cells_list_is_a_plain_array_with_fields_used_by_frontend()
    {
        var admin = await _api.CreateAdminAsync();
        var cellId = await admin.CreateFreshCellAsync(price: 123m);

        var response = await _api.CreateClient().GetCellsAsync();

        response.EnsureSuccessStatusCode();
        var root = await ApiClient.ReadJsonAsync(response);
        Assert.Equal(System.Text.Json.JsonValueKind.Array, root.ValueKind); // без Envelope

        var cell = root.EnumerateArray().Single(c => c.GetProperty("storageId").GetGuid() == cellId);
        Assert.True(cell.TryGetProperty("numberStorageCalls", out _));
        Assert.Equal(123m, cell.GetProperty("price").GetDecimal());
        Assert.True(cell.TryGetProperty("floor", out _));
        Assert.Equal(0, cell.GetProperty("status").GetInt32()); // Free = 0, число, а не строка
    }

    [Fact]
    public async Task Single_cell_is_wrapped_in_envelope_with_result()
    {
        var admin = await _api.CreateAdminAsync();
        var cellId = await admin.CreateFreshCellAsync(price: 77m);

        var response = await _api.CreateClient().GetCellAsync(cellId);

        response.EnsureSuccessStatusCode();
        var root = await ApiClient.ReadJsonAsync(response);
        Assert.True(root.TryGetProperty("result", out var result));
        Assert.True(root.TryGetProperty("timestamp", out _));
        Assert.Equal(77m, result.GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task Unknown_cell_returns_error_envelope_not_server_error()
    {
        var response = await _api.CreateClient().GetCellAsync(Guid.NewGuid());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True((int)response.StatusCode < 500);

        var root = await ApiClient.ReadJsonAsync(response);
        Assert.True(root.TryGetProperty("message", out var message)); // фронт показывает именно это поле
        Assert.False(string.IsNullOrWhiteSpace(message.GetString()));
    }
}
