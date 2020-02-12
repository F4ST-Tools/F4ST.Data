using System;

namespace F4ST.Data.Dapper.SQLite
{
    public class SqliteProvider: IDbProvider
    {
        public string Key => "SQLite";
        
        public Type GetConnectionModel => typeof(DapperConnectionConfig);
        
        public Type GetRepository => typeof(DapperRepository);
        
    }
}