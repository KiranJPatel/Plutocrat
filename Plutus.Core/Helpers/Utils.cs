using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Plutus.Core.Helpers
{
    class Utils
    {
        private static int[] GetMaxColumnsWidth(string[,] arrValues)
        {
            var maxColumnsWidth = new int[arrValues.GetLength(1)];
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
                {
                    int newLength = arrValues[rowIndex, colIndex].Length;
                    int oldLength = maxColumnsWidth[colIndex];

                    if (newLength > oldLength)
                    {
                        maxColumnsWidth[colIndex] = newLength;
                    }
                }
            }

            return maxColumnsWidth;
        }

        public static bool IsBullish(Binance.Candlestick objCurrent)
        {
            return objCurrent.Open < objCurrent.Close;
        }

        public static bool IsBearish(Binance.Candlestick objCurrent)
        {
            return objCurrent.Open > objCurrent.Close;
        }

        public static bool IsGap(Binance.Candlestick objPrevious, Binance.Candlestick objCurrent)
        {
            return Math.Max(objPrevious.Open, objPrevious.Close) < Math.Min(objCurrent.Open, objCurrent.Close);
        }

        public static bool IsGapUpOpening(Binance.Candlestick objPrevious, Binance.Candlestick objCurrent)
        {
            if (objPrevious == null)
            {
                return false;
            }
            return objCurrent.Open > objPrevious.High;
        }

        public static bool IsGapDownOpening(Binance.Candlestick objPrevious, Binance.Candlestick objCurrent)
        {
            if (objPrevious == null)
            {
                return false;
            }
            return objCurrent.Open < objPrevious.Low;
        }

        public static bool IsPartialGapUpOpening(Binance.Candlestick objPrevious, Binance.Candlestick objCurrent)
        {
            if (objPrevious == null)
            {
                return false;
            }
            return objCurrent.Open > objPrevious.Low && objCurrent.Open < objPrevious.High;
        }

        public static bool IsPartialGapDownOpening(Binance.Candlestick objPrevious, Binance.Candlestick objCurrent)
        {
            if (objPrevious == null)
            {
                return false;
            }
            return objCurrent.Open < objPrevious.Close && objCurrent.Open > objPrevious.Low;
        }

        public static bool IsStrongBullishBarReversal(Binance.Candlestick objPrevious, Binance.Candlestick objCurrent)
        {
            if (objPrevious == null)
            {
                return false;
            }
            return objCurrent.Low < objPrevious.Low && objCurrent.Close > objPrevious.High;
        }

        public static bool IsBullishBarReversal(Binance.Candlestick objPrevious, Binance.Candlestick objCurrent)
        {
            if (objPrevious == null)
            {
                return false;
            }
            return objCurrent.Low < objPrevious.Low && objCurrent.Close > objPrevious.Close;
        }

        public static bool IsStrongBearishBarReversal(Binance.Candlestick objPrevious, Binance.Candlestick objCurrent)
        {
            if (objPrevious == null)
            {
                return false;
            }
            return objCurrent.High > objPrevious.High && objCurrent.Close < objPrevious.Low;
        }

        public static bool IsBearishBarReversal(Binance.Candlestick objPrevious, Binance.Candlestick objCurrent)
        {
            if (objPrevious == null)
            {
                return false;
            }
            return objCurrent.High > objPrevious.High && objCurrent.Close < objPrevious.Close;
        }
        public static List<HeikinAshi> GenerateHeikinAshi(IEnumerable<Binance.Candlestick> objCandlestickData)
        {
            List<HeikinAshi> lsHeikinAshi = new List<HeikinAshi>();
            decimal dcHeikinAshiOpen = 0;
            decimal dcHeikinAshiClose = 0;
            decimal dcHeikinAshiHigh = 0;
            decimal dcHeikinAshiLow = 0;

            decimal? dcPrevHeikinAshiOpen = null;
            decimal? dcPrevHeikinAshiClose = null;
            bool bPrevHeikinAshiTrend = false;

            decimal dcLow = 0;
            decimal dcHigh = 0;
            decimal dcOpenPrice = 0;
            decimal dcClosePrice = 0;
            foreach (Binance.Candlestick objCurrent in objCandlestickData)
            {
                dcOpenPrice = objCurrent.Open;
                dcHigh = objCurrent.High;
                dcLow = objCurrent.Low;
                dcClosePrice = objCurrent.Close;

                // close
                dcHeikinAshiClose = (dcOpenPrice + dcHigh + dcLow + dcClosePrice) / 4;

                // open
                dcHeikinAshiOpen = (dcPrevHeikinAshiOpen == null) ? (dcOpenPrice + dcClosePrice) / 2 : (decimal)(dcPrevHeikinAshiOpen + dcPrevHeikinAshiClose) / 2;

                // high
                decimal[] arrH = { dcHigh, dcHeikinAshiOpen, dcHeikinAshiClose };
                dcHeikinAshiHigh = arrH.Max();

                // low
                decimal[] arrL = { dcLow, dcHeikinAshiOpen, dcHeikinAshiClose };
                dcHeikinAshiLow = arrL.Min();

                // trend (bullish (buy / green), bearish (sell / red)
                // strength (size of directional shadow / no shadow is strong)
                bool trend;
                decimal strength;

                if (dcHeikinAshiClose > dcHeikinAshiOpen)
                {
                    trend = true;
                    strength = dcHeikinAshiOpen - dcHeikinAshiLow;
                }
                else if (dcHeikinAshiClose < dcHeikinAshiOpen)
                {
                    trend = false;
                    strength = dcHeikinAshiHigh - dcHeikinAshiOpen;
                }
                else
                {
                    trend = bPrevHeikinAshiTrend;
                    strength = dcHeikinAshiHigh - dcHeikinAshiLow;
                }

                decimal dcCandleStrength = (dcHeikinAshiHigh - dcHeikinAshiLow) > 0 ? 100 * (dcHeikinAshiOpen - dcHeikinAshiLow) / (dcHeikinAshiHigh - dcHeikinAshiLow) - 50 : -50;
                dcCandleStrength = dcHeikinAshiClose > dcHeikinAshiOpen ? dcCandleStrength : dcCandleStrength * -1;

                lsHeikinAshi.Add(new HeikinAshi
                {
                    TradedDate = objCurrent.OpenTime.ToString(),
                    Open = dcHeikinAshiOpen,
                    Close = dcHeikinAshiClose,
                    High = dcHeikinAshiHigh,
                    Low = dcHeikinAshiLow,
                    IsBullish = trend,
                    Weakness = strength,
                    CandleStrength = dcCandleStrength
                });

                dcPrevHeikinAshiOpen = dcHeikinAshiOpen;
                dcPrevHeikinAshiClose = dcHeikinAshiClose;
            }
            return lsHeikinAshi;
        }
    }

    public static class TableParser
    {
        public static string ToStringTable<T>(
          this IEnumerable<T> values,
          string[] columnHeaders,
          params Func<T, object>[] valueSelectors)
        {
            return ToStringTable(values.ToArray(), columnHeaders, valueSelectors);
        }

        public static string ToStringTable<T>(
          this T[] values,
          string[] columnHeaders,
          params Func<T, object>[] valueSelectors)
        {
            var arrValues = new string[values.Length + 1, valueSelectors.Length];

            // Fill headers
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                arrValues[0, colIndex] = columnHeaders[colIndex];
            }

            // Fill table rows
            for (int rowIndex = 1; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    arrValues[rowIndex, colIndex] = valueSelectors[colIndex]
                      .Invoke(values[rowIndex - 1]).ToString();
                }
            }

            return ToStringTable(arrValues);
        }

        public static string ToStringTable(this string[,] arrValues)
        {
            int[] maxColumnsWidth = GetMaxColumnsWidth(arrValues);
            var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

            var sb = new StringBuilder();
            for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    // Print cell
                    string cell = arrValues[rowIndex, colIndex];
                    cell = cell.PadRight(maxColumnsWidth[colIndex]);
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                // Print end of line
                sb.Append(" | ");
                sb.AppendLine();

                // Print splitter
                if (rowIndex == 0)
                {
                    sb.AppendFormat(" |{0}| ", headerSpliter);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static int[] GetMaxColumnsWidth(string[,] arrValues)
        {
            var maxColumnsWidth = new int[arrValues.GetLength(1)];
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
                {
                    int newLength = arrValues[rowIndex, colIndex].Length;
                    int oldLength = maxColumnsWidth[colIndex];

                    if (newLength > oldLength)
                    {
                        maxColumnsWidth[colIndex] = newLength;
                    }
                }
            }

            return maxColumnsWidth;
        }
    }

}
