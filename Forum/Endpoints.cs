using Forum.Data;
using Forum.Data.Entities;
using Forum.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using System.Text.Json;

namespace Forum
{
    public static class Endpoints
    {
        public static void AddTopicApi(this WebApplication app)
        {
            var topicsGroup = app.MapGroup("/api").AddFluentValidationAutoValidation();
            topicsGroup.MapGet("/topics", async ([AsParameters] GetTopicsParameters parameters, [AsParameters] SearchParameters searchParameters) =>
            {
                var queryable = parameters.DbContext.Topics.AsQueryable().OrderBy(o => o.CreatedAt);

                var pagedList = await PagedList<Topic>.CreateAsync(queryable, searchParameters.PageNumber!.Value, searchParameters.PageSize!.Value);

                var paginationMetadata = pagedList.CreatePaginationMetadata(parameters.linkGenerator, parameters.httpContext, "GetTopics");

                // per header, Pagination: {}
                // "Pagination":
                parameters.httpContext.Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

                //return Results.Ok((await parameters.DbContext.Topics.ToListAsync()).Select(topic => topic.ToDto()));
                return Results.Ok(pagedList.Select(topic => topic.ToDto()));
            }).WithName("GetTopics");

            topicsGroup.MapGet("/topics/{topicId}", async ([AsParameters] GetTopicParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                return Results.Ok(topic.ToDto());
            });

            topicsGroup.MapPost("/topics", async ([AsParameters] CreateTopicParameters parameters) => {
                var topic = new Topic { Title = parameters.dto.Title, Description = parameters.dto.Description, CreatedAt = DateTime.UtcNow, IsDeleted = false };
                parameters.DbContext.Topics.Add(topic);

                await parameters.DbContext.SaveChangesAsync();

                return Results.Created($"api/topics/{topic.Id}", topic.ToDto());
            });

            topicsGroup.MapPut("/topics/{topicId}", async ([AsParameters] UpdateTopicParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                topic.Description = parameters.dto.Description;

                parameters.DbContext.Topics.Update(topic);
                await parameters.DbContext.SaveChangesAsync();

                return Results.Ok(topic.ToDto());
            });

            topicsGroup.MapDelete("/topics/{topicId}", async ([AsParameters] DeleteTopicParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var posts = parameters.DbContext.Posts.Where(post => post.Topic == topic).ToList();

                foreach (var post in posts)
                {
                    var comments = parameters.DbContext.Comments.Where(comment => comment.Post == post).ToList();

                    parameters.DbContext.Comments.RemoveRange(comments);
                }

                parameters.DbContext.Posts.RemoveRange(posts);

                parameters.DbContext.Topics.Remove(topic);
                await parameters.DbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }

        public static void AddPostsApi(this WebApplication app)
        {
            var postsGroup = app.MapGroup("/api/topics/{topicId}").AddFluentValidationAutoValidation();
            postsGroup.MapGet("/posts", async ([AsParameters] GetPostsParameters parameters) =>
            {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var posts = await parameters.DbContext.Posts
                    .Where(post => post.Topic == topic)
                    .Select(post => post.ToDto())
                    .ToListAsync();

                if (posts.Count == 0)
                    return Results.NotFound(); // galeciau ir nedet jei assuminsiu kad yra postu

                return Results.Ok(posts);
            });

            postsGroup.MapGet("/posts/{postId}", async ([AsParameters] GetPostParameters parameters) =>
            {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await parameters.DbContext.Posts
                    .Where(post => post.Topic == topic && post.Id == parameters.postId)
                    .FirstOrDefaultAsync();

                if (post == null)
                    return Results.NotFound();

                return Results.Ok(post.ToDto());
            });

            postsGroup.MapPost("/posts", async ([AsParameters] CreatePostParameters parameters) =>
            {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = new Post
                {
                    Title = parameters.dto.Title,
                    Body = parameters.dto.Body,
                    CreatedAt = DateTime.UtcNow,
                    Topic = topic,
                    IsDeleted = false
                };

                parameters.DbContext.Posts.Add(post);
                await parameters.DbContext.SaveChangesAsync();

                return Results.Created($"/api/topics/{topic.Id}/posts/{post.Id}", post.ToDto());
            });

            postsGroup.MapPut("/posts/{postId}", async ([AsParameters] UpdatePostParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await parameters.DbContext.Posts
                    .Where(post => post.Topic == topic && post.Id == parameters.postId)
                    .FirstOrDefaultAsync();

                if (post == null)
                    return Results.NotFound();

                post.Body = parameters.dto.Body;
                //post.CreatedAt = DateTime.UtcNow; ? ar updatint

                parameters.DbContext.Posts.Update(post);
                await parameters.DbContext.SaveChangesAsync();

                return Results.Ok(post.ToDto());
            });

            postsGroup.MapDelete("/posts/{postId}", async ([AsParameters] DeletePostParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await parameters.DbContext.Posts
                    .Where(p => p.Topic == topic && p.Id == parameters.postId)
                    .FirstOrDefaultAsync();

                if (post == null)
                    return Results.NotFound();

                var comments = parameters.DbContext.Comments.Where(comment => comment.Post == post).ToList();
                parameters.DbContext.Comments.RemoveRange(comments);

                //post.IsDeleted = true;
                parameters.DbContext.Posts.Remove(post);
                await parameters.DbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }

        public static void AddCommentsApi(this WebApplication app)
        {
            var CommentsGroup = app.MapGroup("/api/topics/{topicId}/posts/{postId}").AddFluentValidationAutoValidation();
            CommentsGroup.MapGet("/comments", async ([AsParameters] GetCommentsParameters parameters) =>
            {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await parameters.DbContext.Posts.FindAsync(parameters.postId);

                if (post == null)
                    return Results.NotFound();

                var comments = await parameters.DbContext.Comments
                    .Where(comment => comment.Post == post)
                    .Select(comment => comment.ToDto())
                    .ToListAsync();

                if (comments.Count == 0)
                    return Results.NotFound(); // galeciau ir nedet jei assuminsiu kad yra komentaru

                return Results.Ok(comments);
            });

            CommentsGroup.MapGet("/comments/{commentId}", async ([AsParameters] GetCommentParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await parameters.DbContext.Posts.FindAsync(parameters.postId);

                if (post == null)
                    return Results.NotFound();

                var comment = await parameters.DbContext.Comments
                    .Where(comment => comment.Post == post && comment.Id == parameters.commentId)
                    .FirstOrDefaultAsync();

                if (comment == null)
                    return Results.NotFound();

                return Results.Ok(comment.ToDto());
            });

            CommentsGroup.MapPost("/comments", async ([AsParameters] CreateCommentParameters parameters) =>
            {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await parameters.DbContext.Posts.FindAsync(parameters.postId);

                if (post == null)
                    return Results.NotFound();

                var comment = new Comment { Content = parameters.dto.Content, CreatedAt = DateTime.UtcNow, IsDeleted = false, Post = post };
                parameters.DbContext.Comments.Add(comment);

                await parameters.DbContext.SaveChangesAsync();

                return Results.Created($"api/topics/{topic.Id}/posts/{post.Id}/comments/{comment.Id}", comment.ToDto());
            });

            CommentsGroup.MapPut("/comments/{commentId}", async ([AsParameters] UpdateCommentParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await parameters.DbContext.Posts.FindAsync(parameters.postId);

                if (post == null)
                    return Results.NotFound();

                var comment = await parameters.DbContext.Comments
                    .Where(comment => comment.Post == post && comment.Id == parameters.commentId)
                    .FirstOrDefaultAsync();

                if (comment == null)
                    return Results.NotFound();

                comment.Content = parameters.dto.Content;
                parameters.DbContext.Update(comment);
                await parameters.DbContext.SaveChangesAsync();

                return Results.Ok(comment.ToDto());
            });

            CommentsGroup.MapDelete("/comments/{commentId}", async ([AsParameters] DeleteCommentParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await parameters.DbContext.Posts.FindAsync(parameters.postId);

                if (post == null)
                    return Results.NotFound();

                var comment = await parameters.DbContext.Comments
                    .Where(comment => comment.Post == post && comment.Id == parameters.commentId)
                    .FirstOrDefaultAsync();

                if (comment == null)
                    return Results.NotFound();

                parameters.DbContext.Comments.Remove(comment);
                await parameters.DbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }
    }
}
