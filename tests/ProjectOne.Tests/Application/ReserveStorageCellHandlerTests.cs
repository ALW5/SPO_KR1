using CSharpFunctionalExtensions;
using ProjectOne.Application.Rentalagreement;
using ProjectOne.Application.StorageCell;
using ProjectOne.Application.StorageCell.ReserveStorageCell;
using ProjectOne.Domain.Classes;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;
using Size = ProjectOne.Domain.Enum.Size;

namespace ProjectOne.Tests.Application;

public class ReserveStorageCellHandlerTests
{
    private readonly FakeCellRepository _cells = new();
    private readonly FakeRentalRepository _rentals = new();
    private readonly ReserveStorageCellHandler _handler;

    public ReserveStorageCellHandlerTests()
        => _handler = new ReserveStorageCellHandler(_cells, _rentals);

    private StorageCell AddCell(decimal pricePerDay)
    {
        var cell = StorageCell.Create(
            StorageId.Create(Guid.NewGuid()),
            WareHouseId.Create(Guid.NewGuid()),
            "A-1",
            SizeOfStorage.Create(1, 1, 1).Value,
            pricePerDay, 1, Size.Small).Value;

        _cells.Items[cell.Id.Value] = cell;
        return cell;
    }

    private static DateTime Day(int offsetFromToday)
        => DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(offsetFromToday), DateTimeKind.Utc);

    [Fact]
    public async Task Total_price_is_calculated_on_server_and_client_value_is_ignored()
    {
        var cell = AddCell(pricePerDay: 100m);

        var result = await _handler.ReserveAsync(
            cell.Id.Value, Guid.NewGuid(),
            new ReserveStorageCellRequest(Day(1), Day(4), TotalPrice: 1m)); // клиент "врёт": 1 вместо 300

        Assert.True(result.IsSuccess);
        var created = Assert.Single(_rentals.Created);
        Assert.Equal(300m, created.TotalPrice);
    }

    [Fact]
    public async Task Rental_is_created_for_the_given_client()
    {
        var cell = AddCell(50m);
        var clientId = Guid.NewGuid();

        await _handler.ReserveAsync(
            cell.Id.Value, clientId, new ReserveStorageCellRequest(Day(0), Day(1), 0));

        Assert.Equal(clientId, Assert.Single(_rentals.Created).ClientId);
    }

    [Fact]
    public async Task End_date_not_after_start_date_is_validation_error()
    {
        var cell = AddCell(100m);

        var result = await _handler.ReserveAsync(
            cell.Id.Value, Guid.NewGuid(), new ReserveStorageCellRequest(Day(2), Day(2), 0));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.ErrorType);
        Assert.Empty(_rentals.Created);
    }

    [Fact]
    public async Task Start_date_in_the_past_is_validation_error()
    {
        var cell = AddCell(100m);

        var result = await _handler.ReserveAsync(
            cell.Id.Value, Guid.NewGuid(), new ReserveStorageCellRequest(Day(-3), Day(1), 0));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.ErrorType);
    }

    [Fact]
    public async Task Booked_cell_gives_conflict()
    {
        var cell = AddCell(100m);
        cell.Reserve();

        var result = await _handler.ReserveAsync(
            cell.Id.Value, Guid.NewGuid(), new ReserveStorageCellRequest(Day(1), Day(2), 0));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.ErrorType);
    }

    [Fact]
    public async Task Losing_the_race_in_repository_gives_conflict()
    {
        var cell = AddCell(100m);
        _rentals.TryCreateResult = false; // кто-то успел занять ячейку между проверкой и сохранением

        var result = await _handler.ReserveAsync(
            cell.Id.Value, Guid.NewGuid(), new ReserveStorageCellRequest(Day(1), Day(2), 0));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.ErrorType);
    }

    [Fact]
    public async Task Unknown_cell_returns_error()
    {
        var result = await _handler.ReserveAsync(
            Guid.NewGuid(), Guid.NewGuid(), new ReserveStorageCellRequest(Day(1), Day(2), 0));

        Assert.True(result.IsFailure);
    }

    // ---------------- fakes ----------------

    private sealed class FakeCellRepository : IStorageCellRepository
    {
        public Dictionary<Guid, StorageCell> Items { get; } = new();

        public Task AddStorageCellAsync(StorageCell storageCell, CancellationToken cancellationToken = default)
        {
            Items[storageCell.Id.Value] = storageCell;
            return Task.CompletedTask;
        }

        public Task<Result<StorageCell, Error>> GetStorageCellByIdAsync(StorageId storageId)
            => Task.FromResult(
                Items.TryGetValue(storageId.Value, out var cell)
                    ? Result.Success<StorageCell, Error>(cell)
                    : Result.Failure<StorageCell, Error>(Errors.General.NotFound(storageId.Value)));

        public Task UpdateStorageCellAsync(StorageCell storageCell, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteStorageCellAsync(StorageCell storageCell, CancellationToken cancellationToken = default)
        {
            Items.Remove(storageCell.Id.Value);
            return Task.CompletedTask;
        }

        public Task<List<StorageCell>> GetAllStorageCellsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Values.ToList());
    }

    private sealed class FakeRentalRepository : IRentalagreementRepository
    {
        public List<Rentalagreement> Created { get; } = new();
        public bool TryCreateResult { get; set; } = true;

        public Task<bool> TryCreateWithReservationAsync(
            Rentalagreement rentalagreement, CancellationToken cancellationToken = default)
        {
            if (TryCreateResult)
                Created.Add(rentalagreement);

            return Task.FromResult(TryCreateResult);
        }

        public Task<bool> ExistsForStorageAsync(StorageId storageId, CancellationToken cancellationToken = default)
            => Task.FromResult(Created.Any(r => r.StorageId == storageId));

        public Task AddRentalagreementAsync(Rentalagreement rentalagreement, CancellationToken cancellationToken = default)
        {
            Created.Add(rentalagreement);
            return Task.CompletedTask;
        }

        public Task<Result<Rentalagreement, Error>> GetRentalagreementByIdAsync(RentalagreementId id)
            => Task.FromResult(
                Created.FirstOrDefault(x => x.Id == id) is { } found
                    ? Result.Success<Rentalagreement, Error>(found)
                    : Result.Failure<Rentalagreement, Error>(Errors.General.NotFound()));

        public Task DeleteRentalagreementAsync(Rentalagreement rentalagreement, CancellationToken cancellationToken = default)
        {
            Created.Remove(rentalagreement);
            return Task.CompletedTask;
        }

        public Task<List<Rentalagreement>> GetAllAsync(CancellationToken cancellationToken)
            => Task.FromResult(Created.ToList());

        public Task UpdateRentalagreementAsync(Rentalagreement rentalagreement, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
