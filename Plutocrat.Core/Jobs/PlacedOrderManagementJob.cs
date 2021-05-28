using System.Threading.Tasks;
using FluentScheduler;
using Plutocrat.Core.Interfaces;

namespace Plutocrat.Core.Jobs
{
    public class PlacedOrderManagementJob : IJob
    {
        private readonly IPlutocratService _PlutocratService;
        private readonly bool _test;

        public PlacedOrderManagementJob(IPlutocratService PlutocratService, bool test)
        {
            _PlutocratService = PlutocratService;
            _test = test;
        }

        public async void Execute()
        {
            // Get all open orders
            var orders = await _PlutocratService.GetAllOpenOrders();

            Parallel.ForEach(orders, async (orderTuple) =>
            {
                var order = orderTuple.Item2;
                var key = orderTuple.Item1;
                
                // Get the current price of the order 
                var newPrice = await _PlutocratService.GetPrice(order.Base, order.Symbol);

                // If current price is bigger than price + ProfitStop, OR if current price is lower than price - LossStop, sell
                if (newPrice > order.Price + order.ProfitStop ||
                    newPrice < order.Price - order.LossStop)
                {
                    if (_test)
                    {
                        await _PlutocratService.SellTest(key, order.Symbol, order.Base, order.Amount, newPrice);
                    }
                    else
                    {
                        await _PlutocratService.Sell(key, order.Symbol, order.Base, order.Amount, newPrice);
                    }
                }
            });
        }
    }
}