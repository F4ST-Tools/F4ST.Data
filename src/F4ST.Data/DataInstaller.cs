using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using F4ST.Common.Containers;
using F4ST.Common.Extensions;
using F4ST.Common.Mappers;
using F4ST.Common.Tools;
using Microsoft.Extensions.Configuration;

namespace F4ST.Data
{
    public class DataInstaller : IIoCInstaller
    {
        internal static readonly Dictionary<string, DbConnectionModel> ConnectionModels = new Dictionary<string, DbConnectionModel>();

        public void Install(WindsorContainer container, IMapper mapper)
        {
            LoadDbProviders(container);
        }

        public int Priority => -89;

        private void LoadDbProviders(WindsorContainer container)
        {
            var providers = Globals.GetImplementedInterfaceOf<IDbProvider>().ToArray();

            var conf = IoC.Resolve<IConfiguration>();
            var configs = conf.GetSection("DbConnection").GetChildren();

            foreach (var config in configs ?? new List<IConfigurationSection>())
            {
                var cc = config.Get(typeof(DbConnectionModel)) as DbConnectionModel;
                var provider = providers?.FirstOrDefault(p =>
                    string.Equals(p.Key, cc.Provider, StringComparison.CurrentCultureIgnoreCase));

                if (provider == null)
                    throw new Exception("Provider not found");

                ConnectionModels.Add(cc.Name, config.Get(provider.GetConnectionModel) as DbConnectionModel);

                container.Register(Component
                    .For<IRepository>()
                    .ImplementedBy(provider.GetRepository)
                    .Named($"Rep_{cc.Name}")
                    .LifestyleTransient());
            }
        }

    }
}