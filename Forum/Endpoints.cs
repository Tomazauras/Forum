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

                var resources = pagedList.Select(topic =>
                {
                    var links = CreateLinksForSingleTopic(topic.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                    return new ResourceDto<TopicDTO>(topic.ToDto(), links);
                }).ToArray();

                var links = CreateLinksForTopics(parameters.linkGenerator, parameters.httpContext,
                    pagedList.GetPreviousPageLink(parameters.linkGenerator,parameters.httpContext, "GetTopics"),
                    pagedList.GetNextPageLink(parameters.linkGenerator, parameters.httpContext, "GetTopics"))
                .ToArray();


                // per header, Pagination: {}
                // "Pagination":
                //var paginationMetadata = pagedList.CreatePaginationMetadata(parameters.linkGenerator, parameters.httpContext, "GetTopics");
                //parameters.httpContext.Response.Headers.Append("Pagination", JsonSerializer.Serialize(paginationMetadata));

                return Results.Ok(new ResourceDto<ResourceDto<TopicDTO>[]>(resources, links));
            }).WithName("GetTopics");

            topicsGroup.MapGet("/topics/{topicId}", async ([AsParameters] GetTopicParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var links = CreateLinksForSingleTopic(topic.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                var topicDto = topic.ToDto();
                var resource = new ResourceDto<TopicDTO>(topicDto, links);

                return Results.Ok(resource);
            }).WithName("GetTopic");

            topicsGroup.MapPost("/topics", async ([AsParameters] CreateTopicParameters parameters) => {
                var topic = new Topic { Title = parameters.dto.Title, Description = parameters.dto.Description, CreatedAt = DateTime.UtcNow, IsDeleted = false };
                parameters.DbContext.Topics.Add(topic);

                await parameters.DbContext.SaveChangesAsync();

                var links = CreateLinksForSingleTopic(topic.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                var topicDto = topic.ToDto();
                var resource = new ResourceDto<TopicDTO>(topicDto, links);

                return Results.Created(links[0].Href, resource);
            }).WithName("CreateTopic");

            topicsGroup.MapPut("/topics/{topicId}", async ([AsParameters] UpdateTopicParameters parameters) => {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                topic.Description = parameters.dto.Description;

                parameters.DbContext.Topics.Update(topic);
                await parameters.DbContext.SaveChangesAsync();

                var links = CreateLinksForSingleTopic(topic.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                var topicDto = topic.ToDto();
                var resource = new ResourceDto<TopicDTO>(topicDto, links);

                return Results.Ok(resource);
            }).WithName("UpdateTopic");

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
            }).WithName("DeleteTopic");
        }

        public static void AddPostsApi(this WebApplication app)
        {
            var postsGroup = app.MapGroup("/api/topics/{topicId}").AddFluentValidationAutoValidation();
            postsGroup.MapGet("/posts", async ([AsParameters] GetPostsParameters parameters, [AsParameters] SearchParameters searchParameters) =>
            {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var queryable = parameters.DbContext.Posts
                    .Where(post => post.Topic.Id == topic.Id)
                    .OrderBy(post => post.CreatedAt);

                var pagedList = await PagedList<Post>.CreateAsync(queryable, searchParameters.PageNumber!.Value, searchParameters.PageSize!.Value);

                if (!pagedList.Any())
                    return Results.NotFound();

                var resources = pagedList.Select(post =>
                {
                    var links = CreateLinksForSinglePost(topic.Id, post.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                    return new ResourceDto<PostDTO>(post.ToDto(), links);
                }).ToArray();

                var links = CreateLinksForPosts(topic.Id, parameters.linkGenerator, parameters.httpContext,
                    pagedList.GetPreviousPageLink(parameters.linkGenerator, parameters.httpContext, "GetPosts", new { topicId = topic.Id }),
                    pagedList.GetNextPageLink(parameters.linkGenerator, parameters.httpContext, "GetPosts", new { topicId = topic.Id }))
                .ToArray();

                return Results.Ok(new ResourceDto<ResourceDto<PostDTO>[]>(resources, links));
            }).WithName("GetPosts");

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

                var links = CreateLinksForSinglePost(topic.Id, post.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                var postDto = post.ToDto();
                var resource = new ResourceDto<PostDTO>(postDto, links);

                return Results.Ok(resource);
            }).WithName("GetPost");

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

                var links = CreateLinksForSinglePost(topic.Id, post.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                var postDto = post.ToDto();
                var resource = new ResourceDto<PostDTO>(postDto, links);

                return Results.Created(links[0].Href, resource);
            }).WithName("CreatePost");

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

                var links = CreateLinksForSinglePost(topic.Id, post.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                var postDto = post.ToDto();
                var resource = new ResourceDto<PostDTO>(postDto, links);

                return Results.Ok(resource);
            }).WithName("UpdatePost");

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
            }).WithName("DeletePost");
        }

        public static void AddCommentsApi(this WebApplication app)
        {
            var CommentsGroup = app.MapGroup("/api/topics/{topicId}/posts/{postId}").AddFluentValidationAutoValidation();
            CommentsGroup.MapGet("/comments", async ([AsParameters] GetCommentsParameters parameters, [AsParameters] SearchParameters searchParameters) =>
            {
                var topic = await parameters.DbContext.Topics.FindAsync(parameters.topicId);

                if (topic == null)
                    return Results.NotFound();

                var post = await parameters.DbContext.Posts.FindAsync(parameters.postId);

                if (post == null)
                    return Results.NotFound();

                var queryable = parameters.DbContext.Comments
                    .Where(comment => comment.Post.Id == post.Id)
                    .OrderBy(comment => comment.CreatedAt);

                var pagedList = await PagedList<Comment>.CreateAsync(queryable, searchParameters.PageNumber!.Value, searchParameters.PageSize!.Value);

                if (!pagedList.Any())
                    return Results.NotFound();

                var resources = pagedList.Select(comment =>
                {
                    var links = CreateLinksForSingleComment(topic.Id, post.Id, comment.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                    return new ResourceDto<CommentDTO>(comment.ToDto(), links);
                }).ToArray();

                var links = CreateLinksForComments(topic.Id, post.Id, 
                    parameters.linkGenerator, parameters.httpContext,
                    pagedList.GetPreviousPageLink(parameters.linkGenerator, parameters.httpContext, "GetComments", new { topicId = topic.Id, postId = post.Id }),
                    pagedList.GetNextPageLink(parameters.linkGenerator, parameters.httpContext, "GetComments", new { topicId = topic.Id, postId = post.Id })
                ).ToArray();

                return Results.Ok(new ResourceDto<ResourceDto<CommentDTO>[]>(resources, links));
            }).WithName("GetComments");

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

                var links = CreateLinksForSingleComment(topic.Id, post.Id, comment.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                var commentDto = comment.ToDto();
                var resource = new ResourceDto<CommentDTO>(commentDto, links);

                return Results.Ok(resource);
            }).WithName("GetComment");

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

                var links = CreateLinksForSingleComment(topic.Id, post.Id, comment.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                var commentDto = comment.ToDto();
                var resource = new ResourceDto<CommentDTO>(commentDto, links);

                return Results.Created(links[0].Href, resource);
            }).WithName("CreateComment");

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

                var links = CreateLinksForSingleComment(topic.Id, post.Id, comment.Id, parameters.linkGenerator, parameters.httpContext).ToArray();
                var commentDto = comment.ToDto();
                var resource = new ResourceDto<CommentDTO>(commentDto, links);

                return Results.Ok(resource);
            }).WithName("UpdateComment");

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
            }).WithName("DeleteComment");
        }


        static IEnumerable<LinkDto> CreateLinksForSingleTopic(int topicId, LinkGenerator linkGenerator, HttpContext httpContext)
        {
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetTopic", new { topicId = topicId }), "self", "GET");
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "UpdateTopic", new { topicId = topicId }), "edit", "PUT");
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "DeleteTopic", new { topicId = topicId }), "remove", "DELETE");
        }

        static IEnumerable<LinkDto> CreateLinksForTopics(LinkGenerator linkGenerator, HttpContext httpContext, string? previousPageLink, string? nextPageLink)
        {
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetTopics"), "self", "GET");

            if (previousPageLink != null)
            {
                yield return new LinkDto(previousPageLink, "previousPage", "GET");
            }

            if (nextPageLink != null)
            {
                yield return new LinkDto(nextPageLink, "nextPage", "GET");
            }
        }

        static IEnumerable<LinkDto> CreateLinksForSinglePost(int topicId, int postId, LinkGenerator linkGenerator, HttpContext httpContext)
        {
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetPost", new { topicId = topicId, postId = postId }), "self", "GET");
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "UpdatePost", new { topicId = topicId, postId = postId }), "edit", "PUT");
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "DeletePost", new { topicId = topicId, postId = postId }), "remove", "DELETE");
        }

        static IEnumerable<LinkDto> CreateLinksForPosts(int topicId, LinkGenerator linkGenerator, HttpContext httpContext, string? previousPageLink, string? nextPageLink)
        {
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetPosts", new { topicId = topicId }), "self", "GET");

            if (previousPageLink != null)
            {
                yield return new LinkDto(previousPageLink, "previousPage", "GET");
            }

            if (nextPageLink != null)
            {
                yield return new LinkDto(nextPageLink, "nextPage", "GET");
            }
        }

        static IEnumerable<LinkDto> CreateLinksForSingleComment(int topicId, int postId, int commentId, LinkGenerator linkGenerator, HttpContext httpContext)
        {
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetComment", new { topicId = topicId, postId = postId, commentId = commentId }), "self", "GET");
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "UpdateComment", new { topicId = topicId, postId = postId, commentId = commentId }), "edit", "PUT");
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "DeleteComment", new { topicId = topicId, postId = postId, commentId = commentId }), "remove", "DELETE");
        }

        static IEnumerable<LinkDto> CreateLinksForComments(int topicId, int postId, LinkGenerator linkGenerator, HttpContext httpContext, string? previousPageLink, string? nextPageLink)
        {
            yield return new LinkDto(linkGenerator.GetUriByName(httpContext, "GetPosts", new { topicId = topicId, postId = postId }), "self", "GET");

            if (previousPageLink != null)
            {
                yield return new LinkDto(previousPageLink, "previousPage", "GET");
            }

            if (nextPageLink != null)
            {
                yield return new LinkDto(nextPageLink, "nextPage", "GET");
            }
        }

    }
}
