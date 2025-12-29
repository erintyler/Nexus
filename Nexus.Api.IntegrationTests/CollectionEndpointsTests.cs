using System.Net;
using System.Net.Http.Json;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.Collections.AddImagePostToCollection;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Application.Features.Collections.CreateCollection;
using Nexus.Application.Features.Collections.RemoveImagePostFromCollection;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests;

[Collection("Aspire")]
public class CollectionEndpointsTests(AspireAppHostFixture fixture) : IClassFixture<AspireAppHostFixture>
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task CreateCollection_WithValidData_ReturnsCreatedCollection()
    {
        // Arrange
        var command = new CreateCollectionCommand("Test Collection");

        // Act
        var response = await _client.PostAsJsonAsync("/api/collections", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateCollectionResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Collection", result.Title);
    }

    [Fact]
    public async Task CreateCollection_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateCollectionCommand("");

        // Act
        var response = await _client.PostAsJsonAsync("/api/collections", command);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetCollectionById_WithExistingId_ReturnsCollection()
    {
        // Arrange - Create a collection first
        var createCommand = new CreateCollectionCommand("Test Collection for Get");
        var createResponse = await _client.PostAsJsonAsync("/api/collections", createCommand);
        var createdCollection = await createResponse.Content.ReadFromJsonAsync<CreateCollectionResponse>();
        Assert.NotNull(createdCollection);

        // Act
        var response = await _client.GetAsync($"/api/collections/{createdCollection.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CollectionReadModel>();
        Assert.NotNull(result);
        Assert.Equal(createdCollection.Id, result.Id);
        Assert.Equal("Test Collection for Get", result.Title);
    }

    [Fact]
    public async Task GetCollectionById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/collections/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddImagePostToCollection_WithValidData_ReturnsOk()
    {
        // Arrange - Create collection and image post
        var createCollectionCommand = new CreateCollectionCommand("Test Collection for Add");
        var collectionResponse = await _client.PostAsJsonAsync("/api/collections", createCollectionCommand);
        var collection = await collectionResponse.Content.ReadFromJsonAsync<CreateCollectionResponse>();
        Assert.NotNull(collection);

        var createImageCommand = new CreateImagePostCommand(
            "Test Image",
            new[] { new TagDto(TagType.Character, "test_character") },
            "image/png"
        );
        var imageResponse = await _client.PostAsJsonAsync("/api/images", createImageCommand);
        var image = await imageResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(image);

        // Act
        var response = await _client.PostAsync($"/api/collections/{collection.Id}/images/{image.Id}", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify the image was added by getting the collection
        var getResponse = await _client.GetAsync($"/api/collections/{collection.Id}");
        var updatedCollection = await getResponse.Content.ReadFromJsonAsync<CollectionReadModel>();
        Assert.NotNull(updatedCollection);
        Assert.Contains(updatedCollection.ImagePostIds, id => id == image.Id);
    }

    [Fact]
    public async Task RemoveImagePostFromCollection_WithExistingImage_ReturnsOk()
    {
        // Arrange - Create collection, image post, and add image to collection
        var createCollectionCommand = new CreateCollectionCommand("Test Collection for Remove");
        var collectionResponse = await _client.PostAsJsonAsync("/api/collections", createCollectionCommand);
        var collection = await collectionResponse.Content.ReadFromJsonAsync<CreateCollectionResponse>();
        Assert.NotNull(collection);

        var createImageCommand = new CreateImagePostCommand(
            "Test Image for Remove",
            new[] { new TagDto(TagType.Character, "test_character") },
            "image/png"
        );
        var imageResponse = await _client.PostAsJsonAsync("/api/images", createImageCommand);
        var image = await imageResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(image);

        await _client.PostAsync($"/api/collections/{collection.Id}/images/{image.Id}", null);

        // Act
        var response = await _client.DeleteAsync($"/api/collections/{collection.Id}/images/{image.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify the image was removed by getting the collection
        var getResponse = await _client.GetAsync($"/api/collections/{collection.Id}");
        var updatedCollection = await getResponse.Content.ReadFromJsonAsync<CollectionReadModel>();
        Assert.NotNull(updatedCollection);
        Assert.DoesNotContain(updatedCollection.ImagePostIds, id => id == image.Id);
    }
}
