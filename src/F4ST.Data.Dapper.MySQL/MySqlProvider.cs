using System;

namespace F4ST.Data.Dapper.MySQL
{
    public class MySqlProvider: IDbProvider
    {
        public string Key => "SQLite";
        
        public Type GetConnectionModel => typeof(DapperConnectionConfig);
        
        public Type GetRepository => typeof(DapperRepository);
        
        public void Init()
        {
            Dapper.DapperConnection.SetDialect(Dialect.MySQL);
        }
    }
}