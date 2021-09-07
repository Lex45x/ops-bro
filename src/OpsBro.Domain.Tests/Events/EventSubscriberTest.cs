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
        private static readonly EventSubscriber defaultEventSubscriber = new EventSubscriber("",
            HttpMethod.Post,
            "",
            new List<HttpHeader>());

        [Test]
        public void Constructor_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber(urlTemplate: null,
                    HttpMethod.Post,
                    "{}",
                    new List<HttpHeader>()));

            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber("",
                    method: null,
                    "{}",
                    new List<HttpHeader>()));

            Assert.Throws<ArgumentNullException>(() =>
                new EventSubscriber("",
                    HttpMethod.Post,
                    null,
                    new List<HttpHeader>()));
        }

        [Test]
        public void Dispatch_ArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => defaultEventSubscriber.Handle(null, new JObject()));
        }
    }
}