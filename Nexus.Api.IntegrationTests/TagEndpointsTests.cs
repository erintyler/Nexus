using System.Net;
using System.Net.Http.Json;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.ImagePosts.GetImageById;
using Nexus.Application.Features.Tags.GetTagMigrations;
using Nexus.Application.Features.Tags.GetTags;
using Nexus.Application.Features.Tags.MigrateTag;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests;

[Collection("Aspire")]
public class TagEndpointsTests(AspireAppHostFixture fixture) : IClassFixture<AspireAppHostFixture>
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task SearchTags_WithNoFilter_ReturnsAllTags()
    {
        // Arrange - Create some images with tags
        var tag1 = new TagDto(TagType.Character, "search_test_char1");
        var tag2 = new TagDto(TagType.Character, "search_test_char2");

        await _client.PostAsJsonAsync("/api/images",
            new CreateImagePostCommand("Image 1", new[] { tag1 }, "image/png"));
        await _client.PostAsJsonAsync("/api/images",
            new CreateImagePostCommand("Image 2", new[] { tag2 }, "image/png"));

        // Act
        var response = await _client.GetAsync("/api/tags/search?pageNumber=1&pageSize=50");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagCountDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task SearchTags_WithSearchTerm_ReturnsFilteredTags()
    {
        // Arrange - Create images with specific tags
        var uniqueTag = $"unique_search_tag_{Guid.NewGuid():N}";
        var tag = new TagDto(TagType.Character, uniqueTag);

        await _client.PostAsJsonAsync("/api/images",
            new CreateImagePostCommand("Image with unique tag", new[] { tag }, "image/png"));

        // Act
        var response = await _client.GetAsync($"/api/tags/search?searchTerm={uniqueTag}&pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagCountDto>>();
        Assert.NotNull(result);
        Assert.True(result.Items.Any(t => t.Id.Contains(uniqueTag)));
    }

    [Fact]
    public async Task SearchTags_WithPagination_ReturnsPagedResults()
    {
        // Arrange - Ensure we have some tags
        for (int i = 0; i < 5; i++)
        {
            var tag = new TagDto(TagType.Character, $"pagination_test_char_{i}");
            await _client.PostAsJsonAsync("/api/images",
                new CreateImagePostCommand($"Image {i}", new[] { tag }, "image/png"));
        }

        // Act
        var response = await _client.GetAsync("/api/tags/search?pageNumber=1&pageSize=3");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagCountDto>>();
        Assert.NotNull(result);
        Assert.True(result.Items.Count <= 3);
        Assert.True(result.HasNextPage || result.TotalCount <= 3);
    }

    [Fact]
    public async Task MigrateTag_WithValidTags_ReturnsSuccessResponse()
    {
        // Arrange - Create images with source tag
        var sourceTag = new TagDto(TagType.Character, $"migrate_source_{Guid.NewGuid():N}");
        var targetTag = new TagDto(TagType.Character, $"migrate_target_{Guid.NewGuid():N}");

        var createCommand1 = new CreateImagePostCommand("Image 1 for migration", new[] { sourceTag }, "image/png");
        var response1 = await _client.PostAsJsonAsync("/api/images", createCommand1);
        var image1 = await response1.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(image1);

        var createCommand2 = new CreateImagePostCommand("Image 2 for migration", new[] { sourceTag }, "image/png");
        var response2 = await _client.PostAsJsonAsync("/api/images", createCommand2);
        var image2 = await response2.Content.ReadFromJsonAsync<CreateImagePostResponse>();
        Assert.NotNull(image2);

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);

        // Act
        var migrateResponse = await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, migrateResponse.StatusCode);
        var result = await migrateResponse.Content.ReadFromJsonAsync<MigrateTagResponse>();
        Assert.NotNull(result);
        Assert.True(result.PostsMigrated >= 2);

        // Verify tags were migrated
        var getImage1 = await _client.GetAsync($"/api/images/{image1.Id}");
        var updatedImage1 = await getImage1.Content.ReadFromJsonAsync<ImagePostDto>();
        Assert.NotNull(updatedImage1);
        Assert.Contains(updatedImage1.Tags, t => t.Type == targetTag.Type && t.Value == targetTag.Value);
        Assert.DoesNotContain(updatedImage1.Tags, t => t.Type == sourceTag.Type && t.Value == sourceTag.Value);
    }

    [Fact]
    public async Task MigrateTag_WithSameSourceAndTarget_ReturnsBadRequest()
    {
        // Arrange
        var tag = new TagDto(TagType.Character, "same_tag");
        var migrateCommand = new MigrateTagCommand(tag, tag);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetTagMigrations_WithNoFilter_ReturnsAllMigrations()
    {
        // Arrange - Create a tag migration
        var sourceTag = new TagDto(TagType.Character, $"migration_history_source_{Guid.NewGuid():N}");
        var targetTag = new TagDto(TagType.Character, $"migration_history_target_{Guid.NewGuid():N}");

        var createCommand = new CreateImagePostCommand("Image for migration history", new[] { sourceTag }, "image/png");
        await _client.PostAsJsonAsync("/api/images", createCommand);

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Act
        var response = await _client.GetAsync("/api/tags/migrations?pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetTagMigrations_WithSourceTagFilter_ReturnsFilteredMigrations()
    {
        // Arrange - Create a tag migration with unique tags
        var sourceTag = new TagDto(TagType.Character, $"filter_source_{Guid.NewGuid():N}");
        var targetTag = new TagDto(TagType.Character, $"filter_target_{Guid.NewGuid():N}");

        var createCommand = new CreateImagePostCommand("Image for filtered migration", new[] { sourceTag }, "image/png");
        await _client.PostAsJsonAsync("/api/images", createCommand);

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Act
        var response = await _client.GetAsync($"/api/tags/migrations?sourceTag.Type={sourceTag.Type}&sourceTag.Value={sourceTag.Value}&pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
        Assert.True(result.Items.All(m => m.SourceTag.Type == sourceTag.Type && m.SourceTag.Value == sourceTag.Value));
    }

    [Fact]
    public async Task GetTagMigrations_WithTargetTagFilter_ReturnsFilteredMigrations()
    {
        // Arrange - Create a tag migration with unique tags
        var sourceTag = new TagDto(TagType.Character, $"target_filter_source_{Guid.NewGuid():N}");
        var targetTag = new TagDto(TagType.Character, $"target_filter_target_{Guid.NewGuid():N}");

        var createCommand = new CreateImagePostCommand("Image for target filtered migration", new[] { sourceTag }, "image/png");
        await _client.PostAsJsonAsync("/api/images", createCommand);

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _client.PostAsJsonAsync("/api/tags/migrate", migrateCommand);

        // Act
        var response = await _client.GetAsync($"/api/tags/migrations?targetTag.Type={targetTag.Type}&targetTag.Value={targetTag.Value}&pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
        Assert.True(result.Items.All(m => m.TargetTag.Type == targetTag.Type && m.TargetTag.Value == targetTag.Value));
    }
}
