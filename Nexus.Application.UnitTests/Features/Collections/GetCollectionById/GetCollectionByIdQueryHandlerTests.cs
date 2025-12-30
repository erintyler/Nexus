using AutoFixture;
using Nexus.Application.Features.Collections.GetCollectionById;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Domain.Errors;

namespace Nexus.Application.UnitTests.Features.Collections.GetCollectionById;

public class GetCollectionByIdQueryHandlerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Handle_ShouldReturnCollection_WhenCollectionExists()
    {
        // Arrange
        var collectionId = _fixture.Create<Guid>();
        var expectedCollection = new CollectionReadModel
        {
            Id = collectionId,
            Title = "Test Collection",
            CreatedBy = _fixture.Create<Guid>().ToString(),
            ImagePostIds = [_fixture.Create<Guid>(), _fixture.Create<Guid>()],
            AggregatedTags = []
        };

        var query = new GetCollectionByIdQuery(collectionId);

        // Act
        var result = GetCollectionByIdQueryHandler.Handle(
            query,
            expectedCollection);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(collectionId, result.Value.Id);
        Assert.Equal(expectedCollection.Title, result.Value.Title);
        Assert.Equal(2, result.Value.ImagePostIds.Count);
    }

    [Fact]
    public void Handle_ShouldReturnFailure_WhenCollectionNotFound()
    {
        // Arrange
        var collectionId = _fixture.Create<Guid>();
        var query = new GetCollectionByIdQuery(collectionId);

        // Act
        var result = GetCollectionByIdQueryHandler.Handle(
            query,
            null);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CollectionErrors.NotFound, result.Errors);
    }
}
