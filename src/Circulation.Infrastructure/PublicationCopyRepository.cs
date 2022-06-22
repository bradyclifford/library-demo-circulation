using System.Data;
using System.Threading.Tasks;
using Circulation.Domain.PublicationCopies;
using Dapper;
using Dapper.FastCrud;
using Dapper.FastCrud.Mappings;

namespace Circulation.Infrastructure
{
    public class PublicationCopyRepository : IPublicationCopyRepository
    {
        private static readonly EntityMapping<PublicationCopy> EntityMapping;

        private readonly IDbConnection _dbConnection;

        static PublicationCopyRepository()
        {
            EntityMapping = new EntityMapping<PublicationCopy>()
                .SetSchemaName("dbo")
                .SetTableName("PublicationCopy");
        }

        public PublicationCopyRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<PublicationCopy> Retrieve(string isbn, int copyId)
        {
            var found = await _dbConnection.QuerySingleAsync<PublicationCopy>(
                "SELECT isbn, publicationCopyId, location, expectedReturnDate FROM dbo.PublicationCopy WHERE isbn = @isbn AND publicationCopyId = @publicationCopyId",
                new {isbn, publicationCopyId = copyId});
            return found;
        }

        public async Task<PublicationCopy> FindAvailable(string isbn)
        {
            var found = await _dbConnection.QueryFirstOrDefaultAsync<PublicationCopy>(
                "SELECT isbn, publicationCopyId, location, expectedReturnDate FROM dbo.PublicationCopy WHERE isbn = @isbn AND location in ('CheckedInWaitingToBeShelved','Shelved')",
                new { isbn});
            return found;
        }

        public async Task WriteBack(PublicationCopy modifiedPublicationCopy)
        {
            //TODO: concurrency checking. make sure the "rowversion" hasn't changed since we retrieved it.
        }

        public async Task StoreNew(PublicationCopy publicationCopy)
        {
            await _dbConnection.ExecuteAsync(
                @"INSERT INTO dbo.PublicationCopy (isbn, publicationCopyId, location, expectedReturnDate)
VALUES (@Isbn, @PublicationCopyId, @Location, @ExpectedReturnDate)",
                new {
                    publicationCopy.Isbn,
                    publicationCopy.PublicationCopyId,
                    Location = publicationCopy.Location.ToString(),
                    publicationCopy.ExpectedReturnDate
                });

        }
    }
}
