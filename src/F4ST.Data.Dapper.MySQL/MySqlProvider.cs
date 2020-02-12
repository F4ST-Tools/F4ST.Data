using System;

namespace F4ST.Data.Dapper.MySQL
{
    public class MySqlProvider: IDbProvider
    {
        public string Key => "MySQL";
        
        public Type GetConnectionModel => typeof(DapperConnectionConfig);
        
        public Type GetRepository => typeof(DapperRepository);
        

    }
}