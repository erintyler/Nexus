using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;
using static Nexus.Frontend.Client.Components.DesignSystem.NexusAlert;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusAlertTests : Bunit.TestContext
{
    [Fact]
    public void NexusAlert_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.ChildContent, "Alert message")
        );

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("Alert message", alert.TextContent);
    }

    [Fact]
    public void NexusAlert_RendersWithSuccessType()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.Type, AlertType.Success)
            .Add(p => p.ChildContent, "Success")
        );

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("from-emerald-50", alert.ClassName);
        Assert.Contains("border-emerald-500", alert.ClassName);
    }

    [Fact]
    public void NexusAlert_RendersWithErrorType()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.Type, AlertType.Error)
            .Add(p => p.ChildContent, "Error")
        );

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("from-red-50", alert.ClassName);
        Assert.Contains("border-red-500", alert.ClassName);
    }

    [Fact]
    public void NexusAlert_RendersWithWarningType()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.Type, AlertType.Warning)
            .Add(p => p.ChildContent, "Warning")
        );

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("from-amber-50", alert.ClassName);
        Assert.Contains("border-amber-500", alert.ClassName);
    }

    [Fact]
    public void NexusAlert_RendersWithInfoType()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.Type, AlertType.Info)
            .Add(p => p.ChildContent, "Info")
        );

        // Assert
        var alert = cut.Find("div[role='alert']");
        Assert.Contains("from-purple-50", alert.ClassName);
        Assert.Contains("border-purple-500", alert.ClassName);
    }

    [Fact]
    public void NexusAlert_ShowsTitle_WhenTitleIsProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.Title, "Alert Title")
            .Add(p => p.ChildContent, "Message")
        );

        // Assert
        var title = cut.Find("h4");
        Assert.Equal("Alert Title", title.TextContent);
    }

    [Fact]
    public void NexusAlert_ShowsIcon_WhenShowIconIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.ShowIcon, true)
            .Add(p => p.ChildContent, "Message")
        );

        // Assert
        var icon = cut.Find("i");
        Assert.NotNull(icon);
    }

    [Fact]
    public void NexusAlert_HidesIcon_WhenShowIconIsFalse()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.ShowIcon, false)
            .Add(p => p.ChildContent, "Message")
        );

        // Assert
        Assert.DoesNotContain("<svg", cut.Markup);
    }

    [Fact]
    public void NexusAlert_ShowsDismissButton_WhenDismissibleIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.ChildContent, "Message")
        );

        // Assert
        var dismissButton = cut.Find("button");
        Assert.NotNull(dismissButton);
    }

    [Fact]
    public void NexusAlert_TriggersOnDismiss_WhenDismissButtonClicked()
    {
        // Arrange
        var dismissed = false;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusAlert>(parameters => parameters
            .Add(p => p.Dismissible, true)
            .Add(p => p.OnDismiss, EventCallback.Factory.Create(this, () => dismissed = true))
            .Add(p => p.ChildContent, "Message")
        );

        // Act
        var dismissButton = cut.Find("button");
        dismissButton.Click();

        // Assert
        Assert.True(dismissed);
    }
}
