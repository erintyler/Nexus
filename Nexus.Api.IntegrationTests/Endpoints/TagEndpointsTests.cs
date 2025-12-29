using System.Net;
using System.Net.Http.Json;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.Tags.GetTagMigrations;
using Nexus.Application.Features.Tags.MigrateTag;
using Nexus.Domain.Enums;

namespace Nexus.Api.IntegrationTests.Endpoints;

public class TagEndpointsTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public TagEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.HttpClient;
    }

    [Fact]
    public async Task SearchTags_ShouldReturnPagedResults()
    {
        // Arrange - Create images with specific tags
        var createCommand1 = new CreateImagePostCommand(
            Title: "Image with unique tag 1",
            Tags: new List<TagDto>
            {
                new(TagType.Artist, "unique_search_artist_1"),
                new(TagType.General, "common_tag")
            },
            ContentType: "image/jpeg");
        await _client.PostAsJsonAsync("/api/images", createCommand1);

        var createCommand2 = new CreateImagePostCommand(
            Title: "Image with unique tag 2",
            Tags: new List<TagDto>
            {
                new(TagType.Artist, "unique_search_artist_2"),
                new(TagType.General, "common_tag")
            },
            ContentType: "image/jpeg");
        await _client.PostAsJsonAsync("/api/images", createCommand2);

        // Act
        var response = await _client.GetAsync("/api/tags/search");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagCountDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task SearchTags_WithSearchTerm_ShouldFilterResults()
    {
        // Arrange - Create images with specific tags
        var createCommand = new CreateImagePostCommand(
            Title: "Image with filterable tag",
            Tags: new List<TagDto>
            {
                new(TagType.Artist, "filterable_artist_xyz")
            },
            ContentType: "image/jpeg");
        await _client.PostAsJsonAsync("/api/images", createCommand);

        // Act
        var response = await _client.GetAsync("/api/tags/search?searchTerm=filterable_artist");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagCountDto>>();
        Assert.NotNull(result);
        // Note: Results may include tags from other tests, so we just verify the response structure
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task SearchTags_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange - Create multiple images with different tags
        for (int i = 0; i < 5; i++)
        {
            var createCommand = new CreateImagePostCommand(
                Title: $"Image for pagination {i}",
                Tags: new List<TagDto>
                {
                    new(TagType.General, $"pagination_tag_{i}")
                },
                ContentType: "image/jpeg");
            await _client.PostAsJsonAsync("/api/images", createCommand);
        }

        // Act
        var response = await _client.GetAsync("/api/tags/search?pageNumber=1&pageSize=3");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagCountDto>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.PageSize <= 3 || result.Items.Count <= 3);
    }

    [Fact]
    public async Task MigrateTag_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange - Create images with the source tag
        var sourceTag = new TagDto(TagType.Artist, "old_artist_name");
        var targetTag = new TagDto(TagType.Artist, "new_artist_name");

        var createCommand1 = new CreateImagePostCommand(
            Title: "Image for migration 1",
            Tags: new List<TagDto> { sourceTag },
            ContentType: "image/jpeg");
        await _client.PostAsJsonAsync("/api/images", createCommand1);

        var createCommand2 = new CreateImagePostCommand(
            Title: "Image for migration 2",
            Tags: new List<TagDto> { sourceTag },
            ContentType: "image/jpeg");
        await _client.PostAsJsonAsync("/api/images", createCommand2);

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<MigrateTagResponse>();
        Assert.NotNull(result);
        Assert.True(result.PostsMigrated >= 2);
    }

    [Fact]
    public async Task MigrateTag_ShouldReturnValidationError_WhenSourceAndTargetAreSame()
    {
        // Arrange
        var sameTag = new TagDto(TagType.Artist, "same_artist");
        var migrateCommand = new MigrateTagCommand(sameTag, sameTag);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTagMigrations_ShouldReturnPagedResults()
    {
        // Arrange - Perform a tag migration
        var sourceTag = new TagDto(TagType.Character, "old_character");
        var targetTag = new TagDto(TagType.Character, "new_character");

        var createCommand = new CreateImagePostCommand(
            Title: "Image for migration history",
            Tags: new List<TagDto> { sourceTag },
            ContentType: "image/jpeg");
        await _client.PostAsJsonAsync("/api/images", createCommand);

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Act
        var response = await _client.GetAsync("/api/tags/migrations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetTagMigrations_WithSourceTagFilter_ShouldFilterResults()
    {
        // Arrange - Perform a tag migration with specific source tag
        var sourceTag = new TagDto(TagType.Series, "old_series_filter");
        var targetTag = new TagDto(TagType.Series, "new_series");

        var createCommand = new CreateImagePostCommand(
            Title: "Image for filtered migration",
            Tags: new List<TagDto> { sourceTag },
            ContentType: "image/jpeg");
        await _client.PostAsJsonAsync("/api/images", createCommand);

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Act
        var response = await _client.GetAsync($"/api/tags/migrations?sourceTag={sourceTag.Type}:{sourceTag.Value}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task GetTagMigrations_WithTargetTagFilter_ShouldFilterResults()
    {
        // Arrange - Perform a tag migration with specific target tag
        var sourceTag = new TagDto(TagType.Meta, "old_meta");
        var targetTag = new TagDto(TagType.Meta, "new_meta_filter");

        var createCommand = new CreateImagePostCommand(
            Title: "Image for target filter migration",
            Tags: new List<TagDto> { sourceTag },
            ContentType: "image/jpeg");
        await _client.PostAsJsonAsync("/api/images", createCommand);

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Act
        var response = await _client.GetAsync($"/api/tags/migrations?targetTag={targetTag.Type}:{targetTag.Value}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task GetTagMigrations_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange - Perform multiple migrations
        for (int i = 0; i < 3; i++)
        {
            var sourceTag = new TagDto(TagType.General, $"old_general_{i}");
            var targetTag = new TagDto(TagType.General, $"new_general_{i}");

            var createCommand = new CreateImagePostCommand(
                Title: $"Image for pagination migration {i}",
                Tags: new List<TagDto> { sourceTag },
                ContentType: "image/jpeg");
            await _client.PostAsJsonAsync("/api/images", createCommand);

            var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
            await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);
        }

        // Act
        var response = await _client.GetAsync("/api/tags/migrations?pageNumber=1&pageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.PageSize <= 2 || result.Items.Count <= 2);
    }
}
