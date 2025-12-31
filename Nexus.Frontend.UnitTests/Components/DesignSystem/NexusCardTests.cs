using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;
using static Nexus.Frontend.Client.Components.DesignSystem.NexusCard;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusCardTests : Bunit.TestContext
{
    [Fact]
    public void NexusCard_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusCard>(parameters => parameters
            .Add(p => p.ChildContent, "Card Content")
        );

        // Assert
        var card = cut.Find("div");
        Assert.Contains("Card Content", card.TextContent);
        Assert.Contains("rounded-2xl", card.ClassName);
    }

    [Fact]
    public void NexusCard_RendersWithElevatedVariant()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusCard>(parameters => parameters
            .Add(p => p.Variant, CardVariant.Elevated)
            .Add(p => p.ChildContent, "Elevated")
        );

        // Assert
        var card = cut.Find("div");
        Assert.Contains("bg-white/95", card.ClassName);
        Assert.Contains("shadow-xl", card.ClassName);
    }

    [Fact]
    public void NexusCard_RendersWithInteractiveVariant()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusCard>(parameters => parameters
            .Add(p => p.Variant, CardVariant.Interactive)
            .Add(p => p.ChildContent, "Interactive")
        );

        // Assert
        var card = cut.Find("div");
        Assert.Contains("cursor-pointer", card.ClassName);
    }

    [Fact]
    public void NexusCard_RendersWithBorderedVariant()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusCard>(parameters => parameters
            .Add(p => p.Variant, CardVariant.Bordered)
            .Add(p => p.ChildContent, "Bordered")
        );

        // Assert
        var card = cut.Find("div");
        Assert.Contains("border-2", card.ClassName);
    }

    [Fact]
    public void NexusCard_ShowsArrow_WhenInteractiveAndShowArrowIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusCard>(parameters => parameters
            .Add(p => p.Variant, CardVariant.Interactive)
            .Add(p => p.ShowArrow, true)
            .Add(p => p.ChildContent, "Content")
        );

        // Assert
        var arrow = cut.Find("svg");
        Assert.NotNull(arrow);
    }

    [Fact]
    public void NexusCard_HidesArrow_WhenShowArrowIsFalse()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusCard>(parameters => parameters
            .Add(p => p.Variant, CardVariant.Interactive)
            .Add(p => p.ShowArrow, false)
            .Add(p => p.ChildContent, "Content")
        );

        // Assert
        Assert.DoesNotContain("M9 5l7 7-7 7", cut.Markup);
    }

    [Fact]
    public void NexusCard_TriggersOnClick_WhenClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusCard>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true))
            .Add(p => p.ChildContent, "Clickable")
        );

        // Act
        var card = cut.Find("div");
        card.Click();

        // Assert
        Assert.True(clicked);
    }
}
