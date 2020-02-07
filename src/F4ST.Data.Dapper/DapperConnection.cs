using System.Data;
/*using System.Data.SqlClient;
using System.Data.SQLite;
using MySql.Data.MySqlClient;
using Npgsql;*/

namespace F4ST.Data.Dapper
{
    public static class DapperConnection 
    {
        public static void SetDialect(Dialect provider)
        {
            DapperFramework.SetDialect(provider);
        }
    }
}