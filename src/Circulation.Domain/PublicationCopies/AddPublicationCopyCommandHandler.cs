using System.Collections.Generic;
using System.Threading.Tasks;

namespace Circulation.Domain.PublicationCopies
{
    public sealed class AddPublicationCopyCommandDto
    {
        public string Isbn { get; set; }
    }

    public sealed class AddPublicationCopyCommandHandler
    {
        private readonly IPublicationCopyIdGenerator _idGenerator;
        private readonly IPublicationCopyRepository _repository;
        private readonly IEventPublisher _publisher;

        public AddPublicationCopyCommandHandler(IPublicationCopyIdGenerator idGenerator, IPublicationCopyRepository repository, IEventPublisher publisher)
        {
            _idGenerator = idGenerator;
            _repository = repository;
            _publisher = publisher;
        }

        public async Task<CommandResult<int>> Handle(AddPublicationCopyCommandDto addPublicationCopyCommandCommand)
        {
            var newCopy = await PublicationCopy.Create(_idGenerator, addPublicationCopyCommandCommand.Isbn);
            var @event = new PublicationCopyAddedEventDto { PublicationId = newCopy.Isbn, PublicationCopyId = newCopy.PublicationCopyId };

            // store it in the DB.
            await _repository.StoreNew(newCopy);
            // and publish the event as long as the DB change was successful
            await _publisher.Publish($"publicationcopies/{newCopy.Isbn}/{newCopy.PublicationCopyId}", @event);

            //NOTE: this is not transactionally consistent. We might save the record in the DB without publishing the event. We might complete both, but the end-user doesn't see the successful response.
            // The end-user would get an error or timeout and try again.
            // Ideally, both the call to the repository and the call to the event publisher would be idempotent,
            // so that when the user retries, it does only the things that weren't already done.

            // finally return the events that we published.
            return new object[] {@event};
        }
    }

    public interface IPublicationCopyIdGenerator
    {
        Task<int> NextId();
    }
}
