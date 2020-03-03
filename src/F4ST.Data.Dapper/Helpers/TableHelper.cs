using System.Collections.Generic;

namespace Dapper.Contrib.Linq2Dapper.Helpers
{
    internal class TableHelper
    {
        internal string Name { get; set; }
        internal Dictionary<string, string> Columns { get; set; }
        internal string Identifier { get; set; }
    }
}