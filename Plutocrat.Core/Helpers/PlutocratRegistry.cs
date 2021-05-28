using FluentScheduler;
using Plutocrat.Core.Interfaces;
using Plutocrat.Core.Jobs;

namespace Plutocrat.Core.Helpers
{
    public class PlutocratRegistry : Registry
    {
        public PlutocratRegistry(IPlutocratService PlutocratService, ISettingsLoader settings)
        {
            if (settings.BuyInterval == 0)
            {
                Schedule((() => new BuyJob(PlutocratService, settings.Test))).ToRunNow();
            }
            else
            {
                Schedule((() => new BuyJob(PlutocratService, settings.Test))).ToRunNow().AndEvery(settings.BuyInterval)
                    .Milliseconds();
            }

            if (settings.PlacedOrderManagementInterval == 0)
            {
                Schedule((() => new PlacedOrderManagementJob(PlutocratService, settings.Test))).ToRunNow();
            }
            else
            {
                Schedule((() => new PlacedOrderManagementJob(PlutocratService, settings.Test))).ToRunNow().AndEvery(settings.PlacedOrderManagementInterval)
                    .Milliseconds();
            }

            if (settings.DownTrendNotificationJobInterval == 0)
            {
                Schedule((() => new DownTrendNotificationJob(PlutocratService, settings.Test))).ToRunNow();
            }
            else
            {
                Schedule((() => new DownTrendNotificationJob(PlutocratService, settings.Test))).ToRunNow().AndEvery(settings.DownTrendNotificationJobInterval)
                    .Milliseconds();
            }

            //Schedule((() => new DownloaderJob(PlutocratService, settings.Test))).ToRunNow();
        }
    }
}