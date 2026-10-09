using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.WareHouse.GetWareHouse;

public class GetWareHouseHandler
{
    private readonly IWareHouseRepository _wareHouseRepository;

    public GetWareHouseHandler(IWareHouseRepository wareHouseRepository)
        => _wareHouseRepository = wareHouseRepository;

    public async Task<Result<WareHouseResponse, Error>> GetAsync(
        Guid wareHouseId, CancellationToken cancellationToken = default)
    {
        var id = WareHouseId.Create(wareHouseId);
        var wareHouse = await _wareHouseRepository.GetWareHouseByIdAsync(id);

        if (wareHouse.IsFailure)
            return wareHouse.Error;

        var w = wareHouse.Value;
        return new WareHouseResponse(w.Id.Value, w.Name, w.Description, w.Address, w.Floor);
    }
}

public record WareHouseResponse(Guid Id, string Name, string? Description, string Address, int Floor);