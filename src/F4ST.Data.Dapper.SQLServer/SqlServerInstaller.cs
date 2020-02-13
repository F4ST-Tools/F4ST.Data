using Castle.MicroKernel.Registration;
using Castle.Windsor;
using F4ST.Common.Containers;
using F4ST.Common.Mappers;

namespace F4ST.Data.Dapper.SQLServer
{
    public class SqlServerInstaller : IIoCInstaller
    {
        public int Priority => -88;
        public void Install(WindsorContainer container, IMapper mapper)
        {
            container.Register(Component.
                For<IDapperConnection>()
                .ImplementedBy<SqlServerConnection>()
                .Named("Dapper.SQLServer")
                .LifestyleTransient());
        }
    }
}