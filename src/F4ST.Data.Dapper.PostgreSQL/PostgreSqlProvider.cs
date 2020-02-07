using System;

namespace F4ST.Data.Dapper.PostgreSQL
{
    public class PostgreSqlProvider: IDbProvider
    {
        public string Key => "PostgreSQL";
        
        public Type GetConnectionModel => typeof(DapperConnectionConfig);
        
        public Type GetRepository => typeof(DapperRepository);
        
        public void Init()
        {
            Dapper.DapperConnection.SetDialect(Dialect.PostgreSQL);
        }
    }
}