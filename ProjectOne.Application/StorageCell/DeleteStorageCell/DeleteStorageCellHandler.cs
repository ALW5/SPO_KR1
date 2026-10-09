using CSharpFunctionalExtensions;
using ProjectOne.Application.Rentalagreement;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.StorageCell.DeleteStorageCell;

public class DeleteStorageCellHandler
{
    private readonly IStorageCellRepository _repo;
    private readonly IRentalagreementRepository _rentalRepo;

    public DeleteStorageCellHandler(IStorageCellRepository repo, IRentalagreementRepository rentalRepo)
    {
        _repo = repo;
        _rentalRepo = rentalRepo;
    }

    public async Task<UnitResult<Error>> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var storageId = StorageId.Create(id);

        var storageCell = await _repo.GetStorageCellByIdAsync(storageId);

        if (storageCell.IsFailure)
            return storageCell.Error;

        // FK Rentalagreement -> StorageCell стоит как Restrict: вместо 500 отдаём 409.
        if (await _rentalRepo.ExistsForStorageAsync(storageId, cancellationToken))
            return Error.Conflict("cell.has.rentals", "Cell has rental agreements and cannot be deleted");

        await _repo.DeleteStorageCellAsync(storageCell.Value, cancellationToken);

        return UnitResult.Success<Error>();
    }
}
