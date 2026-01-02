using Bunit;
using Xunit;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusIconTests : Bunit.TestContext
{
    [Fact]
    public void NexusIcon_RendersCorrectly()
    {
        // Act
        var cut = RenderComponent<Nexus.Frontend.Client.Components.DesignSystem.NexusIcon>(parameters => parameters
            .Add(p => p.Icon, "fa-solid fa-bars"));

        // Assert
        var icon = cut.Find("i");
        Assert.NotNull(icon);
    }

    [Theory]
    [InlineData("fa-xs", "fa-xs")]
    [InlineData("fa-sm", "fa-sm")]
    [InlineData("fa-lg", "fa-lg")]
    [InlineData("fa-2x", "fa-2x")]
    public void NexusIcon_AppliesCorrectSize(string size, string expectedClass)
    {
        // Act
        var cut = RenderComponent<Nexus.Frontend.Client.Components.DesignSystem.NexusIcon>(parameters => parameters
            .Add(p => p.Icon, "fa-solid fa-user")
            .Add(p => p.Size, size));

        // Assert
        var icon = cut.Find("i");
        Assert.Contains(expectedClass, icon.GetAttribute("class"));
    }

    [Theory]
    [InlineData("fa-solid fa-bars")]
    [InlineData("fa-solid fa-xmark")]
    [InlineData("fa-solid fa-user")]
    [InlineData("fa-solid fa-gear")]
    [InlineData("fa-solid fa-circle-check")]
    [InlineData("fa-solid fa-triangle-exclamation")]
    [InlineData("fa-solid fa-circle-xmark")]
    [InlineData("fa-solid fa-circle-info")]
    public void NexusIcon_RendersAllIconTypes(string iconClass)
    {
        // Act
        var cut = RenderComponent<Nexus.Frontend.Client.Components.DesignSystem.NexusIcon>(parameters => parameters
            .Add(p => p.Icon, iconClass));

        // Assert
        var icon = cut.Find("i");
        Assert.NotNull(icon);
        Assert.Contains(iconClass, icon.GetAttribute("class"));
    }

    [Fact]
    public void NexusIcon_AcceptsAdditionalAttributes()
    {
        // Act
        var cut = RenderComponent<Nexus.Frontend.Client.Components.DesignSystem.NexusIcon>(parameters => parameters
            .Add(p => p.Icon, "fa-solid fa-home")
            .AddUnmatched("data-testid", "test-icon"));

        // Assert
        var icon = cut.Find("i");
        Assert.Equal("test-icon", icon.GetAttribute("data-testid"));
    }

    [Fact]
    public void NexusIcon_HandlesShortIconNames()
    {
        // Act - test that "user" becomes "fa-solid fa-user"
        var cut = RenderComponent<Nexus.Frontend.Client.Components.DesignSystem.NexusIcon>(parameters => parameters
            .Add(p => p.Icon, "user"));

        // Assert
        var icon = cut.Find("i");
        Assert.Contains("fa-solid fa-user", icon.GetAttribute("class"));
    }

    [Fact]
    public void NexusIcon_PreservesFullClassNames()
    {
        // Act - test that full class names are preserved
        var cut = RenderComponent<Nexus.Frontend.Client.Components.DesignSystem.NexusIcon>(parameters => parameters
            .Add(p => p.Icon, "fa-brands fa-github"));

        // Assert
        var icon = cut.Find("i");
        Assert.Contains("fa-brands fa-github", icon.GetAttribute("class"));
    }
}
