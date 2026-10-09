using ProjectOne.Domain.Common;

namespace ProjectOne.Domain.VO;

public sealed record StorageId : MethodsId
{
    private StorageId(Guid value) : base(value){}
    
    public static StorageId NewStorageId() => new(Guid.NewGuid());
    
    public static StorageId Empty() => new(Guid.Empty);
    
    public static StorageId Create(Guid value) => new(value);
}