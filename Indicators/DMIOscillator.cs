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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class DMIOscillator : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "DMIOscillator";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period					= 10;
				UpColor					= Brushes.Blue;
				DownColor					= Brushes.Red;
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "DmiOsc");
				AddPlot(Brushes.Red, "DmiOscLine");
				Plots[0].Width = 2;
			}
		}

		protected override void OnBarUpdate()
		{
			double plotValue = DM(Period).DiPlus[0] - DM(Period).DiMinus[0];
			DmiOsc[0] = plotValue;
			DmiOscLine[0] = plotValue;
			
			if (plotValue >= 0)
				BarBrush	= UpColor;
			
			else 
				BarBrush	= DownColor;
		}

		#region Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="UpColor", Order=2, GroupName="Parameters")]
		public Brush UpColor
		{ get; set; }

		[Browsable(false)]
		public string UpColorSerializable
		{
			get { return Serialize.BrushToString(UpColor); }
			set { UpColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="DownColor", Order=3, GroupName="Parameters")]
		public Brush DownColor
		{ get; set; }

		[Browsable(false)]
		public string DownColorSerializable
		{
			get { return Serialize.BrushToString(DownColor); }
			set { DownColor = Serialize.StringToBrush(value); }
		}			

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DmiOsc
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DmiOscLine
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DMIOscillator[] cacheDMIOscillator;
		public DMIOscillator DMIOscillator(int period, Brush upColor, Brush downColor)
		{
			return DMIOscillator(Input, period, upColor, downColor);
		}

		public DMIOscillator DMIOscillator(ISeries<double> input, int period, Brush upColor, Brush downColor)
		{
			if (cacheDMIOscillator != null)
				for (int idx = 0; idx < cacheDMIOscillator.Length; idx++)
					if (cacheDMIOscillator[idx] != null && cacheDMIOscillator[idx].Period == period && cacheDMIOscillator[idx].UpColor == upColor && cacheDMIOscillator[idx].DownColor == downColor && cacheDMIOscillator[idx].EqualsInput(input))
						return cacheDMIOscillator[idx];
			return CacheIndicator<DMIOscillator>(new DMIOscillator(){ Period = period, UpColor = upColor, DownColor = downColor }, input, ref cacheDMIOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DMIOscillator DMIOscillator(int period, Brush upColor, Brush downColor)
		{
			return indicator.DMIOscillator(Input, period, upColor, downColor);
		}

		public Indicators.DMIOscillator DMIOscillator(ISeries<double> input , int period, Brush upColor, Brush downColor)
		{
			return indicator.DMIOscillator(input, period, upColor, downColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DMIOscillator DMIOscillator(int period, Brush upColor, Brush downColor)
		{
			return indicator.DMIOscillator(Input, period, upColor, downColor);
		}

		public Indicators.DMIOscillator DMIOscillator(ISeries<double> input , int period, Brush upColor, Brush downColor)
		{
			return indicator.DMIOscillator(input, period, upColor, downColor);
		}
	}
}

#endregion
