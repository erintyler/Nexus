using AutoFixture;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Domain.UnitTests.Entities;

public class CollectionTests
{
    private readonly Fixture _fixture = new();

    #region Create Tests

    [Fact]
    public void Create_ShouldReturnSuccess_WhenAllParametersAreValid()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = _fixture.CreateString(50);

        // Act
        var result = Collection.Create(userId, title);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(title, result.Value.Title);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty;
        var title = _fixture.CreateString(50);

        // Act
        var result = Collection.Create(userId, title);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CollectionErrors.UserIdEmpty, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = string.Empty;

        // Act
        var result = Collection.Create(userId, title);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CollectionErrors.TitleEmpty, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsWhitespace()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = "   ";

        // Act
        var result = Collection.Create(userId, title);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CollectionErrors.TitleEmpty, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsTooShort()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = _fixture.CreateString(Collection.MinTitleLength - 1);

        // Act
        var result = Collection.Create(userId, title);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CollectionErrors.TitleTooShort, result.Errors);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsTooLong()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var title = _fixture.CreateString(Collection.MaxTitleLength + 1);

        // Act
        var result = Collection.Create(userId, title);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CollectionErrors.TitleTooLong, result.Errors);
    }

    #endregion

    #region AddImagePost Tests

    [Fact]
    public void AddImagePost_ShouldReturnSuccess_WhenImagePostIdIsValid()
    {
        // Arrange
        var collection = CreateCollection();
        var imagePostId = _fixture.Create<Guid>();

        // Act
        var result = collection.AddImagePost(imagePostId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(imagePostId, result.Value.ImagePostId);
    }

    [Fact]
    public void AddImagePost_ShouldReturnFailure_WhenImagePostIdIsEmpty()
    {
        // Arrange
        var collection = CreateCollection();
        var imagePostId = Guid.Empty;

        // Act
        var result = collection.AddImagePost(imagePostId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(ImagePostErrors.NotFound, result.Errors);
    }

    [Fact]
    public void AddImagePost_ShouldReturnFailure_WhenImagePostAlreadyExists()
    {
        // Arrange
        var collection = CreateCollection();
        var imagePostId = _fixture.Create<Guid>();
        
        var addEvent = collection.AddImagePost(imagePostId).Value;
        collection.Apply(addEvent);

        // Act
        var result = collection.AddImagePost(imagePostId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CollectionErrors.ImagePostAlreadyExists, result.Errors);
    }

    [Fact]
    public void Apply_ImagePostAddedToCollectionDomainEvent_ShouldAddImagePostId()
    {
        // Arrange
        var collection = CreateCollection();
        var imagePostId = _fixture.Create<Guid>();
        var addEvent = collection.AddImagePost(imagePostId).Value;

        // Act
        collection.Apply(addEvent);

        // Assert
        Assert.Contains(imagePostId, collection.ImagePostIds);
        Assert.Single(collection.ImagePostIds);
    }

    #endregion

    #region RemoveImagePost Tests

    [Fact]
    public void RemoveImagePost_ShouldReturnSuccess_WhenImagePostExistsInCollection()
    {
        // Arrange
        var collection = CreateCollection();
        var imagePostId = _fixture.Create<Guid>();
        
        var addEvent = collection.AddImagePost(imagePostId).Value;
        collection.Apply(addEvent);

        // Act
        var result = collection.RemoveImagePost(imagePostId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(imagePostId, result.Value.ImagePostId);
    }

    [Fact]
    public void RemoveImagePost_ShouldReturnFailure_WhenImagePostNotInCollection()
    {
        // Arrange
        var collection = CreateCollection();
        var imagePostId = _fixture.Create<Guid>();

        // Act
        var result = collection.RemoveImagePost(imagePostId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(CollectionErrors.ImagePostNotFound, result.Errors);
    }

    [Fact]
    public void Apply_ImagePostRemovedFromCollectionDomainEvent_ShouldRemoveImagePostId()
    {
        // Arrange
        var collection = CreateCollection();
        var imagePostId = _fixture.Create<Guid>();
        
        var addEvent = collection.AddImagePost(imagePostId).Value;
        collection.Apply(addEvent);
        
        var removeEvent = collection.RemoveImagePost(imagePostId).Value;

        // Act
        collection.Apply(removeEvent);

        // Assert
        Assert.DoesNotContain(imagePostId, collection.ImagePostIds);
        Assert.Empty(collection.ImagePostIds);
    }

    #endregion

    #region Helper Methods

    private Collection CreateCollection(Guid? userId = null, string? title = null)
    {
        var collection = new Collection();
        var createdEvent = Collection.Create(
            userId ?? _fixture.Create<Guid>(),
            title ?? _fixture.CreateString(50)).Value;
        
        collection.Apply(createdEvent);
        return collection;
    }

    #endregion
}
