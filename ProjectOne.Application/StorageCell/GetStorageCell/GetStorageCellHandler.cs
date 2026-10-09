using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.Enum;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.StorageCell.GetStorageCell;

public class GetStorageCellHandler
{
    private readonly IStorageCellRepository _storageCellRepository;
    public GetStorageCellHandler(IStorageCellRepository storageCellRepository)
    {
        _storageCellRepository = storageCellRepository;
    }

    public async Task<Result<GetStorageCellResponse, Error>> GetAsync(
        Guid storageCellId, CancellationToken cancellationToken)
    {
        var storageId = StorageId.Create(storageCellId);
        
        var storageCell = await _storageCellRepository.GetStorageCellByIdAsync(storageId);
        
        if(storageCell.IsFailure)
            return storageCell.Error;
        
        var s = storageCell.Value;
        
        return new GetStorageCellResponse(s.Id.Value, s.WareHouseId.Value, s.NumberStorageCalls, s.SizeOfStorage,
            s.Price, s.Floor, s.Status);
    }
}

public record GetStorageCellResponse(Guid storageId,
    Guid WareHouseId,
    string numberStorageCalls,
    SizeOfStorage sizeOfStorage,
    decimal price,
    int? floor,
    Status status);