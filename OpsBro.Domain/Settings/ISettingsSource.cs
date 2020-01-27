using System.Collections.Generic;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction;

namespace OpsBro.Domain.Settings
{
    public interface ISettings
    {
        ICollection<Listener> Listeners { get; }
        ICollection<EventDispatcher> EventDispatchers { get; }
    }
}