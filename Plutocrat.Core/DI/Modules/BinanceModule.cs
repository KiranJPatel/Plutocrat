using Autofac;
using Binance;
using Plutocrat.Core.ExchangeHandlers;
using Plutocrat.Core.Interfaces;

namespace Plutocrat.Core.DI.Modules
{
    public class BinanceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(c =>
            {
                var configuration = c.Resolve<ISettingsLoader>();

                var binanceConfig = configuration.ExhangeConfigurations.Find(b => b.ExhangeName.Equals("Binance"));
                var user = new BinanceApiUser(binanceConfig.ApiKey, binanceConfig.SecretKey);
                
                return user;
            }).SingleInstance();

            builder.RegisterType<BinanceApi>().SingleInstance();

            builder.RegisterType<BinanceHandler>().As<IBinanceHandler>().SingleInstance();
        }
    }
}