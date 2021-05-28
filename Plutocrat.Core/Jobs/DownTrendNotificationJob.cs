using System;
using System.Threading.Tasks;
using FluentScheduler;
using Plutocrat.Core.Enums;
using Plutocrat.Core.Interfaces;

namespace Plutocrat.Core.Jobs
{
    public class DownTrendNotificationJob : IJob
    {
        private readonly IPlutocratService _PlutocratService;
        private readonly bool _test;

        public DownTrendNotificationJob(IPlutocratService PlutocratService, bool test)
        {
            _PlutocratService = PlutocratService;
            _test = test;
        }

        public void Execute()
        {
            Parallel.ForEach(_PlutocratService.Orders, async (order) =>
            {
                var prediction = await _PlutocratService.GetSMAAnalysisPrediction(order.Base, order.Symbol, Period.Hourly, null);

                if (prediction == PricePrediction.Bearish)
                {
                    Console.WriteLine($"{order.Symbol} has bearish signals.");
                }
                else
                {
                    Console.WriteLine($"{order.Symbol} does not have bearish signals.");
                }
            });
        }
    }
}