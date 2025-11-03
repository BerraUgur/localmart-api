using WebAPI.Models;

namespace WebAPI.Services.Abstract;

public interface IAddressService
{
    Task<IEnumerable<Address>> GetAllAddressesAsync();
    Task<Address> GetAddressByIdAsync(int id);
    Task<IEnumerable<Address>> GetAddressesByUserIdAsync(int userId);
    Task<Address> CreateAddressAsync(Address address);
    Task UpdateAddressAsync(Address address);
    Task DeleteAddressAsync(int id);
}