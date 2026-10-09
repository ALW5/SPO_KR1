using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using ProjectOne.Application.Rentalagreement;
using ProjectOne.Domain.Classes;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.Enum;
using ProjectOne.Domain.VO;

namespace ProjectOne.Infrastructure.Repositories;

public class RentalagreementRepository : IRentalagreementRepository
{
    
    private readonly ApplicationDbContext _dbContext;
    
    public RentalagreementRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddRentalagreementAsync(Rentalagreement rentalagreement, CancellationToken cancellationToken = default)
    {
        await _dbContext.Rentalagreements.AddAsync(rentalagreement, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<Rentalagreement, Error>> GetRentalagreementByIdAsync(RentalagreementId id)
    {
        var rentalagreement = await _dbContext.Rentalagreements
            .Include(r => r.StorageCell)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rentalagreement is null)
            return Errors.General.NotFound();
        
        return rentalagreement;
    }

    public async Task DeleteRentalagreementAsync(Rentalagreement rentalagreement,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Rentalagreements.Remove(rentalagreement);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Rentalagreement>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rentalagreements
            .Include(r => r.StorageCell)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TryCreateWithReservationAsync(
        Rentalagreement rentalagreement, CancellationToken cancellationToken = default)
    {
        var storageId = rentalagreement.StorageId;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Условный UPDATE: сработает только для одного из параллельных запросов.
        var affected = await _dbContext.StorageCalls
            .Where(s => s.Id == storageId && s.Status == Status.Free)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(s => s.Status, Status.Booked),
                cancellationToken);

        if (affected == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await _dbContext.Rentalagreements.AddAsync(rentalagreement, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ExistsForStorageAsync(StorageId storageId, CancellationToken cancellationToken = default)
        => await _dbContext.Rentalagreements.AnyAsync(r => r.StorageId == storageId, cancellationToken);

    public async Task UpdateRentalagreementAsync(Rentalagreement rentalagreement,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}