using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Domain.Classes;

public class Rentalagreement : Common.Entity<RentalagreementId>
{
    private Rentalagreement(RentalagreementId id) : base(id) {}

    private Rentalagreement(
        RentalagreementId rentalagreementId,
        StorageId storageId,
        Guid clientId,
        DateTime startDate,
        DateTime endDate,
        decimal totalPrice) : base(rentalagreementId)
    {
        StorageId = storageId;
        ClientId = clientId;
        StartDate = startDate;
        EndDate = endDate;
        TotalPrice = totalPrice;
    }
    
    public StorageCell StorageCell { get; private set; }
    public StorageId StorageId { get; private set; }
    
    public Guid ClientId { get; private set; }
    
    public DateTime StartDate { get; private set; }
    
    public DateTime EndDate { get; private set; }
    
    public decimal TotalPrice { get; private set; }

    public static Result<Rentalagreement, Error> Create(
        RentalagreementId rentalagreementId,
        StorageId storageId,
        Guid clientId,
        DateTime startDate,
        DateTime endDate,
        decimal totalPrice)
    {
        if (totalPrice < 0)
            return Errors.General.ValueIsInvalid("Total price");
        
        var rentalagreement = new Rentalagreement(
            rentalagreementId,
            storageId,
            clientId,
            startDate,
            endDate,
            totalPrice);

        return rentalagreement;
    }
    
    public UnitResult<Error> Update(DateTime startDate, DateTime endDate, decimal totalPrice)
    {
        if (startDate >= endDate)
            return Error.Validation("date.range.invalid", "Start date must be before end date");

        if (totalPrice < 0)
            return Errors.General.ValueIsInvalid("Total price");

        StartDate = startDate;
        EndDate = endDate;
        TotalPrice = totalPrice;

        return Result.Success<Error>();
    }
}