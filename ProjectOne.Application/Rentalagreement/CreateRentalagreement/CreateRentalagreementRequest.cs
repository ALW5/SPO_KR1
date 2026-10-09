namespace ProjectOne.Application.Rentalagreement.CreateRentalagreement;

public record CreateRentalagreementRequest(
    Guid storageId,
    DateTime startDate,
    DateTime endDate,
    decimal totalPrice);