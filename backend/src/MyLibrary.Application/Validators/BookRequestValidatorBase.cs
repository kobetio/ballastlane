using FluentValidation;
using MyLibrary.Application.DTOs.Books;

namespace MyLibrary.Application.Validators;

/// <summary>
/// Shared business rules for creating/updating a book (Specification.md §4).
/// </summary>
public abstract class BookRequestValidatorBase<T> : AbstractValidator<T> where T : IBookRequest
{
    protected BookRequestValidatorBase()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(150).WithMessage("Title must not exceed 150 characters.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(100).WithMessage("Author must not exceed 100 characters.");

        RuleFor(x => x.Genre)
            .MaximumLength(50).WithMessage("Genre must not exceed 50 characters.")
            .When(x => x.Genre is not null);

        RuleFor(x => x.PublicationYear)
            .InclusiveBetween(1450, DateTime.UtcNow.Year)
            .WithMessage($"Publication year must be between 1450 and {DateTime.UtcNow.Year}.")
            .When(x => x.PublicationYear.HasValue);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);
    }
}
