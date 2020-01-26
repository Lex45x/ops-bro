using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using OpsBro.Abstractions.Bind;
using OpsBro.Abstractions.Entities;

namespace OpsBro.Domain.Bind
{
    public class BindingService : IBindingService
    {
        public IEnumerable<Event> Bind(Listener listener, JObject jObject)
        {
            foreach (var binder in listener.Binders)
            {
                var validBinder = true;

                foreach (var binderValidator in binder.Validators)
                {
                    if (!validBinder)
                        continue;

                    var valueEquals = JToken.DeepEquals(jObject.SelectToken(binderValidator.Path), JToken.FromObject(binderValidator.Value ?? JValue.CreateNull()));

                    validBinder = valueEquals == (binderValidator.Operator == Operator.Equals);
                }

                if (!validBinder)
                    continue;

                var data = new JObject();

                foreach (var binding in binder.Bindings)
                {
                    var source = jObject.SelectToken(binding.Path);

                    if (binding.Selector != null)
                    {
                        var match = Regex.Match(source.ToObject<string>(), binding.Selector);

                        source = match.Value;
                    }

                    data[binding.Property] = source;
                }

                var @event = new Event
                {
                    Data = data,
                    Name = binder.Event
                };

                yield return @event;
            }
        }
    }
}