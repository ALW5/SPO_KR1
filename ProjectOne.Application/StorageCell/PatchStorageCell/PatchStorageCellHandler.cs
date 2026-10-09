using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.Enum;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.StorageCell.PatchStorageCell;

public record PatchStorageCellRequest(
    string numberStorageCalls,
    decimal price,
    int? floor);

public class PatchStorageCellHandler
{
    private readonly IStorageCellRepository _storageCellRepository;
    
    public PatchStorageCellHandler(IStorageCellRepository storageCellRepository)
    {
        _storageCellRepository = storageCellRepository;
    }

    public async Task<Result<Guid, Error>> PatchAsync(
        Guid id, PatchStorageCellRequest request, CancellationToken cancellationToken = default)
    {
        var storageId = StorageId.Create(id);
        
        var storageCell = await _storageCellRepository.GetStorageCellByIdAsync(storageId);
        
        if (storageCell.IsFailure)
            return storageCell.Error;
        
        storageCell.Value.Patch(request.numberStorageCalls, request.price, request.floor);

        await _storageCellRepository.UpdateStorageCellAsync(storageCell.Value, cancellationToken);

        return id;
    }
}