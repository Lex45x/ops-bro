using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpsBro.Abstractions.Contracts.Events;
using OpsBro.Abstractions.Entities;
using OpsBro.Domain.Bind;
using OpsBro.Domain.EventSubscribers;

namespace OpsBro.Domain.Tests
{
    [TestFixture]
    public class BindingServiceTest
    {
        public Listener Listener => new Listener
        {
            Name = "test",
            Binders = new List<Binder>
            {
                new Binder
                {
                    Name = "first",
                    Event = "first_event",
                    Validators = new List<Validator>
                    {
                        new Validator
                        {
                            Path = "key",
                            Value = new JValue("superB")
                        },
                        new Validator
                        {
                            Path = "story.objective",
                            Value = new JValue(75)
                        }
                    },
                    Bindings = new List<SelectionBinding>
                    {
                        new SelectionBinding
                        {
                            Path = "some_url",
                            Property = "avatar",
                            Selector = "[a-z]{1,6}"
                        },
                        new SelectionBinding
                        {
                            Path = "extra_value",
                            Property = "age"
                        }
                    }
                }
            }
        };

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

        public ICollection<EventDefinition> EventDefinitions => new List<EventDefinition>
        {
            new EventDefinition
            {
                Name = "first_event",
                Schema =
                    "{\"$id\":\"http://example.com/example.json\",\"type\":\"object\",\"properties\":{\"avatar\":{\"$id\":\"/properties/avatar\",\"type\":\"string\",\"title\":\"The Avatar Schema \",\"default\":\"\",\"examples\":[\"asd\"]},\"age\":{\"$id\":\"/properties/age\",\"type\":\"integer\",\"title\":\"The Age Schema \",\"default\":0,\"examples\":[12]}},\"required\":[\"avatar\",\"age\"]}",
                Subscribers = new List<EventSubscriber>
                {
                    new EventSubscriber
                    {
                        Metadata = new JObject
                        {
                            ["api_key"] = "supervalue"
                        },
                        UrlTemplate = "http://requestbin.fullcontact.com/13ci09q1/{age}",
                        UrlBindings = new List<BasicBinding>
                        {
                            new BasicBinding
                            {
                                Path = "{age}",
                                Property = "data.age"
                            }
                        },
                        Method = "POST",
                        BodyTemplate = new JObject
                        {
                            ["hardcoded"] = "value",
                            ["data-template"] = "",
                            ["meta-template"] = ""
                        },
                        BodyBindings = new List<BasicBinding>
                        {
                            new BasicBinding
                            {
                                Path = "data-template",
                                Property = "data.avatar"
                            },
                            new BasicBinding
                            {
                                Path = "meta-template",
                                Property = "meta.api_key"
                            }
                        },
                        HeaderBindings = new List<BasicBinding>
                        {
                            new BasicBinding
                            {
                                Path = "Authorization",
                                Property = "meta.api_key"
                            }
                        }
                    }
                }
            }
        };

        [Test]
        public async Task Test()
        {
            var bindingService = new BindingService();

            var events = bindingService.Bind(Listener, Data).ToList();

            var subscriber = new GenericEventSubscriber();

            foreach (var @event in events)
            {
                await subscriber.OnEvent(new GenericEvent
                {
                    Event = @event,
                    Definition = EventDefinitions.FirstOrDefault(definition => definition.Name == @event.Name)
                });
            }
        }
    }
}