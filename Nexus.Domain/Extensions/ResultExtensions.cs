using Nexus.Domain.Common;

namespace Nexus.Domain.Extensions;

public static class ResultExtensions
{
    extension<T>(IEnumerable<Result<T>> results)
    {
        public IEnumerable<Error> WithIndexedErrors(string fieldName)
        {
            return results
                .Index()
                .Where(r => r.Item.IsFailure)
                .SelectMany(r => r.Item.Errors
                    .Select(e => e.WithIndex(r.Index, fieldName)));
        }
    }
}