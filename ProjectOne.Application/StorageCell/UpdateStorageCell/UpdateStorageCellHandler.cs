using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.Enum;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.StorageCell.UpdateStorageCell;

public record UpdateStorageCellRequest(
    string numberStorageCalls,
    SizeOfStorage sizeOfStorage,
    decimal price,
    int? floor,
    Size size);

public class UpdateStorageCellHandler
{
    private readonly IStorageCellRepository _storageCellRepository;
    
    public UpdateStorageCellHandler(IStorageCellRepository storageCellRepository)
    {
        _storageCellRepository = storageCellRepository;
    }

    public async Task<Result<Guid, Error>> UpdateAsync(
        Guid Id, UpdateStorageCellRequest request, CancellationToken cancellationToken = default)
    {
        var storageId = StorageId.Create(Id);

        var storageCell = await _storageCellRepository.GetStorageCellByIdAsync(storageId);
        
        if (storageCell.IsFailure)
            return storageCell.Error;
        
        var updateStorageCell = storageCell.Value.Update(
            request.numberStorageCalls,
            request.sizeOfStorage,
            request.price,
            request.floor,
            request.size);

        if (storageCell.IsFailure)
            return updateStorageCell.Error;

        await _storageCellRepository.UpdateStorageCellAsync(storageCell.Value, cancellationToken);
        
        return storageId.Value;
    }
}