using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events.Templates
{
    /// <summary>
    /// Adds header to <see cref="HttpHeaders"/> with value from model found by <see cref="TemplateRule{TTemplate}.Property"/>
    /// </summary>
    public class HttpHeader : TemplateRule<HttpHeaders>
    {
        /// <summary>
        /// The name of the header to assign value from <see cref="TemplateRule.Property"/>
        /// </summary>
        public string Name { get; }

        public HttpHeader(string property, string name) : base(property)
        {
            Name = name;
        }

        /// <inheritdoc />
        public override HttpHeaders Apply(HttpHeaders template, JObject model)
        {
            var propertyToken = model.SelectToken(Property);
            template.Add(Name, propertyToken.ToObject<string>());
            return template;
        }
    }
}