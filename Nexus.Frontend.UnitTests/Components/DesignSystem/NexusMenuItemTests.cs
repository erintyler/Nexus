using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusMenuItemTests : Bunit.TestContext
{
    [Fact]
    public void NexusMenuItem_RendersWithContent()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenuItem>(parameters => parameters
            .Add(p => p.ChildContent, "Home")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Contains("Home", link.TextContent);
    }

    [Fact]
    public void NexusMenuItem_SetsHref()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenuItem>(parameters => parameters
            .Add(p => p.Href, "/home")
            .Add(p => p.ChildContent, "Home")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Equal("/home", link.GetAttribute("href"));
    }

    [Fact]
    public void NexusMenuItem_ShowsActiveState_WhenIsActiveIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenuItem>(parameters => parameters
            .Add(p => p.IsActive, true)
            .Add(p => p.ChildContent, "Active")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Contains("bg-gradient-to-r", link.ClassName);
        Assert.Contains("text-white", link.ClassName);
    }

    [Fact]
    public void NexusMenuItem_ShowsInactiveState_WhenIsActiveIsFalse()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenuItem>(parameters => parameters
            .Add(p => p.IsActive, false)
            .Add(p => p.ChildContent, "Inactive")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Contains("text-gray-700", link.ClassName);
        Assert.Contains("hover:bg-purple-50", link.ClassName);
    }

    [Fact]
    public void NexusMenuItem_RendersIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenuItem>(parameters => parameters
            .Add(p => p.Icon, builder => builder.AddMarkupContent(0, "<svg class='icon'></svg>"))
            .Add(p => p.ChildContent, "Home")
        );

        // Assert
        var icon = cut.Find("svg.icon");
        Assert.NotNull(icon);
    }

    [Fact]
    public void NexusMenuItem_RendersBadge()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenuItem>(parameters => parameters
            .Add(p => p.Badge, builder => builder.AddMarkupContent(0, "<span class='badge'>New</span>"))
            .Add(p => p.ChildContent, "Messages")
        );

        // Assert
        var badge = cut.Find("span.badge");
        Assert.Contains("New", badge.TextContent);
    }

    [Fact]
    public void NexusMenuItem_UsesMobileStyling_WhenIsMobileIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenuItem>(parameters => parameters
            .Add(p => p.IsMobile, true)
            .Add(p => p.ChildContent, "Mobile")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Contains("w-full", link.ClassName);
    }

    [Fact]
    public void NexusMenuItem_TriggersOnClick_WhenClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenuItem>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true))
            .Add(p => p.ChildContent, "Clickable")
        );

        // Act
        var link = cut.Find("a");
        link.Click();

        // Assert
        Assert.True(clicked);
    }
}
