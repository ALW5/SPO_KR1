using ProjectOne.Domain.VO;
using Size = ProjectOne.Domain.Enum.Size;

namespace ProjectOne.Application.StorageCell.CreateStorageCell;

public record CreateStorageCellRequest(
    Guid wareHouseId,
    string numberStorageCalls,
    SizeOfStorageRequest sizeOfStorage,
    decimal price,
    int? floor,
    Size size);