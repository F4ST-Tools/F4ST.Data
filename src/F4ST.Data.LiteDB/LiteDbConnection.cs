using F4ST.Common.Containers;
using F4ST.Data;
using LiteDB;

namespace F4ST.Data.LiteDB
{
    public class LiteDbConnection : ILiteDbConnection
    {
        public LiteDatabase Connection { get; }

        public LiteDbConnection(DbConnectionModel dbConnection)
        {
            //var dbConnection = IoC.Resolve<DbConnectionModel>("");
            var config = dbConnection as LiteDbConnectionConfig;

            Connection = new LiteDatabase(config.ConnectionString);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}