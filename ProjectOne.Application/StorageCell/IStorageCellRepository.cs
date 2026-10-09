using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.StorageCell;

public interface IStorageCellRepository
{
    public Task AddStorageCellAsync(
        Domain.Classes.StorageCell storageCell, 
        CancellationToken cancellationToken = default);
    
    public Task<Result<Domain.Classes.StorageCell, Error>> GetStorageCellByIdAsync(StorageId storageId);

    public Task UpdateStorageCellAsync(
        Domain.Classes.StorageCell storageCell,
        CancellationToken cancellationToken = default
    );
    public Task DeleteStorageCellAsync(
        Domain.Classes.StorageCell storageCell,
        CancellationToken cancellationToken = default
    );

    public Task<List<Domain.Classes.StorageCell>> GetAllStorageCellsAsync(CancellationToken cancellationToken = default);
    
}