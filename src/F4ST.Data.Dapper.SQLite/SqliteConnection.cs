using System.Data;
using System.Data.SQLite;

namespace F4ST.Data.Dapper.SQLite
{
    public class SqliteConnection : IDapperConnection
    {
        public IDbConnection Connection { get; }

        public SqliteConnection(DbConnectionModel dbConnection)
        {
            var config = dbConnection as DapperConnectionConfig;
            Connection = new SQLiteConnection(config.ConnectionString);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}