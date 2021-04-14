// ***********************************************************************
// Assembly         : JDT.NT8
// Author           : JasonnatorDayTrader
// Created          : 08-28-2020
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 08-28-2020
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8	
// ***********************************************************************
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using System;

namespace JDT.NT8.Numerics
{
    /// <summary>
    /// Class VolumeStatistics. This class cannot be inherited.
    /// </summary>
    public sealed class VolumeStatistics
    {
        #region Fields

        /// <summary>
        /// The request
        /// </summary>
        private readonly BarsRequest request;

        /// <summary>
        /// Flag for whether the statistics have been calculated.
        /// </summary>
        private bool isCalculated;

        /// <summary>
        /// The highest volume
        /// </summary>
        private double volumeHighest;

        /// <summary>
        /// The lowest volume
        /// </summary>
        private double volumeLowest;

        /// <summary>
        /// The volume mean
        /// </summary>
        private double volumeMean;

        /// <summary>
        /// The volume standard deviation
        /// </summary>
        private double volumeStandardDeviation;

        /// <summary>
        /// The volume kurtosis
        /// </summary>
        private double volumeKurtosis;

        /// <summary>
        /// The volume skewness
        /// </summary>
        private double volumeSkewness;

        /// <summary>
        /// The volume median
        /// </summary>
        private double volumeMedian;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeStatistics"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public VolumeStatistics(BarsRequest request)
        {
            this.isCalculated = false;
            this.request = request;
            this.request.Request(this.RequestCallback);
        }

        #endregion Constructors

        #region Properties

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
        /// Gets the highest volume.
        /// </summary>
        /// <value>The highest volume.</value>
        public double VolumeHighest
        {
            get { return this.volumeHighest; }
        }

        /// <summary>
        /// Gets the lowest.
        /// </summary>
        /// <value>The lowest.</value>
        public double VolumeLowest
        {
            get { return this.volumeLowest; }
        }

        /// <summary>
        /// Gets the volume mean.
        /// </summary>
        /// <value>The volume mean.</value>
        public double VolumeMean
        {
            get
            {
                return this.volumeMean;
            }
        }

        /// <summary>
        /// Gets the volume median.
        /// </summary>
        /// <value>The volume median.</value>
        public double VolumeMedian
        {
            get
            {
                return this.volumeMedian;
            }
        }

        /// <summary>
        /// Gets the volume standard deviation.
        /// </summary>
        /// <value>The volume standard deviation.</value>
        public double VolumeStandardDeviation
        {
            get
            {
                return this.volumeStandardDeviation;
            }
        }

        /// <summary>
        /// Gets the volume kurtosis.
        /// </summary>
        /// <value>The volume kurtosis.</value>
        public double VolumeKurtosis
        {
            get
            {
                return this.volumeKurtosis;
            }
        }

        /// <summary>
        /// Gets the volume skewness.
        /// </summary>
        /// <value>The volume skewness.</value>
        public double VolumeSkewness
        {
            get
            {
                return this.volumeSkewness;
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
            return $"mean {this.volumeMean:N} std dev {this.volumeStandardDeviation:N} resolution {this.request.BarsPeriod.BarsPeriodType}";
        }

        /// <summary>
        /// Makes the request.  Should be called asynchronously.
        /// </summary>
        public void MakeRequest()
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
        private void RequestCallback(BarsRequest request, ErrorCode error, string errorMessage)
        {
            if (error == ErrorCode.NoError)
            {
                int count = request.Bars.BarsSeries.Count;

                double[] volumes = new double[count];
                double lowest = double.MaxValue;
                double highest = double.MinValue;

                for (int i = 0; i < count; i++)
                {
                    volumes[i] = request.Bars.BarsSeries.GetVolume(i);

                    if (volumes[i] < lowest)
                    {
                        lowest = volumes[i];
                    }

                    if (volumes[i] > highest)
                    {
                        highest = volumes[i];
                    }
                }

                this.volumeMean = MathNet.Numerics.Statistics.Statistics.Mean(volumes);
                this.volumeStandardDeviation = MathNet.Numerics.Statistics.Statistics.StandardDeviation(volumes);
                this.volumeKurtosis = MathNet.Numerics.Statistics.Statistics.Kurtosis(volumes);
                this.volumeSkewness = MathNet.Numerics.Statistics.Statistics.Skewness(volumes);
                this.volumeMedian = MathNet.Numerics.Statistics.Statistics.Median(volumes);
                
                this.volumeLowest = lowest;
                this.volumeHighest = highest;

                this.isCalculated = true;
            }
        }

        #endregion Methods
    }
}
