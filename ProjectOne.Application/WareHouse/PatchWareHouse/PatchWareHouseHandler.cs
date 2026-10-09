using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.WareHouse.PatchWareHouse;

public record PatchWareHouseRequest(string? name, string? description, string? address, int? floor);

public class PatchWareHouseHandler
{
    private readonly IWareHouseRepository _wareHouseRepository;

    public PatchWareHouseHandler(IWareHouseRepository wareHouseRepository)
        => _wareHouseRepository = wareHouseRepository;

    public async Task<Result<Guid, Error>> PatchAsync(
        Guid wareHouseId, PatchWareHouseRequest request, CancellationToken cancellationToken = default)
    {
        var id = WareHouseId.Create(wareHouseId);
        var wareHouseResult = await _wareHouseRepository.GetWareHouseByIdAsync(id);

        if (wareHouseResult.IsFailure)
            return wareHouseResult.Error;

        wareHouseResult.Value.Patch(request.name, request.description, request.address, request.floor);

        await _wareHouseRepository.UpdateWareHouseAsync(wareHouseResult.Value, cancellationToken);

        return wareHouseId;
    }
}