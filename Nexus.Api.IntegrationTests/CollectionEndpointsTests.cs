using System.Net;
using Alba;
using AutoFixture;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Models;
using Nexus.Application.Features.Collections.CreateCollection;
using Nexus.Application.Features.ImagePosts.CreateImagePost;
using Nexus.Domain.Enums;
using Xunit;

namespace Nexus.Api.IntegrationTests;

/// <summary>
/// Integration tests for Collection API endpoints.
/// Tests the full HTTP request/response cycle including Wolverine and Marten.
/// Each test gets its own isolated Alba host instance.
/// </summary>
public class CollectionEndpointsTests : IClassFixture<AlbaWebApplicationFixture>
{
    private readonly AlbaWebApplicationFixture _fixture;
    private readonly Fixture _autoFixture = new();

    public CollectionEndpointsTests(AlbaWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateCollection_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var command = new CreateCollectionCommand(
            Title: _autoFixture.Create<string>()
        );

        // Act
        var result = await host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        // Assert
        var response = result.ReadAsJson<CreateCollectionResponse>();
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(command.Title, response.Title);
    }

    [Fact]
    public async Task CreateCollection_WithEmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var command = new CreateCollectionCommand(
            Title: ""
        );

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task GetCollectionById_WhenCollectionExists_ReturnsOk()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var createCommand = new CreateCollectionCommand(
            Title: _autoFixture.Create<string>()
        );

        var createResult = await host.Scenario(scenario =>
        {
            scenario.Post.Json(createCommand).ToUrl("/api/collections");
            scenario.StatusCodeShouldBe(HttpStatusCode.Created);
        });

        var createdCollection = createResult.ReadAsJson<CreateCollectionResponse>();

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{createdCollection.Id}");
            scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetCollectionById_WhenCollectionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Get.Url($"/api/collections/{nonExistentId}");
            scenario.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }
}
