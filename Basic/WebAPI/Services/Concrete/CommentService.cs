// Basic/WebAPI/Services/Concrete/CommentService.cs
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;
using WebAPI.Services.Abstract;

namespace WebAPI.Services.Concrete;

    public class CommentService : ICommentService
    {
        private readonly ApplicationDBContext _context;

        public CommentService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByProductIdAsync(int productId)
        {
            return await _context.Comments.Include(x=>x.User)
                .Where(c => c.ProductId == productId)
                .ToListAsync();
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments.Include(x=>x.User).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }
    }