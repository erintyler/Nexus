using System.Reflection;

namespace Nexus.Application.Helpers;

public static class CodeGeneration
{
    public static bool IsRunningGeneration()
    {
        return Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider" || Environment.GetCommandLineArgs().Contains("codegen");
    }
}