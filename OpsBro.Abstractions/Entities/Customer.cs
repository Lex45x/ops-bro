using System.Collections.Generic;

namespace OpsBro.Abstractions.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        public ICollection<Listener> Listeners { get; set; }
        public ICollection<EventDefinition> EventDefinitions { get; set; }
    }
}