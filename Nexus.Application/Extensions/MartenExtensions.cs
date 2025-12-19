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
    }
    
    extension<T>(IEventStream<T> stream) where T : AggregateRoot
    {
        public void AppendToAggregateStream(T aggregate)
        {
            var domainEvents = aggregate.GetDomainEvents();
            stream.AppendMany(domainEvents);
            aggregate.ClearDomainEvents();
        }
    }
}