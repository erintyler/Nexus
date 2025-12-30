using Alba;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Api.IntegrationTests.Utilities;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.ImagePosts.GetImageById;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests.Tests;

public class ImageEndpointsTests : IClassFixture<AlbaWebApplicationFixture>
{
    private readonly AlbaWebApplicationFixture _fixture;

    public ImageEndpointsTests(AlbaWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateImage_WithValidData_ReturnsCreatedWithId()
    {
        // Arrange
        var tags = TestDataGenerator.CreateTags(3);
        var command = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: tags,
            ContentType: "image/jpeg"
        );

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        var result = response.ReadAsJson<CreateImagePostResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateImage_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var tags = TestDataGenerator.CreateTags(1);
        var command = new CreateImagePostCommand(
            Title: "",
            Tags: tags,
            ContentType: "image/jpeg"
        );

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(422);
        });
    }

    [Fact]
    public async Task CreateImage_WithNoTags_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: Array.Empty<TagDto>(),
            ContentType: "image/jpeg"
        );

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(422);
        });
    }

    [Fact]
    public async Task GetImageById_WithExistingImage_ReturnsOk()
    {
        // Arrange - Create an image first
        var tags = TestDataGenerator.CreateTags(2);
        var createCommand = new CreateImagePostCommand(
            Title: "Test Image for Get",
            Tags: tags,
            ContentType: "image/png"
        );

        var createResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        var created = createResponse.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{created!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<ImagePostDto>();
        Assert.NotNull(result);
        Assert.Equal("Test Image for Get", result.Title);
        Assert.Equal(tags.Count, result.Tags.Count);
    }

    [Fact]
    public async Task GetImageById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{nonExistentId}");
            scenario.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task SearchImagesByTags_WithMatchingTags_ReturnsResults()
    {
        // Arrange - Create images with specific tags
        var tag1 = TestDataGenerator.CreateTagDto(TagType.Artist, "test-artist");
        var tag2 = TestDataGenerator.CreateTagDto(TagType.Series, "test-series");

        var createCommand = new CreateImagePostCommand(
            Title: "Searchable Image",
            Tags: new[] { tag1, tag2 },
            ContentType: "image/jpeg"
        );

        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/search?tags={tag1.Type}:{tag1.Value}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<ImagePostDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        Assert.Contains(result.Items, img => img.Title == "Searchable Image");
    }

    [Fact]
    public async Task MarkImageUploadComplete_WithExistingImage_ReturnsOk()
    {
        // Arrange - Create an image first
        var tags = TestDataGenerator.CreateTags(1);
        var createCommand = new CreateImagePostCommand(
            Title: "Image for Upload Complete",
            Tags: tags,
            ContentType: "image/jpeg"
        );

        var createResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        var created = createResponse.ReadAsJson<CreateImagePostResponse>();

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Put.Url($"/api/images/{created!.Id}/upload-complete");
            scenario.StatusCodeShouldBe(200);
        });
    }

    [Fact]
    public async Task MarkImageUploadComplete_WithNonExistentImage_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Put.Url($"/api/images/{nonExistentId}/upload-complete");
            scenario.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task AddTagsToImage_WithValidTags_ReturnsOk()
    {
        // Arrange - Create an image first
        var initialTags = TestDataGenerator.CreateTags(1);
        var createCommand = new CreateImagePostCommand(
            Title: "Image for Adding Tags",
            Tags: initialTags,
            ContentType: "image/jpeg"
        );

        var createResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        var created = createResponse.ReadAsJson<CreateImagePostResponse>();
        var newTags = TestDataGenerator.CreateTags(2);

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(newTags).ToUrl($"/api/images/{created!.Id}/tags");
            scenario.StatusCodeShouldBe(200);
        });

        // Verify tags were added
        var getResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{created!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = getResponse.ReadAsJson<ImagePostDto>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Tags.Count); // 1 initial + 2 new
    }

    [Fact]
    public async Task AddTagsToImage_WithNonExistentImage_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var tags = TestDataGenerator.CreateTags(1);

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(tags).ToUrl($"/api/images/{nonExistentId}/tags");
            scenario.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task RemoveTagsFromImage_WithExistingTags_ReturnsOk()
    {
        // Arrange - Create an image with tags
        var tags = TestDataGenerator.CreateTags(3);
        var createCommand = new CreateImagePostCommand(
            Title: "Image for Removing Tags",
            Tags: tags,
            ContentType: "image/jpeg"
        );

        var createResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        var created = createResponse.ReadAsJson<CreateImagePostResponse>();
        var tagsToRemove = new[] { tags[0] };

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Delete.Json(tagsToRemove).ToUrl($"/api/images/{created!.Id}/tags");
            scenario.StatusCodeShouldBe(200);
        });

        // Verify tag was removed
        var getResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{created!.Id}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = getResponse.ReadAsJson<ImagePostDto>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Tags.Count); // 3 initial - 1 removed
    }

    [Fact]
    public async Task RemoveTagsFromImage_WithNonExistentImage_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var tags = TestDataGenerator.CreateTags(1);

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Delete.Json(tags).ToUrl($"/api/images/{nonExistentId}/tags");
            scenario.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task GetImageHistory_WithExistingImage_ReturnsHistory()
    {
        // Arrange - Create an image and modify it
        var tags = TestDataGenerator.CreateTags(1);
        var createCommand = new CreateImagePostCommand(
            Title: "Image for History",
            Tags: tags,
            ContentType: "image/jpeg"
        );

        var createResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        var created = createResponse.ReadAsJson<CreateImagePostResponse>();

        // Add more tags to create history
        var newTags = TestDataGenerator.CreateTags(1);
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(newTags).ToUrl($"/api/images/{created!.Id}/tags");
            scenario.StatusCodeShouldBe(200);
        });

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{created!.Id}/history");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<HistoryDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetImageHistory_WithNonExistentImage_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{nonExistentId}/history");
            scenario.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task GetImageHistory_WithDateFilters_ReturnsFilteredHistory()
    {
        // Arrange - Create an image
        var tags = TestDataGenerator.CreateTags(1);
        var createCommand = new CreateImagePostCommand(
            Title: "Image for Filtered History",
            Tags: tags,
            ContentType: "image/jpeg"
        );

        var createResponse = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        var created = createResponse.ReadAsJson<CreateImagePostResponse>();
        var dateFrom = DateTimeOffset.UtcNow.AddDays(-3);
        var dateTo = DateTimeOffset.UtcNow.AddDays(1);

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/images/{created!.Id}/history?dateFrom={dateFrom:s}&dateTo={dateTo:s}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<HistoryDto>>();
        Assert.NotNull(result);
    }
}
