namespace F4ST.Data.RavenDB
{
    public class RavenDbConnectionConfig: DbConnectionModel
    {
        public string[] Servers { get; set; }
        public string DatabaseName { get; set; }
    }
}