using CSharpFunctionalExtensions;
using ProjectOne.Application.StorageCell.ReserveStorageCell;
using ProjectOne.Domain.Common;

namespace ProjectOne.Application.Rentalagreement.CreateRentalagreement;

public class CreateRentalagreementHandler
{
    private readonly ReserveStorageCellHandler _reserveHandler;

    public CreateRentalagreementHandler(ReserveStorageCellHandler reserveHandler)
    {
        _reserveHandler = reserveHandler;
    }

    /// <summary>
    /// Создание договора = бронирование ячейки (одна и та же логика, что и в /reserve).
    /// currentUserId берётся из токена в контроллере, а не из запроса.
    /// </summary>
    public Task<Result<Guid, Error>> CreateAsync(
        CreateRentalagreementRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
        => _reserveHandler.ReserveAsync(
            request.storageId,
            currentUserId,
            new ReserveStorageCellRequest(request.startDate, request.endDate, request.totalPrice),
            cancellationToken);
}
