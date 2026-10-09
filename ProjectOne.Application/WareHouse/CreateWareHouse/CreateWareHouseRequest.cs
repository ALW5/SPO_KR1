namespace ProjectOne.Application.WareHouse.CreateWareHouse;

public record CreateWareHouseRequest(string name, string? description, string address, int floor);