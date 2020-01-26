using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using OpsBro.Abstractions.Contracts.Events;
using OpsBro.Abstractions.Entities;

namespace OpsBro.Domain.EventSubscribers
{
    //todo: must be as a separate service
    public class GenericEventSubscriber : EventSubscriber<GenericEvent>
    {
        private readonly IGenericLogger<GenericEventSubscriber> logger;

        public GenericEventSubscriber(IGenericLogger<GenericEventSubscriber> logger)
        {
            this.logger = logger;
        }

        public override async Task OnEvent(GenericEvent @event, CancellationToken cancellationToken = new CancellationToken())
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(new JObject
                {
                    ["Message"] = "New event appear",
                    ["Event"] = JToken.FromObject(@event)
                });
            }

            var schema = JSchema.Parse(@event.Definition.Schema);

            if (logger.IsDebugEnabled)
            {
                logger.Debug(new JObject
                {
                    ["Message"] = "Schema is valid",
                    ["Event"] = JToken.FromObject(@event)
                });
            }

            @event.Event.Data.Validate(schema);

            if (logger.IsDebugEnabled)
            {
                logger.Debug(new JObject
                {
                    ["Message"] = "Event is valid",
                    ["Event"] = JToken.FromObject(@event)
                });
            }

            foreach (var subscriber in @event.Definition.Subscribers)
            {
                var model = new JObject
                {
                    ["data"] = @event.Event.Data,
                    ["meta"] = subscriber.Metadata
                };

                if (logger.IsDebugEnabled)
                {
                    logger.Debug(new JObject
                    {
                        ["Message"] = "Created model for subscriber",
                        ["Event"] = JToken.FromObject(@event),
                        ["Subscriber"] = JToken.FromObject(subscriber)
                    });
                }

                var url = ApplyUrlBindings(subscriber.UrlTemplate, model, subscriber.UrlBindings);

                if (logger.IsDebugEnabled)
                {
                    logger.Debug(new JObject
                    {
                        ["Message"] = "Url binding succeed",
                        ["Event"] = JToken.FromObject(@event),
                        ["Url"] = JToken.FromObject(url)
                    });
                }

                var body = ApplyBodyBindings(subscriber.BodyTemplate, model, subscriber.BodyBindings);

                if (logger.IsDebugEnabled)
                {
                    logger.Debug(new JObject
                    {
                        ["Message"] = "Body bindings succeed",
                        ["Event"] = JToken.FromObject(@event),
                        ["Body"] = JToken.FromObject(body)
                    });
                }

                var httpClient = new HttpClient();

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = new HttpMethod(subscriber.Method),
                    Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json"),
                    RequestUri = new Uri(url)
                };

                if (logger.IsDebugEnabled)
                {
                    logger.Debug(new JObject
                    {
                        ["Message"] = "Request message created",
                        ["Event"] = JToken.FromObject(@event)
                    });
                }

                ApplyHeaderBindings(httpRequestMessage.Headers, model, subscriber.HeaderBindings);

                if (logger.IsDebugEnabled)
                {
                    logger.Debug(new JObject
                    {
                        ["Message"] = "Headers bindings succeed",
                        ["Event"] = JToken.FromObject(@event),
                        ["Headers"] = JToken.FromObject(httpRequestMessage.Headers)
                    });
                }

                var responseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken);

                if (logger.IsDebugEnabled)
                {
                    logger.Debug(new JObject
                    {
                        ["Message"] = "Request sent!",
                        ["Event"] = JToken.FromObject(@event),
                        ["Response"] = JToken.FromObject(responseMessage),
                        ["Body"] = await responseMessage.Content.ReadAsStringAsync()
                    });
                }
            }
        }

        private static string ApplyUrlBindings(string url, JToken data, IEnumerable<BasicBinding> bindings)
        {
            return bindings.Aggregate(url,
                (current, binding) => current.Replace(binding.Path, data.SelectToken(binding.Property).ToString()));
        }

        private static void ApplyHeaderBindings(HttpHeaders headers, JToken data, IEnumerable<BasicBinding> bindings)
        {
            foreach (var binding in bindings)
            {
                headers.Add(binding.Path, data.SelectToken(binding.Property).ToString());
            }
        }

        private static JToken ApplyBodyBindings(JToken body, JToken data, IEnumerable<BasicBinding> bindings)
        {
            var bodyClone = body.DeepClone();

            foreach (var binding in bindings)
            {
                var bodyToken = bodyClone.SelectToken(binding.Path);
                var propertyToken = data.SelectToken(binding.Property);

                bodyToken.Replace(propertyToken);
            }

            return bodyClone;
        }
    }
}