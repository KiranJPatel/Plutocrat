using System;
using System.Threading.Tasks;
using FluentScheduler;
using Plutocrat.Core.Enums;
using Plutocrat.Core.Interfaces;

namespace Plutocrat.Core.Jobs
{
    public class BuyJob : IJob
    {
        private readonly IPlutocratService _PlutocratService;
        private readonly bool _test;

        public BuyJob(IPlutocratService PlutocratService, bool test)
        {
            _PlutocratService = PlutocratService;
            _test = test;
        }

        public void Execute()
        {
            Parallel.ForEach(_PlutocratService.Orders, async (order) =>
            {
                // Check price of symbol
                var price = await _PlutocratService.GetPrice(order.Base, order.Symbol);

                // Determine if its going up or down

                var prediction = await _PlutocratService.GetSMAAnalysisPrediction(order.Base, order.Symbol, Period.Hourly, null);

                //var prediction = await _PlutocratService.GetEMAAnalysisPrediction(order.Base, order.Symbol, Period.Hourly, null);

                //var prediction = await _PlutocratService.GetDEMAAnalysisPrediction(order.Base, order.Symbol, Period.Hourly, null);

                //var prediction = await _PlutocratService.GetTEMAAnalysisPrediction(order.Base, order.Symbol, Period.Hourly, null);

                //var prediction = await _PlutocratService.GetBullishCandleStickPrediction(order.Base, order.Symbol, Period.FifteenMin);

                //var prediction = await _PlutocratService.GetBullishHeikinAshiCandleStickPrediction(order.Base, order.Symbol, Period.Hourly);

                //var prediction = await _PlutocratService.GetParabolicSARPrediction(order.Base, order.Symbol, Period.Hourly, null);

                //var prediction = await _PlutocratService.GetAroonPrediction(order.Base, order.Symbol, Period.Hourly, null);

                if (prediction == PricePrediction.Bullish)
                {
                    // If its going up,
                    var buyAmount = order.BuyAmount;

                    // Calculate cost
                    var cost = price * buyAmount;

                    //TODO:Scope for improvement for risk analysis
                    // Check if balance is sufficent
                    var isBalanceSufficent = await _PlutocratService.CheckBalance(cost, order.Base);

                    if (isBalanceSufficent)
                    {
                        Console.WriteLine($"{order.Symbol} has bullish signals to buy at Price:{price}, Quantity:{buyAmount} and Cost:{cost}");
                        //Commented below code to avoid any trade execution
                        //// If balance is sufficient, buy
                        //if (_test)
                        //{
                        //    await _PlutocratService.BuyTest(order.Symbol, order.Base, buyAmount, price);
                        //}
                        //else
                        //{
                        //    await _PlutocratService.Buy(order.Symbol, order.Base, buyAmount, price);
                        //}
                    }
                    else
                    {
                        Console.WriteLine($"{order.Symbol} has bullish signals to buy at Price:{price}, Quantity:{buyAmount} but insuffcient balance to execute trade of cost:{cost}");
                    }
                }
                else
                {
                    Console.WriteLine($"{order.Symbol} does not have any bullish signals.");
                }
            });
        }
    }
}