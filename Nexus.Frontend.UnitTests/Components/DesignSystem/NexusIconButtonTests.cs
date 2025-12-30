using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;
using static Nexus.Frontend.Client.Components.DesignSystem.NexusIconButton;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusIconButtonTests : Bunit.TestContext
{
    [Fact]
    public void NexusIconButton_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
        Assert.Contains("text-purple-600", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersWithPrimaryColor()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Color, IconButtonColor.Primary)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("text-purple-600", button.ClassName);
        Assert.Contains("hover:text-purple-800", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersWithSecondaryColor()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Color, IconButtonColor.Secondary)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("text-gray-600", button.ClassName);
        Assert.Contains("hover:text-gray-800", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersWithSuccessColor()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Color, IconButtonColor.Success)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("text-emerald-600", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersWithDangerColor()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Color, IconButtonColor.Danger)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("text-red-600", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersWithWarningColor()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Color, IconButtonColor.Warning)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("text-amber-600", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersWithInfoColor()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Color, IconButtonColor.Info)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("text-blue-600", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersWithSmallSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Size, IconButtonSize.Small)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-4 h-4'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("p-1", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersWithMediumSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Size, IconButtonSize.Medium)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("p-2", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersWithLargeSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Size, IconButtonSize.Large)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-8 h-8'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("p-3", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_RendersAsDisabled_WhenDisabledIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
        Assert.Contains("cursor-not-allowed", button.ClassName);
        Assert.Contains("opacity-50", button.ClassName);
    }

    [Fact]
    public void NexusIconButton_SetsAriaLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.AriaLabel, "Delete item")
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Equal("Delete item", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void NexusIconButton_TriggersOnClickEvent_WhenClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true))
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void NexusIconButton_SetsTypeAttribute()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.Type, "submit")
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Equal("submit", button.GetAttribute("type"));
    }

    [Fact]
    public void NexusIconButton_DefaultTypeIsButton()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
        );

        // Assert
        var button = cut.Find("button");
        Assert.Equal("button", button.GetAttribute("type"));
    }

    [Fact]
    public void NexusIconButton_PassesAdditionalAttributes()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusIconButton>(parameters => parameters
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<svg class='w-6 h-6'><path /></svg>"))
            .AddUnmatched("data-testid", "custom-icon-button")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Equal("custom-icon-button", button.GetAttribute("data-testid"));
    }
}
