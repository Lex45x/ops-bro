using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events.Templates
{
    /// <summary>
    /// Adds header to <see cref="HttpHeaders"/> with value from model found by <see cref="TemplateRule{TTemplate}.Property"/>
    /// </summary>
    public class HeaderTemplateRule : TemplateRule<HttpHeaders>
    {
        /// <summary>
        /// The name of the header to assign value from <see cref="TemplateRule.Property"/>
        /// </summary>
        public string HeaderName { get; }

        public HeaderTemplateRule(string property, string headerName) : base(property)
        {
            HeaderName = headerName;
        }

        /// <inheritdoc />
        public override HttpHeaders Apply(HttpHeaders template, JObject model)
        {
            var propertyToken = model.SelectToken(Property);
            template.Add(HeaderName, propertyToken.ToObject<string>());
            return template;
        }
    }
}