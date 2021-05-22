// ***********************************************************************
// Assembly         : NinjaTrader.Custom
// Author           : JasonnatorDayTrader
// Created          : 03-25-2021
//
// Last Modified By : JasonnatorDayTrader
// Last Modified On : 04-17-2021
// ***********************************************************************
// Created in support of my YouTube channel https://www.youtube.com/user/Jasonnator
// Example developed specifically to help the https://www.futures.io community.
// Code freely available at https://gitlab.com/jasonnatordaytrader/jdt.nt8
// License: https://gitlab.com/jasonnatordaytrader/jdt.nt8/-/blob/master/LICENSE
// ***********************************************************************

#region Using declarations

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.IO;

namespace NinjaTrader.NinjaScript.Strategies
{
	#endregion

	/// <summary>
	/// Struct TrainingSample.  Using a struct instead of a class for performance reasons.
	/// </summary>
	internal struct TrainingSample
	{
		#region Fields

		/// <summary>
		/// The empty struct.
		/// </summary>
		public static readonly TrainingSample Empty = new TrainingSample();

		#endregion Fields

		#region Properties

		public int IsLong { get; set; }

		/// <summary>
		/// Gets or sets whether it's a winner.
		/// </summary>
		/// <value>The is winner.</value>
		public int IsWinner { get; set; }

		/// <summary>
		/// Gets or sets the window data array.
		/// </summary>
		/// <value>The window data.</value>
		public double[] WindowData { get; set; }

		#endregion Properties
	};


	/// <summary>
	/// This class serves as a BASIC outline of how NT8 could be connected up to TensorFlow.
	/// Communication could be via REST or grpc.
	/// Use this as a starting point combined with the REST example to get predictions from a served TF model.
	/// </summary>
	/// <remarks>
	/// This is super basic strategy which uses a moving average crossover + swing levels to place trades
	/// based on a 50% retracement level in a trending direction.  This is bascially an ABC pattern strategy.
	/// The goal was not to create a profitable strategy, but rather to provide a framework and proof of concept.
	/// 
	/// It is NOT all inclusive.  There is still significant setup to do on the tensorflow and tensorflow serving
	/// side(s) as well as creating well-formed messages for tf model inference.  I have tried to include notes
	/// and hints on where you will need to implement your way of handling the prediction pipeline.
	/// 
	/// The fastest way, IMO, to get started would be to create REST messages and send inferences via http
	/// then act upon the responses. Grpc via C# is possible but is extremely difficult, brittle, and very techincal.
	/// 
	/// This is a fairly advanced way to build algos.  Good luck and I hope this example helps!!!
	/// </remarks>
	public class TensorFlowFib50Strat : Strategy
	{
		#region Fields

		/// <summary>
		/// The synchronize object.
		/// </summary>
		private readonly object syncObject = new object();

		/// <summary>
		/// The binary writer.  Remember to close and dispose!
		/// </summary>
		private BinaryWriter binaryWriter;

		/// <summary>
		/// The current trade.
		/// </summary>
		private long currentTrade;

		/// <summary>
		/// The ema14.
		/// </summary>
		private EMA ema14;

		/// <summary>
		/// The ema50.
		/// </summary>
		private EMA ema50;

		/// <summary>
		/// The indicator values array.
		/// </summary>
		private double[] indicatorValues;

		/// <summary>
		/// The is bear trend traded.
		/// </summary>
		private bool isBearTrendTraded = false;

		/// <summary>
		/// The is bull trend traded.
		/// </summary>
		private bool isBullTrendTraded = false;

		/// <summary>
		/// The is long signal flag.
		/// </summary>
		private bool isLongSignal = false;

		/// <summary>
		/// The is short signal flag.
		/// </summary>
		private bool isShortSignal = false;

		/// <summary>
		/// Writing training data
		/// </summary>
		private bool isWritingTrainingData = true;

		/// <summary>
		/// The last swing high value.
		/// </summary>
		private double lastSwingHighValue;

		/// <summary>
		/// The last swing low value.
		/// </summary>
		private double lastSwingLowValue;

		/// <summary>
		/// The queue worker helper object.
		/// </summary>
		private TFQueueWorker<double> queueWorker;

		/// <summary>
		/// The retracement.
		/// TODO: expose as ninjascript property.
		/// </summary>
		private double retracement = 0.5;

		/// <summary>
		/// The retracement price.
		/// </summary>
		private double retracementPrice = 0.0;

		/// <summary>
		/// The swing indicator.
		/// </summary>
		private Swing swing5;

		/// <summary>
		/// The training sample dictionary which tracks historical trades for training output.
		/// </summary>
		private Dictionary<long, TrainingSample> trainingSampleDictionary = new Dictionary<long, TrainingSample>();

		/// <summary>
		/// The training file information.
		/// </summary>
		private FileInfo trainingFileInfo;

		/// <summary>
		/// The window size.
		/// </summary>
		private int windowSize = 50;

		private ATR atr14;

		#endregion Fields

		#region Events

		/// <summary>
		/// Occurs when [prediction received].
		/// </summary>
		internal event EventHandler<TFEventArgs> TfModelPredictionReceived;

		#endregion Events

		#region Methods

		/// <summary>
		/// An event driven method which is called whenever a bar is updated. The frequency in which OnBarUpdate is called will be determined by the "Calculate" property. OnBarUpdate() is the method where all of your script's core bar based calculation logic should be contained.
		/// </summary>
		protected override void OnBarUpdate()
		{
			if (base.CurrentBar < this.windowSize)
			{
				return;
			}

			// cross up
			if (this.ema14.Value[1] < this.ema50.Value[1] && this.ema14.Value[0] > this.ema50.Value[0])
			{
                // show up trend
                Draw.VerticalLine(this, Guid.NewGuid().ToString(), base.Time[0], Brushes.DarkGreen);
                Draw.VerticalLine(this, Guid.NewGuid().ToString(), base.Time[1], Brushes.DarkGreen);

				this.Reset();
			}

			// cross down
			if (this.ema14.Value[1] > this.ema50.Value[1] && this.ema14.Value[0] < this.ema50.Value[0])
			{
                // show down trend
                Draw.VerticalLine(this, Guid.NewGuid().ToString(), Time[0], Brushes.Firebrick);
                Draw.VerticalLine(this, Guid.NewGuid().ToString(), Time[1], Brushes.Firebrick);

				this.Reset();
			}

			this.queueWorker.AddValue(this.ema14);
			this.queueWorker.AddValue(this.ema50);
			//this.queueWorker.AddValue(this.atr14);

			// entry logic
			if (base.Position.MarketPosition == MarketPosition.Flat)
			{
				if (this.ema14.Value[0] > this.ema50.Value[0] && !this.isBullTrendTraded) // bull trend
				{
					// "stick" to the highest previous swing high
					this.lastSwingHighValue = Math.Max(this.lastSwingHighValue, base.High[0]);
					this.lastSwingLowValue = this.swing5.Values[1][0];

					this.retracementPrice = base.Instrument.MasterInstrument.RoundToTickSize(this.lastSwingLowValue + ((this.lastSwingHighValue - this.lastSwingLowValue) * (1 - this.retracement)));

					if (base.Close[0] < this.retracementPrice)
					{
						Draw.VerticalLine(this, Guid.NewGuid().ToString(), base.Time[0], Brushes.LawnGreen);
						// this is where we would ask the tfModel to give us a prediction
						// in this example, it is simulated with this local TfModelGetPrediction method
						// in a production environment, we would fire off this prediction request then listen to the tfModel's PredictionReceived event
						//tfModel.Predict()
						this.TfModelGetPrediction(true, this.lastSwingHighValue, this.lastSwingLowValue);
					}
				}
				else if (this.ema14.Value[0] < this.ema50.Value[0] && !this.isBearTrendTraded) // bear trend
				{
					// "stick" to the lowest previous swing low
					this.lastSwingHighValue = this.swing5.Values[0][0];
					this.lastSwingLowValue = Math.Min(this.lastSwingLowValue, base.Low[0]);

					this.retracementPrice = base.Instrument.MasterInstrument.RoundToTickSize(this.lastSwingHighValue - ((this.lastSwingHighValue - this.lastSwingLowValue) * (1 - this.retracement)));

					if (base.Close[0] > this.retracementPrice)
					{
						Draw.VerticalLine(this, Guid.NewGuid().ToString(), Time[0], Brushes.Red);
						// this is where we would ask the tfModel to give us a prediction
						// in this example, it is simulated with this local TfModelGetPrediction method
						// in a production environment, we would fire off this prediction request then listen to the tfModel's PredictionReceived event
						//tfModel.Predict()
						this.TfModelGetPrediction(false, this.lastSwingHighValue, this.lastSwingLowValue);
					}
				}
			}
			else // in a position, do any trade management here
			{
				// trade management
				// not implementing any here in order to only test the affect of the tfModel on trade performance
			}
		}

		/// <summary>
		/// An event driven method which is called on an incoming execution of an order managed by a strategy. An execution is another name for a fill of an order.
		/// </summary>
		/// <param name="execution">An Execution object representing the execution</param>
		/// <param name="executionId">A string value representing the execution id</param>
		/// <param name="price">A double value representing the execution price</param>
		/// <param name="quantity">An int value representing the execution quantity</param>
		/// <param name="marketPosition">A MarketPosition object representing the position of the execution. (long or short)</param>
		/// <param name="orderId">A string representing the order id</param>
		/// <param name="time">A DateTime value representing the time of the execution</param>
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			base.OnExecutionUpdate(execution, executionId, price, quantity, marketPosition, orderId, time);

			if (execution.IsEntryStrategy)
			{
				this.currentTrade = time.Ticks;

				lock (this.syncObject)
				{
					if (!this.trainingSampleDictionary.ContainsKey(this.currentTrade))
					{
						TrainingSample sample = TrainingSample.Empty;
						sample.IsWinner = -1;
						sample.IsLong = marketPosition == MarketPosition.Long ? 1 : 0;
						sample.WindowData = this.indicatorValues;

						// record opening data for trade
						this.trainingSampleDictionary.Add(this.currentTrade, sample);
					}
				}
			}

			if (execution.IsExitStrategy)
			{
				lock (this.syncObject)
				{
					if (this.trainingSampleDictionary.ContainsKey(this.currentTrade) && base.SystemPerformance.AllTrades.Count > 0)
					{
						// get the performance metrics of the last trade
						TradesPerformance tradePerformance = base.SystemPerformance.AllTrades.GetTrades(base.Instrument.FullName, "", 1).TradesPerformance;

						if (tradePerformance.GrossProfit > 0) // profitable
						{
							// have to spin up a new struct, modify it, then assign it to the dictionary
							TrainingSample sample = TrainingSample.Empty;
							sample = this.trainingSampleDictionary[this.currentTrade];
							sample.IsWinner = 1;

							this.trainingSampleDictionary[this.currentTrade] = sample;
						}
						else if (tradePerformance.GrossLoss < 0) // it's a loss
						{
							// have to spin up a new struct, modify it, then assign it to the dictionary
							TrainingSample sample = TrainingSample.Empty;
							sample = this.trainingSampleDictionary[this.currentTrade];
							sample.IsWinner = 0;

							this.trainingSampleDictionary[this.currentTrade] = sample;
						}
						else // something weird happened, don't record this trade sample for training, remove it from storage dictionary						
						{
							lock (this.syncObject)
							{
								// HACK: I think this is partly because of the way events fire in NT8.  Workaround implemented.

								// can't remove items from a dictionary while iterating so make a list of stuff to remove, then remove them all.
								List<long> removeList = new List<long>();

								foreach (long key in this.trainingSampleDictionary.Keys)
								{
									// remove any trade sample where winner/loser was not set
									if (this.trainingSampleDictionary[key].IsWinner == -1)
									{
										removeList.Add(key);
									}
								}

								foreach (long key in removeList)
								{
									this.trainingSampleDictionary.Remove(key);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// An event driven method which is called whenever the script enters a new State. The OnStateChange() method can be used to configure script properties, create one-time behavior when going from historical to real-time, as well as manage clean up resources on termination.
		/// </summary>
		protected override void OnStateChange()
		{
			if (base.State == State.SetDefaults)
			{
				base.Description = @"Testing.";
				base.Name = "Fib50Strat";
				base.Calculate = Calculate.OnBarClose;
				base.EntriesPerDirection = 1;
				base.EntryHandling = EntryHandling.AllEntries;
				base.IsExitOnSessionCloseStrategy = false;
				base.ExitOnSessionCloseSeconds = 30;
				base.IsFillLimitOnTouch = false;
				base.MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
				base.OrderFillResolution = OrderFillResolution.Standard;
				base.Slippage = 0;
				base.StartBehavior = StartBehavior.WaitUntilFlat;
				base.TimeInForce = TimeInForce.Gtc;
				base.TraceOrders = false;
				base.RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
				base.StopTargetHandling = StopTargetHandling.PerEntryExecution;
				base.BarsRequiredToTrade = 20;

				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				base.IsInstantiatedOnEachOptimizationIteration = true;
			}
			else if (base.State == State.Configure)
			{
			}
			else if (base.State == State.DataLoaded)
			{
				// add the indicators without having to draw everything
				lock (base.NinjaScripts)
				{
					base.NinjaScripts.Add(this.ema14 = new EMA() { Period = 14, Parent = this, IsCreatedByStrategy = true });
					base.NinjaScripts.Add(this.ema50 = new EMA() { Period = 50, Parent = this, IsCreatedByStrategy = true });
					base.NinjaScripts.Add(this.swing5 = new Swing() { Strength = 5, Parent = this, IsCreatedByStrategy = true });
					base.NinjaScripts.Add(this.atr14 = new ATR() { Period = 14, Parent = this, IsCreatedByStrategy = true });
				}

				this.queueWorker = new TFQueueWorker<double>(this.windowSize);

				this.queueWorker.AddIndicator(this.ema14);
				this.queueWorker.AddIndicator(this.ema50);
				//this.queueWorker.AddIndicator(this.atr14);

				if (this.isWritingTrainingData)
				{
					string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), System.Environment.SpecialFolder.Desktop.ToString(), "TF").ToString();

					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}

					this.trainingFileInfo = new FileInfo(string.Format("{0}\\{1}{2}Training.bin", path, this.GetType().Name, this.Instrument.FullName));

					// always ensure a new file is created and used
					if (File.Exists(this.trainingFileInfo.FullName))
					{
						File.Delete(this.trainingFileInfo.FullName);
					}

					this.binaryWriter = new BinaryWriter(File.Open(this.trainingFileInfo.FullName, FileMode.OpenOrCreate));
				}

				// subscribe to the tfModel's event (simulated in this example with a local event)
				//tfModel.TfModelPredictionReceived += this.Fib50Strat_PredictionReceived;
				this.TfModelPredictionReceived += this.TensorFlowFib50Strat_PredictionReceived;
			}
			else if (base.State == State.Transition)
			{
				// write out all historical trade information for training

				if (this.binaryWriter != null && this.isWritingTrainingData)
				{
					lock (this.syncObject)
					{
						foreach (long key in this.trainingSampleDictionary.Keys)
						{
							// write out all stored indicator values
							int length = this.trainingSampleDictionary[key].WindowData.Length;
							for (int i = 0; i < length; i++)
							{
								this.binaryWriter.Write(Convert.ToSingle(this.trainingSampleDictionary[key].WindowData[i]));
							}

							// write out direction (long = 1, short = 0)
							//this.binaryWriter.Write(Convert.ToBoolean(this.trainingDictionary[key].IsLong));

							// write out label
							this.binaryWriter.Write(Convert.ToInt32(this.trainingSampleDictionary[key].IsWinner));
							//this.binaryWriter.Write(Convert.ToBoolean(this.trainingDictionary[key].IsWinner));
						}
					}

					this.binaryWriter.Close();
					this.binaryWriter.Dispose();
				}
			}
			else if (base.State == State.Terminated) // let's prevent memory leaks
			{
				if (this.TfModelPredictionReceived != null) // is anyone subscribed to our event
				{
					this.TfModelPredictionReceived -= this.TensorFlowFib50Strat_PredictionReceived;
				}

				if (this.binaryWriter != null)
				{
					this.binaryWriter.Dispose();
				}
			}
		}

		/// <summary>
		/// This method simulates what we would get back from TF Serving model and we trade accordingly.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void TensorFlowFib50Strat_PredictionReceived(object sender, TFEventArgs args)
		{
			if (args.IsLongSignal)
			{
				this.isBullTrendTraded = true; // prevent repeat entries

				base.EnterLong();
				base.SetProfitTarget(CalculationMode.Price, args.SwingHigh);
				base.SetStopLoss(CalculationMode.Price, args.SwingLow);
			}
			else
			{
				this.isBearTrendTraded = true; // prevent repeat entries

				base.EnterShort();
				base.SetProfitTarget(CalculationMode.Price, args.SwingLow);
				base.SetStopLoss(CalculationMode.Price, args.SwingHigh);
			}
		}

		/// <summary>
		/// Gets the prediction.  This method would normally live inside the tfModel and would get
		/// called whenever the NT8 strategy called tfModel.Predict().
		/// This is also where you would have to do any message forming (JSON for REST or byte[]'s for grpc)
		/// if it wasn't done previously.
		/// </summary>
		/// <param name="isLongSignal">if set to <c>true</c> [is long signal].</param>
		/// <param name="swingHigh">The swing high.</param>
		/// <param name="swingLow">The swing low.</param>
		private void TfModelGetPrediction(bool isLongSignal, double swingHigh, double swingLow)
		{
			// if no one is subscribed to our event, don't fire off a prediction request
			if (this.TfModelPredictionReceived != null)
			{
				// if the queue is not filled, don't fire off trade, otherwise, we want the snapshot at the time of entry of all historical values we stored
				if (this.queueWorker.TryGetInputValues(out this.indicatorValues))
				{
					// NOTES
					// Everything in grpc input/output message(s) are byte[]'s
					// Message could be structured {Head, Body, EntireMessage}
					// Set the input message body to a byte[] (converted from indicator value array)
					// Input message is fully custom for grpc, REST could use JSON representation
					// tfModel would fire off a PredictionReceived event which you'd be subscribed to.
					// In this example, it is simulated below with TfModelPredictionReceived
					// ENDNOTES

					// tfModel.InputMessage.Body.SetBody(this.indicatorValues);
					// tfModel.Predict();

					// tfModel would be firing its own version of this.  For this example, it is simulated with this "dummy" event
					this.TfModelPredictionReceived.Invoke("TF serving simulated model", new TFEventArgs(isLongSignal, swingHigh, swingLow, this.retracementPrice));
				}
			}
		}

		/// <summary>
		/// Resets the various field values.
		/// </summary>
		private void Reset()
		{
			this.lastSwingHighValue = double.MinValue;
			this.lastSwingLowValue = double.MaxValue;
			this.retracementPrice = 0.0;
			this.isBullTrendTraded = false;
			this.isBearTrendTraded = false;
			this.isLongSignal = false;
			this.isShortSignal = false;
		}

		#endregion Methods
	}


	/// <summary>
	/// Class TFEventArgs. This class cannot be inherited.
	/// Implements the <see cref="System.EventArgs" />
	/// </summary>
	/// <seealso cref="System.EventArgs" />
	internal sealed class TFEventArgs : EventArgs
	{
		#region Fields

		/// <summary>
		/// The is long signal.
		/// </summary>
		private readonly bool isLongSignal;

		/// <summary>
		/// The retracement price.
		/// </summary>
		private readonly double retracementPrice;

		/// <summary>
		/// The swing high.
		/// </summary>
		private readonly double swingHigh;

		/// <summary>
		/// The swing low.
		/// </summary>
		private readonly double swingLow;
		#endregion Fields

		#region Constructors

		public TFEventArgs(bool isLongSignal, double swingHigh, double swingLow, double retracementPrice)
		{
			this.isLongSignal = isLongSignal;
			this.swingHigh = swingHigh;
			this.swingLow = swingLow;
			this.retracementPrice = retracementPrice;
		}

		/// <summary>
		/// Prevents a default instance of the <see cref="TFEventArgs"/> class from being created.
		/// </summary>
		private TFEventArgs()
		{
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Gets a value indicating whether this instance is long signal.
		/// </summary>
		/// <value><c>true</c> if this instance is long signal; otherwise, <c>false</c>.</value>
		public bool IsLongSignal
		{
			get
			{
				return isLongSignal;
			}
		}

		/// <summary>
		/// Gets the retracement price.
		/// </summary>
		/// <value>The retracement price.</value>
		public double RetracementPrice
		{
			get
			{
				return retracementPrice;
			}
		}

		/// <summary>
		/// Gets the swing high.
		/// </summary>
		/// <value>The swing high.</value>
		public double SwingHigh
		{
			get
			{
				return swingHigh;
			}
		}

		/// <summary>
		/// Gets the swing low.
		/// </summary>
		/// <value>The swing low.</value>
		public double SwingLow
		{
			get
			{
				return swingLow;
			}
		}
		#endregion Properties
	}


	/// <summary>
	/// TFQueueWorker tracks indicator values up to <see cref="WindowSize" />.  Call <see cref="TryGetInputValues(out T[])" /> to create a 1D array used for TF model processing./&gt;
	/// </summary>
	internal sealed class TFQueueWorker<T> where T : struct // constrain to well-known types (double, int, long, float, etc)
	{
		#region Fields

		/// <summary>
		/// The window size
		/// </summary>
		private readonly int windowSize;

		/// <summary>
		/// The indicator dictionary
		/// </summary>
		private Dictionary<int, Queue<T>> indicatorDictionary;

		/// <summary>
		/// The is queue filled
		/// </summary>
		private bool isQueueFilled;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TFQueueWorker<typeparamref name="T"/>" /> class.
		/// </summary>
		/// <param name="windowSize">The window size.  Do not multiply by the number of indicators.  All indicators will have this window size.</param>
		public TFQueueWorker(int windowSize)
		{
			this.windowSize = windowSize;
			this.indicatorDictionary = new Dictionary<int, Queue<T>>();
		}

		/// <summary>
		/// Prevent user from calling default constructor.  <see cref="WindowSize"/> must be specified.
		/// </summary>
		private TFQueueWorker()
		{
			// do nothing
		}

		#endregion Constructors

		#region Events

		/// <summary>
		/// Occurs when [queue filled].
		/// </summary>
		//public event EventHandler<T[]> QueueFilled;

		#endregion Events

		#region Properties

		/// <summary>
		/// Gets the size of the window.
		/// </summary>
		/// <value>The size of the window.</value>
		public int WindowSize
		{
			get
			{
				return this.windowSize;
			}
		}

		/// <summary>
		/// Gets the worker count.
		/// </summary>
		/// <value>The worker count.</value>
		public int WorkerCount
		{
			get
			{
				return this.indicatorDictionary.Keys.Count;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Adds the indicator.
		/// </summary>
		/// <param name="indicator">The indicator.</param>
		public void AddIndicator(Indicator indicator)
		{
			this.indicatorDictionary.Add(indicator.IndicatorId, new Queue<T>(this.windowSize));
		}

		/// <summary>
		/// Adds the value.  If order is important, it must be added by the indicator/strategy in the proper order.
		/// </summary>
		/// <param name="indicator">The indicator.</param>
		public void AddValue(Indicator indicator)
		{
			// this cast is a major performance hit to support generics!!!
			T indicatorValue = (T)Convert.ChangeType(indicator.Value[0], typeof(T));

			if (this.indicatorDictionary[indicator.IndicatorId].Count < this.windowSize)
			{
				this.indicatorDictionary[indicator.IndicatorId].Enqueue(indicatorValue);
				this.isQueueFilled = false;
			}
			else
			{
				this.indicatorDictionary[indicator.IndicatorId].Dequeue();
				this.indicatorDictionary[indicator.IndicatorId].Enqueue(indicatorValue);
				this.isQueueFilled = true;
			}
		}

		/// <summary>
		/// Gets the signal when requested.
		/// </summary>
		public bool TryGetInputValues(out T[] array)
		{
			bool success = false;
			array = null;

			if (this.isQueueFilled)
			{
				if (this.isQueueFilled)
				{
					int dCount = this.indicatorDictionary.Count;
					T[] args = new T[this.windowSize * dCount];
					int startingPosition = 1;

					int key = -1;
					T[] queueArray;
					for (int i = 0; i < dCount; ++i)
					{
						key = this.indicatorDictionary.ElementAt(i).Key;
						queueArray = this.indicatorDictionary[key].ToArray();

						Array.Copy(queueArray, 0, args, startingPosition - 1, this.windowSize);
						startingPosition += windowSize;
					}

					success = true;
					array = args;
				}
			}

			return success;
		}

		#endregion Methods
	}
}
