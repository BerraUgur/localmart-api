using System;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Services.Concrete
{
    public class LogService
    {
        private readonly ApplicationDBContext _context;
        public LogService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(Log log)
        {
            await _context.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Log>> GetAllLogsAsync()
        {
            return await _context.Logs.OrderByDescending(l => l.Timestamp).ToListAsync();
        }
    }
}
