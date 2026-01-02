using Bunit;
using Xunit;
using static Nexus.Frontend.Client.Components.DesignSystem.NexusSkeleton;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusSkeletonTests : Bunit.TestContext
{
    [Fact]
    public void NexusSkeleton_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSkeleton>();

        // Assert
        var skeleton = cut.Find("div");
        Assert.Contains("animate-pulse", skeleton.ClassName);
        Assert.Contains("w-full", skeleton.ClassName);
        Assert.Contains("h-4", skeleton.ClassName);
    }

    [Fact]
    public void NexusSkeleton_RendersWithRectangleShape()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSkeleton>(parameters => parameters
            .Add(p => p.Shape, SkeletonShape.Rectangle)
        );

        // Assert
        var skeleton = cut.Find("div");
        Assert.Contains("rounded-lg", skeleton.ClassName);
    }

    [Fact]
    public void NexusSkeleton_RendersWithCircleShape()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSkeleton>(parameters => parameters
            .Add(p => p.Shape, SkeletonShape.Circle)
        );

        // Assert
        var skeleton = cut.Find("div");
        Assert.Contains("rounded-full", skeleton.ClassName);
    }

    [Fact]
    public void NexusSkeleton_RendersWithTextShape()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSkeleton>(parameters => parameters
            .Add(p => p.Shape, SkeletonShape.Text)
        );

        // Assert
        var skeleton = cut.Find("div");
        Assert.Contains("rounded", skeleton.ClassName);
    }

    [Fact]
    public void NexusSkeleton_SetsCustomWidth()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSkeleton>(parameters => parameters
            .Add(p => p.Width, "w-1/2")
        );

        // Assert
        var skeleton = cut.Find("div");
        Assert.Contains("w-1/2", skeleton.ClassName);
    }

    [Fact]
    public void NexusSkeleton_SetsCustomHeight()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSkeleton>(parameters => parameters
            .Add(p => p.Height, "h-12")
        );

        // Assert
        var skeleton = cut.Find("div");
        Assert.Contains("h-12", skeleton.ClassName);
    }

    [Fact]
    public void NexusSkeleton_CombinesCustomWidthAndHeight()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSkeleton>(parameters => parameters
            .Add(p => p.Width, "w-3/4")
            .Add(p => p.Height, "h-8")
        );

        // Assert
        var skeleton = cut.Find("div");
        Assert.Contains("w-3/4", skeleton.ClassName);
        Assert.Contains("h-8", skeleton.ClassName);
    }
}
