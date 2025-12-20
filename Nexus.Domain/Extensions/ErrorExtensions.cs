using Nexus.Domain.Common;

namespace Nexus.Domain.Extensions;

public static class ErrorExtensions
{
    extension(Error error)
    {
        public Error WithIndex(int index, string fieldName)
        {
            return error with { Code = $"{fieldName.ToLower()}[{index}].{error.Code}" };
        }
    }
}