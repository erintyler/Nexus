using Nexus.Domain.Enums;

namespace Nexus.Application.Common.Models;

public record TagDto(string Value, TagType Type);