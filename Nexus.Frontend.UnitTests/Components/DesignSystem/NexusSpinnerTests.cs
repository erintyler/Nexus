using Bunit;
using Xunit;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusSpinnerTests : Bunit.TestContext
{
    [Fact]
    public void NexusSpinner_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>();

        // Assert
        var container = cut.Find(".nexus-spinner-container");
        var rings = cut.FindAll(".nexus-spinner-ring");
        Assert.Equal(3, rings.Count);
    }

    [Fact]
    public void NexusSpinner_RendersWithSmallSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.Small)
        );

        // Assert
        var container = cut.Find(".nexus-spinner-container");
        Assert.Contains("w-8", container.ClassName);
        Assert.Contains("h-8", container.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithMediumSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.Medium)
        );

        // Assert
        var container = cut.Find(".nexus-spinner-container");
        Assert.Contains("w-12", container.ClassName);
        Assert.Contains("h-12", container.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithLargeSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.Large)
        );

        // Assert
        var container = cut.Find(".nexus-spinner-container");
        Assert.Contains("w-16", container.ClassName);
        Assert.Contains("h-16", container.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithExtraLargeSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.ExtraLarge)
        );

        // Assert
        var container = cut.Find(".nexus-spinner-container");
        Assert.Contains("w-24", container.ClassName);
        Assert.Contains("h-24", container.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithAllColors()
    {
        var colors = new[]
        {
            (SpinnerColor.Primary, "text-purple-600"),
            (SpinnerColor.Success, "text-emerald-600"),
            (SpinnerColor.Danger, "text-red-600"),
            (SpinnerColor.Warning, "text-amber-600"),
            (SpinnerColor.Info, "text-blue-600"),
            (SpinnerColor.Accent, "text-indigo-600"),
            (SpinnerColor.White, "text-white")
        };

        foreach (var (color, expectedClass) in colors)
        {
            var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
                .Add(p => p.Color, color)
            );

            var container = cut.Find(".nexus-spinner-container");
            Assert.Contains(expectedClass, container.ClassName);
        }
    }
}
