using ProjectOne.Domain.Classes;
using ProjectOne.Domain.Common;
using ProjectOne.Domain.Enum;
using ProjectOne.Domain.VO;
using Size = ProjectOne.Domain.Enum.Size;

namespace ProjectOne.Tests.Domain;

public class StorageCellTests
{
    private static StorageCell NewCell(decimal price = 100m)
    {
        var size = SizeOfStorage.Create(1, 1, 1).Value;

        return StorageCell.Create(
            StorageId.Create(Guid.NewGuid()),
            WareHouseId.Create(Guid.NewGuid()),
            "A-1", size, price, 1, Size.Small).Value;
    }

    [Fact]
    public void New_cell_is_free()
        => Assert.Equal(Status.Free, NewCell().Status);

    [Fact]
    public void Reserve_free_cell_makes_it_booked()
    {
        var cell = NewCell();

        var result = cell.Reserve();

        Assert.True(result.IsSuccess);
        Assert.Equal(Status.Booked, cell.Status);
    }

    [Fact]
    public void Reserve_booked_cell_returns_conflict()
    {
        var cell = NewCell();
        cell.Reserve();

        var result = cell.Reserve();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.ErrorType);
    }

    [Fact]
    public void Release_makes_cell_free_again()
    {
        var cell = NewCell();
        cell.Reserve();

        cell.Release();

        Assert.Equal(Status.Free, cell.Status);
    }

    [Fact]
    public void Create_with_negative_price_fails()
    {
        var size = SizeOfStorage.Create(1, 1, 1).Value;

        var result = StorageCell.Create(
            StorageId.Create(Guid.NewGuid()), WareHouseId.Create(Guid.NewGuid()),
            "A-1", size, -1m, 1, Size.Small);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_with_empty_number_fails()
    {
        var size = SizeOfStorage.Create(1, 1, 1).Value;

        var result = StorageCell.Create(
            StorageId.Create(Guid.NewGuid()), WareHouseId.Create(Guid.NewGuid()),
            "", size, 10m, 1, Size.Small);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, -5)]
    public void SizeOfStorage_rejects_non_positive_dimensions(int w, int h, int d)
        => Assert.True(SizeOfStorage.Create(w, h, d).IsFailure);
}
