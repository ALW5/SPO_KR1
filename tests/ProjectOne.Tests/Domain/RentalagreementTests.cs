using ProjectOne.Domain.Classes;
using ProjectOne.Domain.VO;

namespace ProjectOne.Tests.Domain;

public class RentalagreementTests
{
    private static Rentalagreement NewRental() =>
        Rentalagreement.Create(
            RentalagreementId.Create(Guid.NewGuid()),
            StorageId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            200m).Value;

    [Fact]
    public void Create_with_negative_total_fails()
    {
        var result = Rentalagreement.Create(
            RentalagreementId.Create(Guid.NewGuid()),
            StorageId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            DateTime.UtcNow, DateTime.UtcNow.AddDays(1), -1m);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Update_with_end_before_start_fails()
    {
        var rental = NewRental();

        var result = rental.Update(
            new DateTime(2030, 2, 2, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2030, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            10m);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Update_with_valid_data_changes_rental()
    {
        var rental = NewRental();
        var newStart = new DateTime(2030, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var newEnd = new DateTime(2030, 3, 5, 0, 0, 0, DateTimeKind.Utc);

        var result = rental.Update(newStart, newEnd, 400m);

        Assert.True(result.IsSuccess);
        Assert.Equal(newStart, rental.StartDate);
        Assert.Equal(newEnd, rental.EndDate);
        Assert.Equal(400m, rental.TotalPrice);
    }
}
