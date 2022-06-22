namespace Circulation.Domain.Publications
{
    public class PublicationAdded
    {
        public string Isbn { get; set; }

        public string Title { get; set; }

        public string Authors { get; set; }

        public string CoverImageUrl { get; set; }
    }
}
