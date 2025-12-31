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
        var spinner = cut.Find("i");
        Assert.Contains("fa-spinner", spinner.ClassName);
        Assert.Contains("fa-spin", spinner.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithSmallSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.Small)
        );

        // Assert
        var spinner = cut.Find("i");
        Assert.Contains("fa-sm", spinner.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithMediumSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.Medium)
        );

        // Assert
        var spinner = cut.Find("i");
        Assert.Contains("fa-2x", spinner.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithLargeSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.Large)
        );

        // Assert
        var spinner = cut.Find("i");
        Assert.Contains("fa-3x", spinner.ClassName);
    }

    [Fact]
    public void NexusSpinner_RendersWithExtraLargeSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusSpinner>(parameters => parameters
            .Add(p => p.Size, SpinnerSize.ExtraLarge)
        );

        // Assert
        var spinner = cut.Find("i");
        Assert.Contains("fa-4x", spinner.ClassName);
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

            var wrapper = cut.Find("span");
            Assert.Contains(expectedClass, wrapper.ClassName);
        }
    }
}
