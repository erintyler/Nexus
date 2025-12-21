using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Extensions;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events;

namespace Nexus.Application.Features.ImagePosts.GetImageHistory;

public class GetImageHistoryQueryHandler
{
    public static async Task<Result<PagedResult<HistoryDto>>> Handle(
        GetImageHistoryQuery request, 
        IQuerySession querySession,
        ILogger<GetImageHistoryQueryHandler> logger,
        CancellationToken cancellationToken)
    {
        var events = await querySession.Events.FetchStreamAsync(
            request.Id, 
            timestamp: request.DateTo, 
            token: cancellationToken);
        
        events = events
            .Where(e => request.DateFrom == null || e.Timestamp >= request.DateFrom)
            .ToList();
        
        var count = events.Count;
        if (count == 0)
        {
            return ImagePostErrors.NotFound;
        }

        var validPaginationRequest = request.WithValidPageNumber(count);

        return events
            .Select(e => MapEventsToHistoryDtos(e, logger))
            .ToPagedResult(validPaginationRequest, count);
    }
    
    private static HistoryDto MapEventsToHistoryDtos(IEvent @event, ILogger<GetImageHistoryQueryHandler> logger)
    {
        if (@event.Data is not INexusEvent data)
        {
            logger.LogWarning("Event with ID {EventId} is not a valid INexusEvent", @event.Id);
            return new HistoryDto("Unknown Event", "No description available", @event.Timestamp, @event.UserName);
        }
        
        return new HistoryDto(@data.EventName, @data.Description, @event.Timestamp, @event.UserName);
    }
}