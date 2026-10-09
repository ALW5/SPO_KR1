using System.Net;

namespace ProjectOne.Tests.Integration;

[Collection(ApiCollection.Name)]
public class RentalAccessAndDeletionTests
{
    private readonly ApiFixture _api;

    public RentalAccessAndDeletionTests(ApiFixture api) => _api = api;

    private static DateTime Day(int offset) => ApiClient.Today.AddDays(offset);

    /// <summary>Клиент бронирует новую ячейку; возвращает (id ячейки, id договора).</summary>
    private async Task<(Guid CellId, Guid RentalId)> BookAsync(ApiClient admin, ApiClient owner)
    {
        var cellId = await admin.CreateFreshCellAsync();
        Assert.Equal(HttpStatusCode.NoContent, (await owner.ReserveAsync(cellId, Day(1), Day(3))).StatusCode);
        var rentalId = Assert.Single(await owner.GetRentalIdsAsync());
        return (cellId, rentalId);
    }

    // ------------------------------------------------------------------
    // Чужие аренды (IDOR)
    // ------------------------------------------------------------------

    [Fact]
    public async Task Client_sees_only_own_rentals()
    {
        var admin = await _api.CreateAdminAsync();
        var alice = await _api.CreateRegisteredClientAsync("alice");
        var bob = await _api.CreateRegisteredClientAsync("bob");

        var (_, aliceRental) = await BookAsync(admin, alice);
        var (_, bobRental) = await BookAsync(admin, bob);

        var aliceSees = await alice.GetRentalIdsAsync();
        var bobSees = await bob.GetRentalIdsAsync();

        Assert.Contains(aliceRental, aliceSees);
        Assert.DoesNotContain(bobRental, aliceSees);
        Assert.Contains(bobRental, bobSees);
        Assert.DoesNotContain(aliceRental, bobSees);
    }

    [Fact]
    public async Task Admin_sees_rentals_of_all_clients()
    {
        var admin = await _api.CreateAdminAsync();
        var alice = await _api.CreateRegisteredClientAsync("a2");
        var bob = await _api.CreateRegisteredClientAsync("b2");

        var (_, aliceRental) = await BookAsync(admin, alice);
        var (_, bobRental) = await BookAsync(admin, bob);

        var all = await admin.GetRentalIdsAsync();

        Assert.Contains(aliceRental, all);
        Assert.Contains(bobRental, all);
    }

    [Fact]
    public async Task Client_cannot_read_someone_elses_rental()
    {
        var admin = await _api.CreateAdminAsync();
        var alice = await _api.CreateRegisteredClientAsync("owner");
        var bob = await _api.CreateRegisteredClientAsync("intruder");
        var (_, aliceRental) = await BookAsync(admin, alice);

        var byIntruder = await bob.GetRentalAsync(aliceRental);
        var byOwner = await alice.GetRentalAsync(aliceRental);
        var byAdmin = await admin.GetRentalAsync(aliceRental);

        Assert.Equal(HttpStatusCode.Forbidden, byIntruder.StatusCode);
        Assert.Equal(HttpStatusCode.OK, byOwner.StatusCode);
        Assert.Equal(HttpStatusCode.OK, byAdmin.StatusCode);
    }

    [Fact]
    public async Task Client_cannot_cancel_someone_elses_rental_and_cell_stays_booked()
    {
        var admin = await _api.CreateAdminAsync();
        var alice = await _api.CreateRegisteredClientAsync("victim");
        var bob = await _api.CreateRegisteredClientAsync("attacker");
        var (cellId, aliceRental) = await BookAsync(admin, alice);

        var response = await bob.DeleteRentalAsync(aliceRental);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(1, await admin.GetCellStatusAsync(cellId));
        Assert.Contains(aliceRental, await alice.GetRentalIdsAsync());
    }

    [Fact]
    public async Task Admin_can_cancel_any_rental()
    {
        var admin = await _api.CreateAdminAsync();
        var alice = await _api.CreateRegisteredClientAsync("byadmin");
        var (cellId, aliceRental) = await BookAsync(admin, alice);

        var response = await admin.DeleteRentalAsync(aliceRental);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await admin.GetCellStatusAsync(cellId));
    }

    [Fact]
    public async Task Only_admin_can_edit_rental_terms()
    {
        var admin = await _api.CreateAdminAsync();
        var alice = await _api.CreateRegisteredClientAsync("edit");
        var (_, rentalId) = await BookAsync(admin, alice);

        // клиент не может сам поставить себе цену
        var byClient = await alice.UpdateRentalAsync(rentalId, Day(1), Day(5), totalPrice: 1m);
        var byAdmin = await admin.UpdateRentalAsync(rentalId, Day(1), Day(5), totalPrice: 999m);

        Assert.Equal(HttpStatusCode.Forbidden, byClient.StatusCode);
        Assert.Equal(HttpStatusCode.OK, byAdmin.StatusCode);

        var rental = await ApiClient.ReadResultAsync(await alice.GetRentalAsync(rentalId));
        Assert.Equal(999m, rental.GetProperty("totalPrice").GetDecimal());
    }

    [Fact]
    public async Task Rental_by_id_returns_cell_data_without_server_error()
    {
        // раньше GET /api/Rentalagreement/{id} падал из-за незагруженной ячейки (StorageCell == null)
        var admin = await _api.CreateAdminAsync();
        var alice = await _api.CreateRegisteredClientAsync("byid");
        var (cellId, rentalId) = await BookAsync(admin, alice);

        var response = await alice.GetRentalAsync(rentalId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rental = await ApiClient.ReadResultAsync(response);
        Assert.Equal(cellId, rental.GetProperty("storageId").GetGuid());
        Assert.False(string.IsNullOrEmpty(rental.GetProperty("numberStorageCalls").GetString()));
    }

    // ------------------------------------------------------------------
    // Удаление ячеек
    // ------------------------------------------------------------------

    [Fact]
    public async Task Deleting_cell_with_rental_returns_409_and_keeps_data()
    {
        var admin = await _api.CreateAdminAsync();
        var alice = await _api.CreateRegisteredClientAsync("delcell");
        var (cellId, rentalId) = await BookAsync(admin, alice);

        var response = await admin.DeleteCellAsync(cellId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode); // а не 500
        Assert.Equal(1, await admin.GetCellStatusAsync(cellId));
        Assert.Contains(rentalId, await alice.GetRentalIdsAsync());
    }

    [Fact]
    public async Task Deleting_free_cell_succeeds_and_cell_disappears()
    {
        var admin = await _api.CreateAdminAsync();
        var cellId = await admin.CreateFreshCellAsync();

        var response = await admin.DeleteCellAsync(cellId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.False((await admin.GetCellAsync(cellId)).IsSuccessStatusCode);
    }

    [Fact]
    public async Task Cell_can_be_deleted_after_its_rental_is_cancelled()
    {
        var admin = await _api.CreateAdminAsync();
        var alice = await _api.CreateRegisteredClientAsync("afterdel");
        var (cellId, rentalId) = await BookAsync(admin, alice);

        Assert.Equal(HttpStatusCode.NoContent, (await alice.DeleteRentalAsync(rentalId)).StatusCode);

        // договор удалён -> FK больше не мешает
        Assert.True((await admin.DeleteCellAsync(cellId)).IsSuccessStatusCode);
    }
}
