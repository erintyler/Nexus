using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusDropdownItemTests : Bunit.TestContext
{
    [Fact]
    public void NexusDropdownItem_RendersWithContent()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.ChildContent, "Profile")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Contains("Profile", link.TextContent);
    }

    [Fact]
    public void NexusDropdownItem_SetsHref()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.Href, "/profile")
            .Add(p => p.ChildContent, "Profile")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Equal("/profile", link.GetAttribute("href"));
    }

    [Fact]
    public void NexusDropdownItem_RendersIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.Icon, builder => builder.AddMarkupContent(0, "<svg class='icon'></svg>"))
            .Add(p => p.ChildContent, "Settings")
        );

        // Assert
        var icon = cut.Find("svg.icon");
        Assert.NotNull(icon);
    }

    [Fact]
    public void NexusDropdownItem_RendersBadge()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.Badge, builder => builder.AddMarkupContent(0, "<span class='badge'>3</span>"))
            .Add(p => p.ChildContent, "Notifications")
        );

        // Assert
        var badge = cut.Find("span.badge");
        Assert.Contains("3", badge.TextContent);
    }

    [Fact]
    public void NexusDropdownItem_RendersDescription()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.ChildContent, "Account")
            .Add(p => p.Description, "Manage your account settings")
        );

        // Assert
        Assert.Contains("Manage your account settings", cut.Markup);
    }

    [Fact]
    public void NexusDropdownItem_ShowsDangerState_WhenIsDangerIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.IsDanger, true)
            .Add(p => p.ChildContent, "Delete")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Contains("text-red-700", link.ClassName);
        Assert.Contains("hover:bg-red-50", link.ClassName);
    }

    [Fact]
    public void NexusDropdownItem_ShowsNormalState_WhenIsDangerIsFalse()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.IsDanger, false)
            .Add(p => p.ChildContent, "Edit")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Contains("text-gray-700", link.ClassName);
        Assert.Contains("hover:bg-purple-50", link.ClassName);
    }

    [Fact]
    public void NexusDropdownItem_ShowsDisabledState_WhenIsDisabledIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.IsDisabled, true)
            .Add(p => p.ChildContent, "Disabled")
        );

        // Assert
        var link = cut.Find("a");
        Assert.Contains("cursor-not-allowed", link.ClassName);
        Assert.Contains("opacity-60", link.ClassName);
    }

    [Fact]
    public void NexusDropdownItem_TriggersOnClick_WhenClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true))
            .Add(p => p.ChildContent, "Clickable")
        );

        // Act
        var link = cut.Find("a");
        link.Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void NexusDropdownItem_DoesNotTriggerOnClick_WhenDisabled()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdownItem>(parameters => parameters
            .Add(p => p.IsDisabled, true)
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true))
            .Add(p => p.ChildContent, "Disabled")
        );

        // Act
        var link = cut.Find("a");
        link.Click();

        // Assert
        Assert.False(clicked);
    }
}
