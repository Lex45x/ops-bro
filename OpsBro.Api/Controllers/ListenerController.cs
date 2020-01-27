using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Settings;

namespace OpsBro.Api.Controllers
{
    [Route("api/{listenerName}")]
    public class ListenerController : ControllerBase
    {
        private readonly ISettings settings;

        public ListenerController(ISettings settings)
        {
            this.settings = settings;
        }

        [HttpPost]
        public async Task Call([FromRoute] string listenerName, [FromBody] JObject body, [FromQuery] JObject query)
        {
            listenerName = Uri.UnescapeDataString(listenerName);
            var listener = settings.Listeners.First(customerListener => customerListener.Name == listenerName);

            var payload = new JObject
            {
                ["query"] = query,
                ["body"] = body,
                //issue: Asp.Net core store all unknown headers in lowercase, so it's unable to retrieve original case of the header
                ["headers"] = JToken.FromObject(Request.Headers)
            };
            
            foreach (var @event in listener.ExtractAll(payload))
            {
                var eventDispatcher =
                    settings.EventDispatchers.FirstOrDefault(dispatcher => dispatcher.EventName == @event.Name);

                if (eventDispatcher == null)
                {
                    continue;
                }

                await eventDispatcher.Dispatch(@event);
            }
        }

        
    }
}