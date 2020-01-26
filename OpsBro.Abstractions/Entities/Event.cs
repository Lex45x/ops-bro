using Newtonsoft.Json.Linq;

namespace OpsBro.Abstractions.Entities
{
    public class Event
    {
        public string Name { get; set; }
        public JObject Data { get; set; }
    }
}