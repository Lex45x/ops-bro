using System;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events
{
    public class Event : IEquatable<Event>
    {
        public string Name { get; set; }
        public JObject Data { get; set; }

        public bool Equals(Event other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Equals(Data, other.Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Event) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Data);
        }
    }
}