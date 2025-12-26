using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using Nexus.Frontend.Services;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Nexus.Frontend.UnitTests.Services;

public class JwtCookieTransformTests
{
    [Fact]
    public void ValidateRoute_ShouldNotThrow()
    {
        // Arrange
        var transform = new JwtCookieTransform();
        var mockContext = new Mock<TransformRouteValidationContext>();

        // Act & Assert
        var exception = Record.Exception(() => transform.ValidateRoute(mockContext.Object));
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateCluster_ShouldNotThrow()
    {
        // Arrange
        var transform = new JwtCookieTransform();
        var mockContext = new Mock<TransformClusterValidationContext>();

        // Act & Assert
        var exception = Record.Exception(() => transform.ValidateCluster(mockContext.Object));
        Assert.Null(exception);
    }

    [Fact]
    public void Apply_ShouldNotThrow_WithValidContext()
    {
        // Arrange
        var transform = new JwtCookieTransform();
        var context = new TransformBuilderContext
        {
            Services = new Mock<IServiceProvider>().Object
        };

        // Act & Assert
        var exception = Record.Exception(() => transform.Apply(context));
        Assert.Null(exception);
    }
}
