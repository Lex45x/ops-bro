using System;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction;
using OpsBro.Domain.Extraction.Rules;
using OpsBro.Domain.Extraction.Validation;

namespace OpsBro.Domain.Tests.Extraction
{
    [TestFixture(Author = "lex45x", Category = "Unit,Domain", TestOf = typeof(Extractor))]
    public class ExtractorTest
    {
        [Test]
        public void Constructor_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Extractor(name: null, "", new List<ExtractionRule>(), new List<ValidationRule>()));
            Assert.Throws<ArgumentNullException>(() =>
                new Extractor("", eventName: null, new List<ExtractionRule>(), new List<ValidationRule>()));
            Assert.Throws<ArgumentNullException>(() =>
                new Extractor("", "", extractionRules: null, new List<ValidationRule>()));
            Assert.Throws<ArgumentNullException>(() =>
                new Extractor("", "", new List<ExtractionRule>(), validationRules: null));
        }

        [Test]
        public void TryExtract_ArgumentValidation()
        {
            var extractor = new Extractor("", "", new List<ExtractionRule>(), new List<ValidationRule>());
            Assert.Throws<ArgumentNullException>(() => extractor.TryExtract(payload: null, out var extractedEvent));
        }

        [Test]
        [TestCaseSource(nameof(PayloadValidationTestCases))]
        public void TryExtract_PayloadValidation(PayloadValidationTestCase testCase)
        {
            var extractionRuleMock =
                new Mock<ExtractionRule>(MockBehavior.Strict, ExtractionType.Copy, string.Empty, string.Empty);

            extractionRuleMock
                .Setup(rule => rule.ApplyRule(It.IsAny<JObject>(), It.IsAny<JObject>()))
                .Returns<JObject, JObject>((eventData, payload) => eventData);

            var extractionRules = new List<ExtractionRule>
            {
                extractionRuleMock.Object
            };

            var extractor = new Extractor("", "", extractionRules, testCase.ValidationRules);

            bool? tryExtract = null;
            Event extractedEvent = null;

            Assert.DoesNotThrow(() => tryExtract = extractor.TryExtract(testCase.Payload, out extractedEvent));
            Assert.AreEqual(tryExtract, testCase.ExpectedResult);

            if (testCase.ExpectedResult)
            {
                Assert.IsNotNull(extractedEvent);
                Assert.DoesNotThrow(() =>
                    extractionRuleMock.Verify(rule => rule.ApplyRule(It.IsAny<JObject>(), It.IsAny<JObject>()),
                        Times.Once));
            }
            else
            {
                Assert.IsNull(extractedEvent);
                Assert.DoesNotThrow(() =>
                    extractionRuleMock.Verify(rule => rule.ApplyRule(It.IsAny<JObject>(), It.IsAny<JObject>()),
                        Times.Never));
            }
        }

        [Test]
        public void TryExtract_NoValidationOnPayload_ExtractionRulesApplied()
        {
            var validationRules = new List<ValidationRule>();

            var extractionRuleMock =
                new Mock<ExtractionRule>(MockBehavior.Strict, ExtractionType.Copy, string.Empty, string.Empty);

            extractionRuleMock
                .Setup(rule => rule
                    .ApplyRule(It.IsAny<JObject>(), It.IsAny<JObject>()))
                .Returns<JObject, JObject>((eventData, payload) =>
                {
                    eventData["payload"] = payload;
                    return eventData;
                })
                .Verifiable();

            var extractionRules = new List<ExtractionRule>
            {
                extractionRuleMock.Object
            };

            var eventName = "event";

            var extractor = new Extractor("", eventName, extractionRules, validationRules);

            var testPayload = new JObject();
            bool? tryExtract = null;
            Event extractedEvent = null;

            Assert.DoesNotThrow(() => tryExtract = extractor.TryExtract(testPayload, out extractedEvent));
            Assert.DoesNotThrow(() => extractionRuleMock.Verify());

            Assert.IsTrue(tryExtract);
            Assert.IsNotNull(extractedEvent);
            Assert.AreEqual(extractedEvent.Name, eventName);
            Assert.True(JToken.DeepEquals(extractedEvent.Data["payload"], testPayload));
        }

        public struct PayloadValidationTestCase
        {
            public PayloadValidationTestCase(JObject payload, ICollection<ValidationRule> validationRules,
                bool expectedResult)
            {
                Payload = payload;
                ValidationRules = validationRules;
                ExpectedResult = expectedResult;
            }

            public JObject Payload { get; }
            public ICollection<ValidationRule> ValidationRules { get; }
            public bool ExpectedResult { get; }
        }

        public static IEnumerable<PayloadValidationTestCase> PayloadValidationTestCases()
        {
            var payload = new JObject
            {
                ["property"] = "value"
            };

            return new List<PayloadValidationTestCase>
            {
                new PayloadValidationTestCase(payload,
                    new List<ValidationRule>
                    {
                        new ValueValidationRule("property", "value", ValidationOperator.Equals)
                    },
                    expectedResult: true),
                new PayloadValidationTestCase(payload,
                    new List<ValidationRule>
                    {
                        new ValueValidationRule("property", "another value", ValidationOperator.Equals)
                    }, expectedResult: false),
                new PayloadValidationTestCase(payload, new List<ValidationRule>
                {
                    new ValueValidationRule("property", "another value", ValidationOperator.NotEquals)
                }, expectedResult: true),
                new PayloadValidationTestCase(payload,
                    new List<ValidationRule>
                    {
                        new ValueValidationRule("property", "value", ValidationOperator.NotEquals)
                    },
                    expectedResult: false),
                new PayloadValidationTestCase(new JObject
                {
                    ["id"] = 418,
                    ["name"] = "Larry Masinter",
                    ["gender"] = "teapot",
                    ["tags"] = new JArray
                    {
                        JToken.FromObject("fun"),
                        JToken.FromObject("http"),
                        JToken.FromObject("meme")
                    }
                }, new List<ValidationRule>
                {
                    new ValueValidationRule("id", value: 418, ValidationOperator.Equals),
                    new ValueValidationRule("gender", "coffee machine", ValidationOperator.NotEquals),
                    new ValueValidationRule("tags[1]", "http", ValidationOperator.Equals),
                    new ValueValidationRule("tags[0]", "tea", ValidationOperator.NotEquals)
                }, expectedResult: true),
            };
        }
    }
}