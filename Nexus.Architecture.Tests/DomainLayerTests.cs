using NetArchTest.Rules;

namespace Nexus.Architecture.Tests;

public class DomainLayerTests
{
    private const string DomainNamespace = "Nexus.Domain";
    private const string ApplicationNamespace = "Nexus.Application";
    private const string ApiNamespace = "Nexus.Api";
    private const string InfrastructureNamespace = "Nexus.Infrastructure";

    [Fact]
    public void DomainLayer_Should_NotHaveDependencyOnApplicationLayer()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOnAll(ApplicationNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain layer should not depend on Application layer. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void DomainLayer_Should_NotHaveDependencyOnApiLayer()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOnAll(ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain layer should not depend on API layer. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void DomainLayer_Should_NotHaveDependencyOnInfrastructureLayer()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOnAll(InfrastructureNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain layer should not depend on Infrastructure layer. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void DomainLayer_Should_OnlyDependOnSystemNamespaces()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace(DomainNamespace)
            .Should()
            .OnlyHaveDependencyOn("System", DomainNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain layer should only depend on System namespaces and itself. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Entities_Should_InheritFromEntityBaseClass()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Entities")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .Inherit(typeof(Domain.Primitives.BaseEntity))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All entities should inherit from Entity base class. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ValueObjects_Should_InheritFromValueObjectBaseClass()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.ValueObjects")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .Inherit(typeof(Domain.Primitives.BaseValueObject))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All value objects should inherit from ValueObject base class. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void DomainEvents_Should_ImplementIDomainEventInterface()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Events")
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("DomainEvent")
            .Should()
            .ImplementInterface(typeof(Domain.Events.INexusEvent))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All domain events should implement INexusEvent interface. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void DomainEvents_Should_BeImmutable()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Events")
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("DomainEvent")
            .Should()
            .BeImmutable()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All domain events should be immutable (records or classes with readonly properties). Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void DomainEvents_Should_HaveNameEndingWithDomainEvent()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Events")
            .And()
            .AreClasses()
            .And()
            .ImplementInterface(typeof(Domain.Events.INexusEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All domain events should have names ending with 'DomainEvent'. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Entities_Should_BeSealed()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Entities")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All entities should be sealed to prevent inheritance. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ValueObjects_Should_BeSealed()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Domain.Primitives.BaseEntity).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.ValueObjects")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All value objects should be sealed to prevent inheritance. Failing types: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}");
    }
}

