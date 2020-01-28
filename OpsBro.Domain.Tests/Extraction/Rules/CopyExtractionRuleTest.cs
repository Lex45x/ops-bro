using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpsBro.Domain.Extraction.Rules;

namespace OpsBro.Domain.Tests.Extraction.Rules
{
    [TestFixture(Author = "lex45x", Category = "Unit,Domain", TestOf = typeof(CopyExtractionRule))]
    public class CopyExtractionRuleTest
    {
        public static readonly CopyExtractionRule Empty = new CopyExtractionRule("", "");
        public static readonly CopyExtractionRule DefaultRule = new CopyExtractionRule("key", "property");


        [Test]
        public void Constructor_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => new CopyExtractionRule(path: null, ""));
            Assert.Throws<ArgumentNullException>(() => new CopyExtractionRule("", property: null));
        }

        [Test]
        public void ApplyRule_EmptyRule_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => Empty.ApplyRule(eventData: null, new JObject()));
            Assert.Throws<ArgumentNullException>(() => Empty.ApplyRule(new JObject(), payload: null));
        }

        [Test]
        public void ApplyRule_DefaultRule_EmptyPayload()
        {
            var eventData = new JObject();
            var payload = new JObject();

            var updatedEventData = DefaultRule.ApplyRule(eventData, payload);

            Assert.AreSame(eventData, updatedEventData);
            CollectionAssert.IsEmpty(eventData);
        }

        [Test]
        [TestCaseSource(nameof(GetPlainPayload))]
        public void ApplyRule_DefaultRule_NonEmptyPayload(JToken payloadProperty)
        {
            var eventData = new JObject();

            var payload = new JObject
            {
                [DefaultRule.Path] = payloadProperty
            };

            var updatedEventData = DefaultRule.ApplyRule(eventData, payload);

            Assert.AreSame(eventData, updatedEventData);
            CollectionAssert.IsNotEmpty(eventData);
            Assert.IsTrue(eventData.ContainsKey(DefaultRule.Property));
            Assert.AreEqual(eventData[DefaultRule.Property], payloadProperty);
        }

        [Test]
        [TestCaseSource(nameof(GetJsonPathRules))]
        public void ApplyRule_JsonPathRule_PredefinedPayload(CopyExtractionRule rule)
        {
            var eventData = new JObject();

            var payload = new JObject
            {
                {"Cpu", "Intel"},
                {"Memory", 32},
                {
                    "Drives", new JArray
                    {
                        "DVD",
                        "SSD"
                    }
                }
            };

            var updatedEventData = rule.ApplyRule(eventData, payload);

            Assert.AreSame(eventData, updatedEventData);
            CollectionAssert.IsNotEmpty(eventData);
            Assert.IsTrue(eventData.ContainsKey(rule.Property));
            Assert.AreEqual(eventData[rule.Property], payload.SelectToken(rule.Path));
        }

        private static IEnumerable<CopyExtractionRule> GetJsonPathRules()
        {
            return new List<CopyExtractionRule>
            {
                new CopyExtractionRule("Cpu", "central processing unit"),
                new CopyExtractionRule("Memory", "RAM"),
                new CopyExtractionRule("Drives", "Drives"),
                new CopyExtractionRule("Drives[0]", "Main Drive"),
                new CopyExtractionRule("Drives[-1:]", "Last Drive")
            };
        }

        private static IEnumerable<JToken> GetPlainPayload()
        {
            return new List<JToken>
            {
                JToken.FromObject("some value"),
                JToken.FromObject(o: 42),
                JToken.FromObject(Enumerable.Range(0, 100)),
                JToken.FromObject(new {Key = 15, Value = 22})
            };
        }
    }
}