using System.Collections.Generic;

namespace OpsBro.Abstractions.Entities
{
    public class Listener
    {
        public string Name { get; set; }
        public ICollection<Binder> Binders { get; set; }
    }
}