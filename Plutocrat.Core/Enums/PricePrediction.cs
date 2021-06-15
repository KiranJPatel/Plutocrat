using Plutocrat.Core.Helpers;

namespace Plutocrat.Core.Enums
{
    public enum PricePrediction
    {
        Bullish,
        Bearish,
        Neutral
    }

    public class PriceDetails
    {
        public decimal CurrentPrice { get; set; }

        public decimal MovingAverageShort { get; set; }

        public decimal MovingAverageMedium { get; set; }

        public decimal MovingAverageLong { get; set; }

        public PivotPoints PivotPointsDetails { get; set; }

        public PricePrediction PricePredictionDetails { get; set; }

        public double PriceSurge { get; set; }

        public double VolumeSurge { get; set; }

        public string HeikinAshiCandlestick { get; set; }

        public HeikinAshi HeikinAshiDetails { get; set; }

        public string CandlestickDetails { get; set; }

        public AroonOscillatorDetails AroonOscillatorDtls { get; set; }

        public double CCI { get; set; }

        public double RSI { get; set; }

        public double KlingerVolumeOscillator { get; set; }

        public string CCIDailyTrend { get; set; }

        public string RSIDailyTrend { get; set; }

        public string KVODailyTrend { get; set; }

        public string VolumeTrending { get; set; }

        public string KVOTrending { get; set; }

        public string OBVTrending { get; set; }

        public string VWAPTrending { get; set; }
    }
}