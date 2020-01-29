using System.Collections.Generic;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction;

namespace OpsBro.Domain.Settings
{
    public interface ISettings
    {
        IDictionary<string, Listener> ListenersByListenerName { get; }
        IDictionary<string, EventDispatcher> EventDispatcherByEventName { get; }
    }
}