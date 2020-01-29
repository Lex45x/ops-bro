using System;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events.Templates
{
    /// <summary>
    /// Used to define template rules inside vent subscriber
    /// </summary>
    public abstract class TemplateRule<TTemplate>
    {
        protected TemplateRule(string property)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
        }

        /// <summary>
        /// Path inside model JObject.
        /// </summary>
        public string Property { get; }

        public abstract TTemplate Apply(TTemplate template, JObject model);
    }
}