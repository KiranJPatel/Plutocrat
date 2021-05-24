using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Plutus.Core.Helpers
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

        [Display(Name = "Adjusted Close")]
        public Decimal AdjustedClose { get; set; }
    }

    public class RSICMODetails
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "Relative Strength")]
        public Decimal RS { get; set; }

        [Display(Name = "Relative Strength Index")]
        public Decimal RSI { get; set; }

        [Display(Name = "Chande Momentum Oscillator")]
        public Decimal CMO { get; set; }

        [Display(Name = "Stochastic RSI")]
        public Decimal StochRSI { get; set; }
    }


    public class DoubleExponentialMovingAverage
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "DEMA 1")]
        public double DEMA1 { get; set; }

        [Display(Name = "DEMA 2")]
        public double DEMA2 { get; set; }


        [Display(Name = "DEMA 3")]
        public double DEMA3 { get; set; }
    }

    public class TripleExponentialMovingAverage
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "TEMA 1")]
        public double TEMA1 { get; set; }

        [Display(Name = "TEMA 2")]
        public double TEMA2 { get; set; }

        [Display(Name = "TEMA 3")]
        public double TEMA3 { get; set; }
    }

    public class HullMovingAverage
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "HMA Slow")]
        public double HMASlow { get; set; }

        [Display(Name = "HMA Fast")]
        public double HMAFast { get; set; }
    }

    public class KaufmanAdaptiveMovingAverage
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "KAMA 1")]
        public double KAMA1 { get; set; }

        [Display(Name = "KAMA 2")]
        public double KAMA2 { get; set; }
    }

    public class ParabolicSAR
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "PSAR")]
        public double PSAR { get; set; }
    }

    public class TriangularMovingAverage
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "TRIMA1")]
        public double TRIMA1 { get; set; }

        [Display(Name = "TRIMA2")]
        public double TRIMA2 { get; set; }
    }

    public class TripleExponentialMovingAverageOscillator
    {
        [DataType(DataType.Text)]
        [Display(Name = "Trade Date")]
        public string TradedDate { get; set; }

        [Display(Name = "TRIX1")]
        public double TRIX1 { get; set; }

        [Display(Name = "TRIX2")]
        public double TRIX2 { get; set; }
    }

}
