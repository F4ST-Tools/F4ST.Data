using System.Data;

namespace F4ST.Data.Dapper.MySQL
{
    public class MySqlConnection : IDapperConnection
    {
        public IDbConnection Connection { get; }

        public MySqlConnection(DbConnectionModel dbConnection)
        {
            var config = dbConnection as DapperConnectionConfig;
            Connection = new MySql.Data.MySqlClient.MySqlConnection(config.ConnectionString);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}