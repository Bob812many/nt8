// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 08-16-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 08-21-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8	
// ***********************************************************************
using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.NinjaScript.Indicators
{
    using NinjaTrader.Data;
    using System;
    using System.Linq;

    /// <summary>
    /// Class _BareIndicator. This class cannot be inherited.
    /// Implements the <see cref="NinjaTrader.NinjaScript.Indicators.Indicator" />
    /// </summary>
    /// <seealso cref="NinjaTrader.NinjaScript.Indicators.Indicator" />
    public sealed class _BareIndicator : Indicator
    {
        #region Properties

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public override string DisplayName
        {
            get
            {
                return base.Name;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Called when [bar update].
        /// </summary>
        protected override void OnBarUpdate()
        {
            base.OnBarUpdate();

            if (base.CurrentBars[0] < base.BarsRequiredToPlot)
            {
                return;
            }

            // REQUIRED: necessary to assign any value so strategies using this indicator the
            // OnBarUpdate will be called in this indicator.
            base.Value[0] = double.NaN;
        }

        /// <summary>
        /// Handles the <see cref="E:MarketData" /> event.
        /// </summary>
        /// <param name="marketDataUpdate">The <see cref="MarketDataEventArgs"/> instance containing the event data.</param>
        protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {
            base.OnMarketData(marketDataUpdate);
        }

        /// <summary>
        /// Called when [state change].
        /// </summary>
        protected override void OnStateChange()
        {
            base.OnStateChange();

            switch (base.State)
            {
                case State.Undefined:
                    break;

                case State.SetDefaults:
                    base.Name = $"{this.GetType().Name}";
                    base.Description = @"Template description.";

                    base.Calculate = Calculate.OnEachTick;
                    base.IsSuspendedWhileInactive = true;
                    base.IsAutoScale = true;
                    base.IsOverlay = true;

                    // REQUIRED: setting this to false and this indicator used by a strategy, this indicator's OnBarUpdate will not fire properly
                    //base.IsVisible = true;

                    base.BarsRequiredToPlot = 0;

                    break;

                case State.Configure:
                    // REQUIRED: necessary to add at least 1 plot so strategies using this indicator
                    // the OnBarUpdate will be called in this indicator.
                    base.AddPlot(System.Windows.Media.Brushes.Transparent, "touchPlot");

                    // add any data series necessary
                    //base.AddDataSeries(BarsPeriodType.Minute, 1);
                    break;

                case State.Active:
                    break;

                case State.DataLoaded:
                    // initialize any Series<T>(this) for synchronization with price
                    break;

                case State.Historical:
                    break;

                case State.Transition:
#if DEBUG
                    System.Diagnostics.Debugger.Launch();
#endif
                    break;

                case State.Realtime:
                    break;

                case State.Terminated:
                    break;

                case State.Finalized:
                    break;

                default:
                    break;
            }
        }

        #endregion Methods
    }
}