using System.Net;
using Alba;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests.Tests;

[Collection("AlbaWebApp")]
public class ImageEndpointsTests : IClassFixture<AlbaWebAppFixture>
{
    private readonly AlbaWebAppFixture _fixture;

    public ImageEndpointsTests(AlbaWebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateImage_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new CreateImagePostCommand(
            "Test Image",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "test-character"),
                new TagDto(TagType.Artist, "test-artist")
            },
            "image/jpeg"
        );

        // Act & Assert
        var result = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(command).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var response = result.ReadAsJson<CreateImagePostResponse>();
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
    }

    [Fact]
    public async Task GetImageById_WithExistingImage_ReturnsOk()
    {
        // Arrange - create an image first
        var createCommand = new CreateImagePostCommand(
            "Test Image for Get",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "character1")
            },
            "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(createCommand).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url($"/api/images/{createResponse!.Id}");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetImageById_WithNonExistingImage_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url($"/api/images/{nonExistingId}");
            s.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task AddTagsToImage_WithValidData_ReturnsOk()
    {
        // Arrange - create an image first
        var createCommand = new CreateImagePostCommand(
            "Test Image for Tags",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "initial-character")
            },
            "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(createCommand).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateImagePostResponse>();

        var newTags = new List<TagDto>
        {
            new TagDto(TagType.Artist, "new-artist"),
            new TagDto(TagType.Series, "new-series")
        };

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(newTags).ToUrl($"/api/images/{createResponse!.Id}/tags");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task RemoveTagsFromImage_WithValidData_ReturnsOk()
    {
        // Arrange - create an image with multiple tags
        var createCommand = new CreateImagePostCommand(
            "Test Image for Remove Tags",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "character-to-remove"),
                new TagDto(TagType.Artist, "artist-to-keep")
            },
            "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(createCommand).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateImagePostResponse>();

        var tagsToRemove = new List<TagDto>
        {
            new TagDto(TagType.Character, "character-to-remove")
        };

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Delete.Json(tagsToRemove).ToUrl($"/api/images/{createResponse!.Id}/tags");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task MarkImageUploadComplete_WithValidImage_ReturnsOk()
    {
        // Arrange - create an image first
        var createCommand = new CreateImagePostCommand(
            "Test Image for Upload Complete",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "test-character")
            },
            "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(createCommand).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Put.Url($"/api/images/{createResponse!.Id}/upload-complete");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task SearchImagesByTags_WithValidTags_ReturnsOk()
    {
        // Arrange - create some images
        var command1 = new CreateImagePostCommand(
            "Image 1",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "search-character")
            },
            "image/jpeg"
        );

        await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(command1).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url("/api/images/search?tags[0].Type=Character&tags[0].Value=search-character");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }
}
