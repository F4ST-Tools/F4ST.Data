

using F4ST.Data;
using LiteDB;

namespace F4ST.Data.LiteDB
{
    public class LiteDbConnection : ILiteDbConnection
    {
        private static LiteDatabase _connection = null;
        public LiteDatabase Connection => _connection;

        public LiteDbConnection(/*IAppSetting appSetting*/ DbConnectionModel dbConnection)
        {
            if (_connection != null)
                return;

            var config = dbConnection as LiteDbConnectionConfig;

            _connection = new LiteDatabase(config.ConnectionString);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}