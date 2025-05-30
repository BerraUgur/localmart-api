// Basic/WebAPI/Endpoints/CommentEndpoints.cs
using WebAPI.Models;
using WebAPI.Services.Abstract;

namespace WebAPI.Endpoints;
public static class CommentEndpoints
{
    public static void RegisterCommentEndpoints(this WebApplication app)
    {
        var comments = app.MapGroup("comments").RequireAuthorization()
            .WithTags("comments");

        comments.MapGet("/product/{productId}", async (int productId, ICommentService commentService) =>
        {
            var comments = await commentService.GetCommentsByProductIdAsync(productId);
            return Results.Ok(comments);
        }).AllowAnonymous();

        comments.MapGet("/{id}", async (int id, ICommentService commentService) =>
        {
            var comment = await commentService.GetCommentByIdAsync(id);
            return comment != null ? Results.Ok(comment) : Results.NotFound();
        }).AllowAnonymous();

        comments.MapPost("", async (Comment comment, ICommentService commentService) =>
        {
            var createdComment = await commentService.AddCommentAsync(comment);
            return Results.Created($"/comments/{createdComment.Id}", createdComment);
        });

        comments.MapPut("/{id}", async (int id, Comment comment, ICommentService commentService) =>
        {
            if (id != comment.Id)
            {
                return Results.BadRequest();
            }

            await commentService.UpdateCommentAsync(comment);
            return Results.Ok(comment);
        });

        comments.MapDelete("/{id}", async (int id, ICommentService commentService) =>
        {
            var existingComment = await commentService.GetCommentByIdAsync(id);
            if (existingComment == null)
            {
                return Results.NotFound();
            }

            await commentService.DeleteCommentAsync(id);
            return Results.NoContent();
        });
    }
}