using System.Collections.Generic;

namespace OpsBro.Abstractions.Entities
{
    public class Binder
    {
        public string Name { get; set; }
        public string Event { get; set; }
        public ICollection<AdvancedBinding> Bindings { get; set; }
        public ICollection<Validator> Validators { get; set; }
    }
}