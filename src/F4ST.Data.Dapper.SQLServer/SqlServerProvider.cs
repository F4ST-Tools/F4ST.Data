using System;

namespace F4ST.Data.Dapper.SQLServer
{
    public class SqlServerProvider: IDbProvider
    {
        public string Key => "SQLServer";
        
        public Type GetConnectionModel => typeof(DapperConnectionConfig);
        
        public Type GetRepository => typeof(DapperRepository);
        
    }
}