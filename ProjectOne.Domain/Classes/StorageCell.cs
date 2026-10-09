using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.Enum;
using ProjectOne.Domain.VO;

namespace ProjectOne.Domain.Classes;

public class StorageCell : Common.Entity<StorageId>
{
    private StorageCell(StorageId id) : base(id) { }

    private StorageCell(
        StorageId storageId,
        WareHouseId wareHouseId,
        string numberStorageCalls,
        SizeOfStorage sizeOfStorage,
        decimal price,
        int? floor,
        Size size) : base(storageId)
    {
        WareHouseId = wareHouseId;
        NumberStorageCalls = numberStorageCalls;
        SizeOfStorage = sizeOfStorage;
        Price = price;
        Floor = floor;
        Size = size;
        Status = Status.Free;
    }
    
    public WareHouse WareHouse { get; private set; }
    public WareHouseId WareHouseId { get; private set; }
    
    // номер ячейки
    public string NumberStorageCalls { get; private set; }
    
    // точные размеры
    public SizeOfStorage SizeOfStorage { get; private set; }
    
    // цена в день
    public decimal Price { get; private set; }
    
    // этаж
    public int? Floor { get; private set; }
    
    // размеры (категория)
    public Size Size { get; private set; }
    
    // статус (занят, не занят)
    public Status Status { get; private set; }
    
    public static Result<StorageCell, Error> Create(
        StorageId storageId,
        WareHouseId wareHouseId,
        string numberStorageCalls,
        SizeOfStorage sizeOfStorage,
        decimal price,
        int? floor,
        Size size)
    {
        
        if(string.IsNullOrEmpty(numberStorageCalls))
            return Errors.General.ValueIsEmpty("numberStorageCalls");
        
        if (price < 0)
            return Errors.General.ValueIsInvalid("price");
        
        var storageCall = new StorageCell(
            storageId,
            wareHouseId,
            numberStorageCalls, 
            sizeOfStorage,
            price, 
            floor,
            size);
        
        return storageCall;
    }
    
    public UnitResult<Error> Update(
        string numberStorageCalls,
        SizeOfStorage sizeOfStorage,
        decimal price,
        int? floor,
        Size size)
    {
        if (string.IsNullOrEmpty(numberStorageCalls))
            return Errors.General.ValueIsEmpty("numberStorageCalls");

        if (price < 0)
            return Errors.General.ValueIsInvalid("price");

        NumberStorageCalls = numberStorageCalls;
        SizeOfStorage = sizeOfStorage;
        Price = price;
        Floor = floor;
        Size = size;

        return Result.Success<Error>();
    }

    public void Patch(string? numberStorageCalls, decimal? price, int? floor)
    {
        if (!string.IsNullOrEmpty(numberStorageCalls)) NumberStorageCalls = numberStorageCalls;
        if (price.HasValue && price.Value >= 0) Price = price.Value;
        if (floor.HasValue) Floor = floor.Value;
    }

    public UnitResult<Error> Reserve()
    {
        if (Status != Status.Free)
            return Error.Conflict("cell.already.booked", "Cell is already booked");

        Status = Status.Booked;
        return UnitResult.Success<Error>();
    }

    public void Release() => Status = Status.Free;
    
}