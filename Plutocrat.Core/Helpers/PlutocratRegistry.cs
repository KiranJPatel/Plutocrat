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

            if (settings.SellInterval == 0)
            {
                Schedule((() => new SellJob(PlutocratService, settings.Test))).ToRunNow();
            }
            else
            {
                Schedule((() => new SellJob(PlutocratService, settings.Test))).ToRunNow().AndEvery(settings.SellInterval)
                    .Milliseconds();
            }

            //Schedule((() => new DownloaderJob(PlutocratService, settings.Test))).ToRunNow();
        }
    }
}