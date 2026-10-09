using CSharpFunctionalExtensions;
using ProjectOne.Application.Rentalagreement;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.Enum;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.StorageCell.ReserveStorageCell;

public class ReserveStorageCellHandler
{
    private readonly IStorageCellRepository _storageCellRepository;
    private readonly IRentalagreementRepository _rentalagreementRepository;

    public ReserveStorageCellHandler(
        IStorageCellRepository storageCellRepository,
        IRentalagreementRepository rentalagreementRepository)
    {
        _storageCellRepository = storageCellRepository;
        _rentalagreementRepository = rentalagreementRepository;
    }

    /// <summary>
    /// Бронирует ячейку и создаёт договор. Возвращает Id договора.
    /// Стоимость считается на сервере (цена ячейки в день * число дней),
    /// TotalPrice из запроса игнорируется.
    /// </summary>
    public async Task<Result<Guid, Error>> ReserveAsync(
        Guid storageId,
        Guid clientId,
        ReserveStorageCellRequest request,
        CancellationToken ct = default)
    {
        var storageIdValue = StorageId.Create(storageId);

        var cellResult = await _storageCellRepository
            .GetStorageCellByIdAsync(storageIdValue);

        if (cellResult.IsFailure)
            return cellResult.Error;

        var cell = cellResult.Value;

        // Работаем с датами (без времени) в UTC
        var startDate = DateTime.SpecifyKind(request.StartDate.Date, DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(request.EndDate.Date, DateTimeKind.Utc);

        if (startDate < DateTime.UtcNow.Date)
            return Error.Validation("date.in.past", "Start date must not be in the past");

        var days = (endDate - startDate).Days;
        if (days < 1)
            return Error.Validation("date.range.invalid", "End date must be after start date");

        if (cell.Status != Status.Free)
            return Error.Conflict("cell.already.booked", "Cell is already booked");

        var totalPrice = cell.Price * days;

        var rentalResult = Domain.Classes.Rentalagreement.Create(
            RentalagreementId.Create(Guid.NewGuid()),
            storageIdValue,
            clientId,
            startDate,
            endDate,
            totalPrice);

        if (rentalResult.IsFailure)
            return rentalResult.Error;

        // Смена статуса ячейки и сохранение договора - в одной транзакции.
        var created = await _rentalagreementRepository
            .TryCreateWithReservationAsync(rentalResult.Value, ct);

        if (!created)
            return Error.Conflict("cell.already.booked", "Cell is already booked");

        return rentalResult.Value.Id.Value;
    }
}

// TotalPrice оставлен для обратной совместимости с клиентами, сервером игнорируется.
public record ReserveStorageCellRequest(
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalPrice = 0);
