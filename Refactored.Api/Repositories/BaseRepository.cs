using Microsoft.Extensions.Configuration;

namespace Refactored.Api.Repositories
{
    public abstract class BaseRepository
    {
        protected string ConnectionString;

        protected BaseRepository(IConfiguration config)
        {
            ConnectionString = config.GetConnectionString("ProductsDb");
        }
    }
}