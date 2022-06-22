using System.Data;
using System.Threading.Tasks;
using Circulation.Domain.PublicationCopies;
using Dapper;

namespace Circulation.Infrastructure
{
    public class PublicationCopyIdGenerator : IPublicationCopyIdGenerator
    {
        private readonly IDbConnection _dbConnection;

        public PublicationCopyIdGenerator(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<int> NextId()
        {
            return await _dbConnection.ExecuteScalarAsync<int>("SELECT NEXT VALUE FOR dbo.PublicationCopyId");
        }
    }
}
