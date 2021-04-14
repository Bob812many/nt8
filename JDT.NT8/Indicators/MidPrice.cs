// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 08-28-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 09-12-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8
// ***********************************************************************
using JDT.NT8.Common.Data;
using NinjaTrader.Gui;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Class MidPrice. This class cannot be inherited.
    /// Implements the <see cref="NinjaTrader.NinjaScript.Indicators.Indicator" />
    /// </summary>
    /// <seealso cref="NinjaTrader.NinjaScript.Indicators.Indicator" />
    public sealed class MidPrice : Indicator
    {
        #region Fields

        /// <summary>
        /// The mid brush
        /// </summary>
        private System.Windows.Media.SolidColorBrush midBrush;

        /// <summary>
        /// The mid price
        /// </summary>
        private double midPrice;

        /// <summary>
        /// The session high price
        /// </summary>
        private double sessionHighPrice = double.MinValue;

        /// <summary>
        /// The session iterator
        /// </summary>
        private SafeSessionIterator safeSessionIterator;

        /// <summary>
        /// The session low price
        /// </summary>
        private double sessionLowPrice = double.MaxValue;

        /// <summary>
        /// The stroke width
        /// </summary>
        private float strokeWidth;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the color of the mid.
        /// </summary>
        /// <value>The color of the mid.</value>
        [XmlIgnore]
        [Display(Name = "Mid Color", GroupName = "Parameters", Order = 0, Description = "The mid plot color.")]
        public System.Windows.Media.SolidColorBrush MidColor
        {
            get
            {
                return this.midBrush;
            }
            set
            {
                this.midBrush = value;
            }
        }

        /// <summary>
        /// Gets the mid price series.
        /// </summary>
        /// <value>The mid price series.</value>
        [Browsable(false)]
        public Series<double> MidPriceSeries
        {
            get
            {
                return base.Value;
            }
        }

        /// <summary>
        /// The stroke width of the plotted line.
        /// </summary>
        /// <value>The width of the stroke.</value>
        [NinjaScriptProperty]
        [Range(1.0, float.MaxValue)]
        [Display(Name = "Stroke Width", GroupName = "Parameters", Order = 0, Description = "The mid plot stroke width.  Min is 1.")]
        public float StrokeWidth
        {
            get
            {
                return this.strokeWidth;
            }
            set
            {
                this.strokeWidth = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Called when bar updates.
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

            // track session high and low prices
            if (base.High[0] > this.sessionHighPrice)
            {
                this.sessionHighPrice = base.High[0];
            }

            if (base.Low[0] < this.sessionLowPrice)
            {
                this.sessionLowPrice = base.Low[0];
            }

            // calculate session mid price
            this.midPrice = (this.sessionHighPrice + this.sessionLowPrice) / 2.0;

            this.MidPriceSeries[0] = this.midPrice;
        }

        /// <summary>
        /// Called when [state change].
        /// </summary>
        protected override void OnStateChange()
        {
            base.OnStateChange();

            switch (base.State)
            {
                case State.SetDefaults:
                    base.Name = $"JDT_{this.GetType().Name}";
                    base.Description = @"Tracks the mid price of the current session.";

                    base.Calculate = Calculate.OnEachTick;
                    base.IsSuspendedWhileInactive = true;
                    base.IsAutoScale = true;
                    base.IsOverlay = true;

                    base.DisplayInDataBox = true;

                    // REQUIRED: setting this to false and this indicator used by a strategy, this indicator's OnBarUpdate will not fire properly
                    //base.IsVisible = true;

                    // we can start plotting right away wit this indicator
                    base.BarsRequiredToPlot = 0;

                    this.midBrush = System.Windows.Media.Brushes.DodgerBlue;
                    this.strokeWidth = 3.0f;

                    break;

                case State.Configure:
                    // REQUIRED: necessary to add at least 1 plot so strategies using this indicator
                    // the OnBarUpdate will be called in this indicator.

                    var stroke = new Stroke(this.midBrush, DashStyleHelper.Solid, this.strokeWidth);

                    // We still need to add this plot for sync
                    base.AddPlot(stroke, PlotStyle.Line, "midPlot");

                    break;

                case State.Active:
                    break;

                case State.DataLoaded:
                    if (this.Bars != null)
                    {
                        this.safeSessionIterator = new SafeSessionIterator(this, this.ResetVariables);
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

        private void ResetVariables()
        {
            this.sessionHighPrice = double.MinValue;
            this.sessionLowPrice = double.MaxValue;
        }

        #endregion Methods
    }
}