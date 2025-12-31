using Bunit;
using Xunit;
using static Nexus.Frontend.Client.Components.DesignSystem.NexusSpinner;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusSpinnerTests : Bunit.TestContext
{
    [Fact]
    public void NexusSpinner_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>();

        // Assert
        var spinner = cut.Find("svg");
        Assert.Contains("animate-spin", spinner.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithSmallSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.Small)
        );

        // Assert
        var spinner = cut.Find("svg");
        Assert.Contains("h-4 w-4", spinner.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithMediumSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.Medium)
        );

        // Assert
        var spinner = cut.Find("svg");
        Assert.Contains("h-8 w-8", spinner.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithLargeSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.Large)
        );

        // Assert
        var spinner = cut.Find("svg");
        Assert.Contains("h-12 w-12", spinner.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithExtraLargeSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.ExtraLarge)
        );

        // Assert
        var spinner = cut.Find("svg");
        Assert.Contains("h-16 w-16", spinner.ClassName);
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

            var spinner = cut.Find("svg");
            Assert.Contains(expectedClass, spinner.ClassName);
        }
    }
}
