using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;
using WebAPI.Services.Abstract;

namespace WebAPI.Services.Concrete;

public class AddressService : IAddressService
{
    private readonly ApplicationDBContext _context;
    private readonly IHttpContextAccessor _contextAccessor;
    
    public AddressService(ApplicationDBContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    public async Task<IEnumerable<Address>> GetAllAddressesAsync()
    {
        return await _context.Addresses.ToListAsync();
    }
    
    public async Task<IEnumerable<Address>> GetAddressesByUserIdAsync(int userId) 
    {
        return await _context.Addresses.Include(x=>x.User).Where(a => a.UserId == userId).ToListAsync();
    }

    public async Task<Address> GetAddressByIdAsync(int id)
    {
        return await _context.Addresses.FindAsync(id);
    }

    public async Task<Address> CreateAddressAsync(Address address)
    {
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
        return address;
    }

    public async Task UpdateAddressAsync(Address address)
    {
        _context.Entry(address).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAddressAsync(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address != null)
        {
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
        }
    }
}