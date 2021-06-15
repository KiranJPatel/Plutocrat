using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentScheduler;
using Plutocrat.Core.Enums;
using Plutocrat.Core.Helpers;
using Plutocrat.Core.Interfaces;

namespace Plutocrat.Core.Jobs
{
    public class DownTrendNotificationJob : IJob
    {
        private readonly IPlutocratService _PlutocratService;
        private readonly IBinanceHandler _binanceHandler;
        private readonly bool _test;

        public DownTrendNotificationJob(IPlutocratService PlutocratService, IBinanceHandler BinanceHandler, bool test)
        {
            _PlutocratService = PlutocratService;
            _binanceHandler = BinanceHandler;
            _test = test;
        }

        public void Execute()
        {

            Parallel.ForEach(_PlutocratService.Orders, async (order) =>
            {
                try
                {
                    IEnumerable<Binance.Candlestick> objCandlestickData = await _binanceHandler.GetCandlestick(order.Base, order.Symbol, Period.Daily);

                    var dailyPrediction = await _PlutocratService.GetSMAAnalysisPrediction(order.Base, order.Symbol, Period.Daily, objCandlestickData);

                    var dailyHeikinAshiPrediction = await _PlutocratService.GetHeikinAshiCandleStickPrediction(order.Base, order.Symbol, Period.Daily, objCandlestickData);

                    var aroonPrediction = await _PlutocratService.GetAroonPrediction(order.Base, order.Symbol, Period.Daily, objCandlestickData);

                    DbHandler.Instance.UpsertPricePrediction(DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"), order.Base, order.Symbol, dailyPrediction.PricePredictionDetails.ToString(), dailyPrediction.CurrentPrice,
                        Math.Round(dailyPrediction.MovingAverageShort, 8), Math.Round(dailyPrediction.MovingAverageMedium, 8), Math.Round(dailyPrediction.MovingAverageLong, 8),
                        Math.Round(dailyPrediction.PivotPointsDetails.WoodieSupport1, 8), Math.Round(dailyPrediction.PivotPointsDetails.WoodieSupport2, 8),
                        Math.Round(dailyPrediction.PivotPointsDetails.WoodieResistance1, 8), Math.Round(dailyPrediction.PivotPointsDetails.WoodieResistance2, 8),
                        Math.Round(dailyPrediction.PriceSurge, 8), Math.Round(dailyPrediction.VolumeSurge, 8), dailyHeikinAshiPrediction.HeikinAshiCandlestick, String.Empty,
                        aroonPrediction.AroonOscillatorDtls.AroonUp, aroonPrediction.AroonOscillatorDtls.AroonDown, aroonPrediction.AroonOscillatorDtls.AroonOscillator,
                        aroonPrediction.AroonOscillatorDtls.CrossOverType, aroonPrediction.AroonOscillatorDtls.CrossOverDate, aroonPrediction.AroonOscillatorDtls.LastCrossOverType,
                        aroonPrediction.AroonOscillatorDtls.AroonUpDownTrendLast15Days, aroonPrediction.AroonOscillatorDtls.AroonUpTrendLast15Days, aroonPrediction.AroonOscillatorDtls.AroonUpTrendLast15Days,
                        aroonPrediction.AroonOscillatorDtls.AroonOscTrendLast15Days, dailyPrediction.CCI, dailyPrediction.CCIDailyTrend, dailyPrediction.RSI,
                        dailyPrediction.RSIDailyTrend, dailyPrediction.KlingerVolumeOscillator, dailyPrediction.KVODailyTrend);

                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm tt")} ==> {order.Symbol} has {dailyPrediction.PricePredictionDetails} signals on daily charts. Price({order.Base}):{dailyPrediction.CurrentPrice}, " +
                                        $"MovingAvgShort:{Math.Round(dailyPrediction.MovingAverageShort, 8)}, MovingAvgMed:{Math.Round(dailyPrediction.MovingAverageMedium, 8)}, MovingAvgLong:{Math.Round(dailyPrediction.MovingAverageLong, 8)}, " +
                                        $"Support1:{Math.Round(dailyPrediction.PivotPointsDetails.WoodieSupport1, 8)}, Support2:{Math.Round(dailyPrediction.PivotPointsDetails.WoodieSupport2, 8)}, Resistance1:{Math.Round(dailyPrediction.PivotPointsDetails.WoodieResistance1, 8)}, Resistance2:{Math.Round(dailyPrediction.PivotPointsDetails.WoodieResistance2, 8)}, " +
                                        $"PriceSurge:{ Math.Round(dailyPrediction.PriceSurge, 8)}, VolumeSurge: { Math.Round(dailyPrediction.VolumeSurge, 8)}, " +
                                        $"AroonUp:{aroonPrediction.AroonOscillatorDtls.AroonUp}, AroonDown:{aroonPrediction.AroonOscillatorDtls.AroonDown}, " +
                                        $"AroonOscillator:{aroonPrediction.AroonOscillatorDtls.AroonOscillator}, CrossOverType:{aroonPrediction.AroonOscillatorDtls.CrossOverType}, " +
                                        $"LastCrossOverDate:{aroonPrediction.AroonOscillatorDtls.CrossOverDate}, LastCrossOverType:{aroonPrediction.AroonOscillatorDtls.LastCrossOverType}, " +
                                        $"AroonUpDownTrendLast15Days:{aroonPrediction.AroonOscillatorDtls.AroonUpDownTrendLast15Days}, " +
                                        $"AroonUpTrendLast15Days:{aroonPrediction.AroonOscillatorDtls.AroonUpTrendLast15Days}, " +
                                        $"AroonDownTrendLast15Days:{aroonPrediction.AroonOscillatorDtls.AroonDownTrendLast15Days}, " +
                                        $"AroonOscTrendLast15Days:{aroonPrediction.AroonOscillatorDtls.AroonOscTrendLast15Days}, " +
                                        $"CCI:{dailyPrediction.CCI}, CCIDailyTrend:{dailyPrediction.CCIDailyTrend}, " +
                                        $"RSI:{dailyPrediction.RSI}, RSIDailyTrend:{dailyPrediction.RSIDailyTrend}, " +
                                        $"KlingerVolumeOscillator:{dailyPrediction.KlingerVolumeOscillator}, KVODailyTrend:{dailyPrediction.KVODailyTrend}, " +
                                        $"VolumeTrending:{dailyPrediction.VolumeTrending}, KVOTrending:{dailyPrediction.KVOTrending}, " +
                                        $"OBVTrending:{dailyPrediction.OBVTrending}, VWAPTrending:{dailyPrediction.VWAPTrending}");
                    Console.WriteLine(" ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Issues while processing {order.Symbol}({order.Base}) Error:{ex.Message}");
                }

            });

            Console.WriteLine("###############################################################################################################################");
        }
    }
}