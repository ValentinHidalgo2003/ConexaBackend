using Conexa.Application.DTOs.Movies;
using FluentValidation;

namespace Conexa.Application.Validators;

public class CreateMovieRequestValidator : AbstractValidator<CreateMovieRequest>
{
    public CreateMovieRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EpisodeId).GreaterThan(0);
        RuleFor(x => x.Director).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Producer).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OpeningCrawl).NotEmpty();
    }
}

public class UpdateMovieRequestValidator : AbstractValidator<UpdateMovieRequest>
{
    public UpdateMovieRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EpisodeId).GreaterThan(0);
        RuleFor(x => x.Director).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Producer).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OpeningCrawl).NotEmpty();
    }
}
