using FluentValidation.TestHelper;
using MyLibrary.Application.DTOs.Auth;
using MyLibrary.Application.Validators;

namespace MyLibrary.Tests.Application.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest ValidRequest() => new("Jane Doe", "jane@example.com", "S3curePassword!");

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_HasErrorForName(string name)
    {
        var request = ValidRequest() with { Name = name };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_HasErrorForEmail(string email)
    {
        var request = ValidRequest() with { Email = email };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Validate_WithInvalidPassword_HasErrorForPassword(string password)
    {
        var request = ValidRequest() with { Password = password };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
