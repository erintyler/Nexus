using NetArchTest.Rules;

namespace Nexus.Architecture.Tests;

public class ApplicationLayerTests
{
    private const string ApplicationNamespace = "Nexus.Application";
    private const string ApiNamespace = "Nexus.Api";

    [Fact]
    public void ApplicationLayer_Should_NotHaveDependencyOnApiLayer()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Application.Extensions.EnumerableExtensions).Assembly)
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Application layer should not depend on API layer. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }


    [Fact]
    public void ApplicationLayer_Should_NotHaveDependencyOnInfrastructureLibraries()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Application.Extensions.EnumerableExtensions).Assembly)
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "System.Data",
                "System.Net.Http.Json")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Application layer should not depend on infrastructure-specific libraries. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }


    [Fact]
    public void Services_Should_HaveInterfaceInSameNamespace()
    {
        // Arrange
        var serviceInterfaces = Types.InAssembly(typeof(Application.Extensions.EnumerableExtensions).Assembly)
            .That()
            .ResideInNamespaceMatching($"{ApplicationNamespace}.*Services")
            .And()
            .AreInterfaces()
            .GetTypes()
            .ToList();

        var serviceImplementations = Types.InAssembly(typeof(Application.Extensions.EnumerableExtensions).Assembly)
            .That()
            .ResideInNamespaceMatching($"{ApplicationNamespace}.*Services")
            .And()
            .AreClasses()
            .GetTypes()
            .ToList();

        // Assert - Check that implementations have corresponding interfaces
        foreach (var implementation in serviceImplementations)
        {
            var expectedInterfaceName = $"I{implementation.Name}";
            var hasInterface = implementation.ReflectionType.GetInterfaces()
                .Any(i => i.Name == expectedInterfaceName && serviceInterfaces.Any(si => si.FullName == i.FullName));

            Assert.True(hasInterface,
                $"Service implementation '{implementation.Name}' should have a corresponding interface 'I{implementation.Name}' in the same namespace");
        }
    }
}

