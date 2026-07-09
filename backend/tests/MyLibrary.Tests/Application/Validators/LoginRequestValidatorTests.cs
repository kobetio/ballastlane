using FluentValidation.TestHelper;
using MyLibrary.Application.DTOs.Auth;
using MyLibrary.Application.Validators;

namespace MyLibrary.Tests.Application.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(new LoginRequest("jane@example.com", "S3curePassword!"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_HasErrorForEmail(string email)
    {
        var result = _validator.TestValidate(new LoginRequest(email, "S3curePassword!"));

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyPassword_HasErrorForPassword()
    {
        var result = _validator.TestValidate(new LoginRequest("jane@example.com", ""));

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
