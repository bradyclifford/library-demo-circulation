using System.Threading.Tasks;

namespace Circulation.Domain.PublicationCopies
{
    public interface IPublicationCopyRepository
    {
        Task<PublicationCopy> Retrieve(string isbn, int copyId);
        Task WriteBack(PublicationCopy modifiedPublicationCopy);
        Task StoreNew(PublicationCopy publicationCopy);
        Task<PublicationCopy> FindAvailable(string isbn);
    }
}
