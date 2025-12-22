using Nexus.Application.Common.Models;

namespace Nexus.Application.Features.Tags.MigrateTag;

public record MigrateTagCommand(TagDto Source, TagDto Target);