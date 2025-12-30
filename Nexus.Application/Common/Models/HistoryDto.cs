using System.Text.Json.Serialization;

namespace Nexus.Application.Common.Models;

public record HistoryDto(string Action, string Description, DateTimeOffset Timestamp, string? PerformedBy);