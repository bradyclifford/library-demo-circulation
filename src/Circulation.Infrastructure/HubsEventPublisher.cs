using System.Text;
using System.Threading.Tasks;
using Circulation.Domain;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace Circulation.Infrastructure
{
    public class HubsEventPublisher : IEventPublisher
    {
        private readonly EventHubClient _hubClient;

        public HubsEventPublisher(EventHubClient hubClient)
        {
            _hubClient = hubClient;
        }

        public async Task Publish<T>(string streamId, T payload)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                //TOOD: any further JSON serialization customization
            };

            string payloadAsJson = JsonConvert.SerializeObject(payload, Formatting.None, settings);
            var eventData = new EventData(Encoding.UTF8.GetBytes(payloadAsJson));

            await _hubClient.SendAsync(eventData, streamId);
        }
    }
}
