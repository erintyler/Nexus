using System.Diagnostics;

namespace Nexus.Application.Constants;

public static class TelemetryConstants
{
    public const string ActivitySourceName = "Nexus.Application";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}