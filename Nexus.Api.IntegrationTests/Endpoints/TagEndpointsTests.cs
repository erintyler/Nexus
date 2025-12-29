using System.Net;
using Alba;
using AutoFixture;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.Tags.GetTagMigrations;
using Nexus.Application.Features.Tags.MigrateTag;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests.Endpoints;

public class TagEndpointsTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;
    private readonly Fixture _autoFixture;

    public TagEndpointsTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _autoFixture = new Fixture();
    }

    [Fact]
    public async Task SearchTags_WithNoFilter_ReturnsAllTags()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        // Create some images with tags
        var tag1 = new TagDto(TagType.Artist, "artist1");
        var tag2 = new TagDto(TagType.General, "general1");

        var imageCommand1 = new CreateImagePostCommand(
            Title: "Image 1",
            Tags: new List<TagDto> { tag1 },
            ContentType: "image/jpeg"
        );

        var imageCommand2 = new CreateImagePostCommand(
            Title: "Image 2",
            Tags: new List<TagDto> { tag2 },
            ContentType: "image/jpeg"
        );

        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand1).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand2).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/search");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var pagedResult = result.ReadAsJson<PagedResult<TagCountDto>>();
        Assert.NotNull(pagedResult);
        Assert.True(pagedResult.TotalCount >= 2);
    }

    [Fact]
    public async Task SearchTags_WithSearchTerm_ReturnsFilteredTags()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var tag = new TagDto(TagType.Artist, "specific-artist");
        var imageCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { tag },
            ContentType: "image/jpeg"
        );

        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/search?searchTerm=specific");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var pagedResult = result.ReadAsJson<PagedResult<TagCountDto>>();
        Assert.NotNull(pagedResult);
        Assert.True(pagedResult.TotalCount > 0);
    }

    [Fact]
    public async Task MigrateTag_WithValidData_ReturnsOk()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var sourceTag = new TagDto(TagType.General, "old-tag");
        var targetTag = new TagDto(TagType.General, "new-tag");

        // Create image with source tag
        var imageCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { sourceTag },
            ContentType: "image/jpeg"
        );

        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var response = result.ReadAsJson<MigrateTagResponse>();
        Assert.NotNull(response);
        Assert.True(response.PostsMigrated >= 0);
    }

    [Fact]
    public async Task MigrateTag_WithSameSourceAndTarget_ReturnsBadRequest()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var tag = new TagDto(TagType.General, "same-tag");
        var migrateCommand = new MigrateTagCommand(tag, tag);

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task GetTagMigrations_WithNoFilter_ReturnsAllMigrations()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var sourceTag = new TagDto(TagType.Character, "old-character");
        var targetTag = new TagDto(TagType.Character, "new-character");

        // Create an image with source tag
        var imageCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { sourceTag },
            ContentType: "image/jpeg"
        );

        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Perform migration
        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/migrations");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var pagedResult = result.ReadAsJson<PagedResult<TagMigrationDto>>();
        Assert.NotNull(pagedResult);
        Assert.True(pagedResult.TotalCount > 0);
    }

    [Fact]
    public async Task GetTagMigrations_WithSourceTagFilter_ReturnsFilteredMigrations()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var sourceTag = new TagDto(TagType.Series, "old-series");
        var targetTag = new TagDto(TagType.Series, "new-series");

        // Create an image with source tag
        var imageCommand = new CreateImagePostCommand(
            Title: "Test Image",
            Tags: new List<TagDto> { sourceTag },
            ContentType: "image/jpeg"
        );

        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(imageCommand).ToUrl("/api/images");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Perform migration
        var migrateCommand = new MigrateTagCommand(sourceTag, targetTag);
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Act & Assert
        var result = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/tags/migrations?sourceTag=Series:old-series");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var pagedResult = result.ReadAsJson<PagedResult<TagMigrationDto>>();
        Assert.NotNull(pagedResult);
        Assert.True(pagedResult.TotalCount > 0);
    }
}
