//
// Copyright (C) 2020, NinjaTrader LLC <www.ninjatrader.com>.
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
	public class CustomSmoothFunc : Indicator
	{
		private double mult;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "Smoothing Function as described with PMO from Stock Charts";
				Name						= "CustomSmoothFunc";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, "SmoothPlot");
			}
			else if (State == State.Configure)
			{
				mult = 2.0 / (Period);
			}
		}

		protected override void OnBarUpdate()
		{
			Value[0] = (CurrentBar == 0 ? Input[0] : (Input[0] - Value[1]) * mult + Value[1]);
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CustomSmoothFunc[] cacheCustomSmoothFunc;
		public CustomSmoothFunc CustomSmoothFunc(int period)
		{
			return CustomSmoothFunc(Input, period);
		}

		public CustomSmoothFunc CustomSmoothFunc(ISeries<double> input, int period)
		{
			if (cacheCustomSmoothFunc != null)
				for (int idx = 0; idx < cacheCustomSmoothFunc.Length; idx++)
					if (cacheCustomSmoothFunc[idx] != null && cacheCustomSmoothFunc[idx].Period == period && cacheCustomSmoothFunc[idx].EqualsInput(input))
						return cacheCustomSmoothFunc[idx];
			return CacheIndicator<CustomSmoothFunc>(new CustomSmoothFunc(){ Period = period }, input, ref cacheCustomSmoothFunc);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CustomSmoothFunc CustomSmoothFunc(int period)
		{
			return indicator.CustomSmoothFunc(Input, period);
		}

		public Indicators.CustomSmoothFunc CustomSmoothFunc(ISeries<double> input , int period)
		{
			return indicator.CustomSmoothFunc(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CustomSmoothFunc CustomSmoothFunc(int period)
		{
			return indicator.CustomSmoothFunc(Input, period);
		}

		public Indicators.CustomSmoothFunc CustomSmoothFunc(ISeries<double> input , int period)
		{
			return indicator.CustomSmoothFunc(input, period);
		}
	}
}

#endregion
