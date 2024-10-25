using Forum.Data;
using Forum.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace Forum
{
    public static class Endpoints
    {
        public static void AddTopicApi(this WebApplication app)
        {
            var topicsGroup = app.MapGroup("/api").AddFluentValidationAutoValidation();
            topicsGroup.MapGet("/topics", async (ForumDbContext DbContext) =>
            {
                // reiktu 404 jei nera lygtai?
                return Results.Ok((await DbContext.Topics.ToListAsync()).Select(topic => topic.ToDto()));
            });

            topicsGroup.MapGet("/topics/{topicId}", async (int topicId, ForumDbContext DbContext) => {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                return Results.Ok(topic.ToDto());
            });

            topicsGroup.MapPost("/topics", async (CreateTopicDTO dto, ForumDbContext DbContext) => {
                var topic = new Topic { Title = dto.Title, Description = dto.Description, CreatedAt = DateTime.UtcNow, IsDeleted = false };
                DbContext.Topics.Add(topic);

                await DbContext.SaveChangesAsync();

                return Results.Created($"api/topics/{topic.Id}", topic.ToDto());
            });

            topicsGroup.MapPut("/topics/{topicId}", async (UpdateTopicDTO dto, int topicId, ForumDbContext DbContext) => {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                topic.Description = dto.Description;

                DbContext.Topics.Update(topic);
                await DbContext.SaveChangesAsync();

                return Results.Ok(topic.ToDto());
            });

            topicsGroup.MapDelete("/topics/{topicId}", async (int topicId, ForumDbContext DbContext) => {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var posts = DbContext.Posts.Where(post => post.Topic == topic).ToList();

                foreach (var post in posts)
                {
                    var comments = DbContext.Comments.Where(comment => comment.Post == post).ToList();

                    DbContext.Comments.RemoveRange(comments);
                }

                DbContext.Posts.RemoveRange(posts);
                
                DbContext.Topics.Remove(topic);
                await DbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }

        public static void AddPostsApi(this WebApplication app)
        {
            var postsGroup = app.MapGroup("/api/topics/{topicId}").AddFluentValidationAutoValidation();
            postsGroup.MapGet("/posts", async (int topicId, ForumDbContext DbContext) =>
            {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var posts = await DbContext.Posts
                    .Where(post => post.Topic == topic)
                    .Select(post => post.ToDto())
                    .ToListAsync();

                if (posts.Count == 0)
                    return Results.NotFound(); // galeciau ir nedet jei assuminsiu kad yra postu

                return Results.Ok(posts);
            });

            postsGroup.MapGet("/posts/{postId}", async (int topicId, int postId, ForumDbContext DbContext) =>
            {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await DbContext.Posts
                    .Where(post => post.Topic == topic && post.Id == postId)
                    .FirstOrDefaultAsync();

                if (post == null)
                    return Results.NotFound();

                return Results.Ok(post.ToDto());
            });

            postsGroup.MapPost("/posts", async (CreatePostDTO dto, int topicId, ForumDbContext DbContext) =>
            {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = new Post
                {
                    Title = dto.Title,
                    Body = dto.Body,
                    CreatedAt = DateTime.UtcNow,
                    Topic = topic,
                    IsDeleted = false
                };

                DbContext.Posts.Add(post);
                await DbContext.SaveChangesAsync();

                return Results.Created($"/api/topics/{topic.Id}/posts/{post.Id}", post.ToDto());
            });

            postsGroup.MapPut("/posts/{postId}", async (UpdatePostDTO dto, int topicId, int postId, ForumDbContext DbContext) => {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await DbContext.Posts
                    .Where(post => post.Topic == topic && post.Id == postId)
                    .FirstOrDefaultAsync();

                if (post == null)
                    return Results.NotFound();

                post.Body = dto.Body;
                //post.CreatedAt = DateTime.UtcNow; ? ar updatint

                DbContext.Posts.Update(post);
                await DbContext.SaveChangesAsync();

                return Results.Ok(post.ToDto());
            });

            postsGroup.MapDelete("/posts/{postId}", async (int topicId, int postId, ForumDbContext DbContext) => {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await DbContext.Posts
                    .Where(p => p.Topic == topic && p.Id == postId)
                    .FirstOrDefaultAsync();

                if (post == null)
                    return Results.NotFound();

                var comments = DbContext.Comments.Where(comment => comment.Post == post).ToList();
                DbContext.Comments.RemoveRange(comments);

                //post.IsDeleted = true;
                DbContext.Posts.Remove(post);
                await DbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }

        public static void AddCommentsApi(this WebApplication app)
        {
            var CommentsGroup = app.MapGroup("/api/topics/{topicId}/posts/{postId}").AddFluentValidationAutoValidation();
            CommentsGroup.MapGet("/comments", async (int topicId, int postId, ForumDbContext DbContext) =>
            {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await DbContext.Posts.FindAsync(postId);

                if (post == null)
                    return Results.NotFound();

                var comments = await DbContext.Comments
                    .Where(comment => comment.Post == post)
                    .Select(comment => comment.ToDto())
                    .ToListAsync();

                if (comments.Count == 0)
                    return Results.NotFound(); // galeciau ir nedet jei assuminsiu kad yra komentaru

                return Results.Ok(comments);
            });

            CommentsGroup.MapGet("/comments/{commentId}", async (int topicId, int postId, int commentId, ForumDbContext DbContext) => {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await DbContext.Posts.FindAsync(postId);

                if (post == null)
                    return Results.NotFound();

                var comment = await DbContext.Comments
                    .Where(comment => comment.Post == post && comment.Id == commentId)
                    .FirstOrDefaultAsync();

                if (comment == null)
                    return Results.NotFound();

                return Results.Ok(comment.ToDto());
            });

            CommentsGroup.MapPost("/comments", async (CreateCommentDTO dto, int topicId, int postId, ForumDbContext DbContext) =>
            {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await DbContext.Posts.FindAsync(postId);

                if (post == null)
                    return Results.NotFound();

                var comment = new Comment { Content = dto.Content, CreatedAt = DateTime.UtcNow, IsDeleted = false, Post = post };
                DbContext.Comments.Add(comment);

                await DbContext.SaveChangesAsync();

                return Results.Created($"api/topics/{topic.Id}/posts/{post.Id}/comments/{comment.Id}", comment.ToDto());
            });

            CommentsGroup.MapPut("/comments/{commentId}", async (UpdateCommentDTO dto, int topicId, int postId, int commentId, ForumDbContext DbContext) => {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await DbContext.Posts.FindAsync(postId);

                if (post == null)
                    return Results.NotFound();

                var comment = await DbContext.Comments
                    .Where(comment => comment.Post == post && comment.Id == commentId)
                    .FirstOrDefaultAsync();

                if (comment == null)
                    return Results.NotFound();

                comment.Content = dto.Content;
                DbContext.Update(comment);
                await DbContext.SaveChangesAsync();

                return Results.Ok(comment.ToDto());
            });

            CommentsGroup.MapDelete("/comments/{commentId}", async (int topicId, int postId, int commentId, ForumDbContext DbContext) => {
                var topic = await DbContext.Topics.FindAsync(topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await DbContext.Posts.FindAsync(postId);

                if (post == null)
                    return Results.NotFound();

                var comment = await DbContext.Comments
                    .Where(comment => comment.Post == post && comment.Id == commentId)
                    .FirstOrDefaultAsync();

                if (comment == null)
                    return Results.NotFound();

                DbContext.Comments.Remove(comment);
                await DbContext.SaveChangesAsync();

                return Results.NoContent();
            });
        }
    }
}
