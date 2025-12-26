using AutoFixture;
using Nexus.Application.Features.Collections.RemoveImagePostFromCollection;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Application.UnitTests.Features.Collections.RemoveImagePostFromCollection;

public class RemoveImagePostFromCollectionCommandHandlerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void HandleAsync_ShouldRemoveImagePost_WhenImagePostExistsInCollection()
    {
        // Arrange
        var collectionId = _fixture.Create<Guid>();
        var imagePostId = _fixture.Create<Guid>();
        var collection = CreateCollection(collectionId);

        // Add the image post first
        var addEvent = collection.AddImagePost(imagePostId).Value;
        collection.Apply(addEvent);

        var command = new RemoveImagePostFromCollectionCommand(collectionId, imagePostId);

        // Act
        var (result, events) = RemoveImagePostFromCollectionCommandHandler.HandleAsync(
            command,
            collection);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(events);
    }

    [Fact]
    public void HandleAsync_ShouldReturnFailure_WhenImagePostNotInCollection()
    {
        // Arrange
        var collectionId = _fixture.Create<Guid>();
        var imagePostId = _fixture.Create<Guid>();
        var collection = CreateCollection(collectionId);

        var command = new RemoveImagePostFromCollectionCommand(collectionId, imagePostId);

        // Act
        var (result, events) = RemoveImagePostFromCollectionCommandHandler.HandleAsync(
            command,
            collection);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Empty(events);
        Assert.Contains(CollectionErrors.ImagePostNotFound, result.Errors);
    }

    private Collection CreateCollection(Guid? id = null)
    {
        var collectionId = id ?? _fixture.Create<Guid>();
        var collection = new Collection(collectionId);
        var createEvent = Collection.Create(_fixture.Create<Guid>(), _fixture.CreateString(20)).Value;
        collection.Apply(createEvent);
        return collection;
    }
}
