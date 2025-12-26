using System.Net;
using Alba;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Application.Features.Tags.MigrateTag;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests.Tests;

public class TagEndpointsTests : IClassFixture<AlbaWebAppFixture>
{
    private readonly AlbaWebAppFixture _fixture;

    public TagEndpointsTests(AlbaWebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SearchTags_WithNoFilter_ReturnsOk()
    {
        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url("/api/tags/search");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task SearchTags_WithSearchTerm_ReturnsOk()
    {
        // Arrange - create an image with tags
        var command = new CreateImagePostCommand(
            "Test Image for Tag Search",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "searchable-character"),
                new TagDto(TagType.Artist, "searchable-artist")
            },
            "image/jpeg"
        );

        await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(command).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url("/api/tags/search?searchTerm=searchable");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task SearchTags_WithPagination_ReturnsOk()
    {
        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url("/api/tags/search?pageNumber=1&pageSize=10");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task MigrateTag_WithValidData_ReturnsOk()
    {
        // Arrange - create images with tags to migrate
        var command1 = new CreateImagePostCommand(
            "Image 1 for Migration",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "old-tag-name")
            },
            "image/jpeg"
        );

        var command2 = new CreateImagePostCommand(
            "Image 2 for Migration",
            new List<TagDto>
            {
                new TagDto(TagType.Character, "old-tag-name")
            },
            "image/jpeg"
        );

        await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(command1).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(command2).ToUrl("/api/images");
            s.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var migrateCommand = new MigrateTagCommand(
            new TagDto(TagType.Character, "old-tag-name"),
            new TagDto(TagType.Character, "new-tag-name")
        );

        // Act & Assert
        var result = await _fixture.Host.Scenario(s =>
        {
            s.Post.Json(migrateCommand).ToUrl("/api/tags/migrate");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var response = result.ReadAsJson<MigrateTagResponse>();
        Assert.NotNull(response);
        Assert.True(response.PostsMigrated >= 0);
    }

    [Fact]
    public async Task GetTagMigrations_WithNoFilter_ReturnsOk()
    {
        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url("/api/tags/migrations");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetTagMigrations_WithSourceTagFilter_ReturnsOk()
    {
        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url("/api/tags/migrations?sourceTag.Type=Character&sourceTag.Value=test");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetTagMigrations_WithPagination_ReturnsOk()
    {
        // Act & Assert
        await _fixture.Host.Scenario(s =>
        {
            s.Get.Url("/api/tags/migrations?pageNumber=1&pageSize=10");
            s.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }
}
