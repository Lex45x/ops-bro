using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Moq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using OpsBro.Domain.Events;
using OpsBro.Domain.Events.Templates;
using OpsBro.Domain.Extraction;

namespace OpsBro.Domain.Tests.Events
{
    [TestFixture(Author = "lex45x", Category = "Unit,Domain", TestOf = typeof(EventDispatcher))]
    public class EventDispatcherTest
    {
        public static readonly string EventName = "EventDispatcherTest_Event";
        public static readonly JSchema EmptyJSchema = JSchema.Parse("{}");

        public static readonly EventDispatcher DefaultDispatcher =
            new EventDispatcher(EventName, EmptyJSchema, new List<EventSubscriber>());

        [Test]
        public void Constructor_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EventDispatcher(null, EmptyJSchema, new List<EventSubscriber>()));
            Assert.Throws<ArgumentNullException>(() => new EventDispatcher("", null, new List<EventSubscriber>()));
            Assert.Throws<ArgumentNullException>(() => new EventDispatcher(null, EmptyJSchema, null));
        }

        [Test]
        public void Dispatch_ArgumentValidation()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DefaultDispatcher.Dispatch(null));
        }

        [Test]
        public void Dispatch_InvalidEventName()
        {
            var @event = new Event("", new JObject());
            Assert.ThrowsAsync<InvalidOperationException>(() => DefaultDispatcher.Dispatch(@event));
        }

        [Test]
        public void Dispatch_InvalidSchema()
        {
            var eventDispatcher = new EventDispatcher(EventName,
                JSchema.Parse("{\"required\":[\"required_property\"] }"), new List<EventSubscriber>());

            var @event = new Event(EventName, new JObject
            {
                ["property"] = "value"
            });

            Assert.ThrowsAsync<JSchemaValidationException>(() => eventDispatcher.Dispatch(@event));
        }

        [Test]
        public void Dispatch_ForwardedToSubscribers()
        {
            var firstEventSubscriberMock = CreateEventSubscriberMock();
            var secondEventSubscriberMock = CreateEventSubscriberMock();
            secondEventSubscriberMock.Setup(subscriber => subscriber.Handle(It.IsAny<Event>()))
                .Throws<ApplicationException>();

            var eventDispatcher = new EventDispatcher(EventName, EmptyJSchema, new List<EventSubscriber>
            {
                firstEventSubscriberMock.Object,
                secondEventSubscriberMock.Object
            });

            var @event = new Event(EventName, new JObject());

            Assert.DoesNotThrowAsync(() => eventDispatcher.Dispatch(@event));

            Mock.VerifyAll(firstEventSubscriberMock, secondEventSubscriberMock);
        }

        private static Mock<EventSubscriber> CreateEventSubscriberMock()
        {
            return new Mock<EventSubscriber>("", 
                HttpMethod.Post, 
                new JObject(),
                new List<BodyTemplateRule>(),
                new List<HeaderTemplateRule>(),
                new List<UrlTemplateRule>(),
                new JObject());
        }
    }
}