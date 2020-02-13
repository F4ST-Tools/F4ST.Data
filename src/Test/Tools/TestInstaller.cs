using Castle.Windsor;
using F4ST.Common.Containers;
using F4ST.Common.Mappers;
using F4ST.Common.Tools;
using F4ST.Data;

namespace Test.Tools
{
    public class TestInstaller : IIoCInstaller
    {
        public int Priority => 10;
        public void Install(WindsorContainer container, IMapper mapper)
        {
        }

    }
}