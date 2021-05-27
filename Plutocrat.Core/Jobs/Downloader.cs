using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentScheduler;
using Plutocrat.Core.Enums;
using Plutocrat.Core.Interfaces;

namespace Plutocrat.Core.Jobs
{
    public class DownloaderJob : IJob
    {
        private readonly IPlutocratService _PlutocratService;
        private readonly bool _test;

        public DownloaderJob(IPlutocratService PlutocratService, bool test)
        {
            _PlutocratService = PlutocratService;
            _test = test;
        }

        public void Execute()
        {
            Parallel.ForEach(_PlutocratService.Orders, async (order) =>
            {
                // Check price of symbol
                IEnumerable<Binance.Candlestick> candlestick = await _PlutocratService.GetCandlestick(order.Base, order.Symbol, Period.Hourly);
            });
        }
    }
}