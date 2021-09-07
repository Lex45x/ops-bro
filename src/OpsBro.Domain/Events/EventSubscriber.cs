using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using OpsBro.Domain.Events.Templates;

namespace OpsBro.Domain.Events
{
    /// <summary>
    /// Sends and HTTP request once event has been dispatched
    /// </summary>
    public class EventSubscriber
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public EventSubscriber(HandlebarTemplate urlTemplate, HttpMethod method, HandlebarTemplate bodyTemplate,
            ICollection<HttpHeader> headers)
        {
            UrlTemplate = urlTemplate ?? throw new ArgumentNullException(nameof(urlTemplate));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            BodyTemplate = bodyTemplate ?? throw new ArgumentNullException(nameof(bodyTemplate));
            Headers = headers ?? new List<HttpHeader>(capacity: 0);
        }

        /// <summary>
        /// Template sting for the url
        /// </summary>
        public HandlebarTemplate UrlTemplate { get; }

        /// <summary>
        /// Http method to be called
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// Template of the body
        /// </summary>
        public HandlebarTemplate BodyTemplate { get; }

        /// <summary>
        /// Set of rules to extract model from event/config and apply it to headers template
        /// </summary>
        public ICollection<HttpHeader> Headers { get; }

        /// <summary>
        /// Fill template with model from event and send http request.
        /// </summary>
        /// <param name="extractedEvent"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public virtual async Task Handle(Event extractedEvent, JObject config)
        {
            if (extractedEvent == null)
            {
                throw new ArgumentNullException(nameof(extractedEvent));
            }

            var model = new JObject
            {
                ["event"] = extractedEvent.Data,
                ["config"] = config
            };

            var url = UrlTemplate.Build(model);
            logger.Debug("Url template filled: {url}", url);

            var body = BodyTemplate.Build(model);
            logger.Debug("Body template filled: {body}", body);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = Method,
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
                RequestUri = new Uri(url)
            };

            foreach (var headerTemplateRule in Headers)
            {
                headerTemplateRule.Apply(httpRequestMessage.Headers, model);
            }

            logger.Debug("Headers template filled: {header}", httpRequestMessage.Headers);

            using var httpClient = new HttpClient();
            var responseMessage =
                await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);

            if (logger.IsDebugEnabled)
            {
                var content = await responseMessage.Content.ReadAsStringAsync();
                logger.Debug("Http request sent! Response: {content}", content);
            }

            responseMessage.EnsureSuccessStatusCode();
        }
    }
}