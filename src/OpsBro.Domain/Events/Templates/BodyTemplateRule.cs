using System;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events.Templates
{
    /// <summary>
    /// Replaces token found by <see cref="Path"/> with value from model found by <see cref="TemplateRule{TTemplate}.Property"/>
    /// </summary>
    public class BodyTemplateRule : TemplateRule<JObject>
    {
        /// <summary>
        /// Json Path to the property in template.
        /// </summary>
        public string Path { get; }

        public BodyTemplateRule(string property, string path) : base(property)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <inheritdoc />
        public override JObject Apply(JObject template, JObject model)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var bodyToken = template.SelectToken(Path);
            var propertyToken = model.SelectToken(Property);

            bodyToken.Replace(propertyToken);

            return template;
        }
    }
}