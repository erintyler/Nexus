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
/// Each test gets its own isolated Alba host instance with fresh database and RabbitMQ containers.
/// </summary>
public class TagEndpointsTests : IAsyncLifetime
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
    public async Task SearchTags_WithoutSearchTerm_ReturnsOk()
    {
        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/search");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task SearchTags_WithSearchTerm_ReturnsOk()
    {
        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/search?searchTerm=test");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task MigrateTag_WithValidTags_ReturnsOk()
    {
        // Arrange
        var command = new MigrateTagCommand(
            Source: new TagDto(TagType.General, "old-tag"),
            Target: new TagDto(TagType.General, "new-tag")
        );

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/tags/migrate");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetTagMigrations_WithoutFilters_ReturnsOk()
    {
        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/migrations");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetTagMigrations_WithPagination_ReturnsOk()
    {
        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Get.Url("/api/tags/migrations?pageNumber=1&pageSize=10");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }
}
