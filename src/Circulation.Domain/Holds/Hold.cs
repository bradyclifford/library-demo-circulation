using System;

namespace Circulation.Domain.Holds
{
    public class Hold
    {
        // combination of CardHolderNumber and Isbn make the unique key.
        public string CardHolderNumber { get; private set; }

        public string Isbn { get; private set; }

        public int? PublicationCopyId { get; private set; }

        public DateTime HoldPlaced { get; private set; }

        public HoldStatus Status { get; private set; }

        //TODO: during a check-in, we may "allocate" a copy to this hold
        //TODO: when the hold is picked-up, we delete the hold and create a checkout instead.

    }

    public enum HoldStatus
    {
        Waiting,
        ReadyForCheckout
    }

}
