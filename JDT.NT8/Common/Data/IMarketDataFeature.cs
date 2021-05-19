// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 09-12-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 09-12-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8	
// ***********************************************************************
namespace JDT.NT8.Common.Data
{
    using NinjaTrader.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface IMarketDataFeature.
    /// </summary>
    public interface IMarketDataFeature
    {
        /// <summary>
        /// Called when [bar update].
        /// </summary>
        void OnBarUpdate();

        /// <summary>
        /// Handles the <see cref="E:MarketData" /> event.
        /// </summary>
        /// <param name="marketDataUpdate">The <see cref="MarketDataEventArgs"/> instance containing the event data.</param>
        void OnMarketData(MarketDataEventArgs marketDataUpdate);

        /// <summary>
        /// Handles the <see cref="E:MarketDepth" /> event.
        /// </summary>
        /// <param name="marketDepthUpdate">The <see cref="MarketDepthEventArgs"/> instance containing the event data.</param>
        void OnMarketDepth(MarketDepthEventArgs marketDepthUpdate);
    }
}
