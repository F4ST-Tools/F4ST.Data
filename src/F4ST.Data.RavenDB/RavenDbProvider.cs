using System;

namespace F4ST.Data.RavenDB
{
    public class RavenDbProvider: IDbProvider
    {
        public string Key => "RavenDB";
        
        public Type GetConnectionModel => typeof(RavenDbConnectionConfig);
        
        public Type GetRepository => typeof(RavenDbRepository);
        
        public void Init()
        {
            
        }
    }
}