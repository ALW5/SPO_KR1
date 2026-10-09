using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.WareHouse;

public interface IWareHouseRepository
{
    public Task AddWareHouseAsync(Domain.Classes.WareHouse wareHouse, CancellationToken cancellationToken);
    
    public Task<Result<Domain.Classes.WareHouse, Error>> GetWareHouseByIdAsync(WareHouseId wareHouseId);
    public Task<List<Domain.Classes.WareHouse>> GetAllWareHousesAsync(CancellationToken cancellationToken = default);
    
    public Task UpdateWareHouseAsync(Domain.Classes.WareHouse wareHouse, CancellationToken cancellationToken = default);
    
    public Task DeleteWareHouseAsync(Domain.Classes.WareHouse wareHouse, CancellationToken cancellationToken = default);
}