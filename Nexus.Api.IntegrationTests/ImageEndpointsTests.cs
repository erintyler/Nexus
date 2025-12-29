using System.Net;
using System.Net.Http.Json;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.ImagePosts.GetImageById;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests;

[Collection("Aspire")]
public class ImageEndpointsTests(AspireAppHostFixture fixture) : IClassFixture<AspireAppHostFixture>
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task CreateImage_WithValidData_ReturnsCreatedImage()
    {
        // Arrange
        var command = new CreateImagePostCommand(
            "Test Image Post",
            new[] { new TagDto(TagType.Character, "test_character"), new TagDto(TagType.General, "wolf") },
            "image/png"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/images", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Image Post", result.Title);
    }

    [Fact]
    public async Task CreateImage_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateImagePostCommand(
            "",
            new[] { new TagDto(TagType.Character, "test_character") },
            "image/png"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/images", command);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetImageById_WithExistingId_ReturnsImage()
    {
        // Arrange - Create an image first
        var createCommand = new CreateImagePostCommand(
            "Test Image for Get",
            new[] { new TagDto(TagType.Character, "test_character") },
            "image/png"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(createdImage);

        // Act
        var response = await _client.GetAsync($"/api/images/{createdImage.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ImagePostDto>();
        Assert.NotNull(result);
        Assert.Equal("Test Image for Get", result.Title);
    }

    [Fact]
    public async Task GetImageById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/images/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetImagesByTags_WithExistingTags_ReturnsImages()
    {
        // Arrange - Create images with specific tags
        var tag1 = new TagDto(TagType.Character, "test_search_char");
        var createCommand1 = new CreateImagePostCommand("Image 1", new[] { tag1 }, "image/png");
        await _client.PostAsJsonAsync("/api/images", createCommand1);

        var createCommand2 = new CreateImagePostCommand("Image 2", new[] { tag1 }, "image/png");
        await _client.PostAsJsonAsync("/api/images", createCommand2);

        // Act
        var response = await _client.GetAsync($"/api/images/search?tags[0].Type={tag1.Type}&tags[0].Value={tag1.Value}&pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ImagePostDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 2);
        Assert.All(result.Items, item => Assert.Contains(item.Tags, t => t.Type == tag1.Type && t.Value == tag1.Value));
    }

    [Fact]
    public async Task GetImagesByTags_WithNonExistingTags_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTag = new TagDto(TagType.Character, "non_existent_tag_xyz_123");

        // Act
        var response = await _client.GetAsync($"/api/images/search?tags[0].Type={nonExistentTag.Type}&tags[0].Value={nonExistentTag.Value}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkImageUploadComplete_WithExistingImage_ReturnsOk()
    {
        // Arrange - Create an image first
        var createCommand = new CreateImagePostCommand(
            "Test Image for Upload Complete",
            new[] { new TagDto(TagType.Character, "test_character") },
            "image/png"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(createdImage);

        // Act
        var response = await _client.PutAsync($"/api/images/{createdImage.Id}/upload-complete", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MarkImageUploadComplete_WithNonExistingImage_ReturnsUnprocessableEntity()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/images/{nonExistentId}/upload-complete", null);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task AddTagsToImage_WithValidTags_ReturnsOk()
    {
        // Arrange - Create an image first
        var createCommand = new CreateImagePostCommand(
            "Test Image for Add Tags",
            new[] { new TagDto(TagType.Character, "initial_character") },
            "image/png"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(createdImage);

        var newTags = new[] { new TagDto(TagType.General, "fox"), new TagDto(TagType.Meta, "safe") };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/images/{createdImage.Id}/tags", newTags);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify tags were added
        var getResponse = await _client.GetAsync($"/api/images/{createdImage.Id}");
        var updatedImage = await getResponse.Content.ReadFromJsonAsync<ImagePostDto>();
        Assert.NotNull(updatedImage);
        Assert.Contains(updatedImage.Tags, t => t.Type == TagType.General && t.Value == "fox");
        Assert.Contains(updatedImage.Tags, t => t.Type == TagType.Meta && t.Value == "safe");
    }

    [Fact]
    public async Task RemoveTagsFromImage_WithExistingTags_ReturnsOk()
    {
        // Arrange - Create an image with tags
        var initialTags = new[]
        {
            new TagDto(TagType.Character, "test_character"),
            new TagDto(TagType.General, "wolf"),
            new TagDto(TagType.Meta, "safe")
        };
        var createCommand = new CreateImagePostCommand("Test Image for Remove Tags", initialTags, "image/png");
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(createdImage);

        var tagsToRemove = new[] { new TagDto(TagType.General, "wolf") };

        // Act
        var response = await _client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"/api/images/{createdImage.Id}/tags", UriKind.Relative),
            Content = JsonContent.Create(tagsToRemove)
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify tag was removed
        var getResponse = await _client.GetAsync($"/api/images/{createdImage.Id}");
        var updatedImage = await getResponse.Content.ReadFromJsonAsync<ImagePostDto>();
        Assert.NotNull(updatedImage);
        Assert.DoesNotContain(updatedImage.Tags, t => t.Type == TagType.General && t.Value == "wolf");
        Assert.Contains(updatedImage.Tags, t => t.Type == TagType.Character && t.Value == "test_character");
    }

    [Fact]
    public async Task GetImageHistory_WithExistingImage_ReturnsHistory()
    {
        // Arrange - Create an image and modify it
        var createCommand = new CreateImagePostCommand(
            "Test Image for History",
            new[] { new TagDto(TagType.Character, "test_character") },
            "image/png"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(createdImage);

        // Add some tags to create history
        await _client.PostAsJsonAsync($"/api/images/{createdImage.Id}/tags",
            new[] { new TagDto(TagType.General, "fox") });

        // Act
        var response = await _client.GetAsync($"/api/images/{createdImage.Id}/history?pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<HistoryDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetImageHistory_WithNonExistingImage_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/images/{nonExistentId}/history");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
