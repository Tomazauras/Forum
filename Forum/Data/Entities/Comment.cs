using FluentValidation;

namespace Forum.Data.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public required DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public Post Post { get; set; }

        public CommentDTO ToDto()
        {
            return new CommentDTO(Id, Content, CreatedAt);
        }
    }
    public record CommentDTO(int Id, string Content, DateTime CreatedAt);
    public record CreateCommentDTO(string Content)
    {
        public class CreateCommentDTOValidator : AbstractValidator<CreateCommentDTO>
        {
            public CreateCommentDTOValidator()
            {
                RuleFor(x => x.Content).NotEmpty();
            }
        }
    };
    public record UpdateCommentDTO(string Content)
    {
        public class UpdateCommentDTOValidator : AbstractValidator<UpdateCommentDTO>
        {
            public UpdateCommentDTOValidator()
            {
                RuleFor(x => x.Content).NotEmpty();
            }
        }
    };
}
