using System;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events
{
    public class Event
    {
        public Event(string name, JObject data)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public string Name { get; }
        public JObject Data { get; }
    }
}