using ProjectOne.Domain.Common;

namespace ProjectOne.Domain.VO;

public sealed record RentalagreementId : MethodsId
{
    private RentalagreementId(Guid id) : base(id) { }
    
    public static RentalagreementId NewRentalagreementId() => new(Guid.NewGuid());
    
    public static RentalagreementId Empty() => new(Guid.Empty);
    
    public static RentalagreementId Create(Guid value) => new(value); 
}