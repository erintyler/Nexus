using Bunit;
using Xunit;

namespace Nexus.Frontend.UnitTests.Components.DesignSystem;

public class NexusTooltipTests : Bunit.TestContext
{

    [Fact]
    public void Renders_ChildContent_Correctly()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>"));

        // Assert
        Assert.Contains("Hover me", cut.Markup);
    }

    [Fact]
    public void Renders_TooltipContent_Correctly()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>"));

        // Assert
        Assert.Contains("Tooltip text", cut.Markup);
    }

    [Fact]
    public void Tooltip_Is_Hidden_By_Default()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>"));

        // Assert - tooltip should have opacity-0 and pointer-events-none when hidden
        Assert.Contains("opacity-0", cut.Markup);
        Assert.Contains("pointer-events-none", cut.Markup);
    }

    [Fact]
    public void Tooltip_Position_Top_Renders_Correctly()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>")
            .Add(p => p.Position, TooltipPosition.Top));

        // Assert
        Assert.Contains("bottom-full", cut.Markup);
    }

    [Fact]
    public void Tooltip_Position_Bottom_Renders_Correctly()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>")
            .Add(p => p.Position, TooltipPosition.Bottom));

        // Assert
        Assert.Contains("top-full", cut.Markup);
    }

    [Fact]
    public void Tooltip_Position_Left_Renders_Correctly()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>")
            .Add(p => p.Position, TooltipPosition.Left));

        // Assert
        Assert.Contains("right-full", cut.Markup);
    }

    [Fact]
    public void Tooltip_Position_Right_Renders_Correctly()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>")
            .Add(p => p.Position, TooltipPosition.Right));

        // Assert
        Assert.Contains("left-full", cut.Markup);
    }

    [Fact]
    public void Tooltip_Has_Gradient_Background()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>"));

        // Assert
        Assert.Contains("bg-gradient-to-r", cut.Markup);
        Assert.Contains("from-gray-800", cut.Markup);
        Assert.Contains("to-gray-900", cut.Markup);
    }

    [Fact]
    public void Tooltip_Has_Arrow_Pointer()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>"));

        // Assert - arrow should have rotate-45 transform
        Assert.Contains("rotate-45", cut.Markup);
    }

    [Fact]
    public void Tooltip_Has_Shadow_And_Border()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>"));

        // Assert
        Assert.Contains("shadow-xl", cut.Markup);
        Assert.Contains("border-gray-700", cut.Markup);
    }

    [Fact]
    public void Tooltip_Has_High_Z_Index()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>"));

        // Assert - Check inline style for z-index
        Assert.Contains("z-index: 9999", cut.Markup);
    }

    [Fact]
    public void Tooltip_Supports_Complex_Content()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "Line 1");
                builder.OpenElement(2, "br");
                builder.CloseElement();
                builder.AddContent(3, "Line 2");
                builder.CloseElement();
            }));

        // Assert
        Assert.Contains("Line 1", cut.Markup);
        Assert.Contains("Line 2", cut.Markup);
    }

    [Fact]
    public void Tooltip_Supports_Additional_Attributes()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>")
            .AddUnmatched("data-testid", "custom-tooltip")
            .AddUnmatched("aria-label", "Custom label"));

        // Assert
        Assert.Contains("data-testid=\"custom-tooltip\"", cut.Markup);
        Assert.Contains("aria-label=\"Custom label\"", cut.Markup);
    }

    [Fact]
    public void Tooltip_Has_Smooth_Transition_Classes()
    {
        // Arrange & Act
        var cut = RenderComponent<NexusTooltip>(parameters => parameters
            .AddChildContent("<button>Hover me</button>")
            .Add(p => p.TooltipContent, "<span>Tooltip text</span>"));

        // Assert
        Assert.Contains("transition-all", cut.Markup);
        Assert.Contains("duration-200", cut.Markup);
        Assert.Contains("ease-out", cut.Markup);
    }
}
