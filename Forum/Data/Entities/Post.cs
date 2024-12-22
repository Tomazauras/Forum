using FluentValidation;

namespace Forum.Data.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Body { get; set; }
        public required DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public Topic Topic { get; set; }

        public PostDTO ToDto()
        {
            return new PostDTO(Id, Title, Body, CreatedAt);
        }

    }
    public record PostDTO(int Id, string Title, string Body, DateTime CreatedAt);
    public record CreatePostDTO(string Title, string Body)
    {
        public class CreatePostDTOValidator : AbstractValidator<CreatePostDTO>
        {
            public CreatePostDTOValidator()
            {
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.Body).NotEmpty();
            }
        }
    };
    public record UpdatePostDTO(string Body)
    {
        public class UpdatePostDTOValidator : AbstractValidator<UpdatePostDTO>
        {
            public UpdatePostDTOValidator()
            {
                RuleFor(x => x.Body).NotEmpty();
            }
        }
    };

    public record GetPostsParameters(int topicId, ForumDbContext DbContext, LinkGenerator linkGenerator, HttpContext httpContext);
    public record GetPostParameters(int topicId, int postId, ForumDbContext DbContext, LinkGenerator linkGenerator, HttpContext httpContext);
    public record CreatePostParameters(CreatePostDTO dto, int topicId, ForumDbContext DbContext, LinkGenerator linkGenerator, HttpContext httpContext);
    public record UpdatePostParameters(UpdatePostDTO dto, int topicId, int postId, ForumDbContext DbContext, LinkGenerator linkGenerator, HttpContext httpContext);
    public record DeletePostParameters(int topicId, int postId, ForumDbContext DbContext);
}
