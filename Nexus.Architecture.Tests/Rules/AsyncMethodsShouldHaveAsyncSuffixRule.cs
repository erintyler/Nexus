using Mono.Cecil;
using NetArchTest.Rules;

namespace Nexus.Architecture.Tests.Rules;

public class AsyncMethodsShouldHaveAsyncSuffixRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type)
    {
        // Exclude Marten projection classes - they use Apply methods with Task return type
        if (type.Name.EndsWith("Projection") || IsProjectionClass(type))
        {
            return true;
        }

        var asyncMethodsWithoutSuffix = type.Methods
            .Where(m => m.ReturnType.Name.Contains("Task") &&
                       !m.Name.EndsWith("Async") &&
                       !m.IsSpecialName && // Exclude property getters/setters
                       (m.IsPublic || m.IsFamily)) // Public or protected
            .ToList();

        return !asyncMethodsWithoutSuffix.Any();
    }

    private static bool IsProjectionClass(TypeDefinition type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.Name.Contains("Projection"))
            {
                return true;
            }
            baseType = baseType.Resolve()?.BaseType;
        }
        return false;
    }
}

