using System.Data;
using System.Data.SqlClient;

namespace F4ST.Data.Dapper.SQLServer
{
    public class SqlServerConnection : IDapperConnection
    {
        public IDbConnection Connection { get; }

        public SqlServerConnection(DbConnectionModel dbConnection)
        {
            var config = dbConnection as DapperConnectionConfig;
            Connection = new SqlConnection(config.ConnectionString);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}