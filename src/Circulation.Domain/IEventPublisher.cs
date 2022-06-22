using System.Threading.Tasks;

namespace Circulation.Domain
{
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes a domain event
        /// </summary>
        /// <param name="streamId">Identifies an entity to which the event applies.
        /// Events published to the same stream are to be stored in the same partition and processed sequentially.</param>
        /// <param name="payload">The concrete type of the payload object determines the event type.</param>
        Task Publish<T>(string streamId, T payload);
    }
}
