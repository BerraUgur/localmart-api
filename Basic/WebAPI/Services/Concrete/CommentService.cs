// Basic/WebAPI/Services/Concrete/CommentService.cs
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;
using WebAPI.Services.Abstract;

namespace WebAPI.Services.Concrete;

public class CommentService : ICommentService
{
    private readonly ApplicationDBContext _context;
    private readonly ILogger<CommentService> _logger;
    private readonly IHttpContextAccessor _contextAccessor;

    public CommentService(ApplicationDBContext context, ILogger<CommentService> logger, IHttpContextAccessor contextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    }

    private bool CheckAccess(int userId)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
            return false;
        if (httpContext.User.IsInRole("Admin") || httpContext.User.Claims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value == userId.ToString())
        {
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<Comment>> GetCommentsByProductIdAsync(int productId)
    {
        try
        {
            return await _context.Comments.Include(x => x.User)
                .Where(c => c.ProductId == productId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching comments for productId {productId}.");
            throw new ApplicationException("An error occurred while fetching comments.", ex);
        }
    }

    public async Task<Comment> GetCommentByIdAsync(int id)
    {
        try
        {
            var comment = await _context.Comments.Include(x => x.User).FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
            {
                _logger.LogWarning($"Comment with id {id} not found.");
                throw new KeyNotFoundException("Comment not found.");
            }
            return comment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching comment by id {id}.");
            throw new ApplicationException("An error occurred while fetching the comment.", ex);
        }
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        if (comment == null) throw new ArgumentNullException(nameof(comment));
        try
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment.");
            throw new ApplicationException("An error occurred while adding the comment.", ex);
        }
    }

    public async Task UpdateCommentAsync(Comment comment)
    {
        if (comment == null) throw new ArgumentNullException(nameof(comment));
        try
        {
            // Access control: Only admin or owner can update comment
            if (!CheckAccess(comment.UserId))
            {
                _logger.LogWarning($"Access denied for comment update. UserId: {comment.UserId}");
                throw new UnauthorizedAccessException("Access denied");
            }
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment.");
            throw new ApplicationException("An error occurred while updating the comment.", ex);
        }
    }

    public async Task DeleteCommentAsync(int id)
    {
        try
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                _logger.LogWarning($"Comment with id {id} not found for deletion.");
                throw new KeyNotFoundException("Comment not found.");
            }
            // Access control: Only admin or owner can delete comment
            if (!CheckAccess(comment.UserId))
            {
                _logger.LogWarning($"Access denied for comment deletion. UserId: {comment.UserId}");
                throw new UnauthorizedAccessException("Access denied");
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting comment with id {id}.");
            throw new ApplicationException("An error occurred while deleting the comment.", ex);
        }
    }
}