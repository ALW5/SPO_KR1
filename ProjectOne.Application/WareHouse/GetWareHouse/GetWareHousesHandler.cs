namespace ProjectOne.Application.WareHouse.GetWareHouse;

public class GetWareHousesHandler
{
    private readonly IWareHouseRepository _wareHouseRepository;

    public GetWareHousesHandler(IWareHouseRepository wareHouseRepository)
        => _wareHouseRepository = wareHouseRepository;

    public async Task<List<WareHouseResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        var wareHouses = await _wareHouseRepository.GetAllWareHousesAsync(cancellationToken);

        return wareHouses
            .Select(w => new WareHouseResponse(w.Id.Value, w.Name, w.Description, w.Address, w.Floor))
            .ToList();
    }
}