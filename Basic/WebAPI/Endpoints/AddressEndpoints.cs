using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services.Abstract;

namespace WebAPI.Endpoints;

public static class AddressEndpoints
{
    public static void RegisterAddressEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("address").RequireAuthorization().WithTags("address");

        group.MapGet("/", async (IAddressService addressService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var addresses = await addressService.GetAllAddressesAsync();
                var response = new ApiResponse<object>(200, "Addresses retrieved successfully.", addresses);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving addresses.");
                var response = new ApiResponse<object>(500, "An error occurred while retrieving addresses.", new object());
                return Results.Problem(response.Message);
            }
        }).WithName("GetAllAddresses");

        group.MapGet("/{id:int}", async (int id, IAddressService addressService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var address = await addressService.GetAddressByIdAsync(id);
                if (address is not null)
                {
                    var response = new ApiResponse<object>(200, "Address retrieved successfully.", address);
                    return Results.Ok(response);
                }
                else
                {
                    logger.LogWarning("Address not found. Id: {AddressId}", id);
                    var response = new ApiResponse<object>(404, "Address not found.", new object());
                    return Results.NotFound(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving the address. Id: {AddressId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while retrieving the address.", new object());
                return Results.Problem(response.Message);
            }
        }).WithName("GetAddressById");

        group.MapPost("/", async (Address address, IAddressService addressService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var createdAddress = await addressService.CreateAddressAsync(address);
                var response = new ApiResponse<object>(201, "Address created successfully.", createdAddress);
                return Results.Created($"/api/addresses/{createdAddress.Id}", response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating the address.");
                var response = new ApiResponse<object>(500, "An error occurred while creating the address.", new object());
                return Results.Problem(response.Message);
            }
        }).WithName("CreateAddress");

        group.MapPut("/{id:int}", async (int id, Address address, IAddressService addressService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                if (id != address.Id)
                {
                    logger.LogWarning("Address IDs do not match. Id: {Id}, AddressId: {AddressId}", id, address.Id);
                    var response = new ApiResponse<object>(400, "Address IDs do not match.", new object());
                    return Results.BadRequest(response);
                }
                await addressService.UpdateAddressAsync(address);
                var successResponse = new ApiResponse<object>(204, "Address updated successfully.", new object());
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating the address. Id: {AddressId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while updating the address.", new object());
                return Results.Problem(response.Message);
            }
        }).WithName("UpdateAddress");

        group.MapDelete("/{id:int}", async (int id, IAddressService addressService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                await addressService.DeleteAddressAsync(id);
                var response = new ApiResponse<object>(204, "Address deleted successfully.", new object());
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting the address. Id: {AddressId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while deleting the address.", new object());
                return Results.Problem(response.Message);
            }
        }).WithName("DeleteAddress");
    }
}