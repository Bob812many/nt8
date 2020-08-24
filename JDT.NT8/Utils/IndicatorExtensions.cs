// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 08-21-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 08-21-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8	
// ***********************************************************************
namespace JDT.NT8.Utils
{
    using NinjaTrader.Data;
    using NinjaTrader.NinjaScript;
    using System;
    using System.Collections.Generic;

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


        /// <summary>
        /// Determines whether the time is in session.
        /// </summary>
        /// <param name="sessionIterator">The session iterator.</param>
        /// <param name="time">The time to check.</param>
        /// <returns><c>true</c> if is in session; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsInSession(SessionIterator sessionIterator, DateTime time)
        {
            bool isInSession = false;

            if (sessionIterator == null)
            {
                throw new ArgumentNullException($"{nameof(sessionIterator)} cannot be null.");
            }

            if (time >= sessionIterator.ActualSessionBegin && time <= sessionIterator.ActualSessionEnd)
            {
                isInSession = true;
            }

            return isInSession;
        }
    }
}
