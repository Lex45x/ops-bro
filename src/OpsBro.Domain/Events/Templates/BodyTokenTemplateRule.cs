using System;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events.Templates
{
    public class BodyTokenTemplateRule : BodyTemplateRule
    {
        /// <inheritdoc />
        public BodyTokenTemplateRule(string property, string path)
            : base(property, path)
        {
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

            bodyToken?.Replace(propertyToken ?? JValue.CreateNull());

            return template;
        }
    }
}