using System.Data;
using System.Threading.Tasks;
using Circulation.Domain.Publications;
using Dapper;
using Dapper.FastCrud;
using Dapper.FastCrud.Mappings;

namespace Circulation.Infrastructure
{
    public class PublicationRepository : IPublicationRepository
    {
        private static readonly EntityMapping<Publication> EntityMapping;

        private readonly IDbConnection _dbConnection;

        static PublicationRepository()
        {
            EntityMapping = new EntityMapping<Publication>()
                .SetSchemaName("dbo")
                .SetTableName("Publication");
        }

        public PublicationRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task EnsureExists(Publication publication)
        {
            var insertedCount = await _dbConnection.ExecuteAsync(
                @"INSERT INTO dbo.Publication (isbn)
select new.isbn from (SELECT @isbn as isbn) as new
 LEFT JOIN dbo.Publication as existing on existing.isbn = new.isbn
 WHERE existing.isbn is null
", new {isbn = publication.Isbn});
        }
    }
}
