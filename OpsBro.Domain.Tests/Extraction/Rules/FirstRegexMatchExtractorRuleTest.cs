using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpsBro.Domain.Extraction.Rules;

namespace OpsBro.Domain.Tests.Extraction.Rules
{
    [TestFixture(Author = "lex45x", Category = "Unit", TestOf = typeof(FirstRegexMatchExtractorRule))]
    public class FirstRegexMatchExtractorRuleTest
    {
        public static readonly FirstRegexMatchExtractorRule Empty = new FirstRegexMatchExtractorRule("", "", "");

        public static readonly FirstRegexMatchExtractorRule DefaultRule =
            new FirstRegexMatchExtractorRule("key", "property", "[a-z]");


        [Test]
        public void Constructor_EmptyRule_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => new FirstRegexMatchExtractorRule(path: null, "", ""));
            Assert.Throws<ArgumentNullException>(() => new FirstRegexMatchExtractorRule("", property: null, ""));
            Assert.Throws<ArgumentNullException>(() => new FirstRegexMatchExtractorRule("", "", null));
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
            var value = eventData[DefaultRule.Property].ToObject<string>();
            StringAssert.IsMatch(DefaultRule.Pattern, value);
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidPayload))]
        public void ApplyRule_DefaultRule_InvalidPayload(JToken payloadProperty)
        {
            var eventData = new JObject();

            var payload = new JObject
            {
                [DefaultRule.Path] = payloadProperty
            };

            var updatedEventData = DefaultRule.ApplyRule(eventData, payload);

            Assert.AreSame(eventData, updatedEventData);
            CollectionAssert.IsEmpty(eventData);
        }

        [Test]
        [TestCaseSource(nameof(GetJsonPathRules))]
        public void ApplyRule_JsonPathRule_PredefinedPayload(FirstRegexMatchExtractorRule rule)
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
            var value = eventData[rule.Property].ToObject<string>();
            StringAssert.IsMatch(rule.Pattern, value);
        }

        private static IEnumerable<FirstRegexMatchExtractorRule> GetJsonPathRules()
        {
            return new List<FirstRegexMatchExtractorRule>
            {
                new FirstRegexMatchExtractorRule("Cpu", "central processing unit", "[a-z]"),
                new FirstRegexMatchExtractorRule("Drives[0]", "Main Drive", "D"),
                new FirstRegexMatchExtractorRule("Drives[-1:]", "Last Drive", "S")
            };
        }

        private static IEnumerable<JToken> GetPlainPayload()
        {
            return new List<JToken>
            {
                JToken.FromObject("some value"),
                JToken.FromObject("Some Value In Different Cases")
            };
        }

        private static IEnumerable<JToken> GetInvalidPayload()
        {
            return new List<JToken>
            {
                JToken.FromObject(42),
                JToken.FromObject("NO LOWERCASE"),
                JToken.FromObject("私はあなたを待っていた"),
                JToken.FromObject(Enumerable.Repeat(42, 42)),
                JToken.FromObject(new object())
            };
        }
    }
}