using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.WareHouse.UpdateWareHouse;

public record UpdateWareHouseRequest(string name, string? description, string address, int floor);

public class UpdateWareHouseHandler
{
    private readonly IWareHouseRepository _wareHouseRepository;

    public UpdateWareHouseHandler(IWareHouseRepository wareHouseRepository)
        => _wareHouseRepository = wareHouseRepository;

    public async Task<Result<Guid, Error>> UpdateAsync(
        Guid wareHouseId, UpdateWareHouseRequest request, CancellationToken cancellationToken = default)
    {
        var id = WareHouseId.Create(wareHouseId);
        var wareHouseResult = await _wareHouseRepository.GetWareHouseByIdAsync(id);

        if (wareHouseResult.IsFailure)
            return wareHouseResult.Error;

        var updateResult = wareHouseResult.Value.Update(
            request.name, request.description, request.address, request.floor);

        if (updateResult.IsFailure)
            return updateResult.Error;

        await _wareHouseRepository.UpdateWareHouseAsync(wareHouseResult.Value, cancellationToken);

        return wareHouseId;
    }
}