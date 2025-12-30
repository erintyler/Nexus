using Bunit;
using Xunit;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusTextInputTests : Bunit.TestContext
{
    [Fact]
    public void NexusTextInput_RendersWithDefaultProperties()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>();

        // Assert
        var input = cut.Find("input");
        Assert.NotNull(input);
        Assert.Equal("text", input.GetAttribute("type"));
    }

    [Fact]
    public void NexusTextInput_RendersWithLabel()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.Label, "Email Address")
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("Email Address", label.TextContent);
    }

    [Fact]
    public void NexusTextInput_ShowsRequiredIndicator_WhenIsRequiredIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.Label, "Username")
            .Add(p => p.IsRequired, true)
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("*", label.TextContent);
    }

    [Fact]
    public void NexusTextInput_RendersWithPlaceholder()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.Placeholder, "Enter your name")
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal("Enter your name", input.GetAttribute("placeholder"));
    }

    [Fact]
    public void NexusTextInput_RendersWithValue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.Value, "John Doe")
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal("John Doe", input.GetAttribute("value"));
    }

    [Fact]
    public void NexusTextInput_RendersAsDisabled_WhenDisabledIsTrue()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.Disabled, true)
        );

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("disabled"));
    }

    [Fact]
    public void NexusTextInput_ShowsHelperText()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.HelperText, "Enter a valid email address")
        );

        // Assert
        Assert.Contains("Enter a valid email address", cut.Markup);
    }

    [Fact]
    public void NexusTextInput_ShowsErrorMessage()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.ErrorMessage, "Email is required")
        );

        // Assert
        var errorText = cut.Find("p.text-red-600");
        Assert.Contains("Email is required", errorText.TextContent);
    }

    [Fact]
    public void NexusTextInput_AppliesErrorStyling_WhenErrorMessageIsSet()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.ErrorMessage, "Invalid input")
        );

        // Assert
        var input = cut.Find("input");
        Assert.Contains("border-red-400", input.ClassName);
    }

    [Fact]
    public void NexusTextInput_SetsCustomType()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.Type, "email")
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal("email", input.GetAttribute("type"));
    }

    [Fact]
    public void NexusTextInput_TriggersValueChanged_WhenInputChanges()
    {
        // Arrange
        string? newValue = null;
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.Value, "")
            .Add(p => p.ValueChanged, (string value) => newValue = value)
        );

        // Act
        var input = cut.Find("input");
        input.Input("test@example.com");

        // Assert
        Assert.Equal("test@example.com", newValue);
    }

    [Fact]
    public void NexusTextInput_GeneratesUniqueId()
    {
        // Arrange & Act
        var cut1 = RenderComponent<Client.Components.DesignSystem.NexusTextInput>();
        var cut2 = RenderComponent<Client.Components.DesignSystem.NexusTextInput>();

        // Assert
        var input1 = cut1.Find("input");
        var input2 = cut2.Find("input");
        Assert.NotEqual(input1.GetAttribute("id"), input2.GetAttribute("id"));
    }

    [Fact]
    public void NexusTextInput_UsesCustomId_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.Id, "custom-input-id")
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal("custom-input-id", input.GetAttribute("id"));
    }

    [Fact]
    public void NexusTextInput_AppliesWrapperClasses()
    {
        // Arrange & Act
        var cut = RenderComponent<Client.Components.DesignSystem.NexusTextInput>(parameters => parameters
            .Add(p => p.WrapperClasses, "custom-wrapper-class")
        );

        // Assert
        var wrapper = cut.Find("div");
        Assert.Contains("custom-wrapper-class", wrapper.ClassName);
    }
}
