using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectOne.Domain.Classes;
using ProjectOne.Domain.Enum;
using ProjectOne.Domain.VO;

namespace ProjectOne.Infrastructure.Configurations;

public class StorageCallConfiguration : IEntityTypeConfiguration<StorageCell>
{
    public void Configure(EntityTypeBuilder<StorageCell> builder)
    {
        builder.ToTable("StorageCell");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => StorageId.Create(value));

        builder.Property(c => c.NumberStorageCalls)
            .IsRequired().HasMaxLength(100);

        builder.OwnsOne(c => c.SizeOfStorage, a =>
        {
            a.Property(c => c.Depth).IsRequired().HasColumnName("Depth");
            a.Property(c => c.Height).IsRequired().HasColumnName("Height");
            a.Property(c => c.Width).IsRequired().HasColumnName("Width");
        });
        
        builder
            .Property(c => c.Price)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasColumnName("Price");
        
        builder.Property(c => c.Floor).HasColumnName("Floor");
        
        builder.Property(c => c.Size).IsRequired().HasColumnName("Size");
        
        builder.Property(c => c.Status).HasDefaultValue(Status.Free).IsRequired();
    }
}