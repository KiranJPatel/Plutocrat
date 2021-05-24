using System;
using System.Threading.Tasks;
using FluentScheduler;
using Plutus.Core.Enums;
using Plutus.Core.Interfaces;

namespace Plutus.Core.Jobs
{
    public class BuyJob : IJob
    {
        private readonly IPlutusService _plutusService;
        private readonly bool _test;

        public BuyJob(IPlutusService plutusService, bool test)
        {
            _plutusService = plutusService;
            _test = test;
        }

        public void Execute()
        {
            Parallel.ForEach(_plutusService.Orders, async (order) =>
            {
                // Check price of symbol
                var price = await _plutusService.GetPrice(order.Base, order.Symbol);

                // Determine if its going up or down
                //var prediction = await _plutusService.GetTEMAAnalysisPrediction(order.Base, order.Symbol, Period.Hourly);

                var prediction = await _plutusService.GetBullishCandleStickPrediction(order.Base, order.Symbol, Period.Daily);

                if (prediction == PricePrediction.Bullish || prediction == PricePrediction.Neutral)
                {
                    // If its going up,
                    var buyAmount = order.BuyAmount;

                    // Calculate cost
                    var cost = price * buyAmount;

                    //TODO:Scope for improvement for risk analysis
                    // Check if balance is sufficent
                    var isBalanceSufficent = await _plutusService.CheckBalance(cost, order.Base);

                    if (isBalanceSufficent)
                    {
                        Console.WriteLine($"{order.Symbol} has bullish signals to buy at Price:{price}, Quantity:{buyAmount} and Cost:{cost}");
                        //Commented below code to avoid any trade execution
                        //// If balance is sufficient, buy
                        //if (_test)
                        //{
                        //    await _plutusService.BuyTest(order.Symbol, order.Base, buyAmount, price);
                        //}
                        //else
                        //{
                        //    await _plutusService.Buy(order.Symbol, order.Base, buyAmount, price);
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