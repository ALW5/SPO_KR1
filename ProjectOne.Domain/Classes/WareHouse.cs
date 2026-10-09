using System.Net;
using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Domain.Classes;

public class WareHouse : Common.Entity<WareHouseId>
{
    private readonly List<StorageCell> _storageCalls = [];
    
    private WareHouse(WareHouseId id) : base(id) { }

    private WareHouse(
        WareHouseId warehouseId,
        string name,
        string? description,
        string address,
        int floor) : base(warehouseId)
    {
        Name = name;
        Description = description;
        Address = address;
        Floor = floor;
    }
    
    public string Name { get; private set; }
    
    public string? Description { get; private set; }
    
    public string Address { get; private set; }
    
    public int Floor { get; private set; }
    
    public IReadOnlyList<StorageCell> StorageCalls => _storageCalls;

    public static Result<WareHouse, Error> Create(WareHouseId wareHouseId,string name, string? description, string address, int floor)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.General.ValueIsEmpty("name");

        if (string.IsNullOrWhiteSpace(address))
            return Errors.General.ValueIsEmpty("address");

        if (floor < 0)
            return Errors.General.ValueIsInvalid("floor");
        
        var wareHouse = new WareHouse(
            wareHouseId,
            name,
            description,
            address,
            floor);
        
        return wareHouse;
    }
    
    public UnitResult<Error> Update(string name, string? description, string address, int floor)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.General.ValueIsEmpty("name");

        if (string.IsNullOrWhiteSpace(address))
            return Errors.General.ValueIsEmpty("address");

        if (floor < 0)
            return Errors.General.ValueIsInvalid("floor");

        Name = name;
        Description = description;
        Address = address;
        Floor = floor;

        return Result.Success<Error>();
    }
    
    public void Patch(string? name, string? description, string? address, int? floor)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (description is not null) Description = description;
        if (!string.IsNullOrWhiteSpace(address)) Address = address;
        if (floor.HasValue && floor.Value >= 0) Floor = floor.Value;
    }
}