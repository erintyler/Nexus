using Bunit;
using Xunit;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusTextAreaTests : Bunit.TestContext
{
    [Fact]
    public void NexusTextArea_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>();

        // Assert
        var textarea = cut.Find("textarea");
        Assert.NotNull(textarea);
        Assert.Equal("4", textarea.GetAttribute("rows"));
    }

    [Fact]
    public void NexusTextArea_RendersWithLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.Label, "Description")
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("Description", label.TextContent);
    }

    [Fact]
    public void NexusTextArea_ShowsRequiredIndicator_WhenIsRequiredIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.Label, "Comments")
            .Add(p => p.IsRequired, true)
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("*", label.TextContent);
    }

    [Fact]
    public void NexusTextArea_RendersWithPlaceholder()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.Placeholder, "Enter your description")
        );

        // Assert
        var textarea = cut.Find("textarea");
        Assert.Equal("Enter your description", textarea.GetAttribute("placeholder"));
    }

    [Fact]
    public void NexusTextArea_RendersWithValue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.Value, "Sample text")
        );

        // Assert
        var textarea = cut.Find("textarea");
        Assert.Contains("Sample text", textarea.TextContent);
    }

    [Fact]
    public void NexusTextArea_RendersAsDisabled_WhenDisabledIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.Disabled, true)
        );

        // Assert
        var textarea = cut.Find("textarea");
        Assert.True(textarea.HasAttribute("disabled"));
    }

    [Fact]
    public void NexusTextArea_ShowsHelperText()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.HelperText, "Maximum 500 characters")
        );

        // Assert
        Assert.Contains("Maximum 500 characters", cut.Markup);
    }

    [Fact]
    public void NexusTextArea_ShowsErrorMessage()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.ErrorMessage, "Description is required")
        );

        // Assert
        var errorText = cut.Find("p.text-red-600");
        Assert.Contains("Description is required", errorText.TextContent);
    }

    [Fact]
    public void NexusTextArea_AppliesErrorStyling_WhenErrorMessageIsSet()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.ErrorMessage, "Invalid input")
        );

        // Assert
        var textarea = cut.Find("textarea");
        Assert.Contains("border-red-400", textarea.ClassName);
    }

    [Fact]
    public void NexusTextArea_SetsCustomRows()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.Rows, 10)
        );

        // Assert
        var textarea = cut.Find("textarea");
        Assert.Equal("10", textarea.GetAttribute("rows"));
    }

    [Fact]
    public void NexusTextArea_TriggersValueChanged_WhenInputChanges()
    {
        // Arrange
        string? newValue = null;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.Value, "")
            .Add(p => p.ValueChanged, (string value) => newValue = value)
        );

        // Act
        var textarea = cut.Find("textarea");
        textarea.Input("New text content");

        // Assert
        Assert.Equal("New text content", newValue);
    }

    [Fact]
    public void NexusTextArea_GeneratesUniqueId()
    {
        // Arrange & Act
        var cut1 = RenderComponent<Client.Components.DesignSystem.NexusTextArea>();
        var cut2 = RenderComponent<Client.Components.DesignSystem.NexusTextArea>();

        // Assert
        var textarea1 = cut1.Find("textarea");
        var textarea2 = cut2.Find("textarea");
        Assert.NotEqual(textarea1.GetAttribute("id"), textarea2.GetAttribute("id"));
    }

    [Fact]
    public void NexusTextArea_UsesCustomId_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.Id, "custom-textarea-id")
        );

        // Assert
        var textarea = cut.Find("textarea");
        Assert.Equal("custom-textarea-id", textarea.GetAttribute("id"));
    }

    [Fact]
    public void NexusTextArea_AppliesWrapperClasses()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.WrapperClasses, "custom-wrapper")
        );

        // Assert
        var wrapper = cut.Find("div");
        Assert.Contains("custom-wrapper", wrapper.ClassName);
    }

    [Fact]
    public void NexusTextArea_ShowsCharacterCount_WhenMaxLengthIsSet()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.MaxLength, 100)
            .Add(p => p.Value, "Test")
        );

        // Assert
        Assert.Contains("4 / 100", cut.Markup);
    }

    [Fact]
    public void NexusTextArea_HidesCharacterCount_WhenShowCharacterCountIsFalse()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.MaxLength, 100)
            .Add(p => p.ShowCharacterCount, false)
        );

        // Assert
        Assert.DoesNotContain("/ 100", cut.Markup);
    }

    [Fact]
    public void NexusTextArea_DoesNotShowCharacterCount_WhenMaxLengthIsNotSet()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.Value, "Test")
        );

        // Assert - Check that character count text is not present
        var divs = cut.FindAll("div");
        var hasCharacterCount = divs.Any(d => d.TextContent.Contains(" / "));
        Assert.False(hasCharacterCount);
    }

    [Fact]
    public void NexusTextArea_ShowsWarningColor_WhenNear90PercentOfMaxLength()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.MaxLength, 100)
            .Add(p => p.Value, new string('a', 91))
        );

        // Assert
        Assert.Contains("text-amber-600", cut.Markup);
    }

    [Fact]
    public void NexusTextArea_ShowsErrorColor_WhenAtMaxLength()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.MaxLength, 100)
            .Add(p => p.Value, new string('a', 100))
        );

        // Assert
        Assert.Contains("text-red-600", cut.Markup);
    }

    [Fact]
    public void NexusTextArea_EnforcesMaxLength()
    {
        // Arrange
        string? capturedValue = null;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.MaxLength, 10)
            .Add(p => p.Value, "")
            .Add(p => p.ValueChanged, (string value) => capturedValue = value)
        );

        // Act
        var textarea = cut.Find("textarea");
        textarea.Input("This is a very long text that exceeds the maximum");

        // Assert
        Assert.Equal("This is a ", capturedValue);
        Assert.Equal(10, capturedValue?.Length);
    }

    [Fact]
    public void NexusTextArea_SetsMaxLengthAttribute_WhenMaxLengthIsProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.MaxLength, 200)
        );

        // Assert
        var textarea = cut.Find("textarea");
        Assert.Equal("200", textarea.GetAttribute("maxlength"));
    }

    [Fact]
    public void NexusTextArea_UpdatesCharacterCount_WhenValueChanges()
    {
        // Arrange
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextArea>(parameters => parameters
            .Add(p => p.MaxLength, 100)
            .Add(p => p.Value, "Test")
        );

        // Initial state
        Assert.Contains("4 / 100", cut.Markup);

        // Act
        var textarea = cut.Find("textarea");
        textarea.Input("Updated text");

        // Assert
        Assert.Contains("12 / 100", cut.Markup);
    }
}
