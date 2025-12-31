using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;
using static Nexus.Frontend.Client.Components.DesignSystem.NexusDropdown;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusDropdownTests : Bunit.TestContext
{
    [Fact]
    public void NexusDropdown_RendersWithTriggerContent()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("Menu", button.TextContent);
    }

    [Fact]
    public void NexusDropdown_InitiallyClosed()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
            .Add(p => p.DropdownContent, builder => builder.AddMarkupContent(0, "<div>Dropdown Item</div>"))
        );

        // Assert - Dropdown content is rendered but hidden with pointer-events-none and opacity-0
        var dropdown = cut.Find("div.absolute");
        Assert.Contains("pointer-events-none", dropdown.ClassName);
        Assert.Contains("opacity-0", dropdown.ClassName);
    }

    [Fact]
    public void NexusDropdown_OpensOnClick()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
            .Add(p => p.DropdownContent, builder => builder.AddMarkupContent(0, "<div>Dropdown Item</div>"))
        );

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert - Dropdown is visible with pointer-events-auto and opacity-100
        var dropdown = cut.Find("div.absolute");
        Assert.Contains("pointer-events-auto", dropdown.ClassName);
        Assert.Contains("opacity-100", dropdown.ClassName);
    }

    [Fact]
    public void NexusDropdown_ClosesOnSecondClick()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
            .Add(p => p.DropdownContent, builder => builder.AddMarkupContent(0, "<div>Dropdown Item</div>"))
        );

        var button = cut.Find("button");

        // Act - Open
        button.Click();
        var dropdown = cut.Find("div.absolute");
        Assert.Contains("pointer-events-auto", dropdown.ClassName);

        // Act - Close
        button.Click();

        // Assert - Dropdown is hidden again
        dropdown = cut.Find("div.absolute");
        Assert.Contains("pointer-events-none", dropdown.ClassName);
        Assert.Contains("opacity-0", dropdown.ClassName);
    }

    [Fact]
    public void NexusDropdown_RotatesChevron_WhenOpen()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
        );

        var button = cut.Find("button");
        var chevronWrapper = cut.Find("span.transition-transform");

        // Initially not rotated
        Assert.DoesNotContain("rotate-180", chevronWrapper.ClassName);

        // Act
        button.Click();

        // Assert
        chevronWrapper = cut.Find("span.transition-transform");
        Assert.Contains("rotate-180", chevronWrapper.ClassName);
    }

    [Fact]
    public void NexusDropdown_RendersWithBottomLeftPosition()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
            .Add(p => p.DropdownContent, builder => builder.AddMarkupContent(0, "<div>Item</div>"))
            .Add(p => p.Position, DropdownPosition.BottomLeft)
            .Add(p => p.IsOpen, true)
        );

        // Assert
        var dropdown = cut.Find("div.absolute");
        Assert.Contains("left-0", dropdown.ClassName);
    }

    [Fact]
    public void NexusDropdown_RendersWithBottomRightPosition()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
            .Add(p => p.DropdownContent, builder => builder.AddMarkupContent(0, "<div>Item</div>"))
            .Add(p => p.Position, DropdownPosition.BottomRight)
            .Add(p => p.IsOpen, true)
        );

        // Assert
        var dropdown = cut.Find("div.absolute");
        Assert.Contains("right-0", dropdown.ClassName);
    }

    [Fact]
    public void NexusDropdown_RendersWithElevatedStyle()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
            .Add(p => p.DropdownContent, builder => builder.AddMarkupContent(0, "<div>Item</div>"))
            .Add(p => p.Style, DropdownStyle.Elevated)
            .Add(p => p.IsOpen, true)
        );

        // Assert
        var dropdown = cut.Find("div.absolute");
        Assert.Contains("bg-white", dropdown.ClassName);
    }

    [Fact]
    public void NexusDropdown_RendersWithGlassStyle()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
            .Add(p => p.DropdownContent, builder => builder.AddMarkupContent(0, "<div>Item</div>"))
            .Add(p => p.Style, DropdownStyle.Glass)
            .Add(p => p.IsOpen, true)
        );

        // Assert - Glass style no longer uses backdrop-blur to avoid stacking context issues
        var dropdown = cut.Find("div.absolute");
        Assert.Contains("bg-white/95", dropdown.ClassName);
        Assert.DoesNotContain("backdrop-blur", dropdown.ClassName);
    }

    [Fact]
    public void NexusDropdown_RendersWithGradientStyle()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
            .Add(p => p.DropdownContent, builder => builder.AddMarkupContent(0, "<div>Item</div>"))
            .Add(p => p.Style, DropdownStyle.Gradient)
            .Add(p => p.IsOpen, true)
        );

        // Assert
        var dropdown = cut.Find("div.absolute");
        Assert.Contains("bg-gradient-to-br", dropdown.ClassName);
    }

    [Fact]
    public void NexusDropdown_SetsAriaHaspopup()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Equal("true", button.GetAttribute("aria-haspopup"));
    }
}
