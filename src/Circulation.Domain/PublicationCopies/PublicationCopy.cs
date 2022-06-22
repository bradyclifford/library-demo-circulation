using System;
using System.Threading.Tasks;

namespace Circulation.Domain.PublicationCopies
{
    public class PublicationCopy
    {
        public string Isbn { get; private set; }

        /// <summary>
        /// Unique identifier among copies of the same publication.
        /// Not re-used when a copy is replaced.
        /// </summary>
        public int PublicationCopyId { get; private set; }

        public LocationType Location { get; private set; }

        public bool IsAvailableForCheckout { get; private set; }

        public DateTime? ExpectedReturnDate { get; private set; }

        public static async Task<PublicationCopy> Create(IPublicationCopyIdGenerator idGenerator, string isbn)
        {
            var copyId = await idGenerator.NextId();
            return new PublicationCopy {Isbn = isbn, PublicationCopyId = copyId, Location = LocationType.CheckedInWaitingToBeShelved};
        }
    }

    public enum LocationType
    {
        CheckedInWaitingToBeShelved,
        Shelved,
        HeldAtCirculation,
        CheckedOut
    }
}
