using System;
using System.Collections.Generic;
using System.Text;

namespace F4ST.Data.Dapper
{
    public class DapperConnectionConfig: DbConnectionModel
    {
        public string ConnectionString { get; set; }
    }
}
