using System.Net;
using System.Net.Http.Json;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.ImagePosts.GetImageById;
using Nexus.Domain.Enums;

namespace Nexus.Api.IntegrationTests.Endpoints;

public class ImageEndpointsTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public ImageEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.HttpClient;
    }

    [Fact]
    public async Task CreateImage_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var command = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto>
            {
                new(TagType.Artist, "test_artist"),
                new(TagType.General, "test_tag")
            },
            ContentType: "image/jpeg");

        // Act
        var response = await _client.PostAsJsonAsync("/api/images", command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Image", result.Title);
        Assert.NotEmpty(result.UploadUrl);
    }

    [Fact]
    public async Task CreateImage_ShouldReturnValidationError_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateImagePostCommand(
            Title: "",
            Tags: new List<TagDto>
            {
                new(TagType.General, "test_tag")
            },
            ContentType: "image/jpeg");

        // Act
        var response = await _client.PostAsJsonAsync("/api/images", command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetImageById_ShouldReturnImage_WhenImageExists()
    {
        // Arrange - Create an image first
        var createCommand = new CreateImagePostCommand(
            Title: "Test Image for Get",
            Tags: new List<TagDto>
            {
                new(TagType.Artist, "artist_name")
            },
            ContentType: "image/jpeg");
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand, TestContext.Current.CancellationToken);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(createdImage);

        // Act
        var response = await _client.GetAsync($"/api/images/{createdImage.Id}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ImagePostDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal("Test Image for Get", result.Title);
        Assert.NotEmpty(result.Tags);
    }

    [Fact]
    public async Task GetImageById_ShouldReturnNotFound_WhenImageDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/images/{nonExistentId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchImagesByTags_ShouldReturnPagedResults()
    {
        // Arrange - Create images with specific tags
        var tag1 = new TagDto(TagType.Artist, "search_artist");
        var createCommand = new CreateImagePostCommand(
            Title: "Searchable Image",
            Tags: new List<TagDto> { tag1 },
            ContentType: "image/jpeg");
        await _client.PostAsJsonAsync("/api/images", createCommand, TestContext.Current.CancellationToken);

        // Act
        var response = await _client.GetAsync($"/api/images/search?tags={tag1.Type}:{tag1.Value}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ImagePostDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task AddTagsToImage_ShouldReturnOk_WhenImageExists()
    {
        // Arrange - Create an image first
        var createCommand = new CreateImagePostCommand(
            Title: "Test Image for Tags",
            Tags: new List<TagDto> { new(TagType.Artist, "original_artist") },
            ContentType: "image/jpeg");
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand, TestContext.Current.CancellationToken);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(createdImage);

        var newTags = new List<TagDto>
        {
            new(TagType.Character, "new_character"),
            new(TagType.General, "new_tag")
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/images/{createdImage.Id}/tags", newTags, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify tags were added
        var getResponse = await _client.GetAsync($"/api/images/{createdImage.Id}", TestContext.Current.CancellationToken);
        var updatedImage = await getResponse.Content.ReadFromJsonAsync<ImagePostDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(updatedImage);
        Assert.Contains(updatedImage.Tags, t => t.Type == TagType.Character && t.Value == "new_character");
    }

    [Fact]
    public async Task RemoveTagsFromImage_ShouldReturnOk_WhenImageExists()
    {
        // Arrange - Create an image with multiple tags
        var tag1 = new TagDto(TagType.Artist, "artist_to_keep");
        var tag2 = new TagDto(TagType.General, "tag_to_remove");
        var createCommand = new CreateImagePostCommand(
            Title: "Test Image for Tag Removal",
            Tags: new List<TagDto> { tag1, tag2 },
            ContentType: "image/jpeg");
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand, TestContext.Current.CancellationToken);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(createdImage);

        var tagsToRemove = new List<TagDto> { tag2 };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/images/{createdImage.Id}/tags")
        {
            Content = JsonContent.Create(tagsToRemove)
        };
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify tag was removed
        var getResponse = await _client.GetAsync($"/api/images/{createdImage.Id}", TestContext.Current.CancellationToken);
        var updatedImage = await getResponse.Content.ReadFromJsonAsync<ImagePostDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(updatedImage);
        Assert.DoesNotContain(updatedImage.Tags, t => t.Value == "tag_to_remove");
        Assert.Contains(updatedImage.Tags, t => t.Value == "artist_to_keep");
    }

    [Fact]
    public async Task MarkImageUploadComplete_ShouldReturnOk_WhenImageExists()
    {
        // Arrange - Create an image first
        var createCommand = new CreateImagePostCommand(
            Title: "Test Image for Upload Complete",
            Tags: new List<TagDto> { new(TagType.General, "test") },
            ContentType: "image/jpeg");
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand, TestContext.Current.CancellationToken);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(createdImage);

        // Act
        var response = await _client.PutAsync($"/api/images/{createdImage.Id}/upload-complete", null, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetImageHistory_ShouldReturnPagedResults_WhenImageExists()
    {
        // Arrange - Create an image and modify it
        var createCommand = new CreateImagePostCommand(
            Title: "Test Image for History",
            Tags: new List<TagDto> { new(TagType.Artist, "history_artist") },
            ContentType: "image/jpeg");
        var createResponse = await _client.PostAsJsonAsync("/api/images", createCommand, TestContext.Current.CancellationToken);
        var createdImage = await createResponse.Content.ReadFromJsonAsync<CreateImagePostResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(createdImage);

        // Make some changes to create history
        var newTags = new List<TagDto> { new(TagType.Character, "history_character") };
        await _client.PostAsJsonAsync($"/api/images/{createdImage.Id}/tags", newTags, TestContext.Current.CancellationToken);

        // Act
        var response = await _client.GetAsync($"/api/images/{createdImage.Id}/history", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<HistoryDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetImageHistory_ShouldReturnNotFound_WhenImageDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/images/{nonExistentId}/history", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
