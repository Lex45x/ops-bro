using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction;
using OpsBro.Domain.Extraction.ExtractionRules;
using OpsBro.Domain.Extraction.UnnestingRules;
using OpsBro.Domain.Extraction.ValidationRules;

namespace OpsBro.Domain.Tests.Extraction
{
    [TestFixture(Author = "lex45x", Category = "Unit,Domain", TestOf = typeof(Listener))]
    public class ListenerTest
    {
        [Test]
        public void Constructor_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => new Listener(null, new List<Extractor>(), new List<UnnestingRule>()));
            Assert.Throws<ArgumentNullException>(() => new Listener("", null, new List<UnnestingRule>()));
        }

        [Test]
        public void ExtractAll_ArgumentValidation()
        {
            var listener = new Listener("", new List<Extractor>(), new List<UnnestingRule>());
            Assert.Throws<ArgumentNullException>(() => listener.ExtractAll(null).ToList());
        }

        [Test]
        [SuppressMessage("ReSharper", "RedundantBoolCompare", Justification = "Required for the proper mock setup")]
        public void ExtractAll_EmptyPayload()
        {
            var firstEvent = new Event("first", new JObject());
            var secondEvent = new Event("second", new JObject());
            var thirdEvent = new Event("third", new JObject());

            var firstExtractor = CreateMockedExtractor(firstEvent, returnValue: true);
            var secondExtractor = CreateMockedExtractor(secondEvent, returnValue: false);
            var thirdExtractor = CreateMockedExtractor(thirdEvent, returnValue: true);
            
            var extractor = new Listener("", new List<Extractor>
            {
                firstExtractor,
                secondExtractor,
                thirdExtractor
            }, new List<UnnestingRule>());

            var payload = new JObject();
            ICollection<Event> extractedEvents = null;

            Assert.DoesNotThrow(() => extractedEvents = extractor.ExtractAll(payload).ToList());

            Assert.AreEqual(expected: 2, extractedEvents.Count);
            CollectionAssert.Contains(extractedEvents, firstEvent);
            CollectionAssert.DoesNotContain(extractedEvents, secondEvent);
            CollectionAssert.Contains(extractedEvents, thirdEvent);
        }

        private static Extractor CreateMockedExtractor(Event @event, bool returnValue)
        {
            var extractorMock = new Mock<Extractor>(MockBehavior.Strict,
                "",
                "",
                new List<ExtractionRule>(),
                new List<ValidationRule>());
            extractorMock
                .Setup(mockedExtractor => mockedExtractor.TryExtract(It.IsAny<JObject>(), out @event))
                .Returns(returnValue);

            return extractorMock.Object;
        }
    }
}