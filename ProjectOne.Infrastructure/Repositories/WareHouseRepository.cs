using System.Runtime.InteropServices.JavaScript;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using ProjectOne.Application.WareHouse;
using ProjectOne.Domain.Classes;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Infrastructure.Repositories;

public class WareHouseRepository : IWareHouseRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public WareHouseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddWareHouseAsync(Domain.Classes.WareHouse wareHouse,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Warehouses.AddAsync(wareHouse, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<WareHouse, Error>> GetWareHouseByIdAsync(WareHouseId wareHouseId)
    {
        var wareHouse = await
            _dbContext.Warehouses.
                FirstOrDefaultAsync(s => s.Id == wareHouseId);
        
        if (wareHouse is null)
            return Errors.General.NotFound();
        
        return wareHouse;
    }
    
    public async Task<List<WareHouse>> GetAllWareHousesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Warehouses.ToListAsync(cancellationToken);
    
    public async Task UpdateWareHouseAsync(WareHouse wareHouse, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteWareHouseAsync(WareHouse wareHouse, CancellationToken cancellationToken = default)
    {
        _dbContext.Warehouses.Remove(wareHouse);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}