using Microsoft.Extensions.DependencyInjection;
using ProjectOne.Application.Rentalagreement;
using ProjectOne.Application.StorageCell;
using ProjectOne.Application.WareHouse;
using ProjectOne.Infrastructure.Repositories;

namespace ProjectOne.Infrastructure;

public static class Inject
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ApplicationDbContext>();

        services.AddScoped<IStorageCellRepository, StorageCellRepository>();
        services.AddScoped<IWareHouseRepository, WareHouseRepository>();
        services.AddScoped<IRentalagreementRepository, RentalagreementRepository>();
        
        return services;
    }
}