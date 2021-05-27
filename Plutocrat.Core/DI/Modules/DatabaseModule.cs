using Autofac;
using Plutocrat.Core.Helpers;
using Plutocrat.Core.Interfaces;
using StackExchange.Redis;

namespace Plutocrat.Core.DI.Modules
{
    public class DatabaseModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(c =>
            {
                var configuration = c.Resolve<ISettingsLoader>();
                
                var redis = ConnectionMultiplexer.Connect($"{configuration.RedisUrl},allowAdmin=true");
                return redis;
            }).As<IConnectionMultiplexer>().SingleInstance();

            builder.RegisterType<DatabaseHandler>().As<IDatabaseHandler>();
        }
    }
}