namespace AzureEventStore.Tests
{
    using System;

    public class SomeOtherEvent : IEvent
    {
        public SomeOtherEvent(Guid eventId, DateTime date, int value)
        {
            EventId = eventId;
            Date = date;
            Value = value;
        }

        public Guid EventId { get; }

        public DateTime Date { get; }

        public int Value { get; }
    }
}