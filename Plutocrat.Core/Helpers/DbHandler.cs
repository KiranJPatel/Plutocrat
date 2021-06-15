using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading;

namespace Plutocrat.Core.Helpers
{
    class DbHandler
    {
        private static DbHandler mobjInstance;
        public SQLiteConnection mobjSqlConnection;
        public string msConnectionString = string.Empty;
        public static bool mbApplicationArchitectureMismatch = false;
        public static DbHandler Instance
        {
            get
            {
                if (mobjInstance == null)
                {
                    mobjInstance = new DbHandler();
                }

                return mobjInstance;
            }
        }

        public bool SetDBConnection()
        {
            return SetDBConnection(ref mobjSqlConnection);
        }
        public bool SetDBConnection(ref SQLiteConnection objConnection)
        {
            try
            {
                if (objConnection == null || objConnection.State != ConnectionState.Open)
                {
                    objConnection = new SQLiteConnection(msConnectionString);
                    objConnection.Open();
                }
            }
            catch (Exception e)
            {
                //Check for exception message when 32 bit installer is run on 64 bit machine
                if (e.Message.StartsWith("An attempt was made to load a program with an incorrect format", StringComparison.InvariantCultureIgnoreCase))
                {
                    mbApplicationArchitectureMismatch = true;
                }

                Console.WriteLine("DB Error:" + e.Message);

                return false;
            }
            return true;
        }

        public bool OpenConnection()
        {
            var currentDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            string projectDirectory = currentDirectory.Parent.Parent.Parent.Parent.FullName;

            string sDbRootPath = projectDirectory + @"\Plutocrat.Core\DB\";
            string msDbFullPath = sDbRootPath + "Plutocrat.db";
            Console.WriteLine("Connecting to DB:" + msDbFullPath);
            msConnectionString = "Data Source=" + msDbFullPath + ";DateTimeFormat=Ticks;Compress=True;Pooling=True;UseUTF16Encoding=True;Synchronous=NORMAL;journal mode=WAL;page_size=4096;cache_size=10000;locking_mode=EXCLUSIVE;temp_store=MEMORY;Version=3;";

            try
            {
                if (!System.IO.File.Exists(msDbFullPath))
                {
                    System.IO.Directory.CreateDirectory(sDbRootPath);
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        private bool ExecuteNonQuery(string sQuery)
        {
            SQLiteCommand objSqlCommand = null;
            try
            {
                if (!SetDBConnection())
                {
                    return false;
                }
                objSqlCommand = new SQLiteCommand(sQuery, mobjSqlConnection);
                objSqlCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SQL :(" + sQuery + " ) " + "SQL PARAMETERS :" + GetCommandParameteresValues(ref objSqlCommand) + "Error:" + e.Message);
                return false;
            }
        }
        public bool UpsertPricePrediction(string sPredictionTime, string sBase, string sSymbol, string sPredictionType, decimal dcCurrentPrice, decimal dcMovingAvgShort, decimal dcMovingAvgMed,
                                            decimal dcMovingAvgLong, decimal dcSupport1, decimal dcSupport2, decimal dcResistance1, decimal dcResistance2, double dbPriceSurge,
                                            double dbVolumeSurge, string sCandleSticks, string sHeikinAshiCandleSticks, double dbAroonUp, double dbAroonDown, double dbAroonOscillator,
                                            string sCrossOverType, string sLastCrossOverDate, string sLastCrossOverType, string sAroonUpDownTrendLast15Days, string sAroonUpTrendLast15Days,
                                            string sAroonDownTrendLast15Days, string sAroonOscTrendLast15Days, double dbCCI, string sCCITrend, double dbRSI, string sRSITrend,
                                            double dbKlingerVolumeOscillator, string sKVOTrend)
        {
            SQLiteCommand objCommand = null;
            string sQuery = string.Empty;
            try
            {
                if (!SetDBConnection())
                {
                    return false;
                }

                sQuery = "INSERT INTO PricePrediction(PredictionTime,Base,Symbol,PredictionType,CurrentPrice,MovingAvgShort,MovingAvgMed,MovingAvgLong,Support1,Support2," +
                                                    "Resistance1,Resistance2,PriceSurge,VolumeSurge,CandleSticks,HeikinAshiCandleSticks,AroonUp,AroonDown,AroonOscillator," +
                                                    "CrossOverType,LastCrossOverDate,LastCrossOverType,AroonUpDownTrendLast15Days,AroonUpTrendLast15Days," +
                                                    "AroonDownTrendLast15Days,AroonOscTrendLast15Days,CCI,CCITrend,RSI,RSITrend,KlingerVolumeOscillator,KVOTrend) " +
                                     "VALUES (@PredictionTime,@Base,@Symbol,@PredictionType,@CurrentPrice,@MovingAvgShort,@MovingAvgMed,@MovingAvgLong,@Support1,@Support2," +
                                                    "@Resistance1,@Resistance2,@PriceSurge,@VolumeSurge,@CandleSticks,@HeikinAshiCandleSticks,@AroonUp,@AroonDown,@AroonOscillator," +
                                                    "@CrossOverType,@LastCrossOverDate,@LastCrossOverType,@AroonUpDownTrendLast15Days,@AroonUpTrendLast15Days," +
                                                    "@AroonDownTrendLast15Days,@AroonOscTrendLast15Days,@CCI,@CCITrend,@RSI,@RSITrend,@KlingerVolumeOscillator,@KVOTrend)";
                objCommand = new SQLiteCommand(mobjSqlConnection);
                objCommand.CommandText = sQuery;
                objCommand.Parameters.Add("@PredictionTime", DbType.String).Value = sPredictionTime;
                objCommand.Parameters.Add("@Base", DbType.String).Value = sBase;
                objCommand.Parameters.Add("@Symbol", DbType.String).Value = sSymbol;
                objCommand.Parameters.Add("@PredictionType", DbType.String).Value = sPredictionType;
                objCommand.Parameters.Add("@CurrentPrice", DbType.Decimal).Value = dcCurrentPrice;
                objCommand.Parameters.Add("@MovingAvgShort", DbType.Decimal).Value = dcMovingAvgShort;
                objCommand.Parameters.Add("@MovingAvgMed", DbType.Decimal).Value = dcMovingAvgMed;
                objCommand.Parameters.Add("@MovingAvgLong", DbType.Decimal).Value = dcMovingAvgLong;
                objCommand.Parameters.Add("@Support1", DbType.Decimal).Value = dcSupport1;
                objCommand.Parameters.Add("@Support2", DbType.Decimal).Value = dcSupport2;
                objCommand.Parameters.Add("@Resistance1", DbType.Decimal).Value = dcResistance1;
                objCommand.Parameters.Add("@Resistance2", DbType.Decimal).Value = dcResistance2;
                objCommand.Parameters.Add("@PriceSurge", DbType.Decimal).Value = dbPriceSurge;
                objCommand.Parameters.Add("@VolumeSurge", DbType.Decimal).Value = dbPriceSurge;
                objCommand.Parameters.Add("@CandleSticks", DbType.String).Value = sCandleSticks;
                objCommand.Parameters.Add("@HeikinAshiCandleSticks", DbType.String).Value = sHeikinAshiCandleSticks;
                objCommand.Parameters.Add("@AroonUp", DbType.Decimal).Value = dbAroonUp;
                objCommand.Parameters.Add("@AroonDown", DbType.Decimal).Value = dbAroonDown;
                objCommand.Parameters.Add("@AroonOscillator", DbType.Decimal).Value = dbAroonOscillator;
                objCommand.Parameters.Add("@CrossOverType", DbType.String).Value = sCrossOverType;
                objCommand.Parameters.Add("@LastCrossOverDate", DbType.String).Value = sLastCrossOverDate;
                objCommand.Parameters.Add("@LastCrossOverType", DbType.String).Value = sLastCrossOverType;
                objCommand.Parameters.Add("@AroonUpDownTrendLast15Days", DbType.String).Value = sAroonUpDownTrendLast15Days;
                objCommand.Parameters.Add("@AroonUpTrendLast15Days", DbType.String).Value = sAroonUpTrendLast15Days;
                objCommand.Parameters.Add("@AroonDownTrendLast15Days", DbType.String).Value = sAroonDownTrendLast15Days;
                objCommand.Parameters.Add("@AroonOscTrendLast15Days", DbType.String).Value = sAroonOscTrendLast15Days;
                objCommand.Parameters.Add("@CCI", DbType.Decimal).Value = dbCCI;
                objCommand.Parameters.Add("@CCITrend", DbType.String).Value = sCCITrend;
                objCommand.Parameters.Add("@RSI", DbType.Decimal).Value = dbRSI;
                objCommand.Parameters.Add("@RSITrend", DbType.String).Value = sRSITrend;
                objCommand.Parameters.Add("@KlingerVolumeOscillator", DbType.Decimal).Value = dbKlingerVolumeOscillator;
                objCommand.Parameters.Add("@KVOTrend", DbType.String).Value = sKVOTrend;

                objCommand.ExecuteNonQuery();
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                Console.WriteLine("SQL :(" + sQuery + " ) " + "SQL PARAMETERS :" + GetCommandParameteresValues(ref objCommand) + "Error:" + ex.Message);
                return false;
            }
            finally
            {
                if (objCommand != null)
                {
                    objCommand.Dispose();
                }
            }
            return true;
        }

        #region Conversion and common functions
        public static string EvalNull(object objVal)
        {
            if (objVal == null)
            {
                return "";
            }
            else if (objVal == DBNull.Value)
            {
                return "";
            }

            return objVal.ToString().Trim();
        }

        public static int EvalInt(object objVal)
        {
            int iResult = 0;
            try
            {
                iResult = Convert.ToInt32(objVal);
            }
            catch (Exception)
            {
                return iResult;
            }
            return iResult;
        }

        public static long EvalLong(object objVal)
        {
            long lResult = 0;
            try
            {
                lResult = Convert.ToInt64(objVal);
            }
            catch (Exception)
            {
                return lResult;
            }
            return lResult;
        }

        public static double EvalDouble(object objVal)
        {
            double dbResult = 0;
            try
            {
                dbResult = Convert.ToDouble(objVal);
            }
            catch (Exception)
            {
                return dbResult;
            }
            return dbResult;
        }

        public static int EvalBoolToInt(bool bVal)
        {
            int iResult = 0;
            try
            {
                iResult = bVal ? 1 : 0;
            }
            catch (Exception)
            {
                return iResult;
            }
            return iResult;
        }

        public string GetCommandParameteresValues(ref SQLiteCommand cmd)
        {
            StringBuilder str = new StringBuilder();
            try
            {
                if ((cmd != null))
                {
                    foreach (SQLiteParameter cmdparam in cmd.Parameters)
                    {
                        str.Append("( " + EvalNull(cmdparam.ParameterName) + ":" + EvalNull(cmdparam.Value) + ",)");
                    }
                }

                return str.ToString();
            }
            catch
            {
                return str.ToString();
            }
        }
        #endregion
    }
}
