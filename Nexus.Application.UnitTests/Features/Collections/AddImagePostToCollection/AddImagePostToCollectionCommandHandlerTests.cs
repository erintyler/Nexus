using AutoFixture;
using Nexus.Application.Features.Collections.AddImagePostToCollection;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Application.UnitTests.Features.Collections.AddImagePostToCollection;

public class AddImagePostToCollectionCommandHandlerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void HandleAsync_ShouldAddImagePost_WhenCollectionIsValid()
    {
        // Arrange
        var collectionId = _fixture.Create<Guid>();
        var imagePostId = _fixture.Create<Guid>();
        var collection = CreateCollection(collectionId);

        var command = new AddImagePostToCollectionCommand(collectionId, imagePostId);

        // Act
        var (result, events) = AddImagePostToCollectionCommandHandler.HandleAsync(
            command,
            collection);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(events);
    }

    [Fact]
    public void HandleAsync_ShouldReturnFailure_WhenImagePostAlreadyExists()
    {
        // Arrange
        var collectionId = _fixture.Create<Guid>();
        var imagePostId = _fixture.Create<Guid>();
        var collection = CreateCollection(collectionId);
        
        // Add the image post first
        var addEvent = collection.AddImagePost(imagePostId).Value;
        collection.Apply(addEvent);

        var command = new AddImagePostToCollectionCommand(collectionId, imagePostId);

        // Act
        var (result, events) = AddImagePostToCollectionCommandHandler.HandleAsync(
            command,
            collection);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Empty(events);
        Assert.Contains(CollectionErrors.ImagePostAlreadyExists, result.Errors);
    }

    [Fact]
    public void HandleAsync_ShouldReturnFailure_WhenImagePostIdIsEmpty()
    {
        // Arrange
        var collectionId = _fixture.Create<Guid>();
        var collection = CreateCollection(collectionId);

        var command = new AddImagePostToCollectionCommand(collectionId, Guid.Empty);

        // Act
        var (result, events) = AddImagePostToCollectionCommandHandler.HandleAsync(
            command,
            collection);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Empty(events);
        Assert.Contains("Collection.ImagePostId.Empty", result.Errors.Select(e => e.Code));
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
