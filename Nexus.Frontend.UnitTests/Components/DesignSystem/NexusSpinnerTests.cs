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
        var squares = cut.FindAll(".nexus-spinner-square");
        Assert.Equal(3, squares.Count);
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
        Assert.Contains("w-2", container.ClassName);
        Assert.Contains("h-2", container.ClassName);
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
        Assert.Contains("w-4", container.ClassName);
        Assert.Contains("h-4", container.ClassName);
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
        Assert.Contains("w-6", container.ClassName);
        Assert.Contains("h-6", container.ClassName);
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
        Assert.Contains("w-8", container.ClassName);
        Assert.Contains("h-8", container.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithAllColors()
    {
        var colors = new[]
        {
            (SpinnerColor.Primary, "bg-purple-600"),
            (SpinnerColor.Success, "bg-emerald-600"),
            (SpinnerColor.Danger, "bg-red-600"),
            (SpinnerColor.Warning, "bg-amber-600"),
            (SpinnerColor.Info, "bg-blue-600"),
            (SpinnerColor.Accent, "bg-indigo-600"),
            (SpinnerColor.White, "bg-white")
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
