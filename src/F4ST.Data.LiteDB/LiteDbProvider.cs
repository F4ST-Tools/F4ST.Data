using System;
using F4ST.Common.Containers;

namespace F4ST.Data.LiteDB
{
    public class LiteDbProvider: IDbProvider
    {
        public string Key => "LiteDB";
        
        public Type GetConnectionModel => typeof(LiteDbConnectionConfig);
        
        public Type GetRepository => typeof(LiteDbRepository);
        
        public void Init()
        {
            
        }
    }
}