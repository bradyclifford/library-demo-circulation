using System.Threading.Tasks;

namespace Circulation.Domain.Publications
{
    public interface IPublicationRepository
    {
        Task EnsureExists(Publication publication);
    }
}
