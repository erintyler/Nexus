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
/// Each test gets its own isolated Alba host instance with fresh database and RabbitMQ containers.
/// </summary>
public class ImageEndpointsTests : IAsyncLifetime
{
    private readonly AlbaWebApplicationFixture _fixture = new();
    private readonly Fixture _autoFixture = new();

    public async ValueTask InitializeAsync()
    {
        await _fixture.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task CreateImage_WithValidRequest_ReturnsCreated()
    {
        // Arrange
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
        var result = await _fixture.Host.Scenario(scenario =>
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
        var command = new CreateImagePostCommand(
            Title: "",
            Tags:
            [
                new TagDto(TagType.Artist, "test-artist")
            ],
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
    public async Task GetImageById_WhenImageInProcessing_ReturnsNotFound()
    {
        // Arrange
        var createCommand = new CreateImagePostCommand(
            Title: _autoFixture.Create<string>(),
            Tags:
            [
                new TagDto(TagType.Artist, "test-artist")
            ],
            ContentType: "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createdImage = createResult.ReadAsJson<CreateImagePostResponse>();

        // Mark upload as complete - this triggers image processing
        // Status becomes Processing (not Completed) until ImageProcessor completes
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Put.Url($"/api/images/{createdImage.Id}/upload-complete");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Act & Assert
        // GetImageById only returns images with Completed status
        // Images in Processing status return 404
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{createdImage.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task GetImageById_WhenImageDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{nonExistentId}");
            scenario.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task MarkImageUploadComplete_WhenImageExists_ReturnsOk()
    {
        // Arrange
        var createCommand = new CreateImagePostCommand(
            Title: _autoFixture.Create<string>(),
            Tags:
            [
                new TagDto(TagType.Artist, "test-artist")
            ],
            ContentType: "image/jpeg"
        );

        var createResult = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createdImage = createResult.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Put.Url($"/api/images/{createdImage.Id}/upload-complete");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetImagesByTags_ReturnsOk()
    {
        // Act & Assert
        // Search endpoint works even with no images or only Processing images
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/images/search?tags=Artist:any-artist");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }
}
