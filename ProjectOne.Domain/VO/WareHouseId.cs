using ProjectOne.Domain.Common;

namespace ProjectOne.Domain.VO;

public sealed record WareHouseId : MethodsId
{
    private WareHouseId(Guid id) : base(id)
    {}
    
    public static WareHouseId NewWareHouseId() => new(Guid.NewGuid());
    
    public static WareHouseId Empty() => new(Guid.Empty);

    public static WareHouseId Create(Guid value) => new(value);
}