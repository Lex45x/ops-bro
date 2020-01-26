using OpsBro.Abstractions.Entities;

namespace OpsBro.Abstractions.Contracts.Events
{
    public class GenericEvent : IEvent
    {
        public Event Event { get; set; }
        public EventDefinition Definition { get; set; }
    }
}