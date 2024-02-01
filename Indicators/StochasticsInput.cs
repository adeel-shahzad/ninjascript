// 
// Copyright (C) 2016, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The Stochastic Oscillator is made up of two lines that oscillate between 
	/// a vertical scale of 0 to 100. The %K is the main line and it is drawn as 
	/// a solid line. The second is the %D line and is a moving average of %K. 
	/// The %D line is drawn as a dotted line. Use as a buy/sell signal generator, 
	/// buying when fast moves above slow and selling when fast moves below slow.
	/// v1.0 Modified to use Input as Indicator
	/// v1.1 Modified to include Order Flow CD as input, via Price series check
	/// </summary>
	public class StochasticsInput : Indicator
	{
		private Series<double>		den;
		private Series<double>		fastK;
		private MIN					min;
		private MAX					max;
		private Series<double>		nom;
		private SMA					smaFastK;
		private SMA					smaK;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionStochastics;
				Name						= "StochasticsInput v1.1";
				IsSuspendedWhileInactive	= true;
				PeriodD						= 7;
				PeriodK						= 14;
				Smooth						= 3;

				AddPlot(Brushes.Green,				NinjaTrader.Custom.Resource.StochasticsD);
				AddPlot(Brushes.Orange,				NinjaTrader.Custom.Resource.StochasticsK);

				AddLine(Brushes.DarkViolet,		20,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
				AddLine(Brushes.YellowGreen,	80,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
			}
			else if (State == State.DataLoaded)
			{
				den			= new Series<double>(this);
				nom			= new Series<double>(this);
				fastK		= new Series<double>(this);	
				min			= Input is PriceSeries ? MIN(Low, PeriodK): MIN(Input, PeriodK);
				max			= Input is PriceSeries ? MAX(High, PeriodK): MAX(Input, PeriodK); 	
				smaFastK	= SMA(fastK, Smooth);
				smaK		= SMA(K, PeriodD);
			}
		
		}

		protected override void OnBarUpdate()
		{

			double min0 = min[0];
			nom[0]		= Input is PriceSeries ? Close[0] - min0 : Input[0] - min0;	
			den[0]		= max[0] - min0;

			if (den[0].ApproxCompare(0) == 0)
				fastK[0] = CurrentBar == 0 ? 50 : fastK[1];
			else
				fastK[0] = Math.Min(100, Math.Max(0, 100 * nom[0] / den[0]));

			// Slow %K == Fast %D
			K[0] = smaFastK[0];
			D[0] = smaK[0];
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> D
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> K
		{
			get { return Values[1]; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PeriodD", GroupName = "NinjaScriptParameters", Order = 0)]
		public int PeriodD
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PeriodK", GroupName = "NinjaScriptParameters", Order = 1)]
		public int PeriodK
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth", GroupName = "NinjaScriptParameters", Order = 2)]
		public int Smooth
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private StochasticsInput[] cacheStochasticsInput;
		public StochasticsInput StochasticsInput(int periodD, int periodK, int smooth)
		{
			return StochasticsInput(Input, periodD, periodK, smooth);
		}

		public StochasticsInput StochasticsInput(ISeries<double> input, int periodD, int periodK, int smooth)
		{
			if (cacheStochasticsInput != null)
				for (int idx = 0; idx < cacheStochasticsInput.Length; idx++)
					if (cacheStochasticsInput[idx] != null && cacheStochasticsInput[idx].PeriodD == periodD && cacheStochasticsInput[idx].PeriodK == periodK && cacheStochasticsInput[idx].Smooth == smooth && cacheStochasticsInput[idx].EqualsInput(input))
						return cacheStochasticsInput[idx];
			return CacheIndicator<StochasticsInput>(new StochasticsInput(){ PeriodD = periodD, PeriodK = periodK, Smooth = smooth }, input, ref cacheStochasticsInput);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.StochasticsInput StochasticsInput(int periodD, int periodK, int smooth)
		{
			return indicator.StochasticsInput(Input, periodD, periodK, smooth);
		}

		public Indicators.StochasticsInput StochasticsInput(ISeries<double> input , int periodD, int periodK, int smooth)
		{
			return indicator.StochasticsInput(input, periodD, periodK, smooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.StochasticsInput StochasticsInput(int periodD, int periodK, int smooth)
		{
			return indicator.StochasticsInput(Input, periodD, periodK, smooth);
		}

		public Indicators.StochasticsInput StochasticsInput(ISeries<double> input , int periodD, int periodK, int smooth)
		{
			return indicator.StochasticsInput(input, periodD, periodK, smooth);
		}
	}
}

#endregion
