using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;
using WebAPI.Services.Abstract;

namespace WebAPI.Services.Concrete;

public class AddressService : IAddressService
{
    private readonly ApplicationDBContext _context;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<AddressService> _logger;

    public AddressService(ApplicationDBContext context, IHttpContextAccessor contextAccessor, ILogger<AddressService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Address>> GetAllAddressesAsync()
    {
        try
        {
            return await _context.Addresses.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all addresses.");
            throw new ApplicationException("An error occurred while fetching addresses.", ex);
        }
    }

    public async Task<IEnumerable<Address>> GetAddressesByUserIdAsync(int userId)
    {
        try
        {
            return await _context.Addresses.Include(x => x.User).Where(a => a.UserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching addresses for userId {userId}.");
            throw new ApplicationException("An error occurred while fetching user addresses.", ex);
        }
    }

    public async Task<Address> GetAddressByIdAsync(int id)
    {
        try
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                _logger.LogWarning($"Address with id {id} not found.");
                throw new KeyNotFoundException("Address not found.");
            }
            return address;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching address by id {id}.");
            throw new ApplicationException("An error occurred while fetching the address.", ex);
        }
    }

    public async Task<Address> CreateAddressAsync(Address address)
    {
        if (address == null) throw new ArgumentNullException(nameof(address));
        try
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address.");
            throw new ApplicationException("An error occurred while creating the address.", ex);
        }
    }

    public async Task UpdateAddressAsync(Address address)
    {
        if (address == null) throw new ArgumentNullException(nameof(address));
        try
        {
            _context.Entry(address).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address.");
            throw new ApplicationException("An error occurred while updating the address.", ex);
        }
    }

    public async Task DeleteAddressAsync(int id)
    {
        try
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                _logger.LogWarning($"Address with id {id} not found for deletion.");
                throw new KeyNotFoundException("Address not found.");
            }
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting address with id {id}.");
            throw new ApplicationException("An error occurred while deleting the address.", ex);
        }
    }
}