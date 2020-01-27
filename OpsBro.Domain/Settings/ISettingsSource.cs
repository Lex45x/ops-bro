using System.Collections.Generic;
using OpsBro.Domain.Entities;
using OpsBro.Domain.Entities.Events;

namespace OpsBro.Domain.Settings
{
    public interface ISettings
    {
        ICollection<Listener> Listeners { get; }
        ICollection<EventDispatcher> EventDispatchers { get; }
    }
}