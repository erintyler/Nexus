using Nexus.Application.Common.Models;
using Nexus.Domain.Common;
using Nexus.Domain.ValueObjects;

namespace Nexus.Application.Extensions;

public static class TagExtensions
{
    extension(TagDto tagDto)
    {
        public Result<Tag> ToDomainTag()
        {
            return Tag.Create(tagDto.Value, tagDto.Type);
        }
    }
}