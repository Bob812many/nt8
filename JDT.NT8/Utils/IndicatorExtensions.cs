// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 08-21-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 09-12-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8	
// ***********************************************************************
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using System;
using System.Collections.Generic;

namespace JDT.NT8.Utils
{
    /// <summary>
    /// Helper class for common indicator functionality.
    /// </summary>
    public static class IndicatorExtensions
    {
        /// <summary>
        /// Checks the is holiday.
        /// </summary>
        /// <param name="ninjaScript">The ninja script.</param>
        /// <param name="datetime">The date.</param>
        /// <returns><c>true</c> if is a holiday or partial holiday, <c>false</c> otherwise.</returns>
        public static bool CheckIsHoliday(NinjaScriptBase ninjaScript, DateTime datetime)
        {
            if (ninjaScript.TradingHours != null)
            {
                foreach (KeyValuePair<DateTime, PartialHoliday> partialHoliday in ninjaScript.TradingHours.PartialHolidays)
                {
                    if (datetime.Date == partialHoliday.Value.Date)
                    {
                        return true;
                    }
                }

                foreach (KeyValuePair<DateTime, string> holiday in ninjaScript.TradingHours.Holidays)
                {
                    if (datetime.Date == holiday.Key.Date)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // TODO: Test and implement
        // /// <summary>
        // /// Tries to get the trading hours.
        // /// </summary>
        // /// <param name="tradingHoursName">Name of the trading hours.</param>
        // /// <param name="tradingHours">The trading hours.</param>
        // /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        // public static bool TryGetTradingHours(string tradingHoursName, out TradingHours tradingHours)
        // {
        //     tradingHours = null;
        //     var tradingHoursArray = TradingHours.All.ToArray();

        //     for (int i = 0; i < tradingHoursArray.Length; i++)
        //     {
        //         if (string.Equals(tradingHoursArray[i].Name, tradingHoursName))
        //         {
        //             tradingHours = tradingHoursArray[i];
        //             return true;
        //         }
        //     }

        //     return false;
        // }
    }
}
