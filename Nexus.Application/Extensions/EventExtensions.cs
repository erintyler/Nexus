using JasperFx.Events;
using Nexus.Application.Common.Models;
using Nexus.Domain.Events;

namespace Nexus.Application.Extensions;

public static class EventExtensions
{
    extension(IEvent<INexusEvent> @event)
    {
        public HistoryDto ToHistoryDto()
        {
            return new HistoryDto(@event.Data.EventName, @event.Data.Description, @event.Timestamp, @event.UserName);
        }
    }
}