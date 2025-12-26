using AutoFixture;
using Marten;
using Moq;
using Nexus.Application.Features.Collections.GetCollectionById;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Domain.Errors;

namespace Nexus.Application.UnitTests.Features.Collections.GetCollectionById;

public class GetCollectionByIdQueryHandlerTests
{
    private readonly Mock<IDocumentSession> _mockSession = new();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task HandleAsync_ShouldReturnCollection_WhenCollectionExists()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var expectedCollection = new CollectionReadModel
        {
            Id = collectionId,
            Title = "Test Collection",
            CreatedBy = Guid.NewGuid().ToString(),
            ImagePostIds = [Guid.NewGuid(), Guid.NewGuid()],
            AggregatedTags = []
        };

        _mockSession
            .Setup(s => s.LoadAsync<CollectionReadModel>(
                collectionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCollection);

        var query = new GetCollectionByIdQuery(collectionId);

        // Act
        var result = await GetCollectionByIdQueryHandler.HandleAsync(
            query,
            _mockSession.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(collectionId, result.Value.Id);
        Assert.Equal(expectedCollection.Title, result.Value.Title);
        Assert.Equal(2, result.Value.ImagePostIds.Count);

        _mockSession.Verify(s => s.LoadAsync<CollectionReadModel>(
            collectionId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenCollectionNotFound()
    {
        // Arrange
        var collectionId = Guid.NewGuid();

        _mockSession
            .Setup(s => s.LoadAsync<CollectionReadModel>(
                collectionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CollectionReadModel?)null);

        var query = new GetCollectionByIdQuery(collectionId);

        // Act
        var result = await GetCollectionByIdQueryHandler.HandleAsync(
            query,
            _mockSession.Object,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CollectionErrors.NotFound, result.Errors);

        _mockSession.Verify(s => s.LoadAsync<CollectionReadModel>(
            collectionId,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
