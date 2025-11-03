using WebAPI.Models;

namespace WebAPI.Services.Abstract;
public interface ICommentService
{
    Task<IEnumerable<Comment>> GetCommentsByProductIdAsync(int productId);
    Task<Comment> GetCommentByIdAsync(int id);
    Task<Comment> AddCommentAsync(Comment comment);
    Task UpdateCommentAsync(Comment comment);
    Task DeleteCommentAsync(int id);
}