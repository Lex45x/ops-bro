using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events.Templates
{
    /// <summary>
    /// Replaces all <see cref="Substring"/> in template with value from model found by <see cref="TemplateRule{TTemplate}.Property"/> 
    /// </summary>
    public class UrlTemplateRule : TemplateRule<string>
    {
        /// <summary>
        /// Substring to be replaced in template
        /// </summary>
        public string Substring { get; }

        public UrlTemplateRule(string property, string substring) : base(property)
        {
            Substring = substring;
        }

        /// <inheritdoc />
        public override string Apply(string template, JObject model)
        {
            var propertyValue = model.SelectToken(Property).ToObject<string>();
            var newTemplate = template.Replace(Substring, propertyValue);
            return newTemplate;
        }
    }
}