namespace ProjectOne.Domain.Common;

public abstract record MethodsId
{
    protected  MethodsId(Guid value)
    {
        Value = value;
    }
    
    public Guid Value { get; private set; }
}