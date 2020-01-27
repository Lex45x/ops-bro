using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using NUnit.Framework.Internal;
using OpsBro.Domain.Entities;
using OpsBro.Domain.Entities.Events;
using OpsBro.Domain.Entities.Extraction;
using OpsBro.Domain.Entities.Extraction.Rules;

namespace OpsBro.Domain.Tests
{
    [TestFixture]
    public class BindingServiceTest
    {
        public Listener Listener { get; } =
            new Listener("test", new List<Extractor>
            {
                new Extractor("first", "first event", new List<ExtractionRule>
                {
                    new FirstRegexMatchExtractorRule("some_url", "avatar", "[a-z]{1,6}"),
                    new CopyExtractionRule("extra_value", "age")
                }, new List<ValidationRule>
                {
                    new ValidationRule("key", new JValue("superB"), ValidationOperator.Equals),
                    new ValidationRule("story.objective", new JValue(75), ValidationOperator.Equals)
                })
            });

        public JObject Data => new JObject
        {
            ["key"] = "superB",
            ["story"] = new JObject
            {
                ["objective"] = 75
            },
            ["some_url"] = "http://some.web.site",
            ["extra_value"] = 13
        };


        public EventDispatcher Dispatcher = new EventDispatcher("first_event",
            JSchema.Parse(
                "{\"$id\":\"http://example.com/example.json\",\"type\":\"object\",\"properties\":{\"avatar\":{\"$id\":\"/properties/avatar\",\"type\":\"string\",\"title\":\"The Avatar Schema \",\"default\":\"\",\"examples\":[\"asd\"]},\"age\":{\"$id\":\"/properties/age\",\"type\":\"integer\",\"title\":\"The Age Schema \",\"default\":0,\"examples\":[12]}},\"required\":[\"avatar\",\"age\"]}"),
            new List<EventSubscriber>
            {
                new EventSubscriber("http://requestbin.fullcontact.com/13ci09q1/{age}",
                    HttpMethod.Post,
                    new JObject
                    {
                        ["hardcoded"] = "value",
                        ["data-template"] = "",
                        ["meta-template"] = ""
                    },
                    new List<TemplateRule>
                    {
                        new TemplateRule("data-template", "event.avatar"),
                        new TemplateRule("meta-template", "meta.api_key")
                    },
                    new List<TemplateRule>
                    {
                        new TemplateRule("Authorization", "meta.api_key")
                    },
                    new List<TemplateRule>
                    {
                        new TemplateRule("{age}", "data.age")
                    }, new JObject
                    {
                        ["api_key"] = "supervalue"
                    }
                )
            });

        public async Task Test()
        {
            foreach (var @event in Listener.ExtractAll(Data))
            {
                await Dispatcher.Dispatch(@event);
            }
        }
    }
}