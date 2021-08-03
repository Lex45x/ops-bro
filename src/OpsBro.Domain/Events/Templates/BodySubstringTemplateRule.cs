using System;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events.Templates
{
    /// <summary>
    /// Replaces <see cref="Substring"/> in token found by <see cref="BodyTemplateRule.Path"/> with value from model found by <see cref="TemplateRule{TTemplate}.Property"/>
    /// </summary>
    public class BodySubstringTemplateRule : BodyTemplateRule
    {
        /// <inheritdoc />
        public BodySubstringTemplateRule(string property, string path, string substring)
            : base(property, path)
        {
            Substring = substring;
        }

        /// <summary>
        /// Substring to replace.
        /// </summary>
        public string Substring { get; }

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

            var propertyValue = model.SelectToken(Property)
                ?.ToObject<string>() ?? string.Empty;

            var body = bodyToken?.ToObject<string>() ?? string.Empty;

            var updatedBody = body.Replace(Substring, propertyValue);

            bodyToken?.Replace(updatedBody);

            return template;
        }
    }
}