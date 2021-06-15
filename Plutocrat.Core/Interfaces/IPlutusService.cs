using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plutocrat.Core.Enums;
using Plutocrat.Core.Helpers;

namespace Plutocrat.Core.Interfaces
{
    public interface IPlutocratService
    {
        IList<OrderConfiguration> Orders { get; }

        Task<decimal> GetPrice(string orderBase, string orderSymbol);

        Task Buy(string orderSymbol, string orderBase, decimal buyAmount, decimal price);
        
        Task BuyTest(string orderSymbol, string orderBase, decimal buyAmount, decimal price);

        Task Sell(string key, string orderSymbol, string orderBase, decimal sellAmount, decimal newPrice);

        Task SellTest(string key, string orderSymbol, string orderBase, decimal sellAmount, decimal newPrice);

        Task<bool> CheckBalance(decimal cost, string balanceBase);

        Task<bool> IsOnline();
        
        Task<List<Tuple<string, Order>>> GetAllOpenOrders();

        Task<IEnumerable<Binance.Candlestick>> GetCandlestick(string orderBase, string orderSymbol, Period period);

        Task<PriceDetails> GetAroonPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData);

        Task<PricePrediction> GetParabolicSARPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData);

        Task<PriceDetails> GetSMAAnalysisPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData);

        Task<PricePrediction> GetEMAAnalysisPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData);

        Task<PricePrediction> GetDEMAAnalysisPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData);

        Task<PricePrediction> GetTEMAAnalysisPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData);

        Task<PricePrediction> GetCandleStickPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData);

        Task<PriceDetails> GetHeikinAshiCandleStickPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData);

    }
}