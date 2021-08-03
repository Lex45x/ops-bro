using System;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events.Templates
{
    /// <summary>
    /// Replaces token found by <see cref="Path"/> with value from model found by <see cref="TemplateRule{TTemplate}.Property"/>
    /// </summary>
    public abstract class BodyTemplateRule : TemplateRule<JObject>
    {
        /// <summary>
        /// Json Path to the property in template.
        /// </summary>
        public string Path { get; }

        protected BodyTemplateRule(string property, string path)
            : base(property)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }
    }
}