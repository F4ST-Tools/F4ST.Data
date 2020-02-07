using System.Data;
using Npgsql;

namespace F4ST.Data.Dapper.PostgreSQL
{
    public class PostgreSqlConnection : IDapperConnection
    {
        public IDbConnection Connection { get; }

        public PostgreSqlConnection(DbConnectionModel dbConnection)
        {
            var config = dbConnection as DapperConnectionConfig;
            Connection = new NpgsqlConnection(config.ConnectionString);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}