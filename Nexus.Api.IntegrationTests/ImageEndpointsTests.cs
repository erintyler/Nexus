using System.Net;
using Alba;
using AutoFixture;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests;

/// <summary>
/// Integration tests for Image API endpoints.
/// Tests the full HTTP request/response cycle including Wolverine and Marten.
/// Each test gets its own isolated Alba host instance.
/// </summary>
public class ImageEndpointsTests : IClassFixture<AlbaWebApplicationFixture>
{
    private readonly AlbaWebApplicationFixture _fixture;
    private readonly Fixture _autoFixture = new();

    public ImageEndpointsTests(AlbaWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateImage_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var command = new CreateImagePostCommand(
            Title: _autoFixture.Create<string>(),
            Tags:
            [
                new TagDto(TagType.Artist, "test-artist"),
                new TagDto(TagType.General, "test-tag")
            ],
            ContentType: "image/jpeg"
        );

        // Act
        var result = await host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Assert
        var response = result.ReadAsJson<CreateImagePostResponse>();
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(command.Title, response.Title);
        Assert.NotNull(response.UploadUrl);
    }

    [Fact]
    public async Task CreateImage_WithInvalidTitle_ReturnsBadRequest()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var command = new CreateImagePostCommand(
            Title: "",
            Tags:
            [
                new TagDto(TagType.Artist, "test-artist")
            ],
            ContentType: "image/jpeg"
        );

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task GetImageById_WhenImageExists_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var createCommand = new CreateImagePostCommand(
            Title: _autoFixture.Create<string>(),
            Tags:
            [
                new TagDto(TagType.Artist, "test-artist")
            ],
            ContentType: "image/jpeg"
        );

        var createResult = await host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createdImage = createResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{createdImage.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetImageById_WhenImageDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{nonExistentId}");
            scenario.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task AddTagsToImage_WithValidTags_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var createCommand = new CreateImagePostCommand(
            Title: _autoFixture.Create<string>(),
            Tags:
            [
                new TagDto(TagType.Artist, "test-artist")
            ],
            ContentType: "image/jpeg"
        );

        var createResult = await host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createdImage = createResult.ReadAsJson<CreateImagePostResponse>();

        var newTags = new List<TagDto>
        {
            new(TagType.Character, "new-character"),
            new(TagType.General, "new-tag")
        };

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Post.Json(newTags).ToUrl($"/api/images/{createdImage.Id}/tags");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task RemoveTagsFromImage_WithValidTags_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var createCommand = new CreateImagePostCommand(
            Title: _autoFixture.Create<string>(),
            Tags:
            [
                new TagDto(TagType.Artist, "test-artist"),
                new TagDto(TagType.General, "test-tag")
            ],
            ContentType: "image/jpeg"
        );

        var createResult = await host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createdImage = createResult.ReadAsJson<CreateImagePostResponse>();

        var tagsToRemove = new List<TagDto>
        {
            new(TagType.General, "test-tag")
        };

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Delete.Json(tagsToRemove).ToUrl($"/api/images/{createdImage.Id}/tags");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task MarkImageUploadComplete_WhenImageExists_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var createCommand = new CreateImagePostCommand(
            Title: _autoFixture.Create<string>(),
            Tags:
            [
                new TagDto(TagType.Artist, "test-artist")
            ],
            ContentType: "image/jpeg"
        );

        var createResult = await host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createdImage = createResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Put.Url($"/api/images/{createdImage.Id}/upload-complete");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetImagesByTags_WithValidTags_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var createCommand = new CreateImagePostCommand(
            Title: _autoFixture.Create<string>(),
            Tags:
            [
                new TagDto(TagType.Artist, "searchable-artist")
            ],
            ContentType: "image/jpeg"
        );

        await host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/images/search?tags=Artist:searchable-artist");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }
}
