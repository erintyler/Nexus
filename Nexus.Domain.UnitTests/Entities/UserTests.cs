using AutoFixture;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Domain.UnitTests.Entities;

public class UserTests
{
    private readonly Fixture _fixture = new();

    #region Create Tests

    [Fact]
    public void Create_ShouldReturnSuccess_WhenAllParametersAreValid()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var discordUsername = _fixture.Create<string>();

        // Act
        var result = User.Create(discordId, discordUsername);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(discordId, result.Value.DiscordId);
        Assert.Equal(discordUsername, result.Value.DiscordUsername);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiscordIdIsEmpty()
    {
        // Arrange
        var discordId = string.Empty;
        var discordUsername = _fixture.Create<string>();

        // Act
        var result = User.Create(discordId, discordUsername);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(UserErrors.DiscordIdEmpty, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiscordIdIsWhitespace()
    {
        // Arrange
        var discordId = "   ";
        var discordUsername = _fixture.Create<string>();

        // Act
        var result = User.Create(discordId, discordUsername);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(UserErrors.DiscordIdEmpty, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiscordUsernameIsEmpty()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var discordUsername = string.Empty;

        // Act
        var result = User.Create(discordId, discordUsername);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(UserErrors.DiscordUsernameEmpty, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDiscordUsernameIsWhitespace()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var discordUsername = "   ";

        // Act
        var result = User.Create(discordId, discordUsername);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(UserErrors.DiscordUsernameEmpty, result.Errors);
    }

    [Fact]
    public void Apply_UserCreatedDomainEvent_ShouldSetProperties()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var discordUsername = _fixture.Create<string>();
        var user = new User();
        var createdEvent = User.Create(discordId, discordUsername).Value;

        // Act
        user.Apply(createdEvent);

        // Assert
        Assert.Equal(discordId, user.DiscordId);
        Assert.Equal(discordUsername, user.DiscordUsername);
    }

    #endregion
}
