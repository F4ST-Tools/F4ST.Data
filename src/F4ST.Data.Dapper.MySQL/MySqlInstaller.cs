using Castle.MicroKernel.Registration;
using Castle.Windsor;
using F4ST.Common.Containers;
using F4ST.Common.Mappers;

namespace F4ST.Data.Dapper.MySQL
{
    public class MySqlInstaller : IIoCInstaller
    {
        public void Install(WindsorContainer container, IMapper mapper)
        {
            container.Register(Component.For<IDapperConnection>().ImplementedBy<MySqlConnection>().LifestyleTransient());
        }
    }
}