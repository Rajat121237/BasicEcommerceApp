using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace eCommerce.Infrastructure.DbContext;

public class DapperDbContext
{
    private readonly IConfiguration _configuration;
    private readonly IDbConnection _connection;
    public IDbConnection DbConnection => _connection;

    public DapperDbContext(IConfiguration configuration)
    {
        _configuration = configuration;

        string connectionStringTemplate = _configuration.GetConnectionString("PostgresConnection")!;
        string connectionString = connectionStringTemplate
            .Replace("$MYSQL_HOST", Environment.GetEnvironmentVariable("$POSTGRES_HOST"))
            .Replace("$MYSQL_PASSWORD", Environment.GetEnvironmentVariable("$POSTGRES_PASSWORD"));

        _connection = new NpgsqlConnection(connectionString);
    }
}
