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
namespace NinjaTrader.NinjaScript.Indicators
{
    using JDT.NT8.Common.Data;
    using JDT.NT8.Utils;
    using NinjaTrader.Core;
    using NinjaTrader.Data;
    using NinjaTrader.Gui;
    using NinjaTrader.Gui.Chart;
    using SharpDX;
    using SharpDX.Direct2D1;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    /// <summary>
    /// Class InitialBalanceFast. This class cannot be inherited.  Uses SharpDX custom rendering for max performance.
    /// Implements the <see cref="NinjaTrader.NinjaScript.Indicators.Indicator" />
    /// </summary>
    /// <seealso cref="NinjaTrader.NinjaScript.Indicators.Indicator" />
    public sealed class InitialBalanceFast : Indicator
    {
        #region Fields

        /// <summary>
        /// The cached chart scale
        /// </summary>
        private ChartScale cachedChartScale;

        /// <summary>
        /// The cached render target
        /// </summary>
        private RenderTarget cachedRenderTarget;

        /// <summary>
        /// The highlight opacity
        /// </summary>
        private float highlightOpacity;

        /// <summary>
        /// The mid line sharp dx brush
        /// </summary>
        private SharpDX.Direct2D1.Brush ibLineSharpDxBrush;

        /// <summary>
        /// The ib shaded sharp dx brush
        /// </summary>
        private SharpDX.Direct2D1.Brush ibShadedSharpDxBrush;

        /// <summary>
        /// The initial balance time in minutes
        /// </summary>
        private int ibTimeInMinutes;

        /// <summary>
        /// The initial balance brush
        /// </summary>
        private System.Windows.Media.SolidColorBrush initialBalanceBrush;

        private bool isIBHighBroken;
        private bool isIBLowBroken;

        /// <summary>
        /// The is in session
        /// </summary>
        private bool isInSession;

        /// <summary>
        /// The last bar index
        /// </summary>
        private int lastBarIndex;

        /// <summary>
        /// The session context
        /// </summary>
        private SessionIBContext sessionContext;

        /// <summary>
        /// The session geometry list bottom
        /// </summary>
        List<Vector2> sessionGeometryListBottom;

        /// <summary>
        /// The session geometry list top
        /// </summary>
        List<Vector2> sessionGeometryListTop;

        /// <summary>
        /// The session high price
        /// </summary>
        private double sessionHighPrice;

        /// <summary>
        /// The session ib context
        /// </summary>
        private SessionIBContext sessionIBContext;

        /// <summary>
        /// The session IB dictionary
        /// </summary>
        private SortedDictionary<int, SessionIBContext> sessionIBDictionary;

        /// <summary>
        /// The session iterator
        /// </summary>
        private SafeSessionIterator safeSessionIterator;

        /// <summary>
        /// The session low price
        /// </summary>
        private double sessionLowPrice;

        /// <summary>
        /// The session vertex count
        /// </summary>
        private int sessionVertexCount;

        /// <summary>
        /// The shaded geometry bottom
        /// </summary>
        private List<List<Vector2>> shadedGeometryBottom = new List<List<Vector2>>();

        /// <summary>
        /// The shaded geometry top
        /// </summary>
        private List<List<Vector2>> shadedGeometryTop = new List<List<Vector2>>();

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

        [NinjaScriptProperty]
        [Range(0.0f, 1.0f)]
        [Display(Name = "Highlight Opacity", GroupName = "Parameters", Order = 0, Description = "The highlight opacity.")]
        public float HighlightOpacity
        {
            get
            {
                return this.highlightOpacity;
            }
            set
            {
                this.highlightOpacity = value;
            }
        }

        /// <summary>
        /// IB High Price value series exposed for other Indicators/Strategies.
        /// </summary>
        /// <value>The mid price.</value>
        [Browsable(false)] // we don't need to see this in the NT Indicator grid
        public Series<double> IBHighPriceSeries
        {
            get
            {
                return this.Values[0];
            }
        }

        /// <summary>
        /// IB Low Price value series exposed for other Indicators/Strategies.
        /// </summary>
        /// <value>The ib low price series.</value>
        [Browsable(false)] // we don't need to see this in the NT Indicator grid
        public Series<double> IBLowPriceSeries
        {
            get
            {
                return this.Values[1];
            }
        }

        /// <summary>
        /// Gets or sets the initial balance time in minutes.
        /// </summary>
        /// <value>The ib time in minutes.</value>
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Initial Balance Time", GroupName = "Parameters", Order = 0, Description = "The initial balance time in minutes.")]
        /// 
        public int IBTimeInMinutes
        {
            get
            {
                return this.ibTimeInMinutes;
            }
            set
            {
                this.ibTimeInMinutes = value;
            }
        }

        /// <summary>
        /// Gets or sets the color of the initial balance.
        /// </summary>
        /// <value>The color of the initial balance.</value>
        [XmlIgnore]
        [Display(Name = "Inital Balance Color", GroupName = "Parameters", Order = 0, Description = "The initial balance plot color.")]
        public System.Windows.Media.SolidColorBrush InitialBalanceBrush
        {
            get
            {
                return this.initialBalanceBrush;
            }
            set
            {
                this.initialBalanceBrush = value;
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
            this.cachedRenderTarget = base.RenderTarget;

            if (this.ibLineSharpDxBrush != null)
            {
                this.ibLineSharpDxBrush.Dispose();
            }

            if (this.ibShadedSharpDxBrush != null)
            {
                this.ibShadedSharpDxBrush.Dispose();
            }

            // make sure our render target is valid before trying to create the SharpDX brush
            if (this.cachedRenderTarget != null)
            {                
                this.ibLineSharpDxBrush = new SharpDX.Direct2D1.SolidColorBrush(this.cachedRenderTarget,
                   DrawingExtensions.ToColor4(this.InitialBalanceBrush));
            }
        }
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

            // track session high and low prices
            if (base.High[0] > this.sessionHighPrice)
            {
                this.sessionHighPrice = base.High[0];
            }

            if (base.Low[0] < this.sessionLowPrice)
            {
                this.sessionLowPrice = base.Low[0];
            }

            
            if (base.ChartControl != null && // make sure we are ready to draw our render objects
                base.Time[0] <= this.safeSessionIterator.ActualSessionBegin.AddMinutes(this.ibTimeInMinutes)) // within IB time
            {
                // use fast initializer Zero for SharpDX.Vector2 struct then simply set its properties
                // get latest Y info and we'll convert it to chart coordinates in OnRender
                SharpDX.Vector2 vertexHigh = SharpDX.Vector2.Zero;
                vertexHigh.X = Convert.ToSingle(base.CurrentBar);
                vertexHigh.Y = Convert.ToSingle(this.sessionHighPrice);

                SharpDX.Vector2 vertexLow = SharpDX.Vector2.Zero;
                vertexLow.X = vertexHigh.X;
                vertexLow.Y = Convert.ToSingle(this.sessionLowPrice);

                // add + update when it's a new bar
                if (base.CurrentBar > this.lastBarIndex)
                {
                    this.lastBarIndex = base.CurrentBar;

                    this.sessionContext.IBHigh = vertexHigh;
                    this.sessionContext.IBLow = vertexLow;

                    this.sessionIBDictionary[base.CurrentBar] = this.sessionContext;
                    this.sessionGeometryListTop.Add(vertexHigh);
                    this.sessionGeometryListBottom.Add(vertexLow);
                    this.sessionVertexCount++;
                }
                else if (base.CurrentBar == this.lastBarIndex) // update when same bar and values are different
                {
                    SessionIBContext context = this.sessionIBDictionary[base.CurrentBar];

                    if (this.sessionHighPrice > context.IBHigh.Y)
                    {
                        context.IBHigh.Y = Convert.ToSingle(this.sessionHighPrice);
                        this.sessionIBDictionary[base.CurrentBar] = context;

                        if (this.sessionVertexCount > 0)
                        {
                            this.sessionGeometryListTop[this.sessionGeometryListTop.Count - 1] = context.IBHigh;
                        }
                        else if (this.sessionVertexCount == 0)
                        {
                            this.sessionGeometryListTop.Add(context.IBHigh);
                        }
                    }

                    if (this.sessionLowPrice < context.IBLow.Y || context.IBLow.Y == 0.0f)
                    {
                        context.IBLow.Y = Convert.ToSingle(this.sessionLowPrice);
                        this.sessionIBDictionary[base.CurrentBar] = context;

                        if (this.sessionVertexCount > 0)
                        {
                            this.sessionGeometryListBottom[this.sessionGeometryListBottom.Count - 1] = context.IBLow;
                        }
                        else if (this.sessionVertexCount == 0)
                        {
                            this.sessionGeometryListBottom.Add(context.IBLow);
                        }
                    }
                }

                // REQUIRED: necessary to assign any value so strategies using this indicator the
                // OnBarUpdate will be called in this indicator.
                this.IBHighPriceSeries[0] = this.sessionHighPrice;
                this.IBLowPriceSeries[0] = this.sessionLowPrice;
            }
            else if (base.Time[0] > this.safeSessionIterator.ActualSessionBegin.AddMinutes(this.ibTimeInMinutes) && base.Time[0] <= this.safeSessionIterator.ActualSessionEnd)
            {
                //this.isInSession = false;

                // carry the IB values through until the end of the session
                this.IBHighPriceSeries[0] = this.IBHighPriceSeries[1];
                this.IBLowPriceSeries[0] = this.IBLowPriceSeries[1];

                //if (!this.isIBHighBroken)
                //{
                //    if (base.High[0] >= this.sessionHighPrice)
                //    {
                //        this.isIBHighBroken = true;
                //    }
                //}

                //if (!this.isIBLowBroken)
                //{
                //    if (base.Low[0] <= this.sessionLowPrice)
                //    {
                //        this.isIBLowBroken = true;
                //    }
                //}
            }
        }

        /// <summary>
        /// Let's use the super fast SharpDX to render this indicator.
        /// </summary>
        /// <param name="chartControl">The chart control.</param>
        /// <param name="chartScale">The chart scale.</param>
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            /*
             * topLeft -----------------> topRight
             *                               |
             *                               |
             *         SHADED AREA           |
             *         0---------1           |
             *                               V
             * bottomLeft <-------------- bottomRight
             * 0930                         1030 (60 minutes after open)
             */

            if (chartScale != null)
            {
                this.cachedChartScale = chartScale;
            }

            ChartControl cachedChartControl = chartControl;
            KeyValuePair<int, SessionIBContext>[] sessionArray = this.sessionIBDictionary.ToArray();

            try
            {
                // draw IB lines
                for (int i = 1; i < sessionArray.Length; i++)
                {
                    if (sessionArray[i].Key - sessionArray[i - 1].Key > 1)
                    {
                        continue;
                    }

                    SharpDX.Vector2 vertexHighTo = SharpDX.Vector2.Zero;
                    vertexHighTo.X = Convert.ToSingle(chartControl.GetXByBarIndex(base.ChartBars, (int)sessionArray[i].Value.IBHigh.X));
                    vertexHighTo.Y = Convert.ToSingle(this.cachedChartScale.GetYByValue(sessionArray[i].Value.IBHigh.Y));

                    SharpDX.Vector2 vertexHighFrom = SharpDX.Vector2.Zero;
                    vertexHighFrom.X = Convert.ToSingle(chartControl.GetXByBarIndex(base.ChartBars, (int)sessionArray[i - 1].Value.IBHigh.X));
                    vertexHighFrom.Y = Convert.ToSingle(this.cachedChartScale.GetYByValue(sessionArray[i - 1].Value.IBHigh.Y));


                    SharpDX.Vector2 vertexLowTo = SharpDX.Vector2.Zero;
                    vertexLowTo.X = Convert.ToSingle(chartControl.GetXByBarIndex(base.ChartBars, (int)sessionArray[i].Value.IBLow.X));
                    vertexLowTo.Y = Convert.ToSingle(this.cachedChartScale.GetYByValue(sessionArray[i].Value.IBLow.Y));

                    SharpDX.Vector2 vertexLowFrom = SharpDX.Vector2.Zero;
                    vertexLowFrom.X = Convert.ToSingle(chartControl.GetXByBarIndex(base.ChartBars, (int)sessionArray[i - 1].Value.IBLow.X));
                    vertexLowFrom.Y = Convert.ToSingle(this.cachedChartScale.GetYByValue(sessionArray[i - 1].Value.IBLow.Y));


                    base.RenderTarget.DrawLine(vertexHighFrom, vertexHighTo, this.ibLineSharpDxBrush, this.strokeWidth);
                    base.RenderTarget.DrawLine(vertexLowFrom, vertexLowTo, this.ibLineSharpDxBrush, this.strokeWidth);                    
                }

                // draw shaded IB region
                for (int i = 0; i < this.shadedGeometryTop.Count; i++)
                {
                    Vector2 topLeft = Vector2.Zero;
                    Vector2 topRight = Vector2.Zero;
                    Vector2 bottomLeft = Vector2.Zero;
                    Vector2 bottomRight = Vector2.Zero;

                    using (PathGeometry path = new PathGeometry(this.ibLineSharpDxBrush.Factory))
                    {
                        using (GeometrySink sink = path.Open())
                        {
                            Vector2 vertex = Vector2.Zero;

                            // add top geometry
                            for (int j = 0; j < this.shadedGeometryTop[i].Count; j++)
                            {
                                vertex.X = Convert.ToSingle(chartControl.GetXByBarIndex(base.ChartBars, (int)this.shadedGeometryTop[i][j].X));
                                vertex.Y = Convert.ToSingle(this.cachedChartScale.GetYByValue(this.shadedGeometryTop[i][j].Y));

                                if (j == 0)
                                {
                                    topLeft = vertex;
                                    sink.BeginFigure(topLeft, new FigureBegin());
                                }

                                if (j == this.shadedGeometryTop[i].Count - 1)
                                {
                                    topRight = vertex;
                                }

                                sink.AddLine(vertex);
                            }

                            // add bottom geometry
                            for (int j = this.shadedGeometryBottom[i].Count - 1; j >= 0; j--)
                            {
                                vertex.X = Convert.ToSingle(chartControl.GetXByBarIndex(base.ChartBars, (int)this.shadedGeometryBottom[i][j].X));
                                vertex.Y = Convert.ToSingle(this.cachedChartScale.GetYByValue(this.shadedGeometryBottom[i][j].Y));
                                
                                if (j == this.shadedGeometryBottom[i].Count - 1)
                                {
                                    bottomRight = vertex;
                                }

                                if (j == 0)
                                {
                                    bottomLeft = vertex;
                                }
                                
                                sink.AddLine(vertex);
                            }

                            sink.EndFigure(new FigureEnd());
                            sink.Close();

                            LinearGradientBrushProperties linearGradProperties = default(LinearGradientBrushProperties);

                            linearGradProperties.StartPoint = topLeft;
                            linearGradProperties.EndPoint.X = bottomRight.X;
                            linearGradProperties.EndPoint.Y = topLeft.Y;

                            GradientStopCollection gradientStopCollection = new SharpDX.Direct2D1.GradientStopCollection(this.cachedRenderTarget,
                                new SharpDX.Direct2D1.GradientStop[]
                                {
                                        new SharpDX.Direct2D1.GradientStop() { Color = DrawingExtensions.ToColor4(System.Windows.Media.Brushes.Transparent, 0.0f), Position = 0.0f },
                                        new SharpDX.Direct2D1.GradientStop() { Color = DrawingExtensions.ToColor4(this.InitialBalanceBrush, this.HighlightOpacity), Position = 1.0f }
                                });

                            if (this.cachedRenderTarget != null)
                            {
                                this.ibShadedSharpDxBrush = new SharpDX.Direct2D1.LinearGradientBrush(this.cachedRenderTarget, linearGradProperties, gradientStopCollection);
                            }

                            base.RenderTarget.DrawGeometry(path, this.ibLineSharpDxBrush);
                            base.RenderTarget.FillGeometry(path, this.ibShadedSharpDxBrush);

                            gradientStopCollection.Dispose();

                        }
                    }
                }


                if (this.ibShadedSharpDxBrush != null)
                {
                    this.ibShadedSharpDxBrush.Dispose();
                }
            }
            catch (Exception e)
            {
                string message = e.Message;
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

                    this.initialBalanceBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow);
                    this.strokeWidth = 3.0f;
                    this.ibTimeInMinutes = 60;
                    this.highlightOpacity = 0.25f;

                    break;

                case State.Configure:
                    // REQUIRED: necessary to add at least 1 plot so strategies using this indicator
                    // the OnBarUpdate will be called in this indicator.

                    // We still need to add this plot for sync but we'll make it transparent and use SharpDX for super fast rendering
                    base.AddPlot(System.Windows.Media.Brushes.Transparent, "ibHighPlot");
                    base.AddPlot(System.Windows.Media.Brushes.Transparent, "ibLowPlot");

                    if (base.DisplayInDataBox)
                    {
                        // since we use a transparent "midPlot", we have to use this property for DataBox
                        base.ShowTransparentPlotsInDataBox = true;
                    }
                    break;

                case State.Active:
                    break;

                case State.DataLoaded:
                    // we'll use the current bar as the key in the dictionary for super fast lookup
                    this.sessionIBDictionary = new SortedDictionary<int, SessionIBContext>();

                    if (this.Bars != null)
                    {
                        this.safeSessionIterator = new SafeSessionIterator(this, this.ResetInitialBalanceVariables);
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
                    if (this.ibLineSharpDxBrush != null)
                    {
                        this.ibLineSharpDxBrush.Dispose();
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Resets the mid variables.
        /// </summary>
        private void ResetInitialBalanceVariables()
        {
            if (!this.sessionIBDictionary.ContainsKey(base.CurrentBar))
            {
                this.sessionContext = SessionIBContext.Empty;
                this.sessionContext.IBHigh = Vector2.Zero;
                this.sessionContext.IBLow = Vector2.Zero;

                this.sessionGeometryListTop = new List<Vector2>();
                this.sessionGeometryListBottom = new List<Vector2>();

                // add this sessions high/low geometry to the list
                this.shadedGeometryTop.Add(this.sessionGeometryListTop);
                this.shadedGeometryBottom.Add(this.sessionGeometryListBottom);

                this.sessionIBDictionary.Add(base.CurrentBar, this.sessionContext);
            }

            this.sessionVertexCount = 0;
            this.sessionHighPrice = double.MinValue; // we expect any value bigger is valid
            this.sessionLowPrice = double.MaxValue; // we expect any value smaller is valid
        }

        #endregion Methods

        #region Structs

        private struct SessionIBContext
        {
            #region Fields

            public static readonly SessionIBContext Empty = default(SessionIBContext);

            public Vector2 IBHigh;
            public Vector2 IBLow;

            #endregion Fields
        }

        #endregion Structs
    }
}