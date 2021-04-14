// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 08-24-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 04-13-2021
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8	
// ***********************************************************************
namespace NinjaTrader.NinjaScript.Indicators
{
    using JDT.NT8.Common.Data;
    using JDT.NT8.Numerics;
    using JDT.NT8.Utils;
    using NinjaTrader.Data;
    using System;
    using System.ComponentModel.DataAnnotations;

    public sealed class DemoStatisticsBuilder : Indicator
    {
        #region Fields

        /// <summary>
        /// The days lookback period.
        /// </summary>
        private int daysLookback;

        /// <summary>
        /// The price statistics
        /// </summary>
        private PriceStatistics priceStatistics;

        /// <summary>
        /// The session iterator
        /// </summary>
        private SafeSessionIterator safeSessionIterator;

        /// <summary>
        /// Use high resolution
        /// </summary>
        private bool useHighResolution;

        /// <summary>
        /// The volume statistics
        /// </summary>
        private VolumeStatistics volumeStatistics;
        #endregion Fields

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Days to look back", GroupName = "Parameters", Order = 0, Description = "The number of historical days of statistics.")]
        public int DaysLookback
        {
            get
            {
                return this.daysLookback;
            }
            set
            {
                this.daysLookback = value;
            }
        }

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

        /// <summary>
        /// Use high resolution
        /// </summary>
        [NinjaScriptProperty]
        [Display(Name = "Use High Resolution", GroupName = "Parameters", Order = 0, Description = "Whether to use high resolution historical data (data intensive!).")]
        public bool UseHighResolution
        {
            get
            {
                return this.useHighResolution;
            }
            set
            {
                this.useHighResolution = value;
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

            if (base.CurrentBar < base.BarsRequiredToPlot)
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

            // check if price stats are ready
            if (this.priceStatistics.IsCalculated)
            {
                // do logic which uses the price statistics
            }

            // chec if volue stats are ready
            if (this.volumeStatistics.IsCalculated)
            {
                // do logic which uses the volume statistics
            }

            // REQUIRED: necessary to assign any value so strategies using this indicator the
            // OnBarUpdate will be called in this indicator.
            base.Value[0] = double.NaN;
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

                    base.Calculate = Calculate.OnBarClose;
                    base.IsSuspendedWhileInactive = true;
                    base.IsAutoScale = true;
                    base.IsOverlay = true;

                    // REQUIRED: setting this to false and this indicator used by a strategy, this indicator's OnBarUpdate will not fire properly
                    //base.IsVisible = true;

                    base.BarsRequiredToPlot = 0;

                    this.useHighResolution = false;
                    this.daysLookback = 7;

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

                    if (this.Bars != null)
                    {
                        this.safeSessionIterator = new SafeSessionIterator(this, this.GetSessionStatisticsCallback);
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
//#if DEBUG
//                    System.Diagnostics.Debugger.Launch();
//#endif
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

        /// <summary>
        /// The session statistics callback.
        /// </summary>
        private void GetSessionStatisticsCallback()
        {
            // let's look back 7 days from the beginning of this session
            DateTime fromDateTime = this.safeSessionIterator.ActualSessionBegin.Subtract(TimeSpan.FromDays(this.daysLookback));

            // both of these stats take some time so get them started then check later if they're ready
            StatisticsBuilder.GetHistoricalPriceStatistics(this, fromDateTime, this.safeSessionIterator.ActualSessionBegin, this.useHighResolution, out this.priceStatistics);
            StatisticsBuilder.GetHistoricalVolumeStatistics(this, fromDateTime, this.safeSessionIterator.ActualSessionEnd, this.useHighResolution, out this.volumeStatistics);

            // let's print the new session info to the Output tab
            // https://ninjatrader.com/support/helpGuides/nt8/?output.htm
            base.Print($"Session begin: {this.safeSessionIterator.ActualSessionBegin}, end: {this.safeSessionIterator.ActualSessionEnd}");

        }

        #endregion Methods
    }
}