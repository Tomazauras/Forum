using FluentValidation;
using Forum.Auth.Model;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace Forum.Data.Entities
{
    public class Topic
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime CreatedAt { get; set; }

        // Only can be set/seen by admin
        public bool IsDeleted { get; set; }

        [Required]
        public required string UserId {  get; set; }
        public ForumUser User {  get; set; }

        public TopicDTO ToDto()
        {
            return new TopicDTO(Id, Title, Description, CreatedAt);
        }
    }
    public record TopicDTO(int Id, string Title, string Description, DateTime CreatedAt);
    public record CreateTopicDTO(string Title, string Description) 
    { 
        public class CreateTopicDTOValidator : AbstractValidator<CreateTopicDTO>
        {
            public CreateTopicDTOValidator()
            {
                RuleFor(x => x.Title).NotEmpty().Length(min: 2, max: 100).Matches("[a-zA-Z0-9]+$");
                RuleFor(x => x.Description).NotEmpty().Length(min: 5, max: 400).Matches("[a-zA-Z0-9]+$");
            }
        }
    };
    public record UpdateTopicDTO(string Description) 
    {
        public class UpdateTopicDTOValidator : AbstractValidator<UpdateTopicDTO>
        {
            public UpdateTopicDTOValidator()
            {
                RuleFor(x => x.Description).NotEmpty().Length(min: 5, max: 400).Matches("[a-zA-Z0-9]+$");
            }
        }
    };

    public record GetTopicsParameters(ForumDbContext DbContext, LinkGenerator linkGenerator, HttpContext httpContext);
    public record GetTopicParameters(int topicId, ForumDbContext DbContext, LinkGenerator linkGenerator, HttpContext httpContext);
    public record CreateTopicParameters(CreateTopicDTO dto, ForumDbContext DbContext, LinkGenerator linkGenerator, HttpContext httpContext);
    public record UpdateTopicParameters(UpdateTopicDTO dto, int topicId, ForumDbContext DbContext, LinkGenerator linkGenerator, HttpContext httpContext);
    public record DeleteTopicParameters(int topicId, ForumDbContext DbContext, HttpContext httpContext);
}
