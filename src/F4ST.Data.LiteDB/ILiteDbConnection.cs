using System.Data;
using F4ST.Data;
using LiteDB;

namespace F4ST.Data.LiteDB
{
    public interface ILiteDbConnection : IBaseDbConnection<LiteDatabase>
    {

    }
}