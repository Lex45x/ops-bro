using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Events.Templates;

namespace OpsBro.Domain.Events
{
    /// <summary>
    /// Sends and HTTP request once event has been dispatched
    /// </summary>
    public class EventSubscriber
    {
        public EventSubscriber(string urlTemplate, HttpMethod method, JObject bodyTemplate,
            ICollection<TemplateRule> bodyTemplateRules,
            ICollection<TemplateRule> headerTemplateRules,
            ICollection<TemplateRule> urlTemplateRules, JObject metadata)
        {
            UrlTemplate = urlTemplate ?? throw new ArgumentNullException(nameof(urlTemplate));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            BodyTemplate = bodyTemplate ?? throw new ArgumentNullException(nameof(bodyTemplate));
            BodyTemplateRules = bodyTemplateRules ?? throw new ArgumentNullException(nameof(bodyTemplateRules));
            HeaderTemplateRules = headerTemplateRules ?? throw new ArgumentNullException(nameof(headerTemplateRules));
            UrlTemplateRules = urlTemplateRules ?? throw new ArgumentNullException(nameof(urlTemplateRules));
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Template sting for the url
        /// </summary>
        public string UrlTemplate { get; }

        /// <summary>
        /// Http method to be called
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// Template of the body
        /// </summary>
        public JObject BodyTemplate { get; }

        /// <summary>
        /// Set of rules to extract data from event/metadata and apply it to body template
        /// </summary>
        public ICollection<TemplateRule> BodyTemplateRules { get; }

        /// <summary>
        /// Set of rules to extract model from event/metadata and apply it to headers template
        /// </summary>
        public ICollection<TemplateRule> HeaderTemplateRules { get; }

        /// <summary>
        /// Set of rules to extract model from event/metadata and apply it to headers template
        /// </summary>
        public ICollection<TemplateRule> UrlTemplateRules { get; }

        /// <summary>
        /// All additional model that required for template but can be subject to change independently from template (like authorization model) should be here.
        /// </summary>
        public JObject Metadata { get; }

        /// <summary>
        /// Fill template with model from event and send http request.
        /// </summary>
        /// <param name="extractedEvent"></param>
        /// <returns></returns>
        public virtual async Task Handle(Event extractedEvent)
        {
            if (extractedEvent == null)
            {
                throw new ArgumentNullException(nameof(extractedEvent));
            }

            var model = new JObject
            {
                ["event"] = extractedEvent.Data,
                ["meta"] = Metadata
            };

            var url = ApplyUrlTemplate(UrlTemplate, model, UrlTemplateRules);
            var body = ApplyBodyTemplate(BodyTemplate, model, BodyTemplateRules);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = Method,
                Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json"),
                RequestUri = new Uri(url)
            };

            ApplyHeaderTemplate(httpRequestMessage.Headers, model, HeaderTemplateRules);

            using var httpClient = new HttpClient();
            var responseMessage = await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
            responseMessage.EnsureSuccessStatusCode();
        }

        private static string ApplyUrlTemplate(string url, JToken model, ICollection<TemplateRule> templateRules)
        {
            return templateRules.Aggregate(url,
                (current, binding) =>
                    current.Replace(binding.Token, model.SelectToken(binding.Property).ToObject<string>()));
        }

        private static void ApplyHeaderTemplate(HttpHeaders headers, JToken model,
            ICollection<TemplateRule> templateRules)
        {
            foreach (var binding in templateRules)
            {
                headers.Add(binding.Token, model.SelectToken(binding.Property).ToString());
            }
        }

        private static JToken ApplyBodyTemplate(JToken body, JToken model, ICollection<TemplateRule> templateRules)
        {
            var bodyClone = body.DeepClone();

            foreach (var binding in templateRules)
            {
                var bodyToken = bodyClone.SelectToken(binding.Token);
                var propertyToken = model.SelectToken(binding.Property);

                bodyToken.Replace(propertyToken);
            }

            return bodyClone;
        }
    }
}