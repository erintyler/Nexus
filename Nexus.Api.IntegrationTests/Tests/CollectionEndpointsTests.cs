using System.Net;
using Alba;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.Collections.CreateCollection;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests.Tests;

[Collection("AlbaWebApp")]
public class CollectionEndpointsTests : IClassFixture<AlbaWebAppFixture>
{
    private readonly AlbaWebAppFixture _fixture;

    public CollectionEndpointsTests(AlbaWebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateCollection_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new CreateCollectionCommand("Test Collection");

        // Act & Assert
        var result = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(command).ToUrl("/api/collections");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var response = result.ReadAsJson<CreateCollectionResponse>();
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
    }

    [Fact]
    public async Task GetCollectionById_WithExistingCollection_ReturnsOk()
    {
        // Arrange - create a collection first
        var createCommand = new CreateCollectionCommand("Test Collection for Get");

        var createResult = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(createCommand).ToUrl("/api/collections");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateCollectionResponse>();

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url($"/api/collections/{createResponse!.Id}");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetCollectionById_WithNonExistingCollection_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url($"/api/collections/{nonExistingId}");
            s.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task AddImagePostToCollection_WithValidData_ReturnsOk()
    {
        // Arrange - create a collection and an image
        var collectionCommand = new CreateCollectionCommand("Test Collection for Adding Images");

        var collectionResult = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(collectionCommand).ToUrl("/api/collections");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var collectionResponse = collectionResult.ReadAsJson<CreateCollectionResponse>();

        var imageCommand = new CreateImagePostCommand(
            "Test Image",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "test-character")
            },
            "image/jpeg"
        );

        var imageResult = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(imageCommand).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var imageResponse = imageResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Post.Url($"/api/collections/{collectionResponse!.Id}/images/{imageResponse!.Id}");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task RemoveImagePostFromCollection_WithValidData_ReturnsOk()
    {
        // Arrange - create a collection, an image, and add the image to the collection
        var collectionCommand = new CreateCollectionCommand("Test Collection for Removing Images");

        var collectionResult = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(collectionCommand).ToUrl("/api/collections");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var collectionResponse = collectionResult.ReadAsJson<CreateCollectionResponse>();

        var imageCommand = new CreateImagePostCommand(
            "Test Image to Remove",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "test-character")
            },
            "image/jpeg"
        );

        var imageResult = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(imageCommand).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var imageResponse = imageResult.ReadAsJson<CreateImagePostResponse>();

        // Add image to collection first
        await _fixture.Host.Scenario(s =>
        {
            s.Post.Url($"/api/collections/{collectionResponse!.Id}/images/{imageResponse!.Id}");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Act & Assert - remove image from collection
        await _fixture.Host.Scenario(s =>
        {
            s.Delete.Url($"/api/collections/{collectionResponse!.Id}/images/{imageResponse!.Id}");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }
}
