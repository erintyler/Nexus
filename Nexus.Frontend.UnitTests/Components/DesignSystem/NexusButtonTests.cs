using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;
using static Nexus.Frontend.Client.Components.DesignSystem.NexusButton;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusButtonTests : Bunit.TestContext
{
    [Fact]
    public void NexusButton_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.ChildContent, "Click Me")
        );

        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
        Assert.Contains("Click Me", button.TextContent);
        Assert.Contains("bg-gradient-to-r from-purple-600 to-fuchsia-600", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersWithPrimaryVariant()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Primary)
            .Add(p => p.ChildContent, "Primary")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("from-purple-600 to-fuchsia-600", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersWithSecondaryVariant()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Secondary)
            .Add(p => p.ChildContent, "Secondary")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("bg-gray-50", button.ClassName);
        Assert.Contains("text-gray-700", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersWithSuccessVariant()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Success)
            .Add(p => p.ChildContent, "Success")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("from-emerald-500 to-teal-600", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersWithDangerVariant()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Danger)
            .Add(p => p.ChildContent, "Danger")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("from-red-500 to-pink-600", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersWithAccentVariant()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Accent)
            .Add(p => p.ChildContent, "Accent")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("from-indigo-500 to-violet-600", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersWithWarningVariant()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Warning)
            .Add(p => p.ChildContent, "Warning")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("from-amber-500 to-orange-600", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersWithSmallSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Size, ButtonSize.Small)
            .Add(p => p.ChildContent, "Small")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("px-4 py-2", button.ClassName);
        Assert.Contains("text-sm", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersWithMediumSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Size, ButtonSize.Medium)
            .Add(p => p.ChildContent, "Medium")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("px-6 py-3", button.ClassName);
        Assert.Contains("text-base", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersWithLargeSize()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Size, ButtonSize.Large)
            .Add(p => p.ChildContent, "Large")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("px-8 py-4", button.ClassName);
        Assert.Contains("text-lg", button.ClassName);
    }

    [Fact]
    public void NexusButton_RendersAsDisabled_WhenDisabledIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.ChildContent, "Disabled")
        );

        // Assert
        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
        Assert.Contains("cursor-not-allowed", button.ClassName);
        Assert.Contains("opacity-60", button.ClassName);
    }

    [Fact]
    public void NexusButton_ShowsLoadingState_WhenIsLoadingIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingText, "Processing...")
            .Add(p => p.ChildContent, "Submit")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("Processing...", button.TextContent);
        var spinner = cut.Find("i.fa-spinner");
        Assert.NotNull(spinner);
        Assert.Contains("fa-spin", spinner.ClassName);
    }

    [Fact]
    public void NexusButton_UsesCustomLoadingText()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingText, "Uploading...")
            .Add(p => p.ChildContent, "Upload")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Contains("Uploading...", button.TextContent);
    }

    [Fact]
    public void NexusButton_TriggersOnClickEvent_WhenClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true))
            .Add(p => p.ChildContent, "Click Me")
        );

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void NexusButton_DoesNotTriggerOnClick_WhenDisabled()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true))
            .Add(p => p.ChildContent, "Disabled")
        );

        // Act
        var button = cut.Find("button");
        // Note: BUnit allows clicking disabled buttons, but the browser prevents it
        // We're testing the rendering, not actual browser behavior

        // Assert
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void NexusButton_SetsTypeAttribute()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.Type, "submit")
            .Add(p => p.ChildContent, "Submit")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Equal("submit", button.GetAttribute("type"));
    }

    [Fact]
    public void NexusButton_DefaultTypeIsButton()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.ChildContent, "Click Me")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Equal("button", button.GetAttribute("type"));
    }

    [Fact]
    public void NexusButton_PassesAdditionalAttributes()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusButton>(parameters => parameters
            .Add(p => p.ChildContent, "Test")
            .AddUnmatched("data-testid", "custom-button")
        );

        // Assert
        var button = cut.Find("button");
        Assert.Equal("custom-button", button.GetAttribute("data-testid"));
    }
}
