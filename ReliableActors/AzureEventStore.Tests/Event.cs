namespace AzureEventStore.Tests
{
    using System;

    public class Event : IEvent
    {
        public Event(Guid eventId, string data)
        {
            EventId = eventId;
            Data = data;
        }

        public Guid EventId { get; }

        public string Data { get; }
    }
}