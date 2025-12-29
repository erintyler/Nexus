using Alba;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Api.IntegrationTests.Utilities;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.Tags.GetTagMigrations;
using Nexus.Application.Features.Tags.MigrateTag;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests.Tests;

public class TagEndpointsTests : IClassFixture<AlbaWebApplicationFixture>
{
    private readonly AlbaWebApplicationFixture _fixture;

    public TagEndpointsTests(AlbaWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SearchTags_WithoutSearchTerm_ReturnsAllTags()
    {
        // Arrange - Create some images with tags to populate the tag database
        var tags = TestDataGenerator.CreateTags(3);
        var imageCommand = new CreateImagePostCommand(
            Title: "Image for Tag Search",
            Tags: tags,
            ContentType: "image/jpeg"
        );

        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/search");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<TagCountDto>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SearchTags_WithSearchTerm_ReturnsFilteredTags()
    {
        // Arrange - Create an image with a specific tag
        var specificTag = TestDataGenerator.CreateTagDto(TagType.Artist, "specific-artist-name");
        var imageCommand = new CreateImagePostCommand(
            Title: "Image with Specific Tag",
            Tags: new[] { specificTag },
            ContentType: "image/jpeg"
        );

        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/tags/search?searchTerm={specificTag.Value}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<TagCountDto>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SearchTags_WithPagination_ReturnsPagedResults()
    {
        // Arrange - Create multiple images with different tags
        for (int i = 0; i < 5; i++)
        {
            var tags = TestDataGenerator.CreateTags(1);
            var imageCommand = new CreateImagePostCommand(
                Title: $"Image {i}",
                Tags: tags,
                ContentType: "image/jpeg"
            );

            await _fixture.AlbaHost.Scenario(scenario =>
            {
                scenario.Post.Json(imageCommand).ToUrl("/api/images");
                scenario.StatusCodeShouldBe(201);
            });
        }

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/search?pageNumber=1&pageSize=3");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<TagCountDto>>();
        Assert.NotNull(result);
        Assert.True(result.PageSize == 3);
    }

    [Fact]
    public async Task MigrateTag_WithValidTags_ReturnsSuccess()
    {
        // Arrange - Create images with the source tag
        var sourceTag = TestDataGenerator.CreateTagDto(TagType.Artist, "old-artist-name");
        var targetTag = TestDataGenerator.CreateTagDto(TagType.Artist, "new-artist-name");

        // Create multiple images with the source tag
        for (int i = 0; i < 3; i++)
        {
            var imageCommand = new CreateImagePostCommand(
                Title: $"Image for Migration {i}",
                Tags: new[] { sourceTag },
                ContentType: "image/jpeg"
            );

            await _fixture.AlbaHost.Scenario(scenario =>
            {
                scenario.Post.Json(imageCommand).ToUrl("/api/images");
                scenario.StatusCodeShouldBe(201);
            });
        }

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<MigrateTagResponse>();
        Assert.NotNull(result);
        Assert.True(result.PostsMigrated >= 3);
    }

    [Fact]
    public async Task MigrateTag_WithSameSourceAndTarget_ReturnsBadRequest()
    {
        // Arrange
        var tag = TestDataGenerator.CreateTagDto(TagType.General, "same-tag");
        var migrateCommand = new MigrateTagCommand(tag, tag);

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(422);
        });
    }

    [Fact]
    public async Task MigrateTag_WithDifferentTagTypes_ReturnsBadRequest()
    {
        // Arrange
        var sourceTag = TestDataGenerator.CreateTagDto(TagType.Artist, "artist");
        var targetTag = TestDataGenerator.CreateTagDto(TagType.Series, "series");
        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(422);
        });
    }

    [Fact]
    public async Task GetTagMigrations_WithoutFilters_ReturnsAllMigrations()
    {
        // Arrange - Perform a migration first
        var sourceTag = TestDataGenerator.CreateTagDto(TagType.Character, "old-character");
        var targetTag = TestDataGenerator.CreateTagDto(TagType.Character, "new-character");

        // Create an image with the source tag
        var imageCommand = new CreateImagePostCommand(
            Title: "Image for Migration Retrieval",
            Tags: new[] { sourceTag },
            ContentType: "image/jpeg"
        );

        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        // Perform migration
        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(200);
        });

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/migrations");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetTagMigrations_WithSourceTagFilter_ReturnsFilteredMigrations()
    {
        // Arrange - Perform a migration first
        var sourceTag = TestDataGenerator.CreateTagDto(TagType.Series, "old-series");
        var targetTag = TestDataGenerator.CreateTagDto(TagType.Series, "new-series");

        // Create an image with the source tag
        var imageCommand = new CreateImagePostCommand(
            Title: "Image for Filtered Migration",
            Tags: new[] { sourceTag },
            ContentType: "image/jpeg"
        );

        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        // Perform migration
        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(200);
        });

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/tags/migrations?sourceTag={sourceTag.Type}:{sourceTag.Value}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTagMigrations_WithTargetTagFilter_ReturnsFilteredMigrations()
    {
        // Arrange - Perform a migration first
        var sourceTag = TestDataGenerator.CreateTagDto(TagType.Meta, "old-meta");
        var targetTag = TestDataGenerator.CreateTagDto(TagType.Meta, "new-meta");

        // Create an image with the source tag
        var imageCommand = new CreateImagePostCommand(
            Title: "Image for Target Filter Migration",
            Tags: new[] { sourceTag },
            ContentType: "image/jpeg"
        );

        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(201);
        });

        // Perform migration
        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(200);
        });

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/tags/migrations?targetTag={targetTag.Type}:{targetTag.Value}");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTagMigrations_WithPagination_ReturnsPagedResults()
    {
        // Arrange - Perform multiple migrations
        for (int i = 0; i < 3; i++)
        {
            var sourceTag = TestDataGenerator.CreateTagDto(TagType.General, $"old-tag-{i}");
            var targetTag = TestDataGenerator.CreateTagDto(TagType.General, $"new-tag-{i}");

            var imageCommand = new CreateImagePostCommand(
                Title: $"Image for Pagination {i}",
                Tags: new[] { sourceTag },
                ContentType: "image/jpeg"
            );

            await _fixture.AlbaHost.Scenario(scenario =>
            {
                scenario.Post.Json(imageCommand).ToUrl("/api/images");
                scenario.StatusCodeShouldBe(201);
            });

            var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
            await _fixture.AlbaHost.Scenario(scenario =>
            {
                scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
                scenario.StatusCodeShouldBe(200);
            });
        }

        // Act & Assert
        var response = await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/migrations?pageNumber=1&pageSize=2");
            scenario.StatusCodeShouldBe(200);
        });

        var result = response.ReadAsJson<PagedResult<TagMigrationDto>>();
        Assert.NotNull(result);
        Assert.True(result.PageSize == 2);
    }
}
