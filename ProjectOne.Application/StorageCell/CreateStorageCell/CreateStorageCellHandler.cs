using CSharpFunctionalExtensions;
using ProjectOne.Application.WareHouse;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.StorageCell.CreateStorageCell;

public class CreateStorageCellHandler
{
    private readonly IStorageCellRepository _storageCellRepository;
    private readonly IWareHouseRepository _wareHouseRepository;

    public CreateStorageCellHandler(
        IStorageCellRepository storageCellRepository, 
        IWareHouseRepository wareHouseRepository)
    {
        _storageCellRepository = storageCellRepository;
        _wareHouseRepository = wareHouseRepository;
    }

    public async Task<Result<Guid, Error>> CreateAsync(CreateStorageCellRequest createStorageCellRequest 
        ,CancellationToken cancellationToken = default)
    {
        var storageId = StorageId.NewStorageId();
        
        var wareHouseId = WareHouseId.Create(createStorageCellRequest.wareHouseId);
        
        var wareHouse = await _wareHouseRepository.GetWareHouseByIdAsync(wareHouseId);
        
        if (wareHouse.IsFailure)
            return wareHouse.Error;
        
        var sizeOfStorage = SizeOfStorage.Create(
            createStorageCellRequest.sizeOfStorage.width,
            createStorageCellRequest.sizeOfStorage.height,
            createStorageCellRequest.sizeOfStorage.depth);

        if (sizeOfStorage.IsFailure)
            return sizeOfStorage.Error;

        var storageCell = Domain.Classes.StorageCell.Create(
            storageId,
            wareHouseId,
            createStorageCellRequest.numberStorageCalls,
            sizeOfStorage.Value,
            createStorageCellRequest.price,
            createStorageCellRequest.floor,
            createStorageCellRequest.size);
        
        if (storageCell.IsFailure)
            return storageCell.Error;
        
        await _storageCellRepository.AddStorageCellAsync(storageCell.Value, cancellationToken);
        
        return storageId.Value;
    }
}