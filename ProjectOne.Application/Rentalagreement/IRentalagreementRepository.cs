using CSharpFunctionalExtensions;
using ProjectOne.Application.StorageCell.GetStorageCell;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.Rentalagreement;

public interface IRentalagreementRepository
{
    public Task AddRentalagreementAsync(
        Domain.Classes.Rentalagreement rentalagreement, 
        CancellationToken cancellationToken = default);

    public Task<Result<Domain.Classes.Rentalagreement, Error>> GetRentalagreementByIdAsync(RentalagreementId id);
    
    public Task DeleteRentalagreementAsync(Domain.Classes.Rentalagreement rentalagreement, CancellationToken cancellationToken = default);
    
    public Task<List<Domain.Classes.Rentalagreement>> GetAllAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Атомарно (в одной транзакции) переводит ячейку Free -> Booked и сохраняет договор.
    /// Возвращает false, если ячейку успел занять кто-то другой.
    /// </summary>
    public Task<bool> TryCreateWithReservationAsync(
        Domain.Classes.Rentalagreement rentalagreement,
        CancellationToken cancellationToken = default);

    public Task<bool> ExistsForStorageAsync(StorageId storageId, CancellationToken cancellationToken = default);

    public Task UpdateRentalagreementAsync(Domain.Classes.Rentalagreement rentalagreement, CancellationToken cancellationToken = default);
}