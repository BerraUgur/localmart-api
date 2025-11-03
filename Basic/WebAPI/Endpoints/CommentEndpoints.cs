using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services.Abstract;

namespace WebAPI.Endpoints;
public static class CommentEndpoints
{
    public static void RegisterCommentEndpoints(this WebApplication app)
    {
        var comments = app.MapGroup("comments").RequireAuthorization().WithTags("comments");

        comments.MapGet("/product/{productId}", async (int productId, ICommentService commentService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var commentsData = await commentService.GetCommentsByProductIdAsync(productId);
                var response = new ApiResponse<object>(200, "Comments retrieved successfully.", commentsData);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving comments. ProductId: {ProductId}", productId);
                var response = new ApiResponse<object>(500, "An error occurred while retrieving comments.", new object());
                return Results.Problem(response.Message);
            }
        }).AllowAnonymous();

        comments.MapGet("/{id}", async (int id, ICommentService commentService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var commentData = await commentService.GetCommentByIdAsync(id);
                if (commentData != null)
                {
                    var response = new ApiResponse<object>(200, "Comment retrieved successfully.", commentData);
                    return Results.Ok(response);
                }
                else
                {
                    logger.LogWarning("Comment not found. Id: {CommentId}", id);
                    var response = new ApiResponse<object>(404, "Comment not found.", new object());
                    return Results.NotFound(response);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving comments. Id: {CommentId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while retrieving comments.", new object());
                return Results.Problem(response.Message);
            }
        }).AllowAnonymous();

        comments.MapPost("", async (Comment comment, ICommentService commentService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var createdComment = await commentService.AddCommentAsync(comment);
                var response = new ApiResponse<object>(201, "Comment created successfully.", createdComment);
                return Results.Created($"/comments/{createdComment.Id}", response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving comments.");
                var response = new ApiResponse<object>(500, "An error occurred while retrieving comments.", new object());
                return Results.Problem(response.Message);
            }
        });

        comments.MapPut("/{id}", async (int id, Comment comment, ICommentService commentService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                if (id != comment.Id)
                {
                    logger.LogWarning("Comment IDs do not match. Id: {Id}, CommentId: {CommentId}", id, comment.Id);
                    var response = new ApiResponse<object>(400, "Comment IDs do not match.", new object());
                    return Results.BadRequest(response);
                }
                await commentService.UpdateCommentAsync(comment);
                var successResponse = new ApiResponse<object>(200, "Comment updated successfully.", comment);
                return Results.Ok(successResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating the comment. Id: {CommentId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while updating the comment.", new object());
                return Results.Problem(response.Message);
            }
        });

        comments.MapDelete("/{id}", async (int id, ICommentService commentService, [FromServices] ILogger<object> logger) =>
        {
            try
            {
                var existingComment = await commentService.GetCommentByIdAsync(id);
                if (existingComment == null)
                {
                    logger.LogWarning("Comment not found for deletion. Id: {CommentId}", id);
                    var response = new ApiResponse<object>(404, "Comment not found.", new object());
                    return Results.NotFound(response);
                }
                await commentService.DeleteCommentAsync(id);
                var successResponse = new ApiResponse<object>(204, "Comment deleted successfully.", new object());
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting the comment. Id: {CommentId}", id);
                var response = new ApiResponse<object>(500, "An error occurred while deleting the comment.", new object());
                return Results.Problem(response.Message);
            }
        });
    }
}