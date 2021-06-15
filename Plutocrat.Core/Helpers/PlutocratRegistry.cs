using Autofac;
using FluentScheduler;
using Plutocrat.Core.DI;
using Plutocrat.Core.Interfaces;
using Plutocrat.Core.Jobs;

namespace Plutocrat.Core.Helpers
{
    public class PlutocratRegistry : Registry
    {
        public PlutocratRegistry(IPlutocratService PlutocratService, ISettingsLoader settings)
        {
            Helpers.Utils.SendTelegramMessage("PlutocratRegistry started");
            //if (settings.BuyInterval == 0)
            //{
            //    Schedule((() => new BuyJob(PlutocratService, settings.Test))).ToRunNow();
            //}
            //else
            //{
            //    Schedule((() => new BuyJob(PlutocratService, settings.Test))).ToRunNow().AndEvery(settings.BuyInterval)
            //        .Milliseconds();
            //}

            //if (settings.PlacedOrderManagementInterval == 0)
            //{
            //    Schedule((() => new PlacedOrderManagementJob(PlutocratService, settings.Test))).ToRunNow();
            //}
            //else
            //{
            //    Schedule((() => new PlacedOrderManagementJob(PlutocratService, settings.Test))).ToRunNow().AndEvery(settings.PlacedOrderManagementInterval)
            //        .Milliseconds();
            //}

            var container = AutofacBootstrapper.Init();
            var BinanceHandler = container.Resolve<IBinanceHandler>();

            if (settings.DownTrendNotificationJobInterval == 0)
            {
                Schedule((() => new DownTrendNotificationJob(PlutocratService, BinanceHandler, settings.Test))).ToRunNow();
            }
            else
            {
                Schedule((() => new DownTrendNotificationJob(PlutocratService, BinanceHandler, settings.Test))).ToRunNow().AndEvery(settings.DownTrendNotificationJobInterval)
                    .Milliseconds();
            }

            //Schedule((() => new DownloaderJob(PlutocratService, settings.Test))).ToRunNow();
        }
    }
}