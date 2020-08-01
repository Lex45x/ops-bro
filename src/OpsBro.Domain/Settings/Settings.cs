using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction;
using OpsBro.Domain.Extraction.Validation;

namespace OpsBro.Domain.Settings
{

    public class Settings : ISettings
    {
        public Settings(ICollection<Listener> listeners, ICollection<EventDispatcher> eventDispatchers, JObject config)
        {
            ListenersByListenerName = listeners.ToDictionary(listener => listener.Name);
            EventDispatcherByEventName = eventDispatchers.ToDictionary(dispatcher => dispatcher.EventName);
            Config = config;
        }

        public IDictionary<string, Listener> ListenersByListenerName { get; }
        public IDictionary<string, EventDispatcher> EventDispatcherByEventName { get; }
        public JObject Config { get; }
    }

}