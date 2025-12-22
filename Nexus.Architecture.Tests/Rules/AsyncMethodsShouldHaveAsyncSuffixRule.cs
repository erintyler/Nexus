using Mono.Cecil;
using NetArchTest.Rules;

namespace Nexus.Architecture.Tests.Rules;

public class AsyncMethodsShouldHaveAsyncSuffixRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type)
    {
        var asyncMethodsWithoutSuffix = type.Methods
            .Where(m => m.ReturnType.Name.Contains("Task") && 
                       !m.Name.EndsWith("Async") &&
                       !m.IsSpecialName && // Exclude property getters/setters
                       (m.IsPublic || m.IsFamily)) // Public or protected
            .ToList();

        return !asyncMethodsWithoutSuffix.Any();
    }
}

