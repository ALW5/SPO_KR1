using System.Net;

namespace ProjectOne.Tests.Integration;

[Collection(ApiCollection.Name)]
public class ReservationTests
{
    private readonly ApiFixture _api;

    public ReservationTests(ApiFixture api) => _api = api;

    private static DateTime Day(int offset) => ApiClient.Today.AddDays(offset);

    [Fact]
    public async Task Client_reserves_free_cell_it_becomes_booked_and_rental_appears_in_my_list()
    {
        var admin = await _api.CreateAdminAsync();
        var client = await _api.CreateRegisteredClientAsync("res");
        var cellId = await admin.CreateFreshCellAsync(price: 100m);

        var response = await client.ReserveAsync(cellId, Day(1), Day(4));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // фронт ожидает 204
        Assert.Equal(1, await client.GetCellStatusAsync(cellId));

        var rentals = await ApiClient.ReadJsonAsync(await client.GetRentalsAsync());
        var rental = rentals.EnumerateArray().Single(r => r.GetProperty("storageId").GetGuid() == cellId);
        Assert.Equal(300m, rental.GetProperty("totalPrice").GetDecimal()); // 3 дня * 100
        Assert.True(rental.TryGetProperty("numberStorageCalls", out _));
        Assert.True(rental.TryGetProperty("startDate", out _));
        Assert.True(rental.TryGetProperty("endDate", out _));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999999)]
    public async Task Total_price_is_calculated_by_server_and_client_value_is_ignored(int fakePrice)
    {
        var admin = await _api.CreateAdminAsync();
        var client = await _api.CreateRegisteredClientAsync("price");
        var cellId = await admin.CreateFreshCellAsync(price: 250m);

        var response = await client.ReserveAsync(cellId, Day(1), Day(3), totalPrice: (decimal)fakePrice);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var rentals = await ApiClient.ReadJsonAsync(await client.GetRentalsAsync());
        var rental = rentals.EnumerateArray().Single(r => r.GetProperty("storageId").GetGuid() == cellId);
        Assert.Equal(500m, rental.GetProperty("totalPrice").GetDecimal()); // 2 дня * 250
    }

    [Fact]
    public async Task Reserving_already_booked_cell_returns_409_and_does_not_create_second_rental()
    {
        var admin = await _api.CreateAdminAsync();
        var first = await _api.CreateRegisteredClientAsync("first");
        var second = await _api.CreateRegisteredClientAsync("second");
        var cellId = await admin.CreateFreshCellAsync();

        Assert.Equal(HttpStatusCode.NoContent, (await first.ReserveAsync(cellId, Day(1), Day(2))).StatusCode);

        var response = await second.ReserveAsync(cellId, Day(1), Day(2));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await ApiClient.ReadJsonAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("message").GetString()));
        Assert.Empty(await second.GetRentalIdsAsync());
    }

    [Fact]
    public async Task End_date_must_be_after_start_date()
    {
        var admin = await _api.CreateAdminAsync();
        var client = await _api.CreateRegisteredClientAsync("dates");
        var cellId = await admin.CreateFreshCellAsync();

        var same = await client.ReserveAsync(cellId, Day(2), Day(2));
        var reversed = await client.ReserveAsync(cellId, Day(3), Day(1));

        Assert.Equal(HttpStatusCode.BadRequest, same.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, reversed.StatusCode);
        Assert.Equal(0, await client.GetCellStatusAsync(cellId)); // ячейка осталась свободной
    }

    [Fact]
    public async Task Start_date_in_the_past_is_rejected()
    {
        var admin = await _api.CreateAdminAsync();
        var client = await _api.CreateRegisteredClientAsync("past");
        var cellId = await admin.CreateFreshCellAsync();

        var response = await client.ReserveAsync(cellId, Day(-5), Day(1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await client.GetCellStatusAsync(cellId));
    }

    [Fact]
    public async Task Reserving_unknown_cell_is_an_error_but_not_500()
    {
        var client = await _api.CreateRegisteredClientAsync("unknown");

        var response = await client.ReserveAsync(Guid.NewGuid(), Day(1), Day(2));

        Assert.False(response.IsSuccessStatusCode);
        Assert.True((int)response.StatusCode < 500);
    }

    [Fact]
    public async Task Cancelling_rental_frees_the_cell_and_it_can_be_reserved_again()
    {
        var admin = await _api.CreateAdminAsync();
        var first = await _api.CreateRegisteredClientAsync("cancel");
        var second = await _api.CreateRegisteredClientAsync("again");
        var cellId = await admin.CreateFreshCellAsync();

        await first.ReserveAsync(cellId, Day(1), Day(2));
        var rentalId = Assert.Single(await first.GetRentalIdsAsync());

        var delete = await first.DeleteRentalAsync(rentalId);

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(0, await first.GetCellStatusAsync(cellId));
        Assert.Equal(HttpStatusCode.NoContent, (await second.ReserveAsync(cellId, Day(2), Day(3))).StatusCode);
    }

    [Fact]
    public async Task Parallel_reservations_of_the_same_cell_produce_exactly_one_winner()
    {
        const int contenders = 8;
        var admin = await _api.CreateAdminAsync();
        var cellId = await admin.CreateFreshCellAsync();

        var clients = new List<ApiClient>();
        for (var i = 0; i < contenders; i++)
            clients.Add(await _api.CreateRegisteredClientAsync($"race{i}"));

        var responses = await Task.WhenAll(
            clients.Select(c => c.ReserveAsync(cellId, Day(1), Day(2))));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.NoContent));
        Assert.Equal(contenders - 1, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));

        // и ровно один договор в системе на эту ячейку
        var rentals = await ApiClient.ReadJsonAsync(await admin.GetRentalsAsync());
        Assert.Equal(1, rentals.EnumerateArray().Count(r => r.GetProperty("storageId").GetGuid() == cellId));
    }

    // ------------------------------------------------------------------
    // POST /api/Rentalagreement: пользователь берётся из токена и ячейка реально бронируется
    // ------------------------------------------------------------------

    [Fact]
    public async Task Creating_rental_via_api_books_the_cell_for_the_authenticated_user()
    {
        var admin = await _api.CreateAdminAsync();
        var client = await _api.CreateRegisteredClientAsync("direct");
        var cellId = await admin.CreateFreshCellAsync(price: 40m);

        var response = await client.CreateRentalRawAsync(cellId, Day(1), Day(3), totalPrice: 1m);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(1, await client.GetCellStatusAsync(cellId));

        var rentals = await ApiClient.ReadJsonAsync(await client.GetRentalsAsync());
        var rental = rentals.EnumerateArray().Single(r => r.GetProperty("storageId").GetGuid() == cellId);
        Assert.Equal(80m, rental.GetProperty("totalPrice").GetDecimal()); // цена с сервера, а не 1
    }

    [Fact]
    public async Task Creating_rental_via_api_for_booked_cell_returns_409()
    {
        var admin = await _api.CreateAdminAsync();
        var first = await _api.CreateRegisteredClientAsync("api1");
        var second = await _api.CreateRegisteredClientAsync("api2");
        var cellId = await admin.CreateFreshCellAsync();

        Assert.True((await first.CreateRentalRawAsync(cellId, Day(1), Day(2))).IsSuccessStatusCode);

        var response = await second.CreateRentalRawAsync(cellId, Day(1), Day(2));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
