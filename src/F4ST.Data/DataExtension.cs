using System;
using System.Collections.Generic;
using Castle.MicroKernel.Lifestyle;
using F4ST.Common.Containers;

namespace F4ST.Data
{
    public static class DataExtension
    {
        public static IRepository GetRepository(this IServiceProvider provider, string name)
        {
            return IoC.Resolve<IRepository>($"Rep_{name}", new
            {
                config = DataInstaller.ConnectionModels[name]
            });
        }
    }
}