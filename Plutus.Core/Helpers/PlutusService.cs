using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLPM.TechnicalAnalysis;
using Microsoft.Extensions.Logging;
using Plutus.Core.Enums;
using Plutus.Core.Interfaces;

namespace Plutus.Core.Helpers
{

    public class PlutusService : IPlutusService
    {
        public IList<OrderConfiguration> Orders { get; }

        private readonly IBinanceHandler _binanceHandler;
        private readonly ITAHandler _taHandler;
        private readonly ILogger _logger;
        private readonly IDatabaseHandler _databaseHandler;

        internal static int mi5MinsMovingAvg = 5;
        internal static int mi15MinsMovingAvg = 15;
        internal static int mi1HourMovingAvg = 60;
        internal static int miEMA10Mins = 10;
        internal static int miEMA30Mins = 30;
        internal static int miEMA45Mins = 45;

        internal static int miEMA12 = 12;
        internal static int miEMA14 = 14;
        internal static int miEMA26 = 26;

        public PlutusService(
            IBinanceHandler binanceHandler, 
            IOrdersLoader ordersLoader,
            ILogger logger,
            ITAHandler taHandler, 
            IDatabaseHandler databaseHandler)
        {
            _binanceHandler = binanceHandler;
            Orders = ordersLoader.Orders;
            _logger = logger;
            _taHandler = taHandler;
            _databaseHandler = databaseHandler;
        }

        public async Task<decimal> GetPrice(string orderBase, string orderSymbol)
        {
            var price = await _binanceHandler.GetPrice(orderBase, orderSymbol);
            _logger.LogInformation($"Price for {orderBase}{orderSymbol} is {price}");

            return price;
        }

        public async Task Sell(string key, string orderSymbol, string orderBase, decimal sellAmount, decimal newPrice)
        {
            await _binanceHandler.Sell(orderBase, orderSymbol, sellAmount);
            
            _logger.LogInformation(
                $"Sold {sellAmount} {orderSymbol} for {newPrice * sellAmount} {orderBase} ({newPrice} a piece) | SELLID: {key}");
        }
        
        public async Task SellTest(string key, string orderSymbol, string orderBase, decimal sellAmount, decimal newPrice)
        {
            await _binanceHandler.SellTest(orderBase, orderSymbol, sellAmount);
            
            _logger.LogInformation(
                $"Test sold {sellAmount} {orderSymbol} for {newPrice * sellAmount} {orderBase} ({newPrice} a piece) | SELLID: {key}");
        }
        
        public async Task Buy(string orderSymbol, string orderBase, decimal buyAmount, decimal price)
        {
            await _binanceHandler.Buy(orderBase, orderSymbol, buyAmount);
            
            var order = new Order
            {
                Amount = buyAmount,
                Base = orderBase,
                Price = price,
                Symbol = orderSymbol,
                Test = false
            };

            var key = await _databaseHandler.AddToDatabase(order);
            _logger.LogInformation(
                $"Bought {buyAmount} {orderSymbol} for {price * buyAmount} {orderBase} ({price} a piece) | BUYID: {key}");
        }

        public async Task BuyTest(string orderSymbol, string orderBase, decimal buyAmount, decimal price)
        {
            await _binanceHandler.BuyTest(orderBase, orderSymbol, buyAmount);

            var order = new Order
            {
                Amount = buyAmount,
                Base = orderBase,
                Price = price,
                Symbol = orderSymbol,
                Test = true
            };

            var key = await _databaseHandler.AddToDatabase(order);
            _logger.LogInformation(
                $"Test bought {buyAmount} {orderSymbol} for {price * buyAmount} {orderBase} ({price} a piece) | BUYID: {key}");
        }

        public async Task<bool> CheckBalance(decimal cost, string balanceBase)
        {
            var balance = await GetBalance(balanceBase);

            if (balance >= cost)
            {
                _logger.LogInformation($"Balance is enough to cover {cost}.");
                return true;
            }
            else
            {
                _logger.LogInformation($"Balance is NOT enough to cover {cost}.");
                return false;
            }
        }

        private async Task<decimal> GetBalance(string balanceBase)
        {
            var balance = await _binanceHandler.GetBalance(balanceBase);

            var balanceValue = balance ?? 0m;

            _logger.LogInformation($"Balance for {balanceBase} is {balanceValue}.");

            return balanceValue;
        }

        public async Task<PricePrediction> GetPricePrediction(string orderBase, string orderSymbol, Period period)
        {
            var closingPrices = await _binanceHandler.GetClosingPrices(orderBase, orderSymbol, period);
            var currentPrice = await _binanceHandler.GetPrice(orderBase, orderSymbol);
            
            var prediction = _taHandler.GetPricePrediction(closingPrices, currentPrice);
            _logger.LogInformation($"Prediction for {orderSymbol} is {prediction.ToString()}.");

            return prediction;
        }

        public async Task<List<Tuple<string, Order>>> GetAllOpenOrders()
        {
            var keys = (await _databaseHandler.GetAllKeys()).ToList();

            var orders = new List<Tuple<string,Order>>(keys.Count());
            foreach (var key in keys)
            {
                var order = await _databaseHandler.GetFromDatabase(key);
                orders.Add(new Tuple<string, Order>(key, order));
            }

            return orders;
        }

        public async Task<bool> IsOnline()
        {
            var online = await _binanceHandler.Ping();

            return online;
        }

        public async Task<IEnumerable<Binance.Candlestick>> GetCandlestick(string orderBase, string orderSymbol, Period period)
        {
            var candlestick = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            _logger.LogInformation($"Price for {orderBase}{orderSymbol} returned - Total items:  {candlestick.Count()}");

            return candlestick;
        }

        public async Task<PricePrediction> GetTEMAAnalysisPrediction(string orderBase, string orderSymbol, Period period)
        {
            List<TripleExponentialMovingAverage> lsTripleExponentialMovingAverage = new List<TripleExponentialMovingAverage>();
            TripleExponentialMovingAverage objTripleExponentialMovingAverage = null;

            IEnumerable<Binance.Candlestick> objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            int iTEMA1 = 0, iTEMA2 = 0, iTEMA3 = 0;
            if (period == Period.Hourly)
            {
                iTEMA1 = miEMA10Mins;
                iTEMA2 = miEMA30Mins;
                iTEMA3 = miEMA45Mins;
            }
            else
            {
                iTEMA1 = miEMA12;
                iTEMA2 = miEMA14;
                iTEMA3 = miEMA26;
            }


            string[] tradedDate = objCandlestickData.Select(r => r.OpenTime.ToString()).ToArray();
            double[] inputClose = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Close)).ToArray();

            Decimal currentPrice = await _binanceHandler.GetPrice(orderBase, orderSymbol);



            double[] options = new double[] { iTEMA1 };
            int output_length = inputClose.Length;

            int iFillFactor = tinet.indicators.tema.start(options);
            double[] TEMA = new double[output_length - iFillFactor];

            double[][] inputs = { inputClose };
            double[][] TEMA1Data = { TEMA };
            int success = tinet.indicators.tema.run(inputs, options, TEMA1Data);
            double[] TEMA1Final = new double[output_length];
            TEMA.CopyTo(TEMA1Final, iFillFactor);

            options = new double[] { iTEMA2 };
            iFillFactor = tinet.indicators.tema.start(options);
            TEMA = new double[output_length - iFillFactor];
            double[][] TEMA2Data = { TEMA };
            success = tinet.indicators.tema.run(inputs, options, TEMA2Data);
            double[] TEMA2Final = new double[output_length];
            TEMA.CopyTo(TEMA2Final, iFillFactor);

            options = new double[] { iTEMA3 };
            iFillFactor = tinet.indicators.tema.start(options);
            TEMA = new double[output_length - iFillFactor];
            double[][] TEMA3Data = { TEMA };
            success = tinet.indicators.tema.run(inputs, options, TEMA3Data);
            double[] TEMA3Final = new double[output_length];
            TEMA.CopyTo(TEMA3Final, iFillFactor);

            for (int i = 0; i < inputClose.Length - 1; i++)
            {
                objTripleExponentialMovingAverage = new TripleExponentialMovingAverage();
                objTripleExponentialMovingAverage.TradedDate = tradedDate[i];
                objTripleExponentialMovingAverage.TEMA1 = TEMA1Final[i];
                objTripleExponentialMovingAverage.TEMA2 = TEMA2Final[i];
                objTripleExponentialMovingAverage.TEMA3 = TEMA3Final[i];
                lsTripleExponentialMovingAverage.Add(objTripleExponentialMovingAverage);
            }

            objTripleExponentialMovingAverage = lsTripleExponentialMovingAverage[lsTripleExponentialMovingAverage.Count - 1];

            if (currentPrice > Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA1) && currentPrice > Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA2) 
                && currentPrice > Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA3))
            {
                return PricePrediction.Bullish;
            }
            else if(currentPrice < Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA1) && currentPrice < Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA2) 
                && currentPrice < Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA3))
            {
                return PricePrediction.Bearish;
            }
            else
            {
                return PricePrediction.Neutral;
            }
        }

        public async Task<PricePrediction> GetBullishCandleStickPrediction(string orderBase, string orderSymbol, Period period)
        {
            IEnumerable<Binance.Candlestick> objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            int iCounter = 0;

            int iArrayCount = objCandlestickData.Count();
            double[] dbOpen = new double[iArrayCount];
            double[] dbHigh = new double[iArrayCount];
            double[] dbLow = new double[iArrayCount];
            double[] dbClose = new double[iArrayCount];
            foreach (Binance.Candlestick objCurrent in objCandlestickData)
            {
                dbOpen[iCounter] = Convert.ToDouble(objCurrent.Open);
                dbHigh[iCounter] = Convert.ToDouble(objCurrent.High);
                dbLow[iCounter] = Convert.ToDouble(objCurrent.Low);
                dbClose[iCounter] = Convert.ToDouble(objCurrent.Close);
                iCounter++;
            }

            int iArrayLength = iArrayCount - 1;
            CdlEngulfing objCdlEngulfing = TAMath.CdlEngulfing(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            CdlHammer objCdlHammer = TAMath.CdlHammer(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            CdlInvertedHammer objCdlInvertedHammer = TAMath.CdlInvertedHammer(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            CdlPiercing objCdlPiercing = TAMath.CdlPiercing(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            CdlMorningStar objCdlMorningStar = TAMath.CdlMorningStar(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            Cdl3WhiteSoldiers objCdl3WhiteSoldiers = TAMath.Cdl3WhiteSoldiers(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);


            // Can be optimized to check bullish pattern in last 3 candles i.e. iArrayLength, iArrayLength -1 and iArrayLength-2
            if (objCdlEngulfing.Integer[iArrayLength] == 100 || objCdlHammer.Integer[iArrayLength] == 100 || objCdlInvertedHammer.Integer[iArrayLength] == 100 ||
                objCdlMorningStar.Integer[iArrayLength] == 100 || objCdl3WhiteSoldiers.Integer[iArrayLength] == 100)
            {
                return PricePrediction.Bullish;
            }
            else
            {
                return PricePrediction.Neutral;
            }
        }
    }
}