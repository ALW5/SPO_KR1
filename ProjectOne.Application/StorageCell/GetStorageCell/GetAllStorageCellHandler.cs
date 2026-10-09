namespace ProjectOne.Application.StorageCell.GetStorageCell;

public class GetAllStorageCellHandler
{
    private readonly IStorageCellRepository _storageCellRepository;

    public GetAllStorageCellHandler(IStorageCellRepository storageCellRepository)
    {
        _storageCellRepository = storageCellRepository;
    }

    public async Task<List<GetStorageCellResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var storageCalls = await _storageCellRepository.GetAllStorageCellsAsync(cancellationToken);

        return storageCalls
            .Select(w => new GetStorageCellResponse(w.Id.Value, w.WareHouseId.Value, w.NumberStorageCalls,
                w.SizeOfStorage,
                w.Price, w.Floor, w.Status))
            .ToList();
    }
}