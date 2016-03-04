namespace AzureEventStore.Tests
{
    using System;

    public interface IEvent
    {
        Guid EventId { get; }
    }
}
