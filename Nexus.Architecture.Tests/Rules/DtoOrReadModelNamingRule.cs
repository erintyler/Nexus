using Mono.Cecil;
using NetArchTest.Rules;

namespace Nexus.Architecture.Tests.Rules;

public class DtoOrReadModelNamingRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type)
    {
        return type.Name.EndsWith("Dto") || type.Name.EndsWith("ReadModel");
    }
}

