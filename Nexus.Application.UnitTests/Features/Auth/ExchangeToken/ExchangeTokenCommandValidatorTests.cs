using FluentValidation.TestHelper;
using Nexus.Application.Features.Auth.ExchangeToken;

namespace Nexus.Application.UnitTests.Features.Auth.ExchangeToken;

public class ExchangeTokenCommandValidatorTests
{
    private readonly ExchangeTokenCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_AccessTokenIsEmpty()
    {
        // Arrange
        var command = new ExchangeTokenCommand(string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessToken)
            .WithErrorMessage("Access token is required.");
    }

    [Fact]
    public void Should_HaveError_When_AccessTokenIsNull()
    {
        // Arrange
        var command = new ExchangeTokenCommand(null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessToken)
            .WithErrorMessage("Access token is required.");
    }

    [Fact]
    public void Should_HaveError_When_AccessTokenIsWhitespace()
    {
        // Arrange
        var command = new ExchangeTokenCommand("   ");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessToken)
            .WithErrorMessage("Access token is required.");
    }

    [Fact]
    public void Should_NotHaveError_When_AccessTokenIsValid()
    {
        // Arrange
        var command = new ExchangeTokenCommand("valid-access-token");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AccessToken);
    }
}

