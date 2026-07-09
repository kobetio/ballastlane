using FluentValidation.TestHelper;
using MyLibrary.Application.DTOs.Books;
using MyLibrary.Application.Validators;
using MyLibrary.Domain.Enums;

namespace MyLibrary.Tests.Application.Validators;

public class BookUpdateRequestValidatorTests
{
    private readonly BookUpdateRequestValidator _validator = new();

    private static BookUpdateRequest ValidRequest() => new(
        Title: "Clean Code",
        Author: "Robert C. Martin",
        Genre: "Software Engineering",
        PublicationYear: 2008,
        ReadingStatus: ReadingStatus.Read,
        Rating: 5,
        Notes: "A must-read.");

    [Fact]
    public void Validate_WithFullyValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTitle_HasErrorForTitle()
    {
        var request = ValidRequest() with { Title = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithTitleOver150Characters_HasErrorForTitle()
    {
        var request = ValidRequest() with { Title = new string('a', 151) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithEmptyAuthor_HasErrorForAuthor()
    {
        var request = ValidRequest() with { Author = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Fact]
    public void Validate_WithAuthorOver100Characters_HasErrorForAuthor()
    {
        var request = ValidRequest() with { Author = new string('a', 101) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Fact]
    public void Validate_WithGenreOver50Characters_HasErrorForGenre()
    {
        var request = ValidRequest() with { Genre = new string('a', 51) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Genre);
    }

    [Fact]
    public void Validate_WithPublicationYearBefore1450_HasErrorForPublicationYear()
    {
        var request = ValidRequest() with { PublicationYear = 1449 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PublicationYear);
    }

    [Fact]
    public void Validate_WithPublicationYearInTheFuture_HasErrorForPublicationYear()
    {
        var request = ValidRequest() with { PublicationYear = DateTime.UtcNow.Year + 1 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PublicationYear);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Validate_WithRatingOutsideOneToFive_HasErrorForRating(int rating)
    {
        var request = ValidRequest() with { Rating = rating };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Validate_WithNotesOver1000Characters_HasErrorForNotes()
    {
        var request = ValidRequest() with { Notes = new string('a', 1001) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}
