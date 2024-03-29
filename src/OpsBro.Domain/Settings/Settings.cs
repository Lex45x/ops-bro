﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction;

namespace OpsBro.Domain.Settings
{

    public class Settings : ISettings
    {
        public Settings(ICollection<Listener> listeners, ICollection<EventDispatcher> eventDispatchers, JObject config, string version)
        {
            var logger = LogManager.GetCurrentClassLogger();

            if (version != Version)
            {
                throw new InvalidOperationException($"Settings version mismatch! Expected: {Version}, actual: {version}");
            }

            ListenersByListenerName = listeners.ToDictionary(listener => listener.Name);
            EventDispatcherByEventName = eventDispatchers.ToDictionary(dispatcher => dispatcher.EventName);
            Config = config;
        }

        public IDictionary<string, Listener> ListenersByListenerName { get; }
        public IDictionary<string, EventDispatcher> EventDispatcherByEventName { get; }
        public JObject Config { get; }

        public static readonly string Version = "v0.4";
    }

}