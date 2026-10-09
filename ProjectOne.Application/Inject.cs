using Microsoft.Extensions.DependencyInjection;
using ProjectOne.Application.Auth.Login;
using ProjectOne.Application.Auth.Register;
using ProjectOne.Application.Rentalagreement.CreateRentalagreement;
using ProjectOne.Application.Rentalagreement.DeleteRentalagreement;
using ProjectOne.Application.Rentalagreement.GetRentalagreement;
using ProjectOne.Application.Rentalagreement.UpdateRentalagreement;
using ProjectOne.Application.StorageCell.CreateStorageCell;
using ProjectOne.Application.StorageCell.DeleteStorageCell;
using ProjectOne.Application.StorageCell.GetStorageCell;
using ProjectOne.Application.StorageCell.PatchStorageCell;
using ProjectOne.Application.StorageCell.ReserveStorageCell;
using ProjectOne.Application.StorageCell.UpdateStorageCell;
using ProjectOne.Application.WareHouse.CreateWareHouse;
using ProjectOne.Application.WareHouse.DeleteWareHouse;
using ProjectOne.Application.WareHouse.GetWareHouse;
using ProjectOne.Application.WareHouse.PatchWareHouse;
using ProjectOne.Application.WareHouse.UpdateWareHouse;

namespace ProjectOne.Application;

public static class Inject
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateStorageCellHandler>();
        services.AddScoped<ReserveStorageCellHandler>();
        services.AddScoped<GetStorageCellHandler>();
        services.AddScoped<UpdateStorageCellHandler>();
        services.AddScoped<DeleteStorageCellHandler>();
        services.AddScoped<GetAllStorageCellHandler>();
        services.AddScoped<PatchStorageCellHandler>();

        services.AddScoped<CreateWareHouseHandler>();
        services.AddScoped<GetWareHouseHandler>();
        services.AddScoped<GetWareHousesHandler>();
        services.AddScoped<UpdateWareHouseHandler>();
        services.AddScoped<PatchWareHouseHandler>();
        services.AddScoped<DeleteWareHouseHandler>();

        services.AddScoped<CreateRentalagreementHandler>();
        services.AddScoped<DeleteRentalagreementHandler>();
        services.AddScoped<GetAllRentalagreementHandler>();
        services.AddScoped<GetRentalagreementHandler>();
        services.AddScoped<UpdateRentalagreementHandler>();

        services.AddScoped<LoginHandler>();
        services.AddScoped<RegisterHandler>();
        
        
        return services;
    }
}