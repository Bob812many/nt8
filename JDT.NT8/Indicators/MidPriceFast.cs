// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 08-22-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 09-12-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8
// ***********************************************************************
using JDT.NT8.Common.Data;
using JDT.NT8.Utils;
using NinjaTrader.Gui.Chart;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Serialization;

namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Class MidPriceFast. This class cannot be inherited.  Uses SharpDX custom rendering for max performance.
    /// Implements the <see cref="NinjaTrader.NinjaScript.Indicators.Indicator" />
    /// </summary>
    /// <seealso cref="NinjaTrader.NinjaScript.Indicators.Indicator" />
    public sealed class MidPriceFast : Indicator
    {
        #region Fields

        /// <summary>
        /// The last bar index
        /// </summary>
        private float lastBarSlotIndex;

        /// <summary>
        /// The line point context
        /// </summary>
        //private LinePointContext linePointContext;

        /// <summary>
        /// The mid brush
        /// </summary>
        private System.Windows.Media.SolidColorBrush midBrush;

        /// <summary>
        /// The mid line sharp dx brush
        /// </summary>
        private SharpDX.Direct2D1.Brush midLineSharpDxBrush;

        /// <summary>
        /// The session high price
        /// </summary>
        private double sessionHighPrice;

        /// <summary>
        /// The session iterator
        /// </summary>
        private SafeSessionIterator safeSessionIterator;

        /// <summary>
        /// The session line dictionary
        /// </summary>
        private SortedDictionary<long, List<Vector2>> sessionLineDictionary;

        /// <summary>
        /// The session low price
        /// </summary>
        private double sessionLowPrice;

        /// <summary>
        /// The session mid price
        /// </summary>
        private double sessionMidPrice;

        /// <summary>
        /// The stroke width
        /// </summary>
        private float strokeWidth;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The display name.
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
        /// Gets or sets the color of the mid brush.
        /// </summary>
        /// <value>The color of the mid.</value>
        [XmlIgnore]
        [Display(Name = "Mid Brush Color", GroupName = "Parameters", Order = 0, Description = "The mid plot color.")]
        public System.Windows.Media.SolidColorBrush MidBrush
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
        /// Mid Price value series exposed for other Indicators/Strategies.
        /// </summary>
        /// <value>The mid price.</value>
        [Browsable(false)] // we don't need to see this in the NT Indicator grid
        public Series<double> MidPriceSeries
        {
            get
            {
                return this.Values[0];
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
        /// Called when [render target changed].
        /// </summary>
        public override void OnRenderTargetChanged()
        {
            base.OnRenderTargetChanged();

            if (this.midLineSharpDxBrush != null)
            {
                this.midLineSharpDxBrush.Dispose();
            }

            // make sure our render target is valid before trying to (re)create the SharpDX brush
            if (base.RenderTarget != null)
            {
                this.midLineSharpDxBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget,
                    DrawingExtensions.ToColor4(this.MidBrush));
            }
        }

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
            this.sessionMidPrice = (this.sessionHighPrice + this.sessionLowPrice) / 2.0;

            // make sure we are ready to draw our render objects
            if (base.ChartControl != null)
            {
                // use fast initializer Zero for SharpDX.Vector2 struct then set its properties
                SharpDX.Vector2 vertex = SharpDX.Vector2.Zero;

                // get latest X in bar index slot, we'll convert in OnRender to chart coordinates
                vertex.X = Convert.ToSingle(base.ChartControl.GetSlotIndexByTime(Time[0]));

                // get latest Y info and we'll convert it to chart coordinates in OnRender
                vertex.Y = Convert.ToSingle(this.sessionMidPrice);

                if (this.lastBarSlotIndex < vertex.X)
                {
                    this.sessionLineDictionary[this.safeSessionIterator.ActualSessionBegin.Ticks].Add(vertex);
                    this.lastBarSlotIndex = vertex.X;
                }
            }

            // REQUIRED: necessary to assign any value so strategies using this indicator the
            // OnBarUpdate will be called in this indicator.
            this.MidPriceSeries[0] = this.sessionMidPrice;
        }

        /// <summary>
        /// Let's use the super fast SharpDX to render this indicator with SharpDX.
        /// </summary>
        /// <param name="chartControl">The chart control.</param>
        /// <param name="chartScale">The chart scale.</param>
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            // iterating over an array is MUCH fast than a list so let's pay the penalty for conversion once
            KeyValuePair<long, List<Vector2>>[] sessionArray = this.sessionLineDictionary.ToArray();
            for (int i = 0; i < sessionArray.Length; i++) // sessions collection
            {
                Vector2[] vertexArray = sessionArray[i].Value.ToArray();
                for (int j = 1; j < vertexArray.Length; j++) // lines collection
                {
                        // create the vertices for this line segment
                        SharpDX.Vector2 vertexFrom = SharpDX.Vector2.Zero;
                        vertexFrom.X = Convert.ToSingle(chartControl.GetXByBarIndex(base.ChartBars, (int)vertexArray[j - 1].X));
                        vertexFrom.Y = Convert.ToSingle(chartScale.GetYByValue(vertexArray[j - 1].Y));

                        SharpDX.Vector2 vertexTo = SharpDX.Vector2.Zero;
                        vertexTo.X = Convert.ToSingle(chartControl.GetXByBarIndex(base.ChartBars, (int)vertexArray[j].X));
                        vertexTo.Y = Convert.ToSingle(chartScale.GetYByValue(vertexArray[j].Y));

                        // check that the vertex index is in view (no need to render off screen objects)
                        // we're assuming we're using the primary bars, otherwise you'll need the chartControl.BarsArray
                        if (vertexArray[j].X >= chartControl.PrimaryBars.FromIndex && vertexArray[j].X <= chartControl.PrimaryBars.ToIndex)
                        {
                            base.RenderTarget.DrawLine(vertexFrom, vertexTo, this.midLineSharpDxBrush, this.strokeWidth);
                        }
                }
            }
        }

        /// <summary>
        /// Called when state changes.
        /// </summary>
        protected override void OnStateChange()
        {
            base.OnStateChange();

            switch (base.State)
            {
                case State.SetDefaults:
                    base.Name = $"JDT_{this.GetType().Name}";
                    base.Description = @"Tracks the mid price of the current session. Uses SharpDX for fast rendering.";

                    base.Calculate = Calculate.OnEachTick;
                    base.IsSuspendedWhileInactive = true;
                    base.IsAutoScale = true;
                    base.IsOverlay = true;

                    // Coerce NT to display in combination with ShowTransparentPlotsInDataBox
                    base.DisplayInDataBox = true;

                    // REQUIRED: setting this to false and this indicator used by a strategy, this indicator's OnBarUpdate will not fire properly
                    //base.IsVisible = true;

                    // we can start plotting right away wit this indicator
                    base.BarsRequiredToPlot = 0;

                    this.midBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DodgerBlue);
                    this.strokeWidth = 3.0f;
                    this.lastBarSlotIndex = -1;

                    break;

                case State.Configure:
                    // REQUIRED: necessary to add at least 1 plot so strategies using this indicator
                    // the OnBarUpdate will be called in this indicator.

                    // We still need to add this plot for sync but we'll make it transparent and use SharpDX for super fast rendering
                    base.AddPlot(System.Windows.Media.Brushes.Transparent, "midPlot");

                    if (base.DisplayInDataBox)
                    {
                        // since we use a transparent "midPlot", we have to use this property for DataBox
                        base.ShowTransparentPlotsInDataBox = true;
                    }
                    break;

                case State.Active:
                    break;

                case State.DataLoaded:
                    // we'll use the session time (binary) as the key in the dictionary for super fast lookup
                    this.sessionLineDictionary = new SortedDictionary<long, List<Vector2>>();

                    if (this.Bars != null)
                    {
                        this.safeSessionIterator = new SafeSessionIterator(this, this.ResetMidVariables);
                    }

                    break;

                case State.Historical:
                    break;

                case State.Transition:
                    break;

                case State.Realtime:
                    break;

                case State.Terminated:
                    // we are responsible for disposing unmanaged object per NT8 documentation
                    if (this.midLineSharpDxBrush != null)
                    {
                        this.midLineSharpDxBrush.Dispose();
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Resets the mid variables.
        /// </summary>
        private void ResetMidVariables()
        {
            this.sessionMidPrice = 0.0;
            this.sessionHighPrice = double.MinValue; // we expect any value bigger is valid
            this.sessionLowPrice = double.MaxValue; // we expect any value smaller is valid

            if (!this.sessionLineDictionary.ContainsKey(this.safeSessionIterator.ActualSessionBegin.Ticks))
            {
                this.sessionLineDictionary.Add(this.safeSessionIterator.ActualSessionBegin.Ticks, new List<Vector2>());
            }
        }

        #endregion Methods

        #region Structs

        /// <summary>
        /// Struct LinePointContext.  Consider moving out for reusing in other classes.
        /// </summary>
        //internal struct LinePointContext
        //{
        //    #region Fields

        //    /// <summary>
        //    /// The empty struct initalizer.
        //    /// </summary>
        //    public static readonly LinePointContext Empty = default(LinePointContext);

        //    /// <summary>
        //    /// The vertex
        //    /// </summary>
        //    public List<SharpDX.Vector2> VertexList;

        //    #endregion Fields
        //}

        #endregion Structs
    }
}