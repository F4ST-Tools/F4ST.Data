using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using F4ST.Common.Containers;
using F4ST.Common.Mappers;
using F4ST.Common.Tools;

namespace F4ST.Data
{
    public class DataInstaller : IIoCInstaller
    {
        public void Install(WindsorContainer container, IMapper mapper)
        {
            var appSetting = IoC.Resolve<IAppSetting>();
            var config = appSetting.Get<DbConnectionModel>("DbConnection");

            LoadDbProviders(container, config, appSetting);
        }

        private void LoadDbProviders(WindsorContainer container, DbConnectionModel config, IAppSetting appSetting)
        {
            var providers = Globals.GetImplementedInterfaceOf<IDbProvider>();

            var provider = providers.FirstOrDefault(p =>
                string.Equals(p.Key, config.Provider, StringComparison.CurrentCultureIgnoreCase));

            if (provider == null)
                throw new Exception("Provider not found");

            config = appSetting.Get(provider.GetConnectionModel, "DbConnection", true) as DbConnectionModel;
            container.Register(Component.For<IRepository>().ImplementedBy(provider.GetRepository).LifestyleTransient());
            container.Register(Component.For<DbConnectionModel>().Instance(config).LifestyleSingleton());

            provider.Init();
        }

    }
}