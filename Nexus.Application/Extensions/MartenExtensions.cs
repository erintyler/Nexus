using Marten.Events;
using Nexus.Domain.Primitives;

namespace Nexus.Application.Extensions;

public static class MartenExtensions
{
    extension(IEventStoreOperations events)
    {
        public void StartAggregateStream<T>(T aggregate) where T : AggregateRoot
        {
            var domainEvents = aggregate.GetDomainEvents();
            events.StartStream(aggregate.Id, domainEvents);
            aggregate.ClearDomainEvents();
        }
        
        public void AppendAggregateEvents<T>(T aggregate) where T : AggregateRoot
        {
            var domainEvents = aggregate.GetDomainEvents();
            events.Append(aggregate.Id, domainEvents);
            aggregate.ClearDomainEvents();
        }
    }
}