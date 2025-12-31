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

        // Assert
        Assert.DoesNotContain("Dropdown Item", cut.Markup);
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

        // Assert
        Assert.Contains("Dropdown Item", cut.Markup);
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
        Assert.Contains("Dropdown Item", cut.Markup);

        // Act - Close
        button.Click();

        // Assert
        Assert.DoesNotContain("Dropdown Item", cut.Markup);
    }

    [Fact]
    public void NexusDropdown_RotatesChevron_WhenOpen()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusDropdown>(parameters => parameters
            .Add(p => p.TriggerContent, "Menu")
        );

        var button = cut.Find("button");
        var chevron = cut.Find("svg");

        // Initially not rotated
        Assert.DoesNotContain("rotate-180", chevron.ClassName);

        // Act
        button.Click();

        // Assert
        chevron = cut.Find("svg");
        Assert.Contains("rotate-180", chevron.ClassName);
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

        // Assert
        var dropdown = cut.Find("div.absolute");
        Assert.Contains("backdrop-blur-lg", dropdown.ClassName);
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
