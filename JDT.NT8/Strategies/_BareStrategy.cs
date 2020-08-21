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
namespace NinjaTrader.NinjaScript.Strategies
{
    using NinjaTrader.NinjaScript;
    using NinjaTrader.NinjaScript.Indicators;
    using System;

    /// <summary>
    /// Class _BareStrategy.
    /// Implements the <see cref="NinjaTrader.NinjaScript.Strategies.Strategy" />
    /// </summary>
    /// <seealso cref="NinjaTrader.NinjaScript.Strategies.Strategy" />
    public class _BareStrategy : Strategy
    {
        #region Fields

        /// <summary>
        /// The touch indicator
        /// </summary>
        private _BareIndicator touchIndicator;

        /// <summary>
        /// The touch indicator value
        /// </summary>
        private double touchIndicatorValue;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Called when [bar update].
        /// </summary>
        protected override void OnBarUpdate()
        {
            base.OnBarUpdate();

            try
            {
                // this "touch mechanism" is required to have the indicator OnBarUpdate get called
                // after strategy OnBarUpdate
                this.touchIndicatorValue = this.touchIndicator.Value[0];
            }
            catch (Exception e)
            {
                string reason = e.Message;
            }
        }

        /// <summary>
        /// Called when [state change].
        /// </summary>
        protected override void OnStateChange()
        {
            base.OnStateChange();

            switch (base.State)
            {
                //case State.Undefined:
                //    break;
                case State.SetDefaults:
                    base.Name = $"{this.GetType().Name}";
                    base.Description = @"Template description.";

                    base.Calculate = Calculate.OnEachTick;
                    break;
                //case State.Configure:
                //    break;
                //case State.Active:
                //    break;
                case State.DataLoaded:

                    // NOTE: verified working mechanism which fires OnBarUpdate for the indicator
                    lock (this.NinjaScripts)
                    {
                        this.NinjaScripts.Add(this.touchIndicator = new _BareIndicator() { Parent = this, IsCreatedByStrategy = true });
                    }

                    // NOTE: this mechanism does not work properly in a 3rd party dll scenario.  Use above method which does work.
                    //base.AddChartIndicator(this.touchIndicator = new _BareIndicator());

                    break;
                    //case State.Historical:
                    //    break;
                    //case State.Transition:
                    //    break;
                    //case State.Realtime:
                    //    break;
                    //case State.Terminated:
                    //    break;
                    //case State.Finalized:
                    //    break;
                    //default:
                    //    break;
            }
        }

        #endregion Methods
    }
}