
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using F4ST.Common.Tools;
using Microsoft.Extensions.DependencyModel;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;

namespace F4ST.Data.RavenDB
{
    public class RavenDbConnection : IRavenDbConnection
    {
        private static IDocumentStore _connection = null;
        public IDocumentStore Connection => _connection;

        private IEnumerable<Type> _tableTypes = null;

        public RavenDbConnection(DbConnectionModel dbConnection)
        {
            if (_connection != null)
                return;

            //var config = appSetting.Get<RavenDbConnectionConfig>("DbConnection");
            var config = dbConnection as RavenDbConnectionConfig;

            _connection = new DocumentStore()
            {
                Urls = config.Servers,
                Database = config.DatabaseName,

                Conventions =
                {
                    FindCollectionName = type=>
                    {
                        _tableTypes ??= Globals.GetClassTypeWithAttribute<TableAttribute>();
                        
                        if (_tableTypes.All(t => t != type))
                            return DocumentConventions.DefaultGetCollectionName(type);

                        var name = (type.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute)?.Name;
                        return string.IsNullOrWhiteSpace(name)
                            ? DocumentConventions.DefaultGetCollectionName(type)
                            : name;
                    }

        }
            };

            _connection.Initialize();
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}