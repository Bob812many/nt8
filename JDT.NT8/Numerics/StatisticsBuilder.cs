// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 08-28-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 08-28-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8	
// ***********************************************************************
namespace JDT.NT8.Numerics
{
    using NinjaTrader.Cbi;
    using NinjaTrader.Data;
    using NinjaTrader.NinjaScript;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class StatisticsBuilder.
    /// </summary>
    public static class StatisticsBuilder
    {
        #region Methods

        /// <summary>
        /// Tries to get the historical price statistics.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="fromTime">From time.</param>
        /// <param name="toTime">To time.</param>
        /// <param name="useHighResolution">if set to <c>true</c> [use high resolution].</param>
        /// <param name="statistics">The statistics.</param>
        /// <param name="tradingHours">The trading hours (optional).</param>
        /// <returns><c>true</c> if statistics are ready, <c>false</c> otherwise.</returns>
        public static void GetHistoricalPriceStatistics(NinjaScriptBase parent, DateTime fromTime, DateTime toTime, bool useHighResolution,
            out PriceStatistics statistics, TradingHours tradingHours = null)
        {
            BarsRequest request = new BarsRequest(parent.Instrument, fromTime, toTime);

            if (tradingHours != null)
            {
                request.TradingHours = tradingHours;
            }
            else
            {
                request.TradingHours = parent.TradingHours;
            }

            if (useHighResolution)
            {
                request.BarsPeriod.BarsPeriodType = BarsPeriodType.Tick;
                request.BarsPeriod.BaseBarsPeriodType = BarsPeriodType.Tick;
                request.BarsPeriod.MarketDataType = MarketDataType.Last;
            }

            statistics = new PriceStatistics(request);
            NinjaTrader.Core.Globals.RandomDispatcher.InvokeAsync(statistics.MakeRequest);
        }

        /// <summary>
        /// Tries to get the historical volume statistics.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="fromTime">From time.</param>
        /// <param name="toTime">To time.</param>
        /// <param name="useHighResolution">if set to <c>true</c> [use high resolution].</param>
        /// <param name="statistics">The statistics.</param>
        /// <param name="tradingHours">The trading hours.</param>
        public static void GetHistoricalVolumeStatistics(NinjaScriptBase parent, DateTime fromTime, DateTime toTime, bool useHighResolution,
            out VolumeStatistics statistics, TradingHours tradingHours = null)
        {
            BarsRequest request = new BarsRequest(parent.Instrument, fromTime, toTime);

            if (tradingHours != null)
            {
                request.TradingHours = tradingHours;
            }
            else
            {
                request.TradingHours = parent.TradingHours;
            }

            if (useHighResolution)
            {
                request.BarsPeriod.BarsPeriodType = BarsPeriodType.Tick;
                request.BarsPeriod.BaseBarsPeriodType = BarsPeriodType.Tick;
                request.BarsPeriod.MarketDataType = MarketDataType.Last;
            }

            statistics = new VolumeStatistics(request);
            NinjaTrader.Core.Globals.RandomDispatcher.InvokeAsync(statistics.MakeRequest);
        }

        #endregion Methods
    }
}
