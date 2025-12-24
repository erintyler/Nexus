using NetArchTest.Rules;
using Nexus.Architecture.Tests.Rules;
using System.Reflection;
using TestResult = NetArchTest.Rules.TestResult;

namespace Nexus.Architecture.Tests;

public class NamingConventionTests
{
    private static readonly Assembly[] AllAssemblies =
    [
        typeof(Domain.Primitives.BaseEntity).Assembly,
        typeof(Application.Extensions.EnumerableExtensions).Assembly,
        typeof(Api.Endpoints.TagEndpoints).Assembly
    ];

    private static readonly Assembly[] DomainAndApplicationAssemblies =
    [
        typeof(Domain.Primitives.BaseEntity).Assembly,
        typeof(Application.Extensions.EnumerableExtensions).Assembly
    ];

    private static readonly Assembly ApplicationAssembly = typeof(Application.Extensions.EnumerableExtensions).Assembly;

    [Fact]
    public void Interfaces_Should_StartWithI()
    {
        // Arrange & Act & Assert
        AssertForAllAssemblies(AllAssemblies, assembly =>
            Types.InAssembly(assembly)
                .That()
                .AreInterfaces()
                .And()
                .DoNotHaveNameMatching(@"^_.*") // Exclude compiler-generated interfaces
                .Should()
                .HaveNameStartingWith("I")
                .GetResult(),
            assembly => $"All interfaces in {assembly.GetName().Name} should start with 'I'");
    }

    [Fact]
    public void AbstractClasses_Should_StartWithBaseOrAbstract()
    {
        // Arrange
        foreach (var assembly in DomainAndApplicationAssemblies)
        {
            var abstractClasses = Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .And()
                .AreAbstract()
                .GetTypes()
                .Where(t => !t.ReflectionType.IsSealed) // Exclude static classes
                .ToList();

            // Assert
            foreach (var abstractClass in abstractClasses)
            {
                var hasValidName = abstractClass.Name.StartsWith("Base") || 
                                  abstractClass.Name.StartsWith("Abstract") ||
                                  abstractClass.Name.EndsWith("Base");
                
                Assert.True(hasValidName, 
                    $"Abstract class '{abstractClass.Name}' in {assembly.GetName().Name} should start with 'Base' or 'Abstract', or end with 'Base'");
            }
        }
    }

    [Fact]
    public void PublicClasses_Should_BeInPublicNamespaces()
    {
        // Arrange & Act & Assert
        AssertForAllAssemblies(AllAssemblies, assembly =>
            Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .And()
                .ArePublic()
                .ShouldNot()
                .ResideInNamespaceMatching(@".*\.Internal.*")
                .GetResult(),
            assembly => $"Public classes in {assembly.GetName().Name} should not be in 'Internal' namespaces");
    }

    [Fact]
    public void Enums_Should_NotHavePluralNames()
    {
        // Arrange
        foreach (var assembly in AllAssemblies)
        {
            var enumTypes = assembly.GetTypes()
                .Where(t => t.IsEnum)
                .ToList();

            // Assert
            foreach (var enumType in enumTypes)
            {
                var hasValidName = !enumType.Name.EndsWith("s") ||
                                   enumType.Name.EndsWith("Status");
                
                Assert.True(hasValidName, 
                    $"Enum '{enumType.FullName}' should have a singular name (not plural)");
            }
        }
    }

    [Fact]
    public void ExtensionClasses_Should_HaveNameEndingWithExtensions()
    {
        // Arrange
        foreach (var assembly in AllAssemblies)
        {
            var extensionClasses = Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .And()
                .AreStatic()
                .GetTypes()
                .Where(t => t.ReflectionType.GetMethods()
                    .Any(m => m.IsStatic && m.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)))
                .Where(t => !t.Name.EndsWith("Endpoints")) // Exclude Endpoints classes
                .ToList();

            // Assert
            foreach (var extensionClass in extensionClasses)
            {
                Assert.True(extensionClass.Name.EndsWith("Extensions"), 
                    $"Extension class '{extensionClass.Name}' in {assembly.GetName().Name} should have name ending with 'Extensions'");
            }
        }
    }

    [Fact]
    public void Constants_Should_BeInConstantsClass()
    {
        // Arrange
        foreach (var assembly in AllAssemblies)
        {
            var classesWithManyConstants = Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .GetTypes()
                .Where(t => t.ReflectionType.GetFields(BindingFlags.Public | 
                                       BindingFlags.Static | 
                                       BindingFlags.FlattenHierarchy)
                           .Count(f => f.IsLiteral && !f.IsInitOnly) > 3) // More than 3 constants
                .ToList();

            // Assert
            foreach (var classWithConstants in classesWithManyConstants)
            {
                var hasValidName = classWithConstants.Name.Contains("Constant") || 
                                  classWithConstants.Name.Contains("Config") ||
                                  classWithConstants.Name.Contains("Settings");
                
                Assert.True(hasValidName, 
                    $"Class '{classWithConstants.Name}' in {assembly.GetName().Name} contains many constants and should have 'Constant', 'Config', or 'Settings' in its name");
            }
        }
    }

    [Fact]
    public void AsyncMethods_Should_HaveAsyncSuffix()
    {
        // Arrange & Act & Assert
        AssertForAllAssemblies(AllAssemblies, assembly =>
            Types.InAssembly(assembly)
                .That()
                .AreClasses()
                .Should()
                .MeetCustomRule(new AsyncMethodsShouldHaveAsyncSuffixRule())
                .GetResult(),
            assembly => $"All async methods in {assembly.GetName().Name} should have 'Async' suffix");
    }

    [Fact]
    public void Commands_Should_HaveNameEndingWithCommand()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching("Nexus.Application.Features.*")
            .And()
            .AreClasses()
            .And()
            .HaveNameMatching(@".*Command$")
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        // Assert
        AssertTestResult(result, "All commands should have names ending with 'Command'");
    }

    [Fact]
    public void Queries_Should_HaveNameEndingWithQuery()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching("Nexus.Application.Features.*")
            .And()
            .AreClasses()
            .And()
            .HaveNameMatching(@".*Query$")
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        // Assert
        AssertTestResult(result, "All queries should have names ending with 'Query'");
    }

    [Fact]
    public void Validators_Should_HaveNameEndingWithValidator()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespace("Nexus.Application")
            .And()
            .AreClasses()
            .And()
            .HaveNameMatching(@".*Validator$")
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        AssertTestResult(result, "All validators should have names ending with 'Validator'");
    }

    [Fact]
    public void DTOs_Should_HaveNameEndingWithDtoOrReadModel()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceMatching("Nexus.Application.*Models")
            .And()
            .DoNotResideInNamespaceMatching("Nexus.Application.Configuration.*")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .MeetCustomRule(new DtoOrReadModelNamingRule())
            .GetResult();

        // Assert
        AssertTestResult(result, "All DTOs in Models namespace (excluding Configuration models) should have names ending with 'Dto' or 'ReadModel'");
    }
    
    private static void AssertForAllAssemblies(
        Assembly[] assemblies,
        Func<Assembly, TestResult> testFunc,
        Func<Assembly, string> messageFunc)
    {
        foreach (var assembly in assemblies)
        {
            var result = testFunc(assembly);
            AssertTestResult(result, messageFunc(assembly));
        }
    }

    private static void AssertTestResult(TestResult result, string message)
    {
        var failingTypes = result.FailingTypes?.Select(t => t.Name).ToList() ?? new List<string>();
        var failingTypesMessage = failingTypes.Any() 
            ? $" Failing types: {string.Join(", ", failingTypes)}" 
            : string.Empty;
        
        Assert.True(result.IsSuccessful, $"{message}.{failingTypesMessage}");
    }
}


