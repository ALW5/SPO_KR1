using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using ProjectOne.Application.StorageCell;
using ProjectOne.Domain.Classes;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Infrastructure.Repositories;

public class StorageCellRepository : IStorageCellRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StorageCellRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddStorageCellAsync(Domain.Classes.StorageCell storageCell,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.StorageCalls.AddAsync(storageCell, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<StorageCell, Error>> GetStorageCellByIdAsync(StorageId storageId)
    {
        var storageCell = await _dbContext.StorageCalls.FirstOrDefaultAsync(s => s.Id == storageId);

        if (storageCell is null)
            return Errors.General.NotFound();

        return storageCell;
    }

    public async Task UpdateStorageCellAsync(StorageCell storageCell, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteStorageCellAsync(StorageCell storageCell, CancellationToken cancellationToken = default)
    {
        _dbContext.StorageCalls.Remove(storageCell);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<StorageCell>> GetAllStorageCellsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.StorageCalls.ToListAsync();
}