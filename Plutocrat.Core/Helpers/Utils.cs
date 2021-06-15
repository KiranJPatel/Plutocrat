using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;
using System.IO;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InlineQueryResults;
using System.Net;

namespace Plutocrat.Core.Helpers
{
    public class Utils
    {
        static readonly string token = "1736851685:AAEFobDNlvQ30B1hALpIFWzijPkYcvfcr8s";
        static readonly string chatId = "1736851685";
        static readonly decimal dcLargeBodyMinimum = 0.01M;//greater than 1.0%
        private static DbHandler mobjDbHandler = null;
        public static bool InitializeDependencies()
        {
            // Initialize dbHandler Instance
            mobjDbHandler = DbHandler.Instance;

            //open database
            if (!(mobjDbHandler.OpenConnection()))
            {
                return false;
            }
            return true;
        }

        public static string SendTelegramMessage(string sMessage)
        {
            try
            {
                var bot = new Telegram.Bot.TelegramBotClient(token);
                bot.SendTextMessageAsync(chatId, sMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine("err");
            }
            return "Message Sent";
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

        public static bool IsBullish(Binance.Candlestick objCurrent)
        {
            return objCurrent.Open < objCurrent.Close;
        }

        public static bool IsBearish(Binance.Candlestick objCurrent)
        {
            return objCurrent.Open > objCurrent.Close;
        }

        public static bool IsShortBullish(Binance.Candlestick objCurrent)
        {
            bool isShortBullish = ((objCurrent.Close > objCurrent.Open) &&
                ((objCurrent.High - objCurrent.Low) > (3 * (objCurrent.Close - objCurrent.Open))));
            return isShortBullish;
        }

        public static bool IsShortBearish(Binance.Candlestick objCurrent)
        {
            bool isShortBearish = ((objCurrent.Open > objCurrent.Close) &&
                ((objCurrent.High - objCurrent.Low) > (3 * (objCurrent.Open - objCurrent.Close)))) ;
            return isShortBearish;
        }

        public static bool IsLongBullish(Binance.Candlestick objCurrent)
        {
            bool isLongBullish = (objCurrent.Close >= objCurrent.Open * (1 + dcLargeBodyMinimum) && IsBullish(objCurrent)) &&
                ((objCurrent.Close > objCurrent.Open) && ((objCurrent.Close - objCurrent.Open) / (.001M + objCurrent.High - objCurrent.Low) > 0.6M));
            return isLongBullish;
        }

        public static bool IsLongBearish(Binance.Candlestick objCurrent)
        {
            bool isLongBearish = (objCurrent.Close <= objCurrent.Open * (1 - dcLargeBodyMinimum) && IsBearish(objCurrent)) &&
                (objCurrent.Open > objCurrent.Close) && ((objCurrent.Open - objCurrent.Close) / (.001M + objCurrent.High - objCurrent.Low) > 0.6M);
            return isLongBearish;
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

        public static List<PivotPoints> CalculatePivotPoints(IEnumerable<Binance.Candlestick> objCandlestickData)
        {

            List<PivotPoints> lsPivotPoints = new List<PivotPoints>();
            PivotPoints objPivotPoints = null;

            decimal dcAveragePrice = 0, dcDeMarkFactor = 0;
            foreach (Binance.Candlestick objCandlestick in objCandlestickData)
            {
                objPivotPoints = new PivotPoints();
                objPivotPoints.HighPrice = objCandlestick.High;
                objPivotPoints.LowPrice = objCandlestick.Low;
                objPivotPoints.ClosingPrice = objCandlestick.Close;
                objPivotPoints.OpeningPrice = objCandlestick.Open;

                dcAveragePrice = ((objPivotPoints.HighPrice + objPivotPoints.LowPrice + objPivotPoints.ClosingPrice) / 3);
                objPivotPoints.ClassicResistance1 = ((2 * dcAveragePrice) - objPivotPoints.LowPrice);
                objPivotPoints.ClassicSupport1 = ((2 * dcAveragePrice) - objPivotPoints.HighPrice);
                objPivotPoints.ClassicResistance2 = dcAveragePrice + (objPivotPoints.HighPrice - objPivotPoints.LowPrice);
                objPivotPoints.ClassicSupport2 = dcAveragePrice - (objPivotPoints.HighPrice - objPivotPoints.LowPrice);
                objPivotPoints.ClassicResistance3 = objPivotPoints.HighPrice + 2 * (dcAveragePrice - objPivotPoints.LowPrice);
                objPivotPoints.ClassicSupport3 = objPivotPoints.LowPrice - 2 * (objPivotPoints.HighPrice - dcAveragePrice);
                objPivotPoints.ClassicPivotPoint = dcAveragePrice;

                objPivotPoints.WoodiePivotPoint = (objPivotPoints.HighPrice + objPivotPoints.LowPrice + (2 * objPivotPoints.ClosingPrice)) / 4;
                objPivotPoints.WoodieResistance1 = (2 * objPivotPoints.WoodiePivotPoint) - objPivotPoints.LowPrice;
                objPivotPoints.WoodieSupport1 = (2 * objPivotPoints.WoodiePivotPoint) - objPivotPoints.HighPrice;
                objPivotPoints.WoodieResistance2 = objPivotPoints.WoodiePivotPoint + (objPivotPoints.HighPrice - objPivotPoints.LowPrice);
                objPivotPoints.WoodieSupport2 = objPivotPoints.WoodiePivotPoint - (objPivotPoints.HighPrice - objPivotPoints.LowPrice);

                objPivotPoints.CamarillaResistance1 = objPivotPoints.ClosingPrice + ((objPivotPoints.HighPrice - objPivotPoints.LowPrice) * Convert.ToDecimal(1.0833));
                objPivotPoints.CamarillaSupport1 = objPivotPoints.ClosingPrice - ((objPivotPoints.HighPrice - objPivotPoints.LowPrice) * Convert.ToDecimal(1.0833));
                objPivotPoints.CamarillaResistance2 = objPivotPoints.ClosingPrice + ((objPivotPoints.HighPrice - objPivotPoints.LowPrice) * Convert.ToDecimal(1.1666));
                objPivotPoints.CamarillaSupport2 = objPivotPoints.ClosingPrice - ((objPivotPoints.HighPrice - objPivotPoints.LowPrice) * Convert.ToDecimal(1.1666));
                objPivotPoints.CamarillaResistance3 = objPivotPoints.ClosingPrice + ((objPivotPoints.HighPrice - objPivotPoints.LowPrice) * Convert.ToDecimal(1.2500));
                objPivotPoints.CamarillaSupport3 = objPivotPoints.ClosingPrice - ((objPivotPoints.HighPrice - objPivotPoints.LowPrice) * Convert.ToDecimal(1.2500));
                objPivotPoints.CamarillaResistance4 = objPivotPoints.ClosingPrice + ((objPivotPoints.HighPrice - objPivotPoints.LowPrice) * Convert.ToDecimal(1.5000));
                objPivotPoints.CamarillaSupport4 = objPivotPoints.ClosingPrice - ((objPivotPoints.HighPrice - objPivotPoints.LowPrice) * Convert.ToDecimal(1.5000));

                dcDeMarkFactor = 0;
                if (objPivotPoints.ClosingPrice < objPivotPoints.OpeningPrice)
                {
                    dcDeMarkFactor = (objPivotPoints.HighPrice + (objPivotPoints.LowPrice * 2) + objPivotPoints.ClosingPrice);
                }
                else if (objPivotPoints.ClosingPrice > objPivotPoints.OpeningPrice)
                {
                    dcDeMarkFactor = ((objPivotPoints.HighPrice * 2) + objPivotPoints.LowPrice + objPivotPoints.ClosingPrice);
                }
                else if (objPivotPoints.ClosingPrice == objPivotPoints.OpeningPrice)
                {
                    dcDeMarkFactor = (objPivotPoints.HighPrice + objPivotPoints.LowPrice + (objPivotPoints.ClosingPrice * 2));
                }
                objPivotPoints.DeMarkResistance = (dcDeMarkFactor / 2) - objPivotPoints.LowPrice;
                objPivotPoints.DeMarkSupport = (dcDeMarkFactor / 2) - objPivotPoints.HighPrice;

                objPivotPoints.ChicagoFloorTradingResistance1 = (dcAveragePrice * 2) - objPivotPoints.LowPrice;
                objPivotPoints.ChicagoFloorTradingResistance2 = dcAveragePrice + (objPivotPoints.HighPrice - objPivotPoints.LowPrice);
                objPivotPoints.ChicagoFloorTradingSupport1 = (dcAveragePrice * 2) - objPivotPoints.HighPrice;
                objPivotPoints.ChicagoFloorTradingSupport2 = dcAveragePrice - (objPivotPoints.HighPrice - objPivotPoints.LowPrice);
                lsPivotPoints.Add(objPivotPoints);
            }
            return lsPivotPoints;
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

    public static class TelegramBot
    {
        private static TelegramBotClient Bot;

        public static async Task Main()
        {
            Bot = new TelegramBotClient("1736851685:AAEFobDNlvQ30B1hALpIFWzijPkYcvfcr8s");

            var me = await Bot.GetMeAsync();
            Console.Title = me.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();
            Bot.StopReceiving();
        }

        public static async Task SendTextMesageAsync(string sTextMessage)
        {
            var message = new Message();
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new KeyboardButton[][]
                {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                },
                resizeKeyboard: true
            );

            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: sTextMessage,
                replyMarkup: replyKeyboardMarkup

            );
        }
        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            switch (message.Text.Split(' ').First())
            {
                // Send inline keyboard
                case "/inline":
                    await SendInlineKeyboard(message);
                    break;

                // send custom keyboard
                case "/keyboard":
                    await SendReplyKeyboard(message, "Choose");
                    break;

                // send a photo
                case "/photo":
                    await SendDocument(message);
                    break;

                // request location or contact
                case "/request":
                    await RequestContactAndLocation(message);
                    break;

                default:
                    await Usage(message);
                    break;
            }

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            static async Task SendInlineKeyboard(Message message)
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                // Simulate longer running task
                await Task.Delay(500);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    }
                });
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Choose",
                    replyMarkup: inlineKeyboard
                );
            }

            static async Task SendReplyKeyboard(Message message, String sTextMessage)
            {
                if (message == null)
                {
                    message = new Message();
                }

                var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                    new KeyboardButton[][]
                    {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                    },
                    resizeKeyboard: true
                );

                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: sTextMessage,
                    replyMarkup: replyKeyboardMarkup

                );
            }

            static async Task SendDocument(Message message)
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                const string filePath = @"Files/tux.png";
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
                await Bot.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, fileName),
                    caption: "Nice Picture"
                );
            }

            static async Task RequestContactAndLocation(Message message)
            {
                var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Who or Where are you?",
                    replyMarkup: RequestReplyKeyboard
                );
            }

            static async Task Usage(Message message)
            {
                const string usage = "Usage:\n" +
                                        "/inline   - send inline keyboard\n" +
                                        "/keyboard - send custom keyboard\n" +
                                        "/photo    - send a photo\n" +
                                        "/request  - request location or contact";
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: usage,
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
        }

        // Process Inline Keyboard callback data
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await Bot.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}"
            );

            await Bot.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Received {callbackQuery.Data}"
            );
        }

        #region Inline Mode

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };
            await Bot.AnswerInlineQueryAsync(
                inlineQueryId: inlineQueryEventArgs.InlineQuery.Id,
                results: results,
                isPersonal: true,
                cacheTime: 0
            );
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        #endregion

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }
    }

}
