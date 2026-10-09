using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectOne.Application.Identity;
using ProjectOne.Domain.Classes;

namespace ProjectOne.Infrastructure;

public class ApplicationDbContext(IConfiguration configuration)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private const string StringName = "Db";
    
    public DbSet<Rentalagreement> Rentalagreements => Set<Rentalagreement>();
    public DbSet<StorageCell> StorageCalls => Set<StorageCell>();
    public DbSet<WareHouse> Warehouses => Set<WareHouse>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetConnectionString(StringName));
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());   
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    
    private ILoggerFactory CreateLoggerFactory() 
        => LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
}
