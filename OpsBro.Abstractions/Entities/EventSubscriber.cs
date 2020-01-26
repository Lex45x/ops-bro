using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpsBro.Abstractions.Entities
{
    public class EventSubscriber
    {
        public string UrlTemplate { get; set; }
        public string Method { get; set; }
        public JObject BodyTemplate { get; set; }
        public ICollection<BasicBinding> BodyBindings { get; set; }
        public ICollection<BasicBinding> HeaderBindings { get; set; }
        public ICollection<BasicBinding> UrlBindings { get; set; }
        public JObject Metadata { get; set; }
    }
}