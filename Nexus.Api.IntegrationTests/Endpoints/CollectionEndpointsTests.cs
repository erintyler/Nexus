using System.Net;
using System.Net.Http.Json;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Application.Features.Collections.CreateCollection;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Domain.Enums;

namespace Nexus.Api.IntegrationTests.Endpoints;

public class CollectionEndpointsTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public CollectionEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.HttpClient;
    }

    [Fact]
    public async Task CreateCollection_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var command = new CreateCollectionCommand("Test Collection");

        // Act
        var response = await _client.PostAsJsonAsync("/api/collections", command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateCollectionResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Collection", result.Title);
    }

    [Fact]
    public async Task CreateCollection_ShouldReturnValidationError_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateCollectionCommand("");

        // Act
        var response = await _client.PostAsJsonAsync("/api/collections", command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCollectionById_ShouldReturnCollection_WhenCollectionExists()
    {
        // Arrange - Create a collection first
        var createCommand = new CreateCollectionCommand("Test Collection for Get");
        var createResponse = await _client.PostAsJsonAsync("/api/collections", createCommand, TestContext.Current.CancellationToken);
        var createdCollection = await createResponse.Content.ReadFromJsonAsync<CreateCollectionResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(createdCollection);

        // Act
        var response = await _client.GetAsync($"/api/collections/{createdCollection.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CollectionReadModel>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal("Test Collection for Get", result.Title);
        Assert.Empty(result.ImagePostIds);
    }

    [Fact]
    public async Task GetCollectionById_ShouldReturnNotFound_WhenCollectionDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/collections/{nonExistentId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddImagePostToCollection_ShouldReturnOk_WhenBothExist()
    {
        // Arrange - Create a collection and an image
        var createCollectionCommand = new CreateCollectionCommand("Test Collection for Images");
        var collectionResponse = await _client.PostAsJsonAsync("/api/collections", createCollectionCommand, TestContext.Current.CancellationToken);
        var collection = await collectionResponse.Content.ReadFromJsonAsync<CreateCollectionResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(collection);

        var createImageCommand = new CreateImagePostCommand(
            Title: "Test Image for Collection",
            Tags: new List<TagDto> { new(TagType.Artist, "collection_artist") },
            ContentType: "image/jpeg");
        var imageResponse = await _client.PostAsJsonAsync("/api/images", createImageCommand, TestContext.Current.CancellationToken);
        var image = await imageResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(image);

        // Act
        var response = await _client.PostAsync(
            $"/api/collections/{collection.Id}/images/{image.Id}",
            null,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify image was added to collection
        var getResponse = await _client.GetAsync($"/api/collections/{collection.Id}", TestContext.Current.CancellationToken);
        var updatedCollection = await getResponse.Content.ReadFromJsonAsync<CollectionReadModel>(TestContext.Current.CancellationToken);
        Assert.NotNull(updatedCollection);
        Assert.Contains(image.Id, updatedCollection.ImagePostIds);
    }

    [Fact]
    public async Task AddImagePostToCollection_ShouldReturnError_WhenCollectionDoesNotExist()
    {
        // Arrange - Create only an image
        var createImageCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { new(TagType.General, "test") },
            ContentType: "image/jpeg");
        var imageResponse = await _client.PostAsJsonAsync("/api/images", createImageCommand, TestContext.Current.CancellationToken);
        var image = await imageResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(image);

        var nonExistentCollectionId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync(
            $"/api/collections/{nonExistentCollectionId}/images/{image.Id}",
            null,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task RemoveImagePostFromCollection_ShouldReturnOk_WhenImageExistsInCollection()
    {
        // Arrange - Create a collection, an image, and add the image to the collection
        var createCollectionCommand = new CreateCollectionCommand("Test Collection for Removal");
        var collectionResponse = await _client.PostAsJsonAsync("/api/collections", createCollectionCommand, TestContext.Current.CancellationToken);
        var collection = await collectionResponse.Content.ReadFromJsonAsync<CreateCollectionResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(collection);

        var createImageCommand = new CreateImagePostCommand(
            Title: "Test Image for Removal",
            Tags: new List<TagDto> { new(TagType.Artist, "removal_artist") },
            ContentType: "image/jpeg");
        var imageResponse = await _client.PostAsJsonAsync("/api/images", createImageCommand, TestContext.Current.CancellationToken);
        var image = await imageResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(image);

        await _client.PostAsync($"/api/collections/{collection.Id}/images/{image.Id}", null, TestContext.Current.CancellationToken);

        // Act
        var response = await _client.DeleteAsync($"/api/collections/{collection.Id}/images/{image.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify image was removed from collection
        var getResponse = await _client.GetAsync($"/api/collections/{collection.Id}", TestContext.Current.CancellationToken);
        var updatedCollection = await getResponse.Content.ReadFromJsonAsync<CollectionReadModel>(TestContext.Current.CancellationToken);
        Assert.NotNull(updatedCollection);
        Assert.DoesNotContain(image.Id, updatedCollection.ImagePostIds);
    }

    [Fact]
    public async Task Collection_ShouldAggregateTagsFromImages()
    {
        // Arrange - Create a collection and add multiple images with different tags
        var createCollectionCommand = new CreateCollectionCommand("Test Collection for Tag Aggregation");
        var collectionResponse = await _client.PostAsJsonAsync("/api/collections", createCollectionCommand, TestContext.Current.CancellationToken);
        var collection = await collectionResponse.Content.ReadFromJsonAsync<CreateCollectionResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(collection);

        // Create first image with specific tags
        var createImageCommand1 = new CreateImagePostCommand(
            Title: "Image 1",
            Tags: new List<TagDto>
            {
                new(TagType.Artist, "artist1"),
                new(TagType.Character, "character1")
            },
            ContentType: "image/jpeg");
        var imageResponse1 = await _client.PostAsJsonAsync("/api/images", createImageCommand1, TestContext.Current.CancellationToken);
        var image1 = await imageResponse1.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(image1);

        // Create second image with different tags
        var createImageCommand2 = new CreateImagePostCommand(
            Title: "Image 2",
            Tags: new List<TagDto>
            {
                new(TagType.Artist, "artist2"),
                new(TagType.Series, "series1")
            },
            ContentType: "image/jpeg");
        var imageResponse2 = await _client.PostAsJsonAsync("/api/images", createImageCommand2, TestContext.Current.CancellationToken);
        var image2 = await imageResponse2.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(image2);

        // Add both images to collection
        await _client.PostAsync($"/api/collections/{collection.Id}/images/{image1.Id}", null, TestContext.Current.CancellationToken);
        await _client.PostAsync($"/api/collections/{collection.Id}/images/{image2.Id}", null, TestContext.Current.CancellationToken);

        // Act
        var getResponse = await _client.GetAsync($"/api/collections/{collection.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var result = await getResponse.Content.ReadFromJsonAsync<CollectionReadModel>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(2, result.ImagePostIds.Count);
        Assert.NotEmpty(result.AggregatedTags);
    }
}
