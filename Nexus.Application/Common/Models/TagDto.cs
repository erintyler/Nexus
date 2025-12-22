using Nexus.Domain.Enums;

namespace Nexus.Application.Common.Models;

public record TagDto(TagType Type, string Value);