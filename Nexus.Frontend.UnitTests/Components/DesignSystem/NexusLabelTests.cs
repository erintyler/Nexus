using Bunit;
using Xunit;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusLabelTests : Bunit.TestContext
{
    [Fact]
    public void NexusLabel_RendersWithContent()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusLabel>(parameters => parameters
            .Add(p => p.ChildContent, "Username")
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("Username", label.TextContent);
    }

    [Fact]
    public void NexusLabel_SetsForAttribute()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusLabel>(parameters => parameters
            .Add(p => p.For, "username-input")
            .Add(p => p.ChildContent, "Username")
        );

        // Assert
        var label = cut.Find("label");
        Assert.Equal("username-input", label.GetAttribute("for"));
    }

    [Fact]
    public void NexusLabel_ShowsRequiredIndicator_WhenIsRequiredIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusLabel>(parameters => parameters
            .Add(p => p.IsRequired, true)
            .Add(p => p.ChildContent, "Email")
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("*", label.TextContent);
        var requiredSpan = cut.Find("span.text-red-500");
        Assert.Equal("*", requiredSpan.TextContent);
    }

    [Fact]
    public void NexusLabel_DoesNotShowRequiredIndicator_WhenIsRequiredIsFalse()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusLabel>(parameters => parameters
            .Add(p => p.IsRequired, false)
            .Add(p => p.ChildContent, "Optional Field")
        );

        // Assert
        var label = cut.Find("label");
        Assert.DoesNotContain("*", label.TextContent);
    }

    [Fact]
    public void NexusLabel_HasCorrectStyling()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusLabel>(parameters => parameters
            .Add(p => p.ChildContent, "Label")
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("text-sm", label.ClassName);
        Assert.Contains("font-semibold", label.ClassName);
        Assert.Contains("text-gray-700", label.ClassName);
    }
}
