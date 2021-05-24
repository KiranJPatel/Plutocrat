using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentScheduler;
using Plutus.Core.Enums;
using Plutus.Core.Interfaces;

namespace Plutus.Core.Jobs
{
    public class DownloaderJob : IJob
    {
        private readonly IPlutusService _plutusService;
        private readonly bool _test;

        public DownloaderJob(IPlutusService plutusService, bool test)
        {
            _plutusService = plutusService;
            _test = test;
        }

        public void Execute()
        {
            Parallel.ForEach(_plutusService.Orders, async (order) =>
            {
                // Check price of symbol
                IEnumerable<Binance.Candlestick> candlestick = await _plutusService.GetCandlestick(order.Base, order.Symbol, Period.Hourly);
            });
        }
    }
}