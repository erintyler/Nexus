using AutoFixture;
using Moq;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.Collections.CreateCollection;
using Nexus.Domain.Errors;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Application.UnitTests.Features.Collections.CreateCollection;

public class CreateCollectionCommandHandlerTests
{
    private readonly Mock<IUserContextService> _mockUserContextService = new();
    private readonly Fixture _fixture = new();

    public CreateCollectionCommandHandlerTests()
    {
        // Setup default user context
        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(_fixture.Create<Guid>());
    }

    [Fact]
    public void HandleAsync_ShouldCreateCollection_WhenCommandIsValid()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        _mockUserContextService
            .Setup(s => s.GetUserId())
            .Returns(userId);

        var command = new CreateCollectionCommand(_fixture.CreateString(20));

        // Act
        var (result, stream) = CreateCollectionCommandHandler.HandleAsync(
            command,
            _mockUserContextService.Object);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(stream);

        var response = result.Value;
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(command.Title, response.Title);
        Assert.InRange(response.CreatedAt, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));

        _mockUserContextService.Verify(s => s.GetUserId(), Times.Once);
    }

    [Fact]
    public void HandleAsync_ShouldReturnFailure_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateCollectionCommand(string.Empty);

        // Act
        var (result, stream) = CreateCollectionCommandHandler.HandleAsync(
            command,
            _mockUserContextService.Object);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Null(stream);
        Assert.Contains(CollectionErrors.TitleEmpty, result.Errors);
    }

    [Fact]
    public void HandleAsync_ShouldReturnFailure_WhenTitleIsTooShort()
    {
        // Arrange
        var command = new CreateCollectionCommand(_fixture.CreateString(4));

        // Act
        var (result, stream) = CreateCollectionCommandHandler.HandleAsync(
            command,
            _mockUserContextService.Object);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Null(stream);
        Assert.Contains(CollectionErrors.TitleTooShort, result.Errors);
    }

    [Fact]
    public void HandleAsync_ShouldReturnFailure_WhenTitleIsTooLong()
    {
        // Arrange
        var command = new CreateCollectionCommand(_fixture.CreateString(201));

        // Act
        var (result, stream) = CreateCollectionCommandHandler.HandleAsync(
            command,
            _mockUserContextService.Object);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Null(stream);
        Assert.Contains(CollectionErrors.TitleTooLong, result.Errors);
    }
}
