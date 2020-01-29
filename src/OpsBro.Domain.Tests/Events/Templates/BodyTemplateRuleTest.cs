using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpsBro.Domain.Events;
using OpsBro.Domain.Events.Templates;

namespace OpsBro.Domain.Tests.Events.Templates
{
    [TestFixture(Author = "lex45x", Category = "Unit,Domain", TestOf = typeof(BodyTemplateRule))]
    public class BodyTemplateRuleTest
    {
        [Test]
        public void Constructor_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => new BodyTemplateRule(null, ""));
            Assert.Throws<ArgumentNullException>(() => new BodyTemplateRule("", null));
        }

        [Test]
        public void Dispatch_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => new BodyTemplateRule("", "").Apply(null, new JObject()));
            Assert.Throws<ArgumentNullException>(() => new BodyTemplateRule("", "").Apply(new JObject(), null));
        }
    }
}