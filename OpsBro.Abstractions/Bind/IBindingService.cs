using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OpsBro.Abstractions.Entities;

namespace OpsBro.Abstractions.Bind
{
    public interface IBindingService
    {
        IEnumerable<Event> Bind(Listener listener, JObject jObject);
    }
}