using WebAPI.Models;
using WebAPI.Services.Abstract;

namespace WebAPI.Endpoints;

public static class AddressEndpoints
{
    public static void RegisterAddressEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("address").RequireAuthorization()
            .WithTags("address");

        group.MapGet("/", async (IAddressService addressService) =>
            {
                var addresses = await addressService.GetAllAddressesAsync();
                return Results.Ok(addresses);
            })
            .WithName("GetAllAddresses");

        group.MapGet("/{id:int}", async (int id, IAddressService addressService) =>
            {
                var address = await addressService.GetAddressByIdAsync(id);
                return address is not null ? Results.Ok(address) : Results.NotFound();
            })
            .WithName("GetAddressById");

        group.MapPost("/", async (Address address, IAddressService addressService) =>
            {
                var createdAddress = await addressService.CreateAddressAsync(address);
                return Results.Created($"/api/addresses/{createdAddress.Id}", createdAddress);
            })
            .WithName("CreateAddress");

        group.MapPut("/{id:int}", async (int id, Address address, IAddressService addressService) =>
            {
                if (id != address.Id)
                {
                    return Results.BadRequest();
                }

                await addressService.UpdateAddressAsync(address);
                return Results.NoContent();
            })
            .WithName("UpdateAddress");
        
        // group.MapGet("/user/{userId:int}", async (int userId, IAddressService addressService) => // New endpoint
        //     {
        //         var addresses = await addressService.GetAddressesByUserIdAsync(userId);
        //         return Results.Ok(addresses);
        //     })
        //     .WithName("GetAddressesByUserId");

        group.MapDelete("/{id:int}", async (int id, IAddressService addressService) =>
            {
                await addressService.DeleteAddressAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteAddress");
    }
}