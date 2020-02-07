using Castle.MicroKernel.Registration;
using Castle.Windsor;
using F4ST.Common.Containers;
using F4ST.Common.Mappers;

namespace F4ST.Data.Dapper.PostgreSQL
{
    public class PostgreSqlInstaller : IIoCInstaller
    {
        public void Install(WindsorContainer container, IMapper mapper)
        {
            container.Register(Component.For<IDapperConnection>().ImplementedBy<PostgreSqlConnection>().LifestyleTransient());
        }
    }
}