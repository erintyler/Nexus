using System.Net;
using Alba;
using AutoFixture;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.Tags.MigrateTag;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests;

/// <summary>
/// Integration tests for Tag API endpoints.
/// Tests the full HTTP request/response cycle including Wolverine and Marten.
/// Each test gets its own isolated Alba host instance.
/// </summary>
public class TagEndpointsTests : IClassFixture<AlbaWebApplicationFixture>
{
    private readonly AlbaWebApplicationFixture _fixture;
    private readonly Fixture _autoFixture = new();

    public TagEndpointsTests(AlbaWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SearchTags_WithoutSearchTerm_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/search");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task SearchTags_WithSearchTerm_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/search?searchTerm=test");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task MigrateTag_WithValidTags_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var command = new MigrateTagCommand(
            Source: new TagDto(TagType.General, "old-tag"),
            Target: new TagDto(TagType.General, "new-tag")
        );

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetTagMigrations_WithoutFilters_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/migrations");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetTagMigrations_WithPagination_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/migrations?pageNumber=1&pageSize=10");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }
}
