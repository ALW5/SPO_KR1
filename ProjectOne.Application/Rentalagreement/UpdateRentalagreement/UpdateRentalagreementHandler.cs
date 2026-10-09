using CSharpFunctionalExtensions;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.VO;

namespace ProjectOne.Application.Rentalagreement.UpdateRentalagreement;

public record UpdateRentalagreementRequest(DateTime startDate, DateTime endDate, decimal totalPrice);

public class UpdateRentalagreementHandler
{
    private readonly IRentalagreementRepository _rentalagreementRepository;

    public UpdateRentalagreementHandler(IRentalagreementRepository rentalagreementRepository)
    {
        _rentalagreementRepository = rentalagreementRepository;
    }

    public async Task<Result<Guid, Error>> UpdateAsync(
        Guid id, UpdateRentalagreementRequest rentalagreementRequest,
        CancellationToken cancellationToken)
    {
        var rentalagreementId = RentalagreementId.Create(id);

        var rentalagreementResult = await _rentalagreementRepository.GetRentalagreementByIdAsync(
            rentalagreementId);
        
        if(rentalagreementResult.IsFailure)
            return rentalagreementResult.Error;

        var rentalagreement = rentalagreementResult.Value.Update(
            rentalagreementRequest.startDate, rentalagreementRequest.endDate, rentalagreementRequest.totalPrice);
        
        if (rentalagreement.IsFailure)
            return rentalagreement.Error;
        
        await _rentalagreementRepository.UpdateRentalagreementAsync(rentalagreementResult.Value, cancellationToken);

        return id;
    }
}