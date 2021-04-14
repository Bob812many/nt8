// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 08-24-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 09-12-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8	
// ***********************************************************************
namespace NinjaTrader.NinjaScript.Indicators
{
    using JDT.NT8.Common.Data;
    using JDT.NT8.Utils;
    using NinjaTrader.Data;
    using System;

    public sealed class DemoSessionIterator : Indicator
    {
        #region Fields

        /// <summary>
        /// The session iterator
        /// </summary>
        private SafeSessionIterator safeSessionIterator;

        #endregion Fields

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

            // safeSessionIterator: for more information, see https://www.youtube.com/watch?v=cUE57WHQzJY
            if (this.safeSessionIterator != null)
            {
                this.safeSessionIterator.OnBarUpdate();

                if (!this.safeSessionIterator.InSession)
                {
                    return;
                }
            }
            else
            {
                // our session iterator wasn't initialized properly
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

            // let's make sure we're in session
            if (this.safeSessionIterator != null)
            {
                this.safeSessionIterator.OnMarketData(marketDataUpdate);

                if (!this.safeSessionIterator.InSession)
                {
                    return;
                }
            }

            // perform CPU extensive operations since we're in session
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
                    base.Name = $"JDT_{this.GetType().Name}";
                    base.Description = @"Simple indicator template to demo SessionIterator.";

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
#if DEBUG
                    System.Diagnostics.Debugger.Launch();
#endif
                    // initialize any Series<T>(this) for synchronization with price

                    if (this.Bars != null)
                    {
                        this.safeSessionIterator = new SafeSessionIterator(this, this.ResetCallback);
                        ClearOutputWindow();
                    }
                    else
                    {
                        // Heads up in the NinjaTrader Control Center that something went wrong.
                        NinjaScript.Log($"Bars object not populated in {base.State}.", Cbi.LogLevel.Error);
                    }

                    break;

                case State.Historical:
                    break;

                case State.Transition:
                    break;

                case State.Realtime:
                    break;

                case State.Terminated:
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// The reset callback.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void ResetCallback()
        {
            // let's print the new session info to the Output tab
            // https://ninjatrader.com/support/helpGuides/nt8/?output.htm
            base.Print($"Session begin: {safeSessionIterator.ActualSessionBegin}, end: {safeSessionIterator.ActualSessionEnd}");
        }

        #endregion Methods
    }
}