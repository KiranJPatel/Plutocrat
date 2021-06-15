using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLPM.TechnicalAnalysis;
using Microsoft.Extensions.Logging;
using Plutocrat.Core.Enums;
using Plutocrat.Core.Interfaces;

namespace Plutocrat.Core.Helpers
{

    public class PlutocratService : IPlutocratService
    {
        public IList<OrderConfiguration> Orders { get; }

        private readonly IBinanceHandler _binanceHandler;
        private readonly ILogger _logger;
        private readonly IDatabaseHandler _databaseHandler;

        internal static int mi5MinsMovingAvg = 5;
        internal static int mi15MinsMovingAvg = 15;
        internal static int mi1HourMovingAvg = 60;

        internal static int miMA7 = 7;
        internal static int miMA14 = 14;
        internal static int miMA26 = 26;

        internal static int miRSI = 7;
        internal static int miCCI = 7;

        public PlutocratService(
            IBinanceHandler binanceHandler,
            IOrdersLoader ordersLoader,
            ILogger logger,
            IDatabaseHandler databaseHandler)
        {
            _binanceHandler = binanceHandler;
            Orders = ordersLoader.Orders;
            _logger = logger;
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

        public async Task<List<Tuple<string, Order>>> GetAllOpenOrders()
        {
            var keys = (await _databaseHandler.GetAllKeys()).ToList();

            var orders = new List<Tuple<string, Order>>(keys.Count());
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

        public async Task<PriceDetails> GetAroonPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData)
        {
            PriceDetails objPriceDetails = new PriceDetails();
            List<AroonOscillatorDetails> lsAroonOscillatorDetails = new List<AroonOscillatorDetails>();
            AroonOscillatorDetails objAroonOscillatorDetails = null;
            List<string> lsAroonUpTrend = new List<string>();
            List<string> lsAroonDownTrend = new List<string>();
            List<string> lsAroonOscTrend = new List<string>();
            List<string> lsAroonUpDownTrend = new List<string>();
            IEnumerable<String> lsAroonUpTrendLast15 = null;
            IEnumerable<String> lsAroonDownTrendLast15 = null;
            IEnumerable<String> lsAroonOscTrendLast15 = null;
            IEnumerable<String> lsAroonUpDownTrendLast15 = null;

            IEnumerable<AroonOscillatorDetails> lsAroonOscillatorDetailsLast15 = null;

            if (objCandlestickData == null)
            {
                objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            }

            string[] tradedDate = objCandlestickData.Select(r => r.OpenTime.ToString()).ToArray();
            double[] inputHigh = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.High)).ToArray();
            double[] inputLow = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Low)).ToArray();
            double[] inputClose = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Close)).ToArray();

            double[] options = new double[] { 7 };
            int iFillFactor = tinet.indicators.aroon.start(options);
            int output_length = inputHigh.Length;
            double[] AroonUp = new double[output_length - iFillFactor];
            double[] AroonDown = new double[output_length - iFillFactor];
            double[][] inputs = { inputHigh, inputLow, inputClose };
            double[][] AroonUpDownData = { AroonUp, AroonDown };
            int success = tinet.indicators.aroon.run(inputs, options, AroonUpDownData);
            double[] AroonUpFinal = new double[output_length];
            AroonUp.CopyTo(AroonUpFinal, iFillFactor);
            double[] AroonDownFinal = new double[output_length];
            AroonDown.CopyTo(AroonDownFinal, iFillFactor);

            iFillFactor = tinet.indicators.aroonosc.start(options);
            double[] AroonOsc = new double[output_length - iFillFactor];
            double[][] AroonOscData = { AroonOsc };
            success = tinet.indicators.aroonosc.run(inputs, options, AroonOscData);
            double[] AroonOscFinal = new double[output_length];
            AroonOsc.CopyTo(AroonOscFinal, iFillFactor);

            options = new double[] { };
            iFillFactor = tinet.indicators.crossany.start(options);
            double[] CrossOverDetails = new double[output_length - iFillFactor];
            double[][] inputsCrossover = { AroonUpFinal, AroonDownFinal };
            double[][] CrossOverDetailsData = { CrossOverDetails };
            success = tinet.indicators.crossany.run(inputsCrossover, options, CrossOverDetailsData);
            double[] CrossOverDetailsFinal = new double[output_length];
            CrossOverDetails.CopyTo(CrossOverDetailsFinal, iFillFactor);

            string sCrossOverDate = string.Empty;
            string sLastCrossOverType = string.Empty;
            for (int i = 0; i < inputHigh.Length; i++)
            {
                objAroonOscillatorDetails = new AroonOscillatorDetails();
                objAroonOscillatorDetails.TradedDate = tradedDate[i];
                objAroonOscillatorDetails.CurrentPrice = inputClose[i];
                objAroonOscillatorDetails.AroonUp = AroonUpFinal[i];
                objAroonOscillatorDetails.AroonDown = AroonDownFinal[i];
                objAroonOscillatorDetails.AroonOscillator = AroonOscFinal[i];
                if (CrossOverDetailsFinal[i] == 1)
                {
                    sCrossOverDate = objAroonOscillatorDetails.TradedDate;
                    objAroonOscillatorDetails.CrossOverType = objAroonOscillatorDetails.AroonUp > objAroonOscillatorDetails.AroonDown ? "P" : "N";
                    sLastCrossOverType = objAroonOscillatorDetails.CrossOverType;
                }
                else
                {
                    objAroonOscillatorDetails.CrossOverType = "X";
                }
                objAroonOscillatorDetails.CrossOverDate = sCrossOverDate;
                objAroonOscillatorDetails.LastCrossOverType = sLastCrossOverType;

                if (i > 1)
                {
                    lsAroonUpTrend.Add(AroonUpFinal[i] >= AroonUpFinal[i - 1] ? "P" : "N");
                    lsAroonDownTrend.Add(AroonDownFinal[i] < AroonDownFinal[i - 1] ? "P" : "N");
                    lsAroonOscTrend.Add(AroonOscFinal[i] >= AroonOscFinal[i - 1] ? "P" : "N");
                    lsAroonUpDownTrend.Add(AroonUpFinal[i] >= AroonDownFinal[i] ? "P" : "N");

                    lsAroonUpTrendLast15 = lsAroonUpTrend.Count() > 15 ? lsAroonUpTrend.Skip(Math.Max(0, lsAroonUpTrend.Count() - 15)).Take(15) : lsAroonUpTrend;
                    objAroonOscillatorDetails.AroonUpTrendLast15Days = string.Join("", lsAroonUpTrendLast15);

                    lsAroonDownTrendLast15 = lsAroonDownTrend.Count() > 15 ? lsAroonDownTrend.Skip(Math.Max(0, lsAroonDownTrend.Count() - 15)).Take(15) : lsAroonDownTrend;
                    objAroonOscillatorDetails.AroonDownTrendLast15Days = string.Join("", lsAroonDownTrendLast15);

                    lsAroonOscTrendLast15 = lsAroonOscTrend.Count() > 15 ? lsAroonOscTrend.Skip(Math.Max(0, lsAroonOscTrend.Count() - 15)).Take(15) : lsAroonOscTrend;
                    objAroonOscillatorDetails.AroonOscTrendLast15Days = string.Join("", lsAroonOscTrendLast15);

                    lsAroonUpDownTrendLast15 = lsAroonUpDownTrend.Count() > 15 ? lsAroonUpDownTrend.Skip(Math.Max(0, lsAroonUpDownTrend.Count() - 15)).Take(15) : lsAroonUpDownTrend;
                    objAroonOscillatorDetails.AroonUpDownTrendLast15Days = string.Join("", lsAroonUpDownTrendLast15);
                }
                else
                {
                    objAroonOscillatorDetails.AroonUpTrendLast15Days = string.Empty;
                    objAroonOscillatorDetails.AroonDownTrendLast15Days = string.Empty;
                    objAroonOscillatorDetails.AroonOscTrendLast15Days = string.Empty;
                    objAroonOscillatorDetails.AroonUpDownTrendLast15Days = string.Empty;
                }
                
                lsAroonOscillatorDetails.Add(objAroonOscillatorDetails);
            }

            lsAroonOscillatorDetailsLast15 = lsAroonOscillatorDetails.Skip(Math.Max(0, lsAroonOscillatorDetails.Count() - 15)).Take(15);

            objPriceDetails.AroonOscillatorDtls = objAroonOscillatorDetails;

            return objPriceDetails;
        }

        public async Task<PricePrediction> GetParabolicSARPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData)
        {
            List<ParabolicSAR> lsParabolicSAR = new List<ParabolicSAR>();
            ParabolicSAR objParabolicSAR = null;

            if (objCandlestickData == null)
            {
                objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            }

            string[] tradedDate = objCandlestickData.Select(r => r.OpenTime.ToString()).ToArray();
            double[] inputClose = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Close)).ToArray();
            double[] inputHigh = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.High)).ToArray();
            double[] inputLow = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Low)).ToArray();

            double[] options = new double[] { 0.02, 0.2 };
            int iFillFactor = tinet.indicators.psar.start(options);
            int output_length = inputHigh.Length;
            double[] PSAROut = new double[output_length - iFillFactor];

            double[][] inputs = { inputHigh, inputLow };
            double[][] PSARData = { PSAROut };
            int success = tinet.indicators.psar.run(inputs, options, PSARData);
            double[] PSARFinal = new double[output_length];
            PSAROut.CopyTo(PSARFinal, iFillFactor);

            options = new double[] { };
            iFillFactor = tinet.indicators.crossany.start(options);
            double[] CrossOverDetails = new double[output_length - iFillFactor];
            double[][] inputsCrossover = { inputClose, PSARFinal };
            double[][] PositiveCrossOverDetailsData = { CrossOverDetails };
            success = tinet.indicators.crossany.run(inputsCrossover, options, PositiveCrossOverDetailsData);
            double[] CrossOverDetailsFinal = new double[output_length];
            CrossOverDetails.CopyTo(CrossOverDetailsFinal, iFillFactor);

            for (int i = 0; i < inputHigh.Length; i++)
            {
                objParabolicSAR = new ParabolicSAR();
                objParabolicSAR.TradedDate = tradedDate[i];
                objParabolicSAR.CurrentPrice = inputClose[i];
                objParabolicSAR.PSAR = PSARFinal[i];
                if (CrossOverDetailsFinal[i] == 1)
                {
                    objParabolicSAR.CrossOverType = objParabolicSAR.CurrentPrice > objParabolicSAR.PSAR ? "P" : "N";
                }
                else
                {
                    objParabolicSAR.CrossOverType = "X";
                }
                lsParabolicSAR.Add(objParabolicSAR);
            }

            return PricePrediction.Neutral;
        }

        public async Task<PriceDetails> GetSMAAnalysisPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData)
        {
            PriceDetails objPriceDetails = new PriceDetails();
            List<SimpleMovingAverage> lsSimpleMovingAverage = new List<SimpleMovingAverage>();
            List<PivotPoints> lsPivotPoints = new List<PivotPoints>();
            SimpleMovingAverage objSimpleMovingAverage = null;
            List<double> lsVolume15Days = new List<double>();
            List<double> lsKVO15Days = new List<double>();

            if (objCandlestickData == null)
            {
                objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            }

            if (objCandlestickData.Count() == 0)
            {
                objPriceDetails.PricePredictionDetails = PricePrediction.Neutral;
                return objPriceDetails;
            }

            int iTickCount = objCandlestickData.Count();

            int iSMA1 = miMA7, iSMA2 = miMA14, iSMA3 = miMA26;

            if (iTickCount < iSMA1 || iTickCount < iSMA2 || iTickCount < iSMA3)
            {
                Console.WriteLine($"{orderSymbol} returned records:{objCandlestickData.Count()}");
                objPriceDetails.PricePredictionDetails = PricePrediction.Neutral;
                return objPriceDetails;
            }

            string[] tradedDate = objCandlestickData.Select(r => r.OpenTime.ToString()).ToArray();
            double[] inputHigh = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.High)).ToArray();
            double[] inputLow = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Low)).ToArray();
            double[] inputClose = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Close)).ToArray();
            double[] inputVolume = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Volume)).ToArray();

            Decimal currentPrice = await _binanceHandler.GetPrice(orderBase, orderSymbol);
            objPriceDetails.CurrentPrice = currentPrice;


            double[] options = new double[] { iSMA1 };
            int output_length = inputClose.Length;

            int iFillFactor = tinet.indicators.sma.start(options);
            double[] SMA = new double[output_length - iFillFactor];

            double[][] inputs = { inputClose };
            double[][] SMA1Data = { SMA };
            int success = tinet.indicators.sma.run(inputs, options, SMA1Data);
            double[] SMA1Final = new double[output_length];
            SMA.CopyTo(SMA1Final, iFillFactor);

            options = new double[] { iSMA2 };
            iFillFactor = tinet.indicators.sma.start(options);
            SMA = new double[output_length - iFillFactor];
            double[][] SMA2Data = { SMA };
            success = tinet.indicators.sma.run(inputs, options, SMA2Data);
            double[] SMA2Final = new double[output_length];
            SMA.CopyTo(SMA2Final, iFillFactor);

            options = new double[] { iSMA3 };
            iFillFactor = tinet.indicators.sma.start(options);
            SMA = new double[output_length - iFillFactor];
            double[][] SMA3Data = { SMA };
            success = tinet.indicators.sma.run(inputs, options, SMA3Data);
            double[] SMA3Final = new double[output_length];
            SMA.CopyTo(SMA3Final, iFillFactor);

            options = new double[] { };
            iFillFactor = tinet.indicators.crossany.start(options);
            double[] SMA1CrossOverDetails = new double[output_length - iFillFactor];
            double[][] SMA1InputsCrossover = { inputClose, SMA1Final };
            double[][] SMA1CrossOverDetailsData = { SMA1CrossOverDetails };
            success = tinet.indicators.crossany.run(SMA1InputsCrossover, options, SMA1CrossOverDetailsData);
            double[] SMA1CrossOverDetailsFinal = new double[output_length];
            SMA1CrossOverDetails.CopyTo(SMA1CrossOverDetailsFinal, iFillFactor);

            iFillFactor = tinet.indicators.crossany.start(options);
            double[] SMA2CrossOverDetails = new double[output_length - iFillFactor];
            double[][] SMA2InputsCrossover = { inputClose, SMA2Final };
            double[][] SMA2CrossOverDetailsData = { SMA2CrossOverDetails };
            success = tinet.indicators.crossany.run(SMA2InputsCrossover, options, SMA2CrossOverDetailsData);
            double[] SMA2CrossOverDetailsFinal = new double[output_length];
            SMA2CrossOverDetails.CopyTo(SMA2CrossOverDetailsFinal, iFillFactor);

            iFillFactor = tinet.indicators.crossany.start(options);
            double[] SMA3CrossOverDetails = new double[output_length - iFillFactor];
            double[][] SMA3InputsCrossover = { inputClose, SMA3Final };
            double[][] SMA3CrossOverDetailsData = { SMA3CrossOverDetails };
            success = tinet.indicators.crossany.run(SMA3InputsCrossover, options, SMA3CrossOverDetailsData);
            double[] SMA3CrossOverDetailsFinal = new double[output_length];
            SMA3CrossOverDetails.CopyTo(SMA3CrossOverDetailsFinal, iFillFactor);

            //RSI
            options = new double[] { miRSI };
            iFillFactor = tinet.indicators.rsi.start(options);
            output_length = inputClose.Length;
            double[] rsiDetails = new double[output_length - iFillFactor];
            double[][] rsiInputs = { inputClose };
            double[][] RSIData = { rsiDetails };

            success = tinet.indicators.rsi.run(rsiInputs, options, RSIData);
            double[] RSIFinal = new double[output_length];
            rsiDetails.CopyTo(RSIFinal, iFillFactor);

            //CCI
            options = new double[] { miCCI };
            iFillFactor = tinet.indicators.cci.start(options);
            double[] CCIDetails = new double[output_length - iFillFactor];
            double[][] CCIInputs = { inputHigh, inputLow, inputClose };
            double[][] CCIDetailsData = { CCIDetails };
            success = tinet.indicators.cci.run(CCIInputs, options, CCIDetailsData);
            double[] CCIDetailsFinal = new double[output_length];
            CCIDetails.CopyTo(CCIDetailsFinal, iFillFactor);

            // Klinger Volume Oscillator
            options = new double[] { 5, 14 };
            iFillFactor = tinet.indicators.kvo.start(options);
            double[] kvoDetails = new double[output_length - iFillFactor];


            double[][] kvoData = { kvoDetails };
            double[][] kvoInputs = { inputHigh, inputLow, inputClose, inputVolume };
            success = tinet.indicators.kvo.run(kvoInputs, options, kvoData);
            double[] kvoFinal = new double[output_length];
            kvoDetails.CopyTo(kvoFinal, iFillFactor);

            // On Balance Volume
            options = new double[] { 7 };
            iFillFactor = tinet.indicators.obv.start(options);
            double[] obvDetails = new double[output_length - iFillFactor];
            double[][] obvData = { obvDetails };
            double[][] obvInputs = { inputClose, inputVolume };
            success = tinet.indicators.obv.run(obvInputs, options, obvData);
            double[] obvFinal = new double[output_length];
            obvDetails.CopyTo(obvFinal, iFillFactor);

            // Volume Weighted Moving Average
            options = new double[] { 7 };
            iFillFactor = tinet.indicators.vwma.start(options);
            double[] vwmaDetails = new double[output_length - iFillFactor];
            double[][] vwmaData = { vwmaDetails };
            double[][] vwmaInputs = { inputClose, inputVolume };
            success = tinet.indicators.vwma.run(vwmaInputs, options, vwmaData);
            double[] vwmaFinal = new double[output_length];
            vwmaDetails.CopyTo(vwmaFinal, iFillFactor);


            lsVolume15Days = inputVolume.ToList().Skip(Math.Max(0, inputVolume.Length - 15)).Take(15).ToList();
            lsKVO15Days = kvoFinal.ToList().Skip(Math.Max(0, kvoFinal.Length - 15)).Take(15).ToList();

            string sSMA1CrossOverDate = string.Empty;
            string sSMA2CrossOverDate = string.Empty;
            string sSMA3CrossOverDate = string.Empty;
            
            List<string> lsCCITrend = new List<string>();
            List<string> lsRSITrend = new List<string>();
            List<string> lsKVoTrend = new List<string>();
            for (int i = 0; i < inputClose.Length - 1; i++)
            {
                objSimpleMovingAverage = new SimpleMovingAverage();
                objSimpleMovingAverage.TradedDate = tradedDate[i];
                objSimpleMovingAverage.CurrentPrice = inputClose[i];
                objSimpleMovingAverage.SMA1 = SMA1Final[i];
                objSimpleMovingAverage.SMA2 = SMA2Final[i];
                objSimpleMovingAverage.SMA3 = SMA3Final[i];
                if (SMA1CrossOverDetailsFinal[i] == 1)
                {
                    sSMA1CrossOverDate = objSimpleMovingAverage.TradedDate;
                    objSimpleMovingAverage.SMA1CrossOverType = objSimpleMovingAverage.CurrentPrice > objSimpleMovingAverage.SMA1 ? "P" : "N";
                }
                else
                {
                    objSimpleMovingAverage.SMA1CrossOverType = "X";
                }
                objSimpleMovingAverage.SMA1CrossOverDate = sSMA1CrossOverDate;
                if (SMA2CrossOverDetailsFinal[i] == 1)
                {
                    sSMA2CrossOverDate = objSimpleMovingAverage.TradedDate;
                    objSimpleMovingAverage.SMA2CrossOverType = objSimpleMovingAverage.CurrentPrice > objSimpleMovingAverage.SMA2 ? "P" : "N";
                }
                else
                {
                    objSimpleMovingAverage.SMA2CrossOverType = "X";
                }
                objSimpleMovingAverage.SMA2CrossOverDate = sSMA2CrossOverDate;
                if (SMA3CrossOverDetailsFinal[i] == 1)
                {
                    sSMA3CrossOverDate = objSimpleMovingAverage.TradedDate;
                    objSimpleMovingAverage.SMA3CrossOverType = objSimpleMovingAverage.CurrentPrice > objSimpleMovingAverage.SMA3 ? "P" : "N";
                }
                else
                {
                    objSimpleMovingAverage.SMA3CrossOverType = "X";
                }
                objSimpleMovingAverage.SMA3CrossOverDate = sSMA3CrossOverDate;
                lsSimpleMovingAverage.Add(objSimpleMovingAverage);


                if(i>1)
                {
                    lsCCITrend.Add(CCIDetailsFinal[i] >= CCIDetailsFinal[i - 1] ? "P" : "N");
                    lsRSITrend.Add(RSIFinal[i] >= RSIFinal[i - 1] ? "P" : "N");
                    lsKVoTrend.Add(kvoFinal[i] >= kvoFinal[i - 1] ? "P" : "N");
                }
            }           

            
            lsPivotPoints = Utils.CalculatePivotPoints(objCandlestickData);

            objPriceDetails.PivotPointsDetails = lsPivotPoints[lsPivotPoints.Count - 1];

            objSimpleMovingAverage = lsSimpleMovingAverage[lsSimpleMovingAverage.Count - 1];

            objPriceDetails.MovingAverageShort = Convert.ToDecimal(objSimpleMovingAverage.SMA1);
            objPriceDetails.MovingAverageMedium = Convert.ToDecimal(objSimpleMovingAverage.SMA2);
            objPriceDetails.MovingAverageLong = Convert.ToDecimal(objSimpleMovingAverage.SMA3);

            int iArrayLength = lsSimpleMovingAverage.Count;

            objPriceDetails.CCI = CCIDetailsFinal[iArrayLength - 1];
            objPriceDetails.CCIDailyTrend = String.Join("", lsCCITrend.Skip(Math.Max(0, lsCCITrend.Count() - 15)).Take(15));
            objPriceDetails.RSI = RSIFinal[iArrayLength - 1];
            objPriceDetails.RSIDailyTrend = String.Join("", lsRSITrend.Skip(Math.Max(0, lsRSITrend.Count() - 15)).Take(15));
            objPriceDetails.KlingerVolumeOscillator = kvoFinal[iArrayLength - 1];
            objPriceDetails.KVODailyTrend = String.Join("", lsKVoTrend.Skip(Math.Max(0, lsKVoTrend.Count() - 15)).Take(15));

            double dbPreviousPrice = inputClose[iArrayLength - 1];
            double dbPreviousVolume = inputVolume[iArrayLength - 1];
            double dbCurrentVolume = inputVolume[iArrayLength];

            objPriceDetails.PriceSurge = (Convert.ToDouble(currentPrice) * 100 / dbPreviousPrice) - 100;
            objPriceDetails.VolumeSurge = (dbCurrentVolume * 100 / dbPreviousVolume) - 100;

            objPriceDetails.VolumeTrending = dbCurrentVolume > lsVolume15Days.Average() ? "P" : "N";
            objPriceDetails.KVOTrending = objPriceDetails.KlingerVolumeOscillator > lsKVO15Days.Average() ? "P" : "N";
            objPriceDetails.OBVTrending = obvFinal[iArrayLength - 1] > obvFinal[iArrayLength - 2] ? "P" : "N";
            objPriceDetails.VWAPTrending = vwmaFinal[iArrayLength - 1] > vwmaFinal[iArrayLength - 2] ? "P" : "N";

            if ((currentPrice > Convert.ToDecimal(objSimpleMovingAverage.SMA1) && currentPrice > Convert.ToDecimal(objSimpleMovingAverage.SMA2)) ||
                    (currentPrice > Convert.ToDecimal(objSimpleMovingAverage.SMA2) && currentPrice > Convert.ToDecimal(objSimpleMovingAverage.SMA3)) ||
                    (currentPrice > Convert.ToDecimal(objSimpleMovingAverage.SMA1) && currentPrice > Convert.ToDecimal(objSimpleMovingAverage.SMA2)) &&
                    RSIFinal[iArrayLength - 1] >= 45 && CCIDetailsFinal[iArrayLength - 1] > 0)
            {
                objPriceDetails.PricePredictionDetails = PricePrediction.Bullish;
                return objPriceDetails;
            }
            else if ((currentPrice < Convert.ToDecimal(objSimpleMovingAverage.SMA1) && currentPrice < Convert.ToDecimal(objSimpleMovingAverage.SMA2)) ||
                (currentPrice < Convert.ToDecimal(objSimpleMovingAverage.SMA2) && currentPrice < Convert.ToDecimal(objSimpleMovingAverage.SMA3)) ||
                (currentPrice < Convert.ToDecimal(objSimpleMovingAverage.SMA1) && currentPrice < Convert.ToDecimal(objSimpleMovingAverage.SMA2)) &&
                RSIFinal[iArrayLength - 1] < 45 && CCIDetailsFinal[iArrayLength - 1] <= 0)
            {
                objPriceDetails.PricePredictionDetails = PricePrediction.Bearish;
                return objPriceDetails;
            }
            else
            {
                objPriceDetails.PricePredictionDetails = PricePrediction.Neutral;
                return objPriceDetails;
            }
        }

        public async Task<PricePrediction> GetEMAAnalysisPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData)
        {
            List<ExponentialMovingAverage> lsExponentialMovingAverage = new List<ExponentialMovingAverage>();
            ExponentialMovingAverage objExponentialMovingAverage = null;

            if (objCandlestickData == null)
            {
                objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            }

            int iEMA1 = miMA7, iEMA2 = miMA14, iEMA3 = miMA26;

            string[] tradedDate = objCandlestickData.Select(r => r.OpenTime.ToString()).ToArray();
            double[] inputClose = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Close)).ToArray();

            Decimal currentPrice = await _binanceHandler.GetPrice(orderBase, orderSymbol);



            double[] options = new double[] { iEMA1 };
            int output_length = inputClose.Length;

            int iFillFactor = tinet.indicators.ema.start(options);
            double[] EMA = new double[output_length - iFillFactor];

            double[][] inputs = { inputClose };
            double[][] EMA1Data = { EMA };
            int success = tinet.indicators.ema.run(inputs, options, EMA1Data);
            double[] EMA1Final = new double[output_length];
            EMA.CopyTo(EMA1Final, iFillFactor);

            options = new double[] { iEMA2 };
            iFillFactor = tinet.indicators.ema.start(options);
            EMA = new double[output_length - iFillFactor];
            double[][] EMA2Data = { EMA };
            success = tinet.indicators.ema.run(inputs, options, EMA2Data);
            double[] EMA2Final = new double[output_length];
            EMA.CopyTo(EMA2Final, iFillFactor);

            options = new double[] { iEMA3 };
            iFillFactor = tinet.indicators.ema.start(options);
            EMA = new double[output_length - iFillFactor];
            double[][] EMA3Data = { EMA };
            success = tinet.indicators.ema.run(inputs, options, EMA3Data);
            double[] EMA3Final = new double[output_length];
            EMA.CopyTo(EMA3Final, iFillFactor);

            options = new double[] { };
            iFillFactor = tinet.indicators.crossany.start(options);
            double[] EMA1CrossOverDetails = new double[output_length - iFillFactor];
            double[][] EMA1InputsCrossover = { inputClose, EMA1Final };
            double[][] EMA1CrossOverDetailsData = { EMA1CrossOverDetails };
            success = tinet.indicators.crossany.run(EMA1InputsCrossover, options, EMA1CrossOverDetailsData);
            double[] EMA1CrossOverDetailsFinal = new double[output_length];
            EMA1CrossOverDetails.CopyTo(EMA1CrossOverDetailsFinal, iFillFactor);

            iFillFactor = tinet.indicators.crossany.start(options);
            double[] EMA2CrossOverDetails = new double[output_length - iFillFactor];
            double[][] EMA2InputsCrossover = { inputClose, EMA2Final };
            double[][] EMA2CrossOverDetailsData = { EMA2CrossOverDetails };
            success = tinet.indicators.crossany.run(EMA2InputsCrossover, options, EMA2CrossOverDetailsData);
            double[] EMA2CrossOverDetailsFinal = new double[output_length];
            EMA2CrossOverDetails.CopyTo(EMA2CrossOverDetailsFinal, iFillFactor);

            iFillFactor = tinet.indicators.crossany.start(options);
            double[] EMA3CrossOverDetails = new double[output_length - iFillFactor];
            double[][] EMA3InputsCrossover = { inputClose, EMA3Final };
            double[][] EMA3CrossOverDetailsData = { EMA3CrossOverDetails };
            success = tinet.indicators.crossany.run(EMA3InputsCrossover, options, EMA3CrossOverDetailsData);
            double[] EMA3CrossOverDetailsFinal = new double[output_length];
            EMA3CrossOverDetails.CopyTo(EMA3CrossOverDetailsFinal, iFillFactor);

            string sEMA1CrossOverDate = string.Empty;
            string sEMA2CrossOverDate = string.Empty;
            string sEMA3CrossOverDate = string.Empty;
            for (int i = 0; i < inputClose.Length - 1; i++)
            {
                objExponentialMovingAverage = new ExponentialMovingAverage();
                objExponentialMovingAverage.TradedDate = tradedDate[i];
                objExponentialMovingAverage.CurrentPrice = inputClose[i];
                objExponentialMovingAverage.EMA1 = EMA1Final[i];
                objExponentialMovingAverage.EMA2 = EMA2Final[i];
                objExponentialMovingAverage.EMA3 = EMA3Final[i];
                if (EMA1CrossOverDetailsFinal[i] == 1)
                {
                    sEMA1CrossOverDate = objExponentialMovingAverage.TradedDate;
                    objExponentialMovingAverage.EMA1CrossOverType = objExponentialMovingAverage.CurrentPrice > objExponentialMovingAverage.EMA1 ? "P" : "N";
                }
                else
                {
                    objExponentialMovingAverage.EMA1CrossOverType = "X";
                }
                objExponentialMovingAverage.EMA1CrossOverDate = sEMA1CrossOverDate;
                if (EMA2CrossOverDetailsFinal[i] == 1)
                {
                    sEMA2CrossOverDate = objExponentialMovingAverage.TradedDate;
                    objExponentialMovingAverage.EMA2CrossOverType = objExponentialMovingAverage.CurrentPrice > objExponentialMovingAverage.EMA2 ? "P" : "N";
                }
                else
                {
                    objExponentialMovingAverage.EMA2CrossOverType = "X";
                }
                objExponentialMovingAverage.EMA2CrossOverDate = sEMA2CrossOverDate;
                if (EMA3CrossOverDetailsFinal[i] == 1)
                {
                    sEMA3CrossOverDate = objExponentialMovingAverage.TradedDate;
                    objExponentialMovingAverage.EMA3CrossOverType = objExponentialMovingAverage.CurrentPrice > objExponentialMovingAverage.EMA3 ? "P" : "N";
                }
                else
                {
                    objExponentialMovingAverage.EMA3CrossOverType = "X";
                }
                objExponentialMovingAverage.EMA3CrossOverDate = sEMA3CrossOverDate;
                lsExponentialMovingAverage.Add(objExponentialMovingAverage);
            }

            objExponentialMovingAverage = lsExponentialMovingAverage[lsExponentialMovingAverage.Count - 1];

            if (currentPrice > Convert.ToDecimal(objExponentialMovingAverage.EMA1) && currentPrice > Convert.ToDecimal(objExponentialMovingAverage.EMA2)
                && currentPrice > Convert.ToDecimal(objExponentialMovingAverage.EMA3))
            {
                return PricePrediction.Bullish;
            }
            else if (currentPrice < Convert.ToDecimal(objExponentialMovingAverage.EMA1) && currentPrice < Convert.ToDecimal(objExponentialMovingAverage.EMA2)
                && currentPrice < Convert.ToDecimal(objExponentialMovingAverage.EMA3))
            {
                return PricePrediction.Bearish;
            }
            else
            {
                return PricePrediction.Neutral;
            }
        }

        public async Task<PricePrediction> GetDEMAAnalysisPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData)
        {
            List<DoubleExponentialMovingAverage> lsDoubleExponentialMovingAverage = new List<DoubleExponentialMovingAverage>();
            DoubleExponentialMovingAverage objDoubleExponentialMovingAverage = null;

            if (objCandlestickData == null)
            {
                objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            }

            int iEMA1 = miMA7, iEMA2 = miMA14, iEMA3 = miMA26;


            string[] tradedDate = objCandlestickData.Select(r => r.OpenTime.ToString()).ToArray();
            double[] inputClose = objCandlestickData.AsEnumerable().Select(r => Convert.ToDouble(r.Close)).ToArray();

            Decimal currentPrice = await _binanceHandler.GetPrice(orderBase, orderSymbol);



            double[] options = new double[] { iEMA1 };
            int output_length = inputClose.Length;

            int iFillFactor = tinet.indicators.sma.start(options);
            double[] EMA = new double[output_length - iFillFactor];

            double[][] inputs = { inputClose };
            double[][] EMA1Data = { EMA };
            int success = tinet.indicators.sma.run(inputs, options, EMA1Data);
            double[] EMA1Final = new double[output_length];
            EMA.CopyTo(EMA1Final, iFillFactor);

            options = new double[] { iEMA2 };
            iFillFactor = tinet.indicators.sma.start(options);
            EMA = new double[output_length - iFillFactor];
            double[][] EMA2Data = { EMA };
            success = tinet.indicators.sma.run(inputs, options, EMA2Data);
            double[] EMA2Final = new double[output_length];
            EMA.CopyTo(EMA2Final, iFillFactor);

            options = new double[] { iEMA3 };
            iFillFactor = tinet.indicators.sma.start(options);
            EMA = new double[output_length - iFillFactor];
            double[][] EMA3Data = { EMA };
            success = tinet.indicators.sma.run(inputs, options, EMA3Data);
            double[] EMA3Final = new double[output_length];
            EMA.CopyTo(EMA3Final, iFillFactor);

            options = new double[] { };
            iFillFactor = tinet.indicators.crossany.start(options);
            double[] EMA1CrossOverDetails = new double[output_length - iFillFactor];
            double[][] EMA1InputsCrossover = { inputClose, EMA1Final };
            double[][] EMA1CrossOverDetailsData = { EMA1CrossOverDetails };
            success = tinet.indicators.crossany.run(EMA1InputsCrossover, options, EMA1CrossOverDetailsData);
            double[] EMA1CrossOverDetailsFinal = new double[output_length];
            EMA1CrossOverDetails.CopyTo(EMA1CrossOverDetailsFinal, iFillFactor);

            iFillFactor = tinet.indicators.crossany.start(options);
            double[] EMA2CrossOverDetails = new double[output_length - iFillFactor];
            double[][] EMA2InputsCrossover = { inputClose, EMA2Final };
            double[][] EMA2CrossOverDetailsData = { EMA2CrossOverDetails };
            success = tinet.indicators.crossany.run(EMA2InputsCrossover, options, EMA2CrossOverDetailsData);
            double[] EMA2CrossOverDetailsFinal = new double[output_length];
            EMA2CrossOverDetails.CopyTo(EMA2CrossOverDetailsFinal, iFillFactor);

            iFillFactor = tinet.indicators.crossany.start(options);
            double[] EMA3CrossOverDetails = new double[output_length - iFillFactor];
            double[][] EMA3InputsCrossover = { inputClose, EMA3Final };
            double[][] EMA3CrossOverDetailsData = { EMA3CrossOverDetails };
            success = tinet.indicators.crossany.run(EMA3InputsCrossover, options, EMA3CrossOverDetailsData);
            double[] EMA3CrossOverDetailsFinal = new double[output_length];
            EMA3CrossOverDetails.CopyTo(EMA3CrossOverDetailsFinal, iFillFactor);

            string sEMA1CrossOverDate = string.Empty;
            string sEMA2CrossOverDate = string.Empty;
            string sEMA3CrossOverDate = string.Empty;
            for (int i = 0; i < inputClose.Length - 1; i++)
            {
                objDoubleExponentialMovingAverage = new DoubleExponentialMovingAverage();
                objDoubleExponentialMovingAverage.TradedDate = tradedDate[i];
                objDoubleExponentialMovingAverage.CurrentPrice = inputClose[i];
                objDoubleExponentialMovingAverage.DEMA1 = EMA1Final[i];
                objDoubleExponentialMovingAverage.DEMA2 = EMA2Final[i];
                objDoubleExponentialMovingAverage.DEMA3 = EMA3Final[i];
                if (EMA1CrossOverDetailsFinal[i] == 1)
                {
                    sEMA1CrossOverDate = objDoubleExponentialMovingAverage.TradedDate;
                    objDoubleExponentialMovingAverage.DEMA1CrossOverType = objDoubleExponentialMovingAverage.CurrentPrice > objDoubleExponentialMovingAverage.DEMA1 ? "P" : "N";
                }
                else
                {
                    objDoubleExponentialMovingAverage.DEMA1CrossOverType = "X";
                }
                objDoubleExponentialMovingAverage.DEMA1CrossOverDate = sEMA1CrossOverDate;
                if (EMA2CrossOverDetailsFinal[i] == 1)
                {
                    sEMA2CrossOverDate = objDoubleExponentialMovingAverage.TradedDate;
                    objDoubleExponentialMovingAverage.DEMA2CrossOverType = objDoubleExponentialMovingAverage.CurrentPrice > objDoubleExponentialMovingAverage.DEMA2 ? "P" : "N";
                }
                else
                {
                    objDoubleExponentialMovingAverage.DEMA2CrossOverType = "X";
                }
                objDoubleExponentialMovingAverage.DEMA2CrossOverDate = sEMA2CrossOverDate;
                if (EMA3CrossOverDetailsFinal[i] == 1)
                {
                    sEMA3CrossOverDate = objDoubleExponentialMovingAverage.TradedDate;
                    objDoubleExponentialMovingAverage.DEMA3CrossOverType = objDoubleExponentialMovingAverage.CurrentPrice > objDoubleExponentialMovingAverage.DEMA3 ? "P" : "N";
                }
                else
                {
                    objDoubleExponentialMovingAverage.DEMA3CrossOverType = "X";
                }
                objDoubleExponentialMovingAverage.DEMA3CrossOverDate = sEMA3CrossOverDate;
                lsDoubleExponentialMovingAverage.Add(objDoubleExponentialMovingAverage);
            }

            objDoubleExponentialMovingAverage = lsDoubleExponentialMovingAverage[lsDoubleExponentialMovingAverage.Count - 1];

            if (currentPrice > Convert.ToDecimal(objDoubleExponentialMovingAverage.DEMA1) && currentPrice > Convert.ToDecimal(objDoubleExponentialMovingAverage.DEMA2)
                && currentPrice > Convert.ToDecimal(objDoubleExponentialMovingAverage.DEMA3))
            {
                return PricePrediction.Bullish;
            }
            else if (currentPrice < Convert.ToDecimal(objDoubleExponentialMovingAverage.DEMA1) && currentPrice < Convert.ToDecimal(objDoubleExponentialMovingAverage.DEMA2)
                && currentPrice < Convert.ToDecimal(objDoubleExponentialMovingAverage.DEMA3))
            {
                return PricePrediction.Bearish;
            }
            else
            {
                return PricePrediction.Neutral;
            }
        }

        public async Task<PricePrediction> GetTEMAAnalysisPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData)
        {
            List<TripleExponentialMovingAverage> lsTripleExponentialMovingAverage = new List<TripleExponentialMovingAverage>();
            TripleExponentialMovingAverage objTripleExponentialMovingAverage = null;

            if (objCandlestickData == null)
            {
                objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            }

            int iTEMA1 = miMA7, iTEMA2 = miMA14, iTEMA3 = miMA26;

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

            options = new double[] { };
            iFillFactor = tinet.indicators.crossany.start(options);
            double[] TEMA1CrossOverDetails = new double[output_length - iFillFactor];
            double[][] TEMA1InputsCrossover = { inputClose, TEMA1Final };
            double[][] TEMA1CrossOverDetailsData = { TEMA1CrossOverDetails };
            success = tinet.indicators.crossany.run(TEMA1InputsCrossover, options, TEMA1CrossOverDetailsData);
            double[] TEMA1CrossOverDetailsFinal = new double[output_length];
            TEMA1CrossOverDetails.CopyTo(TEMA1CrossOverDetailsFinal, iFillFactor);

            iFillFactor = tinet.indicators.crossany.start(options);
            double[] TEMA2CrossOverDetails = new double[output_length - iFillFactor];
            double[][] TEMA2InputsCrossover = { inputClose, TEMA2Final };
            double[][] TEMA2CrossOverDetailsData = { TEMA2CrossOverDetails };
            success = tinet.indicators.crossany.run(TEMA2InputsCrossover, options, TEMA2CrossOverDetailsData);
            double[] TEMA2CrossOverDetailsFinal = new double[output_length];
            TEMA2CrossOverDetails.CopyTo(TEMA2CrossOverDetailsFinal, iFillFactor);

            iFillFactor = tinet.indicators.crossany.start(options);
            double[] TEMA3CrossOverDetails = new double[output_length - iFillFactor];
            double[][] TEMA3InputsCrossover = { inputClose, TEMA3Final };
            double[][] TEMA3CrossOverDetailsData = { TEMA3CrossOverDetails };
            success = tinet.indicators.crossany.run(TEMA3InputsCrossover, options, TEMA3CrossOverDetailsData);
            double[] TEMA3CrossOverDetailsFinal = new double[output_length];
            TEMA3CrossOverDetails.CopyTo(TEMA3CrossOverDetailsFinal, iFillFactor);

            string sTEMA1CrossOverDate = string.Empty;
            string sTEMA2CrossOverDate = string.Empty;
            string sTEMA3CrossOverDate = string.Empty;
            for (int i = 0; i < inputClose.Length - 1; i++)
            {
                objTripleExponentialMovingAverage = new TripleExponentialMovingAverage();
                objTripleExponentialMovingAverage.TradedDate = tradedDate[i];
                objTripleExponentialMovingAverage.CurrentPrice = inputClose[i];
                objTripleExponentialMovingAverage.TEMA1 = TEMA1Final[i];
                objTripleExponentialMovingAverage.TEMA2 = TEMA2Final[i];
                objTripleExponentialMovingAverage.TEMA3 = TEMA3Final[i];
                if (TEMA1CrossOverDetailsFinal[i] == 1)
                {
                    sTEMA1CrossOverDate = objTripleExponentialMovingAverage.TradedDate;
                    objTripleExponentialMovingAverage.TEMA1CrossOverType = objTripleExponentialMovingAverage.CurrentPrice > objTripleExponentialMovingAverage.TEMA1 ? "P" : "N";
                }
                else
                {
                    objTripleExponentialMovingAverage.TEMA1CrossOverType = "X";
                }
                objTripleExponentialMovingAverage.TEMA1CrossOverDate = sTEMA1CrossOverDate;
                if (TEMA2CrossOverDetailsFinal[i] == 1)
                {
                    sTEMA2CrossOverDate = objTripleExponentialMovingAverage.TradedDate;
                    objTripleExponentialMovingAverage.TEMA2CrossOverType = objTripleExponentialMovingAverage.CurrentPrice > objTripleExponentialMovingAverage.TEMA2 ? "P" : "N";
                }
                else
                {
                    objTripleExponentialMovingAverage.TEMA2CrossOverType = "X";
                }
                objTripleExponentialMovingAverage.TEMA2CrossOverDate = sTEMA2CrossOverDate;
                if (TEMA3CrossOverDetailsFinal[i] == 1)
                {
                    sTEMA3CrossOverDate = objTripleExponentialMovingAverage.TradedDate;
                    objTripleExponentialMovingAverage.TEMA3CrossOverType = objTripleExponentialMovingAverage.CurrentPrice > objTripleExponentialMovingAverage.TEMA3 ? "P" : "N";
                }
                else
                {
                    objTripleExponentialMovingAverage.TEMA3CrossOverType = "X";
                }
                objTripleExponentialMovingAverage.TEMA3CrossOverDate = sTEMA3CrossOverDate;
                lsTripleExponentialMovingAverage.Add(objTripleExponentialMovingAverage);
            }

            objTripleExponentialMovingAverage = lsTripleExponentialMovingAverage[lsTripleExponentialMovingAverage.Count - 1];

            if (currentPrice > Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA1) && currentPrice > Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA2)
                && currentPrice > Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA3))
            {
                return PricePrediction.Bullish;
            }
            else if (currentPrice < Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA1) && currentPrice < Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA2)
                && currentPrice < Convert.ToDecimal(objTripleExponentialMovingAverage.TEMA3))
            {
                return PricePrediction.Bearish;
            }
            else
            {
                return PricePrediction.Neutral;
            }
        }

        public async Task<PricePrediction> GetCandleStickPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData)
        {
            if (objCandlestickData == null)
            {
                objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
            }

            int iCounter = 0;

            int iArrayCount = objCandlestickData.Count();
            double[] dbOpen = new double[iArrayCount];
            double[] dbHigh = new double[iArrayCount];
            double[] dbLow = new double[iArrayCount];
            double[] dbClose = new double[iArrayCount];
            double[] dbVolume = new double[iArrayCount];
            foreach (Binance.Candlestick objCurrent in objCandlestickData)
            {
                dbOpen[iCounter] = Convert.ToDouble(objCurrent.Open);
                dbHigh[iCounter] = Convert.ToDouble(objCurrent.High);
                dbLow[iCounter] = Convert.ToDouble(objCurrent.Low);
                dbClose[iCounter] = Convert.ToDouble(objCurrent.Close);
                dbVolume[iCounter] = Convert.ToDouble(objCurrent.Volume);
                iCounter++;
            }



            int iArrayLength = iArrayCount - 1;
            CdlEngulfing objCdlEngulfing = TAMath.CdlEngulfing(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            CdlHammer objCdlHammer = TAMath.CdlHammer(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            CdlInvertedHammer objCdlInvertedHammer = TAMath.CdlInvertedHammer(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            CdlPiercing objCdlPiercing = TAMath.CdlPiercing(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            CdlMorningStar objCdlMorningStar = TAMath.CdlMorningStar(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);
            Cdl3WhiteSoldiers objCdl3WhiteSoldiers = TAMath.Cdl3WhiteSoldiers(0, iArrayLength, dbOpen, dbHigh, dbLow, dbClose);

            //RSI
            double[] options = new double[] { miRSI };
            int iFillFactor = tinet.indicators.rsi.start(options);
            int output_length = dbHigh.Length;
            double[] rsiDetails = new double[output_length - iFillFactor];
            double[][] rsiInputs = { dbClose };
            double[][] RSIData = { rsiDetails };

            int success = tinet.indicators.rsi.run(rsiInputs, options, RSIData);
            double[] RSIFinal = new double[output_length];
            rsiDetails.CopyTo(RSIFinal, iFillFactor);

            //CCI
            options = new double[] { miCCI };
            iFillFactor = tinet.indicators.cci.start(options);
            double[] CCIDetails = new double[output_length - iFillFactor];
            double[][] inputs = { dbHigh, dbLow, dbClose };
            double[][] CCIDetailsData = { CCIDetails };
            success = tinet.indicators.cci.run(inputs, options, CCIDetailsData);
            double[] CCIDetailsFinal = new double[output_length];
            CCIDetails.CopyTo(CCIDetailsFinal, iFillFactor);

            // Klinger Volume Oscillator
            options = new double[] { 5, 14 };
            iFillFactor = tinet.indicators.kvo.start(options);
            double[] kvoDetails = new double[output_length - iFillFactor];


            double[][] kvoData = { kvoDetails };
            double[][] kvoInputs = { dbHigh, dbLow, dbClose, dbVolume };
            success = tinet.indicators.kvo.run(kvoInputs, options, kvoData);
            double[] kvoFinal = new double[output_length];
            kvoDetails.CopyTo(kvoFinal, iFillFactor);

            // Can be optimized to check bullish pattern in last 3 candles i.e. iArrayLength, iArrayLength -1 and iArrayLength-2
            if ((objCdlEngulfing.Integer[iArrayLength] == 100 || objCdlHammer.Integer[iArrayLength] == 100 || objCdlInvertedHammer.Integer[iArrayLength] == 100 ||
                objCdlMorningStar.Integer[iArrayLength] == 100 || objCdl3WhiteSoldiers.Integer[iArrayLength] == 100) &&
                RSIFinal[iArrayLength - 1] >= 55 && CCIDetailsFinal[iArrayLength - 1] >= 50 && kvoFinal[iArrayLength - 1] > kvoFinal[iArrayLength - 2] &&
                kvoFinal[iArrayLength - 2] > kvoFinal[iArrayLength - 3])
            {
                return PricePrediction.Bullish;
            }
            else
            {
                return PricePrediction.Neutral;
            }
        }

        public async Task<PriceDetails> GetHeikinAshiCandleStickPrediction(string orderBase, string orderSymbol, Period period, IEnumerable<Binance.Candlestick> objCandlestickData)
        {
            PriceDetails objPriceDetails = new PriceDetails();
            try
            {
                if (objCandlestickData == null)
                {
                    objCandlestickData = await _binanceHandler.GetCandlestick(orderBase, orderSymbol, period);
                }

                List<HeikinAshi> lsHeikinAshi = Utils.GenerateHeikinAshi(objCandlestickData);

                int iCounter = 0;

                int iArrayCount = objCandlestickData.Count();

                double[] dbHeikinAshiOpen = new double[iArrayCount];
                double[] dbHeikinAshiHigh = new double[iArrayCount];
                double[] dbHeikinAshiLow = new double[iArrayCount];
                double[] dbHeikinAshiClose = new double[iArrayCount];
                double[] dbVolume = new double[iArrayCount];

                foreach (HeikinAshi objCurrent in lsHeikinAshi)
                {
                    dbHeikinAshiOpen[iCounter] = Convert.ToDouble(objCurrent.Open);
                    dbHeikinAshiHigh[iCounter] = Convert.ToDouble(objCurrent.High);
                    dbHeikinAshiLow[iCounter] = Convert.ToDouble(objCurrent.Low);
                    dbHeikinAshiClose[iCounter] = Convert.ToDouble(objCurrent.Close);
                    iCounter++;
                }

                iCounter = 0;
                bool[] bIsBullish = new bool[iArrayCount];
                bool[] bIsBearish = new bool[iArrayCount];
                bool[] bIsShortBullish = new bool[iArrayCount];
                bool[] bIsShortBearish = new bool[iArrayCount];
                bool[] bIsLongBullish = new bool[iArrayCount];
                bool[] bIsLongBearish = new bool[iArrayCount];
                bool[] bIsGapUpOpening = new bool[iArrayCount];
                bool[] bIsGapDownOpening = new bool[iArrayCount];
                bool[] bIsStrongBullishBarReversal = new bool[iArrayCount];
                bool[] bIsBullishBarReversal = new bool[iArrayCount];
                bool[] bIsStrongBearishBarReversal = new bool[iArrayCount];
                bool[] bIsBearishBarReversal = new bool[iArrayCount];
                Binance.Candlestick objPrevious = null;
                foreach (Binance.Candlestick objCurrent in objCandlestickData)
                {
                    bIsBullish[iCounter] = Utils.IsBullish(objCurrent);
                    bIsBearish[iCounter] = Utils.IsBearish(objCurrent);
                    bIsShortBullish[iCounter] = Utils.IsShortBullish(objCurrent);
                    bIsShortBearish[iCounter] = Utils.IsShortBearish(objCurrent);
                    bIsLongBullish[iCounter] = Utils.IsLongBullish(objCurrent);
                    bIsLongBearish[iCounter] = Utils.IsLongBearish(objCurrent);
                    bIsGapUpOpening[iCounter] = Utils.IsGapUpOpening(objPrevious, objCurrent);
                    bIsGapDownOpening[iCounter] = Utils.IsGapDownOpening(objPrevious, objCurrent);
                    bIsStrongBullishBarReversal[iCounter] = Utils.IsStrongBullishBarReversal(objPrevious, objCurrent);
                    bIsBullishBarReversal[iCounter] = Utils.IsBullishBarReversal(objPrevious, objCurrent);
                    bIsStrongBearishBarReversal[iCounter] = Utils.IsStrongBearishBarReversal(objPrevious, objCurrent);
                    bIsBearishBarReversal[iCounter] = Utils.IsBearishBarReversal(objPrevious, objCurrent);

                    dbVolume[iCounter] = Convert.ToDouble(objCurrent.Volume);
                    iCounter++;
                    objPrevious = objCurrent;
                }

                int iArrayLength = iArrayCount - 1;

                CdlSpinningTop objCdlSpinningTop = TAMath.CdlSpinningTop(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlEngulfing objCdlEngulfing = TAMath.CdlEngulfing(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlHarami objCdlHarami = TAMath.CdlHarami(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlHaramiCross objCdlHaramiCross = TAMath.CdlHaramiCross(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlHikkake objCdlHikkake = TAMath.CdlHikkake(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                Cdl3Inside objCdl3Inside = TAMath.Cdl3Inside(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlBeltHold objCdlBeltHold = TAMath.CdlBeltHold(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlHangingMan objCdlHangingMan = TAMath.CdlHangingMan(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlHammer objCdlHammer = TAMath.CdlHammer(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlDoji objCdlDoji = TAMath.CdlDoji(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlDragonflyDoji objCdlDragonflyDoji = TAMath.CdlDragonflyDoji(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlGravestoneDoji objCdlGravestoneDoji = TAMath.CdlGravestoneDoji(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlLongLine objCdlLongLine = TAMath.CdlLongLine(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlHomingPigeon objCdlHomingPigeon = TAMath.CdlHomingPigeon(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlTakuri objCdlTakuri = TAMath.CdlTakuri(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlAdvanceBlock objCdlAdvanceBlock = TAMath.CdlAdvanceBlock(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlStickSandwhich objCdlStickSandwhich = TAMath.CdlStickSandwhich(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlHignWave objCdlHignWave = TAMath.CdlHignWave(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlLongLeggedDoji objCdlLongLeggedDoji = TAMath.CdlLongLeggedDoji(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlMatchingLow objCdlMatchingLow = TAMath.CdlMatchingLow(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlRickshawMan objCdlRickshawMan = TAMath.CdlRickshawMan(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlSeperatingLines objCdlSeperatingLines = TAMath.CdlSeperatingLines(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);
                CdlShortLine objCdlShortLine = TAMath.CdlShortLine(0, iArrayLength, dbHeikinAshiOpen, dbHeikinAshiHigh, dbHeikinAshiLow, dbHeikinAshiClose);

                iCounter = 0;
                List<CandleStickPatterns> lsCandleStickPatterns = new List<CandleStickPatterns>();
                CandleStickPatterns objCandleStickPattern = null;
                List<string> lsCandlesticks = new List<string>();
                foreach (Binance.Candlestick objCurrent in objCandlestickData)
                {
                    lsCandlesticks.Clear();
                    objCandleStickPattern = new CandleStickPatterns();
                    objCandleStickPattern.TradedDate = objCurrent.OpenTime.ToString();
                    objCandleStickPattern.OpeningPrice = objCurrent.Open;
                    objCandleStickPattern.ClosingPrice = objCurrent.Close;
                    objCandleStickPattern.HighPrice = objCurrent.High;
                    objCandleStickPattern.LowPrice = objCurrent.Low;
                    objCandleStickPattern.IsBullish = bIsBullish[iCounter];
                    objCandleStickPattern.IsBearish = bIsBearish[iCounter];
                    objCandleStickPattern.IsShortBullish = bIsShortBullish[iCounter];
                    objCandleStickPattern.IsShortBearish = bIsShortBearish[iCounter];
                    objCandleStickPattern.IsLongBullish = bIsLongBullish[iCounter];
                    objCandleStickPattern.IsLongBearish = bIsLongBearish[iCounter];
                    objCandleStickPattern.IsBullishHammerStick = (objCdlHammer.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishHammerStick = (objCdlHammer.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishSpinningTop = (objCdlSpinningTop.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishSpinningTop = (objCdlSpinningTop.Integer[iCounter] == -100);
                    objCandleStickPattern.IsDoji = (objCdlDoji.Integer[iCounter] == 100);
                    objCandleStickPattern.IsDragonFlyDoji = (objCdlDragonflyDoji.Integer[iCounter] == 100);
                    objCandleStickPattern.IsGravestoneDoji = (objCdlGravestoneDoji.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBullishEngulfing = (objCdlEngulfing.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishEngulfing = (objCdlEngulfing.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishHarami = (objCdlHarami.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBullishHaramiCross = (objCdlHaramiCross.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishHarami = (objCdlHarami.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBearishHaramiCross = (objCdlHaramiCross.Integer[iCounter] == -100);
                    objCandleStickPattern.IsGapUpOpening = bIsGapUpOpening[iCounter];
                    objCandleStickPattern.IsGapDownOpening = bIsGapDownOpening[iCounter];
                    objCandleStickPattern.IsStrongBullishBarReversal = bIsStrongBullishBarReversal[iCounter];
                    objCandleStickPattern.IsBullishBarReversal = bIsBullishBarReversal[iCounter];
                    objCandleStickPattern.IsStrongBearishBarReversal = bIsStrongBearishBarReversal[iCounter];
                    objCandleStickPattern.IsBearishBarReversal = bIsBearishBarReversal[iCounter];
                    objCandleStickPattern.IsThreeInsideUp = (objCdl3Inside.Integer[iCounter] == 100);
                    objCandleStickPattern.IsThreeInsideDown = (objCdl3Inside.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishHikkake = (objCdlHikkake.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishHikkake = (objCdlHikkake.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishBeltHold = (objCdlBeltHold.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishBeltHold = (objCdlBeltHold.Integer[iCounter] == -100);
                    objCandleStickPattern.IsHangingMan = (objCdlHangingMan.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishLongLine = (objCdlLongLine.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishLongLine = (objCdlLongLine.Integer[iCounter] == -100);
                    objCandleStickPattern.IsHomingPigeon = (objCdlHomingPigeon.Integer[iCounter] == 100);
                    objCandleStickPattern.IsTakuri = (objCdlTakuri.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBullishAdvanceBlock = (objCdlAdvanceBlock.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishAdvanceBlock = (objCdlAdvanceBlock.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishStickSandwhich = (objCdlStickSandwhich.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishStickSandwhich = (objCdlStickSandwhich.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishHighWave = (objCdlHignWave.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishHighWave = (objCdlHignWave.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishLongLeggedDoji = (objCdlLongLeggedDoji.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishLongLeggedDoji = (objCdlLongLeggedDoji.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishMatchingLow = (objCdlMatchingLow.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishMatchingLow = (objCdlMatchingLow.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishRickshawMan = (objCdlRickshawMan.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishRickshawMan = (objCdlRickshawMan.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishSeparatingLines = (objCdlSeperatingLines.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishSeparatingLines = (objCdlSeperatingLines.Integer[iCounter] == -100);
                    objCandleStickPattern.IsBullishShortLine = (objCdlShortLine.Integer[iCounter] == 100);
                    objCandleStickPattern.IsBearishShortLine = (objCdlShortLine.Integer[iCounter] == -100);

                    if (objCandleStickPattern.IsShortBullish) { lsCandlesticks.Add("IsShortBullish"); };
                    if (objCandleStickPattern.IsShortBearish) { lsCandlesticks.Add("IsShortBearish"); };
                    if (objCandleStickPattern.IsLongBullish) { lsCandlesticks.Add("IsLongBullish"); };
                    if (objCandleStickPattern.IsLongBearish) { lsCandlesticks.Add("IsLongBearish"); };
                    if (objCandleStickPattern.IsBullishHammerStick) { lsCandlesticks.Add("BullishHammerStick"); };
                    if (objCandleStickPattern.IsBearishHammerStick) { lsCandlesticks.Add("BearishHammerStick"); };
                    if (objCandleStickPattern.IsBullishSpinningTop) { lsCandlesticks.Add("BullishSpinningTop"); };
                    if (objCandleStickPattern.IsBearishSpinningTop) { lsCandlesticks.Add("BearishSpinningTop"); };
                    if (objCandleStickPattern.IsDoji) { lsCandlesticks.Add("Doji"); };
                    if (objCandleStickPattern.IsDragonFlyDoji) { lsCandlesticks.Add("DragonFlyDoji"); };
                    if (objCandleStickPattern.IsGravestoneDoji) { lsCandlesticks.Add("GravestoneDoji"); };
                    if (objCandleStickPattern.IsBullishLongLeggedDoji) { lsCandlesticks.Add("BullishLongLeggedDoji"); };
                    if (objCandleStickPattern.IsBearishLongLeggedDoji) { lsCandlesticks.Add("BearishLongLeggedDoji"); };
                    if (objCandleStickPattern.IsBullishEngulfing) { lsCandlesticks.Add("BullishEngulfing"); };
                    if (objCandleStickPattern.IsBearishEngulfing) { lsCandlesticks.Add("BearishEngulfing"); };
                    if (objCandleStickPattern.IsBullishHarami) { lsCandlesticks.Add("BullishHarami"); };
                    if (objCandleStickPattern.IsBullishHaramiCross) { lsCandlesticks.Add("BullishHaramiCross"); };
                    if (objCandleStickPattern.IsBearishHarami) { lsCandlesticks.Add("BearishHarami"); };
                    if (objCandleStickPattern.IsBearishHaramiCross) { lsCandlesticks.Add("BearishHaramiCross"); };
                    if (objCandleStickPattern.IsThreeInsideUp) { lsCandlesticks.Add("ThreeInsideUp"); };
                    if (objCandleStickPattern.IsThreeInsideDown) { lsCandlesticks.Add("ThreeInsideDown"); };
                    if (objCandleStickPattern.IsBearishHikkake) { lsCandlesticks.Add("BearishHikkake"); };
                    if (objCandleStickPattern.IsBullishBeltHold) { lsCandlesticks.Add("BullishBeltHold"); };
                    if (objCandleStickPattern.IsBearishBeltHold) { lsCandlesticks.Add("BearishBeltHold"); };
                    if (objCandleStickPattern.IsHangingMan) { lsCandlesticks.Add("HangingMan"); };
                    if (objCandleStickPattern.IsBullishLongLine) { lsCandlesticks.Add("BullishLongLine"); };
                    if (objCandleStickPattern.IsBearishLongLine) { lsCandlesticks.Add("BearishLongLine"); };
                    if (objCandleStickPattern.IsHomingPigeon) { lsCandlesticks.Add("HomingPigeon"); };
                    if (objCandleStickPattern.IsTakuri) { lsCandlesticks.Add("Takuri"); };
                    if (objCandleStickPattern.IsBullishAdvanceBlock) { lsCandlesticks.Add("BullishAdvanceBlock"); };
                    if (objCandleStickPattern.IsBearishAdvanceBlock) { lsCandlesticks.Add("BearishAdvanceBlock"); };
                    if (objCandleStickPattern.IsBullishStickSandwhich) { lsCandlesticks.Add("BullishStickSandwhich"); };
                    if (objCandleStickPattern.IsBearishStickSandwhich) { lsCandlesticks.Add("BearishStickSandwhich"); };
                    if (objCandleStickPattern.IsBullishHighWave) { lsCandlesticks.Add("BullishHighWave"); };
                    if (objCandleStickPattern.IsBearishHighWave) { lsCandlesticks.Add("BearishHighWave"); };
                    if (objCandleStickPattern.IsBullishMatchingLow) { lsCandlesticks.Add("BullishMatchingLow"); };
                    if (objCandleStickPattern.IsBearishMatchingLow) { lsCandlesticks.Add("BearishMatchingLow"); };
                    if (objCandleStickPattern.IsBullishRickshawMan) { lsCandlesticks.Add("BullishRickshawMan"); };
                    if (objCandleStickPattern.IsBearishRickshawMan) { lsCandlesticks.Add("BearishRickshawMan"); };
                    if (objCandleStickPattern.IsBullishSeparatingLines) { lsCandlesticks.Add("BullishSeparatingLines"); };
                    if (objCandleStickPattern.IsBearishSeparatingLines) { lsCandlesticks.Add("BearishSeparatingLines"); };
                    if (objCandleStickPattern.IsBullishShortLine) { lsCandlesticks.Add("BullishShortLine"); };
                    if (objCandleStickPattern.IsBearishShortLine) { lsCandlesticks.Add("BearishShortLine"); };

                    if (lsCandlesticks.Count == 0)
                    {
                        if (objCandleStickPattern.IsBullish) { lsCandlesticks.Add("Bullish"); };
                        if (objCandleStickPattern.IsBearish) { lsCandlesticks.Add("Bearish"); };
                        if (objCandleStickPattern.IsGapUpOpening) { lsCandlesticks.Add("GapUpOpening"); };
                        if (objCandleStickPattern.IsGapDownOpening) { lsCandlesticks.Add("GapDownOpening"); };
                        if (objCandleStickPattern.IsStrongBullishBarReversal) { lsCandlesticks.Add("StrongBullishBarReversal"); };
                        if (objCandleStickPattern.IsBullishBarReversal) { lsCandlesticks.Add("BullishBarReversal"); };
                        if (objCandleStickPattern.IsStrongBearishBarReversal) { lsCandlesticks.Add("StrongBearishBarReversal"); };
                        if (objCandleStickPattern.IsBearishBarReversal) { lsCandlesticks.Add("BearishBarReversal"); };
                    }

                    objCandleStickPattern.CombinedCandlestickDetails = string.Join(", ", lsCandlesticks);

                    lsCandleStickPatterns.Add(objCandleStickPattern);

                    iCounter++;
                }

                IEnumerable<CandleStickPatterns> LstItems = lsCandleStickPatterns.Skip(Math.Max(0, lsCandleStickPatterns.Count() - 20)).Take(20);
                List<string> lsCandlestikPatternsLastItems = new List<string>();
                foreach (CandleStickPatterns objCandleStickPatterns in LstItems)
                {
                    lsCandlestikPatternsLastItems.Add(String.Format("Date:{0}, Close:{1}, Candle:{2}", objCandleStickPatterns.TradedDate, objCandleStickPatterns.ClosingPrice, objCandleStickPatterns.CombinedCandlestickDetails));
                }
                objPriceDetails.HeikinAshiCandlestick = String.Join(" # ", lsCandlestikPatternsLastItems);
                objPriceDetails.HeikinAshiDetails = lsHeikinAshi[lsHeikinAshi.Count - 1];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                objPriceDetails.PricePredictionDetails = PricePrediction.Neutral;
            }
            return objPriceDetails;
        }
    }
}