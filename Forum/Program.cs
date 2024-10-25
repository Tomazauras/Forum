using FluentValidation;
using FluentValidation.Results;
using Forum;
using Forum.Data;
using Forum.Data.Entities;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;
using SharpGrip.FluentValidation.AutoValidation.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ForumDbContext>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation(configuration =>
{
    configuration.OverrideDefaultResultFactoryWith<ProblemDetailsResultFactory>();
});
var app = builder.Build();

/*
 /api/v1/topics GET List 200
 /api/v1/topics POST Create 201
 /api/v1/topics{id} GET One 200
 /api/v1/topics{id} PUT Modify 200
 /api/v1/topics{id} DELETE Remove 200 content / 204 no content
 */

app.AddTopicApi();
app.AddPostsApi();
app.AddCommentsApi();

app.Run();


public class ProblemDetailsResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IResult CreateResult(EndpointFilterInvocationContext context, ValidationResult validationResult)
    {
        var problemDetails = new HttpValidationProblemDetails(validationResult.ToValidationProblemErrors())
        {
            Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
            Title = "Unprocessed entity",
            Status = 422
        };
        return TypedResults.Problem(problemDetails);
    }
}