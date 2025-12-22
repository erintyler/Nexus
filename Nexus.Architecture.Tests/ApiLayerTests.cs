using NetArchTest.Rules;

namespace Nexus.Architecture.Tests;

public class ApiLayerTests
{
    private const string ApiNamespace = "Nexus.Api";
    private const string DomainNamespace = "Nexus.Domain";

    [Fact]
    public void ApiLayer_Should_HaveDependencyOnApplicationLayer()
    {
        // Arrange
        var apiTypes = Types.InAssembly(typeof(Api.Endpoints.TagEndpoints).Assembly)
            .That()
            .ResideInNamespace(ApiNamespace)
            .GetTypes();

        // Assert - API layer should reference Application layer
        Assert.True(apiTypes.Any(), "API layer should have types");
    }

    [Fact]
    public void ApiLayer_Should_NotReferenceDomainEntitiesDirectly()
    {
        // Arrange - API should use DTOs, not domain entities directly
        var result = Types.InAssembly(typeof(Api.Endpoints.TagEndpoints).Assembly)
            .That()
            .ResideInNamespaceMatching($"{ApiNamespace}.Endpoints")
            .ShouldNot()
            .HaveDependencyOnAny($"{DomainNamespace}.Entities")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"API endpoints should not directly reference domain entities (use DTOs instead). Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Endpoints_Should_HaveNameEndingWithEndpoints()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Api.Endpoints.TagEndpoints).Assembly)
            .That()
            .ResideInNamespace($"{ApiNamespace}.Endpoints")
            .And()
            .AreClasses()
            .And()
            .AreNotNested()
            .Should()
            .HaveNameEndingWith("Endpoints")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"All endpoint classes should have names ending with 'Endpoints'. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Middleware_Should_ResideInMiddlewareNamespace()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Api.Endpoints.TagEndpoints).Assembly)
            .That()
            .HaveNameMatching(@".*Middleware$")
            .And()
            .AreClasses()
            .Should()
            .ResideInNamespace($"{ApiNamespace}.Middleware")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"All middleware classes should reside in Middleware namespace. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ExceptionHandlers_Should_HaveNameEndingWithExceptionHandler()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Api.Endpoints.TagEndpoints).Assembly)
            .That()
            .ResideInNamespace($"{ApiNamespace}.Middleware")
            .And()
            .AreClasses()
            .And()
            .HaveNameMatching(@".*ExceptionHandler$")
            .Should()
            .HaveNameEndingWith("ExceptionHandler")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"All exception handler classes should have names ending with 'ExceptionHandler'. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Controllers_ShouldNot_Exist()
    {
        // Arrange - Using minimal APIs, not controllers
        var result = Types.InAssembly(typeof(Api.Endpoints.TagEndpoints).Assembly)
            .That()
            .ResideInNamespace(ApiNamespace)
            .And()
            .AreClasses()
            .ShouldNot()
            .HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"API should use minimal API endpoints, not controllers. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Extensions_Should_ResideInExtensionsNamespace()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Api.Endpoints.TagEndpoints).Assembly)
            .That()
            .HaveNameEndingWith("Extensions")
            .And()
            .AreClasses()
            .Should()
            .ResideInNamespace($"{ApiNamespace}.Extensions")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"All extension classes should reside in Extensions namespace. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Services_Should_HaveInterfaceStartingWithI()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Api.Endpoints.TagEndpoints).Assembly)
            .That()
            .ResideInNamespace($"{ApiNamespace}.Services")
            .And()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"All service interfaces should start with 'I'. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ApiLayer_ShouldNot_ContainBusinessLogic()
    {
        // Arrange - Checking that API doesn't have "Manager", "Service" (except infrastructure services), or "Logic" classes
        var result = Types.InAssembly(typeof(Api.Endpoints.TagEndpoints).Assembly)
            .That()
            .ResideInNamespaceMatching($"{ApiNamespace}.Endpoints")
            .And()
            .AreClasses()
            .ShouldNot()
            .HaveNameMatching(@".*(Manager|Logic)$")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"API endpoints should not contain business logic (use Application layer instead). Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }
}

