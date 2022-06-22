using System;

namespace Circulation.Domain
{
    public class Checkout
    {
        public string PublicationCopyId { get; set; }

        public DateTime CheckOutDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? CheckedInDate { get; set; }

        public CardHolder CardHolder { get; set; }
    }
}
