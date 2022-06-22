namespace Circulation.Domain.Publications
{
    public class Publication
    {
        public string Isbn { get; private set; }

        public static Publication Create(string isbn)
        {
            return new Publication {Isbn = isbn};
        }
    }
}
