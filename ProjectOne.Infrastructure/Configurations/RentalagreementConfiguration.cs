using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectOne.Domain.Classes;
using ProjectOne.Domain.VO;

namespace ProjectOne.Infrastructure.Configurations;

public class RentalagreementConfiguration : IEntityTypeConfiguration<Rentalagreement>
{
    public void Configure(EntityTypeBuilder<Rentalagreement> builder)
    {
        builder.ToTable("RentalAgreement");
        
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ClientId).IsRequired();
        
        builder.Property(r => r.Id)
            .HasConversion(
                v => v.Value,
                value => RentalagreementId.Create(value));

        builder
            .Property(r => r.TotalPrice)
            .HasPrecision(18, 2);
        
        builder
            .HasOne(r => r.StorageCell)
            .WithMany()
            .HasForeignKey(r => r.StorageId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasIndex(r => r.StorageId);
    }
}