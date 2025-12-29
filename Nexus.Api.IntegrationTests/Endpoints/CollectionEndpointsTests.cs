using System.Net;
using Alba;
using AutoFixture;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Application.Features.Collections.CreateCollection;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests.Endpoints;

public class CollectionEndpointsTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;
    private readonly Fixture _autoFixture;

    public CollectionEndpointsTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _autoFixture = new Fixture();
    }

    [Fact]
    public async Task CreateCollection_WithValidData_ReturnsCreated()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var command = new CreateCollectionCommand(Title: "Test Collection");

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var response = result.ReadAsJson<CreateCollectionResponse>();
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
    }

    [Fact]
    public async Task CreateCollection_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var command = new CreateCollectionCommand(Title: "");

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task GetCollectionById_WithExistingCollection_ReturnsCollection()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var createCommand = new CreateCollectionCommand(Title: "Test Collection");

        var createResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateCollectionResponse>();

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{createResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var collection = result.ReadAsJson<CollectionReadModel>();
        Assert.NotNull(collection);
        Assert.Equal("Test Collection", collection.Title);
    }

    [Fact]
    public async Task GetCollectionById_WithNonExistentCollection_ReturnsNotFound()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{nonExistentId}");
            scenario.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task AddImagePostToCollection_WithValidData_ReturnsOk()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        // Create collection
        var collectionCommand = new CreateCollectionCommand(Title: "Test Collection");
        var collectionResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(collectionCommand).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });
        var collectionResponse = collectionResult.ReadAsJson<CreateCollectionResponse>();

        // Create image post
        var imageCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { new(TagType.General, "test") },
            ContentType: "image/jpeg"
        );
        var imageResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });
        var imageResponse = imageResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Url($"/api/collections/{collectionResponse!.Id}/images/{imageResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Verify image was added to collection
        var collectionCheckResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{collectionResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var collection = collectionCheckResult.ReadAsJson<CollectionReadModel>();
        Assert.NotNull(collection);
        Assert.Single(collection.ImagePostIds);
        Assert.Contains(imageResponse!.Id, collection.ImagePostIds);
    }

    [Fact]
    public async Task AddImagePostToCollection_WithNonExistentCollection_ReturnsUnprocessableEntity()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var nonExistentCollectionId = Guid.NewGuid();

        // Create image post
        var imageCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { new(TagType.General, "test") },
            ContentType: "image/jpeg"
        );
        var imageResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });
        var imageResponse = imageResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Url($"/api/collections/{nonExistentCollectionId}/images/{imageResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.UnprocessableEntity);
        });
    }

    [Fact]
    public async Task RemoveImagePostFromCollection_WithExistingImage_ReturnsOk()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        // Create collection
        var collectionCommand = new CreateCollectionCommand(Title: "Test Collection");
        var collectionResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(collectionCommand).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });
        var collectionResponse = collectionResult.ReadAsJson<CreateCollectionResponse>();

        // Create image post
        var imageCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { new(TagType.General, "test") },
            ContentType: "image/jpeg"
        );
        var imageResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });
        var imageResponse = imageResult.ReadAsJson<CreateImagePostResponse>();

        // Add image to collection
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Url($"/api/collections/{collectionResponse!.Id}/images/{imageResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Delete.Url($"/api/collections/{collectionResponse!.Id}/images/{imageResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Verify image was removed from collection
        var collectionCheckResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{collectionResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var collection = collectionCheckResult.ReadAsJson<CollectionReadModel>();
        Assert.NotNull(collection);
        Assert.Empty(collection.ImagePostIds);
    }
}
