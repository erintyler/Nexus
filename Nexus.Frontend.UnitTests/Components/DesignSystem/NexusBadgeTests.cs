using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;
using static Nexus.Frontend.Client.Components.DesignSystem.NexusBadge;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusBadgeTests : Bunit.TestContext
{
    [Fact]
    public void NexusBadge_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusBadge>(parameters => parameters
            .Add(p => p.ChildContent, "Badge")
        );

        // Assert
        var badge = cut.Find("span");
        Assert.Contains("Badge", badge.TextContent);
        Assert.Contains("rounded-full", badge.ClassName);
    }

    [Fact]
    public void NexusBadge_RendersWithGradientStyle()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusBadge>(parameters => parameters
            .Add(p => p.Style, BadgeStyle.Gradient)
            .Add(p => p.Color, BadgeColor.Primary)
            .Add(p => p.ChildContent, "Gradient")
        );

        // Assert
        var badge = cut.Find("span");
        Assert.Contains("bg-gradient-to-r", badge.ClassName);
        Assert.Contains("from-purple-500", badge.ClassName);
    }

    [Fact]
    public void NexusBadge_RendersWithSoftStyle()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusBadge>(parameters => parameters
            .Add(p => p.Style, BadgeStyle.Soft)
            .Add(p => p.Color, BadgeColor.Primary)
            .Add(p => p.ChildContent, "Soft")
        );

        // Assert
        var badge = cut.Find("span");
        Assert.Contains("bg-purple-100", badge.ClassName);
        Assert.Contains("text-purple-700", badge.ClassName);
    }

    [Fact]
    public void NexusBadge_RendersWithAllColors()
    {
        // Test each color
        var colors = new[]
        {
            BadgeColor.Primary,
            BadgeColor.Success,
            BadgeColor.Danger,
            BadgeColor.Accent,
            BadgeColor.Warning,
            BadgeColor.Info
        };

        foreach (var color in colors)
        {
            var cut = RenderComponent<Client.Components.DesignSystem.NexusBadge>(parameters => parameters
                .Add(p => p.Color, color)
                .Add(p => p.ChildContent, color.ToString())
            );

            var badge = cut.Find("span");
            Assert.NotNull(badge);
        }
    }

    [Fact]
    public void NexusBadge_ShowsLabel_WhenLabelIsProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusBadge>(parameters => parameters
            .Add(p => p.Label, "Category")
            .Add(p => p.ChildContent, "Design")
        );

        // Assert
        var badge = cut.Find("span");
        Assert.Contains("Category:", badge.TextContent);
        Assert.Contains("Design", badge.TextContent);
    }

    [Fact]
    public void NexusBadge_ShowsRemoveButton_WhenRemovableIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusBadge>(parameters => parameters
            .Add(p => p.Removable, true)
            .Add(p => p.ChildContent, "Removable")
        );

        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
    }

    [Fact]
    public void NexusBadge_TriggersOnRemove_WhenRemoveButtonClicked()
    {
        // Arrange
        var removed = false;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusBadge>(parameters => parameters
            .Add(p => p.Removable, true)
            .Add(p => p.OnRemove, EventCallback.Factory.Create(this, () => removed = true))
            .Add(p => p.ChildContent, "Remove Me")
        );

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        Assert.True(removed);
    }
}
