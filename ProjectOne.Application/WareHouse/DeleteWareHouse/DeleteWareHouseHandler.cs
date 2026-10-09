using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.WareHouse.DeleteWareHouse;

public class DeleteWareHouseHandler
{
    private readonly IWareHouseRepository _wareHouseRepository;

    public DeleteWareHouseHandler(IWareHouseRepository wareHouseRepository)
        => _wareHouseRepository = wareHouseRepository;

    public async Task<UnitResult<Error>> DeleteAsync(
        Guid wareHouseId, CancellationToken cancellationToken = default)
    {
        var id = WareHouseId.Create(wareHouseId);
        var wareHouseResult = await _wareHouseRepository.GetWareHouseByIdAsync(id);

        if (wareHouseResult.IsFailure)
            return wareHouseResult.Error;

        await _wareHouseRepository.DeleteWareHouseAsync(wareHouseResult.Value, cancellationToken);

        return UnitResult.Success<Error>();
    }
}