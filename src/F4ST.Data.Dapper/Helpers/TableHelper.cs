using System.Collections.Generic;

namespace F4ST.Data.Dapper.Helpers
{
    internal class TableHelper
    {
        internal string Name { get; set; }
        internal Dictionary<string, string> Columns { get; set; }
        internal string Identifier { get; set; }
    }
}