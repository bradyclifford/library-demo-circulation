using System.Threading.Tasks;

namespace Circulation.Domain.Publications
{
    public class AddPublicationCommandHandler
    {
        private readonly IPublicationRepository _repository;
        private readonly IEventPublisher _publisher;

        public AddPublicationCommandHandler(IPublicationRepository repository, IEventPublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        public async Task Handle(AddPublication request)
        {
            /*
             * TODO:
             * - make sure the request parameters are complete
             */

            // store it in the DB if it's not already there. The following is required to be idempotent.
            await _repository.EnsureExists(Publication.Create(request.Isbn));

            await _publisher.Publish($"publications/{request.Isbn}", new PublicationAdded {Isbn = request.Isbn});

            //NOTE: this is not transactionally consistent. We might save the record in the DB without publishing the event. We might complete both, but the end-user doesn't see the successful response.
            // The end-user would get an error or timeout and try again.
            // Ideally, both the call to the repository and the call to the event publisher would be idempotent,
            // so that when the user retries, it does only the things that weren't already done.
        }
    }
}
