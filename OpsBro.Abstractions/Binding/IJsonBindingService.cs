using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OpsBro.Abstractions.Entities;
using OpsBro.Abstractions.ValueObjects;

namespace OpsBro.Abstractions.Binding
{
    public interface IJsonBindingService
    {
        IEnumerable<Event> Bind(Listener listener, JObject jObject);
    }
}