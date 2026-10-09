using CSharpFunctionalExtensions;
using ProjectOne.Application.StorageCell;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.Rentalagreement.DeleteRentalagreement;

public class DeleteRentalagreementHandler
{
    private readonly IRentalagreementRepository _rentalRepo;
    private readonly IStorageCellRepository _cellRepo;

    public DeleteRentalagreementHandler(
        IRentalagreementRepository rentalRepo, IStorageCellRepository cellRepo)
    {
        _rentalRepo = rentalRepo;
        _cellRepo = cellRepo;
    }

    public async Task<UnitResult<Error>> DeleteAsync(
        Guid id, Guid requesterId, bool isAdmin, CancellationToken ct = default)
    {
        var rentalId = RentalagreementId.Create(id);
        var rentalResult = await _rentalRepo.GetRentalagreementByIdAsync(rentalId);
        if (rentalResult.IsFailure) return rentalResult.Error;

        if (!isAdmin && rentalResult.Value.ClientId != requesterId)
            return Error.Forbidden("rental.forbidden", "You do not have access to this rental agreement");

        var cellResult = await _cellRepo.GetStorageCellByIdAsync(rentalResult.Value.StorageId);
        if (cellResult.IsSuccess)
        {
            cellResult.Value.Release();
            await _cellRepo.UpdateStorageCellAsync(cellResult.Value, ct);
        }

        await _rentalRepo.DeleteRentalagreementAsync(rentalResult.Value, ct);
        return UnitResult.Success<Error>();
    }
}