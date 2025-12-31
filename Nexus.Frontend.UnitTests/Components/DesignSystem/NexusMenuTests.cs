using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;
using static Nexus.Frontend.Client.Components.DesignSystem.NexusMenu;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusMenuTests : Bunit.TestContext
{
    [Fact]
    public void NexusMenu_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>();

        // Assert
        var nav = cut.Find("nav");
        Assert.NotNull(nav);
        Assert.Contains("shadow-lg", nav.ClassName);
    }

    [Fact]
    public void NexusMenu_RendersWithElevatedStyle()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.Style, MenuStyle.Elevated)
        );

        // Assert
        var nav = cut.Find("nav");
        Assert.Contains("bg-white/95", nav.ClassName);
        Assert.Contains("shadow-lg", nav.ClassName);
    }

    [Fact]
    public void NexusMenu_RendersWithGlassStyle()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.Style, MenuStyle.Glass)
        );

        // Assert
        var nav = cut.Find("nav");
        Assert.Contains("bg-white/90", nav.ClassName);
        Assert.Contains("shadow-md", nav.ClassName);
    }

    [Fact]
    public void NexusMenu_RendersWithGradientStyle()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.Style, MenuStyle.Gradient)
        );

        // Assert
        var nav = cut.Find("nav");
        Assert.Contains("bg-gradient-to-r", nav.ClassName);
        Assert.Contains("from-purple-600", nav.ClassName);
    }

    [Fact]
    public void NexusMenu_RendersWithMinimalStyle()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.Style, MenuStyle.Minimal)
        );

        // Assert
        var nav = cut.Find("nav");
        Assert.Contains("bg-white", nav.ClassName);
        Assert.Contains("border-b", nav.ClassName);
    }

    [Fact]
    public void NexusMenu_IsSticky_WhenIsStickyIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.IsSticky, true)
        );

        // Assert
        var nav = cut.Find("nav");
        Assert.Contains("sticky", nav.ClassName);
        Assert.Contains("top-0", nav.ClassName);
    }

    [Fact]
    public void NexusMenu_NotSticky_WhenIsStickyIsFalse()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.IsSticky, false)
        );

        // Assert
        var nav = cut.Find("nav");
        Assert.DoesNotContain("sticky", nav.ClassName);
    }

    [Fact]
    public void NexusMenu_RendersBrandContent()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.BrandContent, builder => builder.AddMarkupContent(0, "<span>MyBrand</span>"))
        );

        // Assert
        Assert.Contains("MyBrand", cut.Markup);
    }

    [Fact]
    public void NexusMenu_RendersMenuItems()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.MenuItems, builder => builder.AddMarkupContent(0, "<a>Home</a><a>About</a>"))
        );

        // Assert
        Assert.Contains("Home", cut.Markup);
        Assert.Contains("About", cut.Markup);
    }

    [Fact]
    public void NexusMenu_ShowsMobileMenuButton()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>();

        // Assert
        var mobileButton = cut.Find("button[aria-label='Toggle menu']");
        Assert.NotNull(mobileButton);
    }

    [Fact]
    public void NexusMenu_TogglesMobileMenu_WhenButtonClicked()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.MobileMenuItems, builder => builder.AddMarkupContent(0, "<div class='mobile-item'>Mobile Item</div>"))
        );

        // Initially mobile menu should not be visible
        Assert.DoesNotContain("Mobile Item", cut.Markup);

        // Act
        var mobileButton = cut.Find("button[aria-label='Toggle menu']");
        mobileButton.Click();

        // Assert
        Assert.Contains("Mobile Item", cut.Markup);

        // Act - Click again to close
        mobileButton.Click();

        // Assert - Menu should be hidden again
        Assert.DoesNotContain("Mobile Item", cut.Markup);
    }

    [Fact]
    public void NexusMenu_UsesMobileMenuItemsWhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.MenuItems, builder => builder.AddMarkupContent(0, "<a>Desktop</a>"))
            .Add(p => p.MobileMenuItems, builder => builder.AddMarkupContent(0, "<a>Mobile</a>"))
        );

        // Open mobile menu
        var mobileButton = cut.Find("button[aria-label='Toggle menu']");
        mobileButton.Click();

        // Assert
        Assert.Contains("Mobile", cut.Markup);
    }

    [Fact]
    public void NexusMenu_UsesFullWidthContainer_WhenIsFullWidthIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.IsFullWidth, true)
        );

        // Assert
        var container = cut.Find("div.flex.items-center");
        Assert.Contains("w-full", container.ClassName);
    }

    [Fact]
    public void NexusMenu_UsesMaxWidthContainer_WhenIsFullWidthIsFalse()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusMenu>(parameters => parameters
            .Add(p => p.IsFullWidth, false)
        );

        // Assert
        var container = cut.Find("div.flex.items-center");
        Assert.Contains("max-w-7xl", container.ClassName);
    }
}
