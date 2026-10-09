using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectOne.Domain.Classes;
using ProjectOne.Domain.VO;

namespace ProjectOne.Infrastructure.Configurations;

public class WareHouseConfiguration : IEntityTypeConfiguration<WareHouse>
{
    public void Configure(EntityTypeBuilder<WareHouse> builder)
    {
        builder.ToTable("WareHouse");
        
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                s => s.Value,
                value => WareHouseId.Create(value));
        
        builder.Property(c => c.Name).IsRequired().HasColumnName("Name");
        
        builder.Property(c => c.Description).HasColumnName("Description");
        
        builder.Property(c => c.Address).IsRequired().HasColumnName("Address");
        
        builder.Property(c => c.Floor).IsRequired().HasColumnName("Floor");
        
        builder
            .HasMany(c => c.StorageCalls)
            .WithOne(c => c.WareHouse)
            .HasForeignKey(c => c.WareHouseId);
    }
}