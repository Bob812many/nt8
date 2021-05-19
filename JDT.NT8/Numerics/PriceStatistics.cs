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
namespace JDT.NT8.Numerics
{
    using NinjaTrader.Cbi;
    using NinjaTrader.Data;
    using System;

    /// <summary>
    /// Class StatisticsBase used to support generics in the helper methods.
    /// </summary>
    public abstract class StatisticsBase
    {
        public abstract void MakeRequest();

        internal abstract void RequestCallback(BarsRequest request, ErrorCode error, string errorMessage);
    }

    /// <summary>
    /// Class PriceStatistics. This class cannot be inherited.
    /// </summary>
    public sealed class PriceStatistics : StatisticsBase
    {
        #region Fields

        /// <summary>
        /// The request
        /// </summary>
        private readonly BarsRequest request;

        /// <summary>
        /// The highest price
        /// </summary>
        private double highestPrice;

        /// <summary>
        /// The high mean
        /// </summary>
        private double highMean;

        /// <summary>
        /// The high standard deviation
        /// </summary>
        private double highStandardDeviation;

        /// <summary>
        /// Flag for whether the statistics have been calculated
        /// </summary>
        private bool isCalculated;

        /// <summary>
        /// The lowest price
        /// </summary>
        private double lowestPrice;

        /// <summary>
        /// The low mean
        /// </summary>
        private double lowMean;

        /// <summary>
        /// The low standard deviation
        /// </summary>
        private double lowStandardDeviation;

        /// <summary>
        /// The mean price.
        /// </summary>
        private double priceMean;

        /// <summary>
        /// The price standard deviation.
        /// </summary>
        private double priceStandardDeviation;

        /// <summary>
        /// The price kurtosis
        /// </summary>
        private double priceKurtosis;

        /// <summary>
        /// The price skewness
        /// </summary>
        private double priceSkewness;

        /// <summary>
        /// The price median
        /// </summary>
        private double priceMedian;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceStatistics" /> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public PriceStatistics(BarsRequest request)
        {
            this.isCalculated = false;
            this.request = request;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the highest price.
        /// </summary>
        /// <value>The highest price.</value>
        public double HighestPrice
        {
            get { return this.highestPrice; }
        }

        /// <summary>
        /// Gets the high mean.
        /// </summary>
        /// <value>The high mean.</value>
        public double HighMean
        {
            get
            {
                return this.highMean;
            }

            private set
            {
                this.highMean = value;
            }
        }

        /// <summary>
        /// Gets the high standard deviation.
        /// </summary>
        /// <value>The high standard deviation.</value>
        public double HighStandardDeviation
        {
            get
            {
                return this.highStandardDeviation;
            }

            private set
            {
                this.highStandardDeviation = value;
            }
        }

        /// <summary>
        /// Flag for whether the statistics have been calculated.
        /// </summary>
        public bool IsCalculated
        {
            get
            {
                return this.isCalculated;
            }
        }

        /// <summary>
        /// Gets the lowest price.
        /// </summary>
        /// <value>The lowest price.</value>
        public double LowestPrice
        {
            get { return this.lowestPrice; }
        }

        /// <summary>
        /// Gets the low mean.
        /// </summary>
        /// <value>The low mean.</value>
        public double LowMean
        {
            get
            {
                return this.lowMean;
            }

            private set
            {
                this.lowMean = value;
            }
        }

        /// <summary>
        /// Gets the low standard deviation.
        /// </summary>
        /// <value>The low standard deviation.</value>
        public double LowStandardDeviation
        {
            get
            {
                return this.lowStandardDeviation;
            }

            private set
            {
                this.lowStandardDeviation = value;
            }
        }

        /// <summary>
        /// Gets the mean price.
        /// </summary>
        /// <value>The mean price.</value>
        public double PriceMean
        {
            get
            {
                return this.priceMean;
            }
        }

        /// <summary>
        /// Gets the price median.
        /// </summary>
        /// <value>The price median.</value>
        public double PriceMedian
        {
            get
            {
                return this.priceMedian;
            }
        }

        /// <summary>
        /// Gets the low standard deviation.
        /// </summary>
        /// <value>The low standard deviation.</value>
        public double PriceStandardDeviation
        {
            get
            {
                return this.priceStandardDeviation;
            }
        }

        /// <summary>
        /// Gets the price kurtosis.
        /// </summary>
        /// <value>The price kurtosis.</value>
        public double PriceKurtosis
        {
            get
            {
                return this.priceKurtosis;
            }
        }

        /// <summary>
        /// Gets the price skewness.
        /// </summary>
        /// <value>The price skewness.</value>
        public double PriceSkewness
        {
            get
            {
                return this.priceSkewness;
            }
        }


        #endregion Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"low mean {this.lowMean:N} low std dev {this.lowStandardDeviation:N} high mean {this.highMean:N} high std dev {this.highStandardDeviation:N} resolution {this.request.BarsPeriod.BarsPeriodType}";
        }

        /// <summary>
        /// Makes the request.  Should be called asynchronously.
        /// </summary>
        public override void MakeRequest()
        {
            if (this.request == null)
            {
                throw new Exception($"Request was null.");
            }

            this.request.Request(this.RequestCallback);
        }

        /// <summary>
        /// Requests the callback.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="error">The error.</param>
        /// <param name="errorMessage">The error message.</param>
        internal override void RequestCallback(BarsRequest request, ErrorCode error, string errorMessage)
        {
            if (error == ErrorCode.NoError)
            {
                int count = request.Bars.BarsSeries.Count;

                double[] lows = new double[count];
                double[] highs = new double[count];
                double[] closes = new double[count];
                double lowest = double.MaxValue;
                double highest = double.MinValue;

                for (int i = 0; i < count; i++)
                {
                    lows[i] = request.Bars.BarsSeries.GetLow(i);
                    highs[i] = request.Bars.BarsSeries.GetHigh(i);
                    closes[i] = request.Bars.BarsSeries.GetClose(i);

                    if (lows[i] < lowest)
                    {
                        lowest = lows[i];
                    }

                    if (highs[i] > highest)
                    {
                        highest = highs[i];
                    }
                }

                this.lowMean = MathNet.Numerics.Statistics.Statistics.Mean(lows);
                this.lowStandardDeviation = MathNet.Numerics.Statistics.Statistics.StandardDeviation(lows);
                this.highMean = MathNet.Numerics.Statistics.Statistics.Mean(highs);
                this.highStandardDeviation = MathNet.Numerics.Statistics.Statistics.StandardDeviation(highs);

                this.priceMean = MathNet.Numerics.Statistics.Statistics.Mean(closes);
                this.priceStandardDeviation = MathNet.Numerics.Statistics.Statistics.StandardDeviation(closes);

                this.priceKurtosis = MathNet.Numerics.Statistics.Statistics.Kurtosis(closes);
                this.priceSkewness = MathNet.Numerics.Statistics.Statistics.Skewness(closes);
                this.priceMedian = MathNet.Numerics.Statistics.Statistics.Median(closes);

                this.lowestPrice = lowest;
                this.highestPrice = highest;

                this.isCalculated = true;
            }
        }

        #endregion Methods
    }
}
