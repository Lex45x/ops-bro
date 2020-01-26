using System.Collections.Generic;

namespace OpsBro.Abstractions.Entities
{
    public class EventDefinition
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public ICollection<EventSubscriber> Subscribers { get; set; }
    }
}