using Autofac;
using Microsoft.Extensions.Logging;
using Plutocrat.Core.Helpers;
using Plutocrat.Core.Interfaces;

namespace Plutocrat.Core.DI.Modules
{
    public class PlutocratModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.Register(c =>
            {
                var factory = c.Resolve<ILoggerFactory>();
                factory.AddConsole();
                return factory.CreateLogger<PlutocratService>();
            }).As<ILogger>();

            builder.RegisterType<PlutocratService>().As<IPlutocratService>();
        }
    }
}