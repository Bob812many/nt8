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
    using NinjaTrader.NinjaScript;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class SafeSessionIterator. This class cannot be inherited.
    /// Implements the <see cref="NinjaTrader.Data.SessionIterator" />
    /// Implements the <see cref="JDT.NT8.Common.Data.IMarketDataFeature" />
    /// This class is a wrapper for the <see cref="NinjaTrader.Data.SessionIterator" /> with the added
    /// in session checks.  With certain bar types, NT8 internally calls <see cref="NinjaTrader.Data.Bars.RemoveLastBar" />
    /// causing some unpredictable behavior with <see cref="NinjaTrader.NinjaScript.NinjaScriptBase.IsFirstTickOfBar" />
    /// and <see cref="NinjaTrader.Data.Bars.IsFirstBarOfSession" />.  This class uses and internal <see cref="InSession" />
    /// to properly account for these variations.  For a video on this, see <see  href="https://www.youtube.com/watch?v=cUE57WHQzJY" />
    /// </summary>
    /// <seealso cref="NinjaTrader.Data.SessionIterator" />
    /// <seealso cref="JDT.NT8.Common.Data.IMarketDataFeature" />
    public sealed class SafeSessionIterator : SessionIterator, IMarketDataFeature
    {
        #region Fields

        /// <summary>
        /// The callback
        /// </summary>
        private readonly Action callback;

        /// <summary>
        /// The parent indicator or strategy.
        /// </summary>
        private readonly NinjaScriptBase parent;

        /// <summary>
        /// The in session
        /// </summary>
        private bool inSession;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeSessionIterator"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="bars">The bars.</param>
        /// <param name="callback">The callback.</param>
        public SafeSessionIterator(NinjaScriptBase parent, Action callback = null)
            : base(parent.Bars)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            
            if (parent.Bars == null)
            {
                throw new ArgumentNullException(nameof(parent.Bars));
            }

            this.parent = parent;
            this.callback = callback;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating whether [in session].
        /// </summary>
        /// <value><c>true</c> if [in session]; otherwise, <c>false</c>.</value>
        public bool InSession
        {
            get
            {
                return this.inSession;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Called when [bar update].
        /// </summary>
        public void OnBarUpdate()
        {
            // check inSession due to certain bar types using remove last bar when building
            if (!this.inSession)
            {
                if (this.parent.IsFirstTickOfBar && this.parent.Bars.IsFirstBarOfSession)
                {
                    if (this.TryGetNextSession(this.parent.Time[0], true))
                    {
                        return;
                    }
                }
            }

            if (this.inSession && (this.parent.Time[0] > base.ActualSessionEnd || this.parent.Time[0] < base.ActualSessionBegin))
            {
                this.inSession = false;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MarketData" /> event.
        /// </summary>
        /// <param name="marketDataUpdate">The <see cref="MarketDataEventArgs"/> instance containing the event data.</param>
        public void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {
            if (!this.inSession)
            {
                if (this.parent.IsFirstTickOfBar && this.parent.Bars.IsFirstBarOfSession)
                {
                    if (this.TryGetNextSession(marketDataUpdate.Time, true))
                    {
                        return;
                    }
                }
            }

            if (this.inSession && (marketDataUpdate.Time > base.ActualSessionEnd || marketDataUpdate.Time < base.ActualSessionBegin))
            {
                this.inSession = false;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MarketDepth" /> event.
        /// </summary>
        /// <param name="marketDepthUpdate">The <see cref="MarketDepthEventArgs"/> instance containing the event data.</param>
        public void OnMarketDepth(MarketDepthEventArgs marketDepthUpdate)
        {
            if (!this.inSession)
            {
                if (this.parent.IsFirstTickOfBar && this.parent.Bars.IsFirstBarOfSession)
                {
                    if (this.TryGetNextSession(marketDepthUpdate.Time, true))
                    {
                        return;
                    }
                }
            }

            if (this.inSession && (marketDepthUpdate.Time > base.ActualSessionEnd || marketDepthUpdate.Time < base.ActualSessionBegin))
            {
                this.inSession = false;
            }
        }

        /// <summary>
        /// Tries to get next session.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="includeEndTimestamp">if set to <c>true</c> [include end timestamp].</param>
        /// <returns><c>true</c> if able to get the next session, <c>false</c> otherwise.</returns>
        public bool TryGetNextSession(DateTime time, bool includeEndTimestamp)
        {
            bool success = false;

            success = base.GetNextSession(time, includeEndTimestamp);

            // this is typically a reset callback but could be anything the consuming class needs
            if (this.callback != null && success)
            {
                this.inSession = true;

                this.callback.Invoke();
            }

            return success;
        }

        #endregion Methods
    }
}
