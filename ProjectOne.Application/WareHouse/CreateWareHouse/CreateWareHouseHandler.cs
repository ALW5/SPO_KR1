using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.WareHouse.CreateWareHouse;

public class CreateWareHouseHandler
{
    private readonly IWareHouseRepository _wareHouseRepository;

    public CreateWareHouseHandler(IWareHouseRepository wareHouseRepository)
    {
        _wareHouseRepository = wareHouseRepository;
    }

    public async Task<Result<Guid, Error>> CreateAsync(CreateWareHouseRequest createWareHouseRequest,
        CancellationToken cancellationToken = default)
    {
        var wareHouseId = WareHouseId.NewWareHouseId();

        var wareHouse = Domain.Classes.WareHouse.Create(
            wareHouseId,
            createWareHouseRequest.name,
            createWareHouseRequest.description,
            createWareHouseRequest.address,
            createWareHouseRequest.floor);

        if (wareHouse.IsFailure)
            return wareHouse.Error;

        await _wareHouseRepository.AddWareHouseAsync(wareHouse.Value, cancellationToken);

        return wareHouseId.Value;
    }
}