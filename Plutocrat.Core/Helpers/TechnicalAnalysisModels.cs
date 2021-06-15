using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Plutocrat.Core.Helpers
{
    public class AroonOscillatorDetails
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "Aroon Up")]
        public double AroonUp { get; set; }

        [Display(Name = "Aroon Down")]
        public double AroonDown { get; set; }

        [Display(Name = "Aroon Oscillator")]
        public double AroonOscillator { get; set; }

        [Display(Name = "Current Price")]
        public double CurrentPrice { get; set; }

        [Display(Name = "Cross Over")]
        public String CrossOverType { get; set; }

        [Display(Name = "CrossOver Date")]
        public string CrossOverDate { get; set; }

        [Display(Name = "Last CrossOver Type")]
        public string LastCrossOverType { get; set; }

        [Display(Name = "Aroon Up Trend Last 15 Days")]
        public string AroonUpTrendLast15Days { get; set; }

        [Display(Name = "Aroon Down Trend Last 15 Days")]
        public string AroonDownTrendLast15Days { get; set; }

        [Display(Name = "Aroon Osc Trend Last 15 Days")]
        public string AroonOscTrendLast15Days { get; set; }

        [Display(Name = "Aroon Up Down Trend Last 15 Days")]
        public string AroonUpDownTrendLast15Days { get; set; }
    }

    public class SimpleMovingAverage
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "Current Price")]
        public double CurrentPrice { get; set; }

        [Display(Name = "SMA 1")]
        public double SMA1 { get; set; }

        [Display(Name = "SMA1 Cross Over")]
        public String SMA1CrossOverType { get; set; }

        [Display(Name = "SMA1 Cross Over Date")]
        public String SMA1CrossOverDate { get; set; }

        [Display(Name = "SMA 2")]
        public double SMA2 { get; set; }

        [Display(Name = "SMA2 Cross Over")]
        public String SMA2CrossOverType { get; set; }

        [Display(Name = "SMA2 Cross Over Date")]
        public String SMA2CrossOverDate { get; set; }

        [Display(Name = "SMA 3")]
        public double SMA3 { get; set; }

        [Display(Name = "SMA3 Cross Over")]
        public String SMA3CrossOverType { get; set; }

        [Display(Name = "SMA3 Cross Over Date")]
        public String SMA3CrossOverDate { get; set; }
    }

    public class ExponentialMovingAverage
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "Current Price")]
        public double CurrentPrice { get; set; }

        [Display(Name = "EMA 1")]
        public double EMA1 { get; set; }

        [Display(Name = "EMA1 Cross Over")]
        public String EMA1CrossOverType { get; set; }

        [Display(Name = "EMA1 Cross Over Date")]
        public String EMA1CrossOverDate { get; set; }

        [Display(Name = "EMA 2")]
        public double EMA2 { get; set; }

        [Display(Name = "EMA2 Cross Over")]
        public String EMA2CrossOverType { get; set; }

        [Display(Name = "EMA2 Cross Over Date")]
        public String EMA2CrossOverDate { get; set; }

        [Display(Name = "EMA 3")]
        public double EMA3 { get; set; }

        [Display(Name = "EMA3 Cross Over")]
        public String EMA3CrossOverType { get; set; }

        [Display(Name = "EMA3 Cross Over Date")]
        public String EMA3CrossOverDate { get; set; }
    }

    public class DoubleExponentialMovingAverage
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "Current Price")]
        public double CurrentPrice { get; set; }

        [Display(Name = "DEMA 1")]
        public double DEMA1 { get; set; }

        [Display(Name = "DEMA1 Cross Over")]
        public String DEMA1CrossOverType { get; set; }

        [Display(Name = "DEMA1 Cross Over Date")]
        public String DEMA1CrossOverDate { get; set; }

        [Display(Name = "DEMA 2")]
        public double DEMA2 { get; set; }

        [Display(Name = "DEMA2 Cross Over")]
        public String DEMA2CrossOverType { get; set; }

        [Display(Name = "DEMA2 Cross Over Date")]
        public String DEMA2CrossOverDate { get; set; }

        [Display(Name = "DEMA 3")]
        public double DEMA3 { get; set; }

        [Display(Name = "DEMA3 Cross Over")]
        public String DEMA3CrossOverType { get; set; }

        [Display(Name = "DEMA3 Cross Over Date")]
        public String DEMA3CrossOverDate { get; set; }
    }

    public class TripleExponentialMovingAverage
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "Current Price")]
        public double CurrentPrice { get; set; }

        [Display(Name = "TEMA 1")]
        public double TEMA1 { get; set; }

        [Display(Name = "TEMA1 Cross Over")]
        public String TEMA1CrossOverType { get; set; }

        [Display(Name = "TEMA1 Cross Over Date")]
        public String TEMA1CrossOverDate { get; set; }

        [Display(Name = "TEMA 2")]
        public double TEMA2 { get; set; }

        [Display(Name = "TEMA2 Cross Over")]
        public String TEMA2CrossOverType { get; set; }

        [Display(Name = "TEMA2 Cross Over Date")]
        public String TEMA2CrossOverDate { get; set; }

        [Display(Name = "TEMA 3")]
        public double TEMA3 { get; set; }

        [Display(Name = "TEMA3 Cross Over")]
        public String TEMA3CrossOverType { get; set; }

        [Display(Name = "TEMA3 Cross Over Date")]
        public String TEMA3CrossOverDate { get; set; }
    }

    public class ParabolicSAR
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "PSAR")]
        public double PSAR { get; set; }

        [Display(Name = "Current Price")]
        public double CurrentPrice { get; set; }

        [Display(Name = "Cross Over")]
        public String CrossOverType { get; set; }
    }

    public class HeikinAshi
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "Opening Price")]
        public Decimal Open { get; set; }

        [Display(Name = "Daily High Price")]
        public Decimal High { get; set; }

        [Display(Name = "Daily Low Price")]
        public Decimal Low { get; set; }

        [Display(Name = "Closing Price")]
        public Decimal Close { get; set; }

        [Display(Name = "Is Bullish")]
        public bool IsBullish { get; set; }

        [Display(Name = "Weakness")]
        public decimal Weakness { get; set; }

        [Display(Name = "CandleStrength")]
        public decimal CandleStrength { get; set; }
    }

    public class CandleStickPatterns
    {
        public string TradedDate { get; set; }
        public Decimal OpeningPrice { get; set; }
        public Decimal HighPrice { get; set; }
        public Decimal LowPrice { get; set; }
        public Decimal ClosingPrice { get; set; }

        public string CombinedCandlestickDetails { get; set; }

        public bool IsBullish { get; set; }
        public bool IsBearish { get; set; }
        public bool IsShortBullish { get; set; }
        public bool IsShortBearish { get; set; }
        public bool IsLongBullish { get; set; }
        public bool IsLongBearish { get; set; }
        public bool IsBullishHammerStick { get; set; }
        public bool IsBearishHammerStick { get; set; }
        public bool IsBullishInvertedHammerStick { get; set; }
        public bool IsBearishInvertedHammerStick { get; set; }
        public bool IsBullishMarubozu { get; set; }
        public bool IsBearishMarubozu { get; set; }
        public bool IsBullishSpinningTop { get; set; }
        public bool IsBearishSpinningTop { get; set; }
        public bool IsDoji { get; set; }
        public bool IsDragonFlyDoji { get; set; }
        public bool IsGravestoneDoji { get; set; }
        public bool IsBullishEngulfing { get; set; }
        public bool IsBearishEngulfing { get; set; }
        public bool IsBullishHarami { get; set; }
        public bool IsBullishHaramiCross { get; set; }
        public bool IsBearishHarami { get; set; }
        public bool IsBearishHaramiCross { get; set; }
        public bool IsDarkCloudCover { get; set; }
        public bool IsPiercingLine { get; set; }
        public bool IsBullishKicker { get; set; }
        public bool IsBearishKicker { get; set; }
        public bool IsAbandonedBaby { get; set; }
        public bool IsUpsideTasukiGap { get; set; }
        public bool IsDownsideTasukiGap { get; set; }
        public bool IsMorningDojiStar { get; set; }
        public bool IsMorningStar { get; set; }
        public bool IsEveningDojiStar { get; set; }
        public bool IsEveningStar { get; set; }
        public bool IsThreeBlackCrows { get; set; }
        public bool IsThreeWhiteSoldiers { get; set; }
        public bool IsThreeInsideUp { get; set; }
        public bool IsThreeInsideDown { get; set; }
        public bool IsThreeOutsideUp { get; set; }
        public bool IsThreeOutsideDown { get; set; }
        public bool IsGapUpOpening { get; set; }
        public bool IsGapDownOpening { get; set; }
        public bool IsPartialGapUpOpening { get; set; }
        public bool IsPartialGapDownOpening { get; set; }
        public bool IsStrongBullishBarReversal { get; set; }
        public bool IsStrongBearishBarReversal { get; set; }
        public bool IsBullishBarReversal { get; set; }
        public bool IsBearishBarReversal { get; set; }
        public bool IsTwoBlackCrows { get; set; }
        public bool IsBullishHikkake { get; set; }
        public bool IsBearishHikkake { get; set; }
        public bool IsModBullishHikkake { get; set; }
        public bool IsModBearishHikkake { get; set; }
        public bool IsBullishBreakaway { get; set; }
        public bool IsBearishBreakaway { get; set; }
        public bool IsBullishBeltHold { get; set; }
        public bool IsBearishBeltHold { get; set; }
        public bool IsHangingMan { get; set; }
        public bool IsInNeck { get; set; }
        public bool IsOnNeck { get; set; }
        public bool IsBullishLongLine { get; set; }
        public bool IsBearishLongLine { get; set; }
        public bool IsHomingPigeon { get; set; }
        public bool IsShootingStar { get; set; }
        public bool IsTakuri { get; set; }
        public bool IsBullishThrusting { get; set; }
        public bool IsBearishThrusting { get; set; }
        public bool IsBullishAdvanceBlock { get; set; }
        public bool IsBearishAdvanceBlock { get; set; }
        public bool IsBullishThreeLineStrike { get; set; }
        public bool IsBearishThreeLineStrike { get; set; }
        public bool IsBullishConcealBabySwallow { get; set; }
        public bool IsBearishConcealBabySwallow { get; set; }
        public bool IsBullishCounterAttack { get; set; }
        public bool IsBearishCounterAttack { get; set; }
        public bool IsBullishStickSandwhich { get; set; }
        public bool IsBearishStickSandwhich { get; set; }
        public bool IsBullishLadderBottom { get; set; }
        public bool IsBearishLadderBottom { get; set; }
        public bool IsBullishHighWave { get; set; }
        public bool IsBearishHighWave { get; set; }
        public bool IsBullishTristar { get; set; }
        public bool IsBearishTristar { get; set; }
        public bool IsBullishKicking { get; set; }
        public bool IsBearishKicking { get; set; }
        public bool IsBullishKickingByLength { get; set; }
        public bool IsBearishKickingByLength { get; set; }
        public bool IsBullishLongLeggedDoji { get; set; }
        public bool IsBearishLongLeggedDoji { get; set; }
        public bool IsBullishMatchingLow { get; set; }
        public bool IsBearishMatchingLow { get; set; }
        public bool IsBullishMatHold { get; set; }
        public bool IsBearishMatHold { get; set; }
        public bool IsBullishRickshawMan { get; set; }
        public bool IsBearishRickshawMan { get; set; }
        public bool IsBullishSeparatingLines { get; set; }
        public bool IsBearishSeparatingLines { get; set; }
        public bool IsBullishShortLine { get; set; }
        public bool IsBearishShortLine { get; set; }
        public bool IsBullishStalledPattern { get; set; }
        public bool IsBearishStalledPattern { get; set; }
        public bool IsThreeStarsInSouth { get; set; }
        public bool IsGapSideSideWhite { get; set; }
        public bool IsIdenticalThreeCrows { get; set; }
        public bool IsRiseFallThreeMethods { get; set; }
        public bool IsUniqueThreeRiver { get; set; }
        public bool IsUpsideGapTwoCrows { get; set; }
        public bool IsXSideGap3Methods { get; set; }
    }

    public class PivotPoints
    {
        [Display(Name = "High Price")]
        public decimal HighPrice { get; set; }

        [Display(Name = "Low Price")]
        public decimal LowPrice { get; set; }

        [Display(Name = "Opening Price")]
        public decimal OpeningPrice { get; set; }

        [Display(Name = "Closing Price")]
        public decimal ClosingPrice { get; set; }

        [Display(Name = "Classic Resistance 1")]
        public decimal ClassicResistance1 { get; set; }

        [Display(Name = "Classic Resistance 2")]
        public decimal ClassicResistance2 { get; set; }

        [Display(Name = "Classic Resistance 3")]
        public decimal ClassicResistance3 { get; set; }

        [Display(Name = "Classic Pivot Point")]
        public decimal ClassicPivotPoint { get; set; }

        [Display(Name = "Classic Support 1")]
        public decimal ClassicSupport1 { get; set; }

        [Display(Name = "Classic Support 2")]
        public decimal ClassicSupport2 { get; set; }

        [Display(Name = "Classic Support 3")]
        public decimal ClassicSupport3 { get; set; }

        [Display(Name = "Woodie Resistance 1")]
        public decimal WoodieResistance1 { get; set; }

        [Display(Name = "Woodie Resistance 2")]
        public decimal WoodieResistance2 { get; set; }

        [Display(Name = "Woodie Pivot Point")]
        public decimal WoodiePivotPoint { get; set; }

        [Display(Name = "Woodie Support 1")]
        public decimal WoodieSupport1 { get; set; }

        [Display(Name = "Woodie Support 2")]
        public decimal WoodieSupport2 { get; set; }

        [Display(Name = "Camarilla Resistance 1")]
        public decimal CamarillaResistance1 { get; set; }

        [Display(Name = "Camarilla Resistance 2")]
        public decimal CamarillaResistance2 { get; set; }

        [Display(Name = "Camarilla Resistance 3")]
        public decimal CamarillaResistance3 { get; set; }

        [Display(Name = "Camarilla Resistance 4")]
        public decimal CamarillaResistance4 { get; set; }

        [Display(Name = "Camarilla Support 1")]
        public decimal CamarillaSupport1 { get; set; }

        [Display(Name = "Camarilla Support 2")]
        public decimal CamarillaSupport2 { get; set; }

        [Display(Name = "Camarilla Support 3")]
        public decimal CamarillaSupport3 { get; set; }

        [Display(Name = "Camarilla Support 4")]
        public decimal CamarillaSupport4 { get; set; }

        [Display(Name = "DeMark Resistance")]
        public decimal DeMarkResistance { get; set; }

        [Display(Name = "DeMark Support")]
        public decimal DeMarkSupport { get; set; }

        [Display(Name = "Chicago Floor Trading Support 1")]
        public decimal ChicagoFloorTradingSupport1 { get; set; }

        [Display(Name = "Chicago Floor Trading Support 2")]
        public decimal ChicagoFloorTradingSupport2 { get; set; }

        [Display(Name = "Chicago Floor Trading Resistance 1")]
        public decimal ChicagoFloorTradingResistance1 { get; set; }

        [Display(Name = "Chicago Floor Trading Resistance 2")]
        public decimal ChicagoFloorTradingResistance2 { get; set; }


    }

}
