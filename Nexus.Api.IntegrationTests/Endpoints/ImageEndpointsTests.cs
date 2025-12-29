using System.Net;
using Alba;
using AutoFixture;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.ImagePosts.GetImageById;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests.Endpoints;

public class ImageEndpointsTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;
    private readonly Fixture _autoFixture;

    public ImageEndpointsTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _autoFixture = new Fixture();
    }

    [Fact]
    public async Task CreateImage_WithValidData_ReturnsCreated()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var command = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto>
            {
                new(TagType.Artist, "test-artist"),
                new(TagType.General, "test-tag")
            },
            ContentType: "image/jpeg"
        );

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var response = result.ReadAsJson<CreateImagePostResponse>();
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
    }

    [Fact]
    public async Task CreateImage_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var command = new CreateImagePostCommand(
            Title: "",
            Tags: new List<TagDto> { new(TagType.General, "test") },
            ContentType: "image/jpeg"
        );

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task GetImageById_WithExistingImage_ReturnsImage()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var createCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { new(TagType.General, "test") },
            ContentType: "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{createResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var imagePost = result.ReadAsJson<ImagePostDto>();
        Assert.NotNull(imagePost);
        Assert.Equal("Test Image", imagePost.Title);
        Assert.Single(imagePost.Tags);
    }

    [Fact]
    public async Task GetImageById_WithNonExistentImage_ReturnsNotFound()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{nonExistentId}");
            scenario.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task GetImagesByTags_WithMatchingTags_ReturnsImages()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var tag = new TagDto(TagType.Artist, "test-artist");
        var createCommand = new CreateImagePostCommand(
            Title: "Test Image with Artist",
            Tags: new List<TagDto> { tag },
            ContentType: "image/jpeg"
        );

        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/search?tags=Artist:test-artist");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var pagedResult = result.ReadAsJson<PagedResult<ImagePostDto>>();
        Assert.NotNull(pagedResult);
        Assert.True(pagedResult.TotalCount > 0);
    }

    [Fact]
    public async Task AddTagsToImage_WithValidTags_ReturnsOk()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var createCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { new(TagType.General, "initial-tag") },
            ContentType: "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateImagePostResponse>();
        var newTags = new List<TagDto> { new(TagType.Character, "new-character") };

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(newTags).ToUrl($"/api/images/{createResponse!.Id}/tags");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Verify tags were added
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{createResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var imagePost = result.ReadAsJson<ImagePostDto>();
        Assert.NotNull(imagePost);
        Assert.Equal(2, imagePost.Tags.Count);
    }

    [Fact]
    public async Task RemoveTagsFromImage_WithExistingTags_ReturnsOk()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var tagToRemove = new TagDto(TagType.General, "remove-me");
        var createCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto>
            {
                new(TagType.Artist, "keep-this"),
                tagToRemove
            },
            ContentType: "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Delete.Json(new List<TagDto> { tagToRemove }).ToUrl($"/api/images/{createResponse!.Id}/tags");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Verify tag was removed
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{createResponse!.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var imagePost = result.ReadAsJson<ImagePostDto>();
        Assert.NotNull(imagePost);
        Assert.Single(imagePost.Tags);
        Assert.DoesNotContain(imagePost.Tags, t => t.Type == TagType.General && t.Value == "remove-me");
    }

    [Fact]
    public async Task MarkImageUploadComplete_WithExistingImage_ReturnsOk()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var createCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { new(TagType.General, "test") },
            ContentType: "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Put.Url($"/api/images/{createResponse!.Id}/upload-complete");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetImageHistory_WithExistingImage_ReturnsHistory()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var createCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { new(TagType.General, "test") },
            ContentType: "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createResponse = createResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{createResponse!.Id}/history");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var pagedResult = result.ReadAsJson<PagedResult<HistoryDto>>();
        Assert.NotNull(pagedResult);
    }
}
