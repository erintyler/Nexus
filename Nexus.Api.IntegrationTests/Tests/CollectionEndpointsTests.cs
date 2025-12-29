using Alba;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Api.IntegrationTests.Utilities;
using Nexus.Application.Features.Collections.Common.Models;
using Nexus.Application.Features.Collections.CreateCollection;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Xunit;

namespace Nexus.Api.IntegrationTests.Tests;

public class CollectionEndpointsTests : IClassFixture<AlbaWebApplicationFixture>
{
    private readonly AlbaWebApplicationFixture _fixture;

    public CollectionEndpointsTests(AlbaWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateCollection_WithValidTitle_ReturnsCreatedWithId()
    {
        // Arrange
        var command = new CreateCollectionCommand(Title: "Test Collection");

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(201);
        });

        var result = response.ReadAsJson<CreateCollectionResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateCollection_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateCollectionCommand(Title: "");

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(422);
        });
    }

    [Fact]
    public async Task GetCollectionById_WithExistingCollection_ReturnsOk()
    {
        // Arrange - Create a collection first
        var createCommand = new CreateCollectionCommand(Title: "Collection for Get");

        var createResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(201);
        });

        var created = createResponse.ReadAsJson<CreateCollectionResponse>();

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{created!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<CollectionReadModel>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Collection for Get", result.Title);
        Assert.Empty(result.ImagePostIds); // No images added yet
    }

    [Fact]
    public async Task GetCollectionById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{nonExistentId}");
            scenario.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task AddImagePostToCollection_WithValidIds_ReturnsOk()
    {
        // Arrange - Create a collection and an image
        var collectionCommand = new CreateCollectionCommand(Title: "Collection for Adding Image");
        var collectionResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(collectionCommand).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(201);
        });
        var collection = collectionResponse.ReadAsJson<CreateCollectionResponse>();

        var tags = TestDataGenerator.CreateTags(1);
        var imageCommand = new CreateImagePostCommand(
            Title: "Image to Add",
            Tags: tags,
            ContentType: "image/jpeg"
        );
        var imageResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });
        var image = imageResponse.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Url($"/api/collections/{collection!.Id}/images/{image!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        // Verify image was added to collection
        var getResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{collection!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = getResponse.ReadAsJson<CollectionReadModel>();
        Assert.NotNull(result);
        Assert.Single(result.ImagePostIds);
        Assert.Contains(image.Id, result.ImagePostIds);
    }

    [Fact]
    public async Task AddImagePostToCollection_WithNonExistentCollection_ReturnsUnprocessableEntity()
    {
        // Arrange - Create an image only
        var tags = TestDataGenerator.CreateTags(1);
        var imageCommand = new CreateImagePostCommand(
            Title: "Image for Non-existent Collection",
            Tags: tags,
            ContentType: "image/jpeg"
        );
        var imageResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });
        var image = imageResponse.ReadAsJson<CreateImagePostResponse>();

        var nonExistentCollectionId = Guid.NewGuid();

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Url($"/api/collections/{nonExistentCollectionId}/images/{image!.Id}");
            scenario.StatusCodeShouldBe(422);
        });
    }

    [Fact]
    public async Task AddImagePostToCollection_WithNonExistentImage_ReturnsUnprocessableEntity()
    {
        // Arrange - Create a collection only
        var collectionCommand = new CreateCollectionCommand(Title: "Collection for Non-existent Image");
        var collectionResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(collectionCommand).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(201);
        });
        var collection = collectionResponse.ReadAsJson<CreateCollectionResponse>();

        var nonExistentImageId = Guid.NewGuid();

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Url($"/api/collections/{collection!.Id}/images/{nonExistentImageId}");
            scenario.StatusCodeShouldBe(422);
        });
    }

    [Fact]
    public async Task RemoveImagePostFromCollection_WithExistingImage_ReturnsOk()
    {
        // Arrange - Create a collection and add an image to it
        var collectionCommand = new CreateCollectionCommand(Title: "Collection for Removing Image");
        var collectionResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(collectionCommand).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(201);
        });
        var collection = collectionResponse.ReadAsJson<CreateCollectionResponse>();

        var tags = TestDataGenerator.CreateTags(1);
        var imageCommand = new CreateImagePostCommand(
            Title: "Image to Remove",
            Tags: tags,
            ContentType: "image/jpeg"
        );
        var imageResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });
        var image = imageResponse.ReadAsJson<CreateImagePostResponse>();

        // Add image to collection
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Url($"/api/collections/{collection!.Id}/images/{image!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        // Act & Assert - Remove the image
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Delete.Url($"/api/collections/{collection!.Id}/images/{image!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        // Verify image was removed from collection
        var getResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{collection!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = getResponse.ReadAsJson<CollectionReadModel>();
        Assert.NotNull(result);
        Assert.Empty(result.ImagePostIds);
    }

    [Fact]
    public async Task RemoveImagePostFromCollection_WithNonExistentCollection_ReturnsUnprocessableEntity()
    {
        // Arrange
        var nonExistentCollectionId = Guid.NewGuid();
        var nonExistentImageId = Guid.NewGuid();

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Delete.Url($"/api/collections/{nonExistentCollectionId}/images/{nonExistentImageId}");
            scenario.StatusCodeShouldBe(422);
        });
    }

    [Fact]
    public async Task CollectionAggregatesTags_WhenImagesAreAdded()
    {
        // Arrange - Create a collection
        var collectionCommand = new CreateCollectionCommand(Title: "Collection with Tag Aggregation");
        var collectionResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(collectionCommand).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(201);
        });
        var collection = collectionResponse.ReadAsJson<CreateCollectionResponse>();

        // Create two images with different tags
        var tags1 = TestDataGenerator.CreateTags(2);
        var imageCommand1 = new CreateImagePostCommand(
            Title: "Image 1",
            Tags: tags1,
            ContentType: "image/jpeg"
        );
        var imageResponse1 = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand1).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });
        var image1 = imageResponse1.ReadAsJson<CreateImagePostResponse>();

        var tags2 = TestDataGenerator.CreateTags(2);
        var imageCommand2 = new CreateImagePostCommand(
            Title: "Image 2",
            Tags: tags2,
            ContentType: "image/jpeg"
        );
        var imageResponse2 = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand2).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });
        var image2 = imageResponse2.ReadAsJson<CreateImagePostResponse>();

        // Add both images to collection
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Url($"/api/collections/{collection!.Id}/images/{image1!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Url($"/api/collections/{collection!.Id}/images/{image2!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        // Act & Assert - Get collection and verify tags are aggregated
        var getResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{collection!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = getResponse.ReadAsJson<CollectionReadModel>();
        Assert.NotNull(result);
        Assert.Equal(2, result.ImagePostIds.Count);
        Assert.NotEmpty(result.AggregatedTags);
    }
}
