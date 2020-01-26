using System.Collections.Generic;
using OpsBro.Abstractions.Entities;

namespace OpsBro.Abstractions.Settings
{
    public interface ISettingsSource
    {
        ICollection<Listener> Listeners { get; }
        ICollection<EventDefinition> EventDefinitions { get; }
    }
}