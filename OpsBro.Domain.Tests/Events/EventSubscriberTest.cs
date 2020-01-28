using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpsBro.Domain.Events;
using OpsBro.Domain.Events.Templates;

namespace OpsBro.Domain.Tests.Events
{
    [TestFixture(Author = "lex45x", Category = "Unit,Domain", TestOf = typeof(EventSubscriber))]
    public class EventSubscriberTest
    {
        private static readonly EventSubscriber DefaultEventSubscriber = new EventSubscriber("",
            HttpMethod.Post,
            new JObject(),
            new List<TemplateRule>(),
            new List<TemplateRule>(),
            new List<TemplateRule>(),
            new JObject());

        [Test]
        public void Constructor_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber(urlTemplate: null,
                    HttpMethod.Post,
                    new JObject(),
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    new JObject()));

            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber("",
                    method: null,
                    new JObject(),
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    new JObject()));

            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber("",
                    HttpMethod.Post,
                    null,
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    new JObject()));

            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber("",
                    HttpMethod.Post,
                    new JObject(),
                    null,
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    new JObject()));

            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber("",
                    HttpMethod.Post,
                    new JObject(),
                    new List<TemplateRule>(),
                    null,
                    new List<TemplateRule>(),
                    new JObject()));

            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber("",
                    HttpMethod.Post,
                    new JObject(),
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    null,
                    new JObject()));

            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber("",
                    HttpMethod.Post,
                    new JObject(),
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    new List<TemplateRule>(),
                    null));
        }

        [Test]
        public void Dispatch_ArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DefaultEventSubscriber.Handle(null));
        }
    }
}