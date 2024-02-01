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
	public class PMO : Indicator
	{
		private Series<double> CustomSmoothInput;
		private CustomSmoothFunc CustomSmoothFunc1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"PMO written to be analogous to the CPMO indicator published in the August 2020 Stocks and Commodities article titled 'The Compare Price Momentum Oscillator' by Vitali Apirine. Multiple instances of this indicator may be added to your chart or script for comparative function.";
				Name										= "PMO";
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
				Period1										= 35;
				Period2										= 20;
				Period3										= 10;
				AddPlot(Brushes.Orange, "PMOLine");
				AddPlot(Brushes.Transparent, "PMOSignal");
			}
			else if (State == State.DataLoaded)
			{
				CustomSmoothInput = new Series<double>(this);
				CustomSmoothFunc1 = CustomSmoothFunc(CustomSmoothInput, Period1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;
			if (Input[0] == 0 || Input[1] == 0)
				return;
			
			CustomSmoothInput[0] = (Input[0] / Input[1]) * 100 - 100;
			PMOLine[0] = 10 * CustomSmoothFunc(CustomSmoothFunc1, Period2)[0];
			PMOSignal[0] = EMA(Values[0], Period3)[0];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period1", Order=1, GroupName="Parameters")]
		public int Period1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period2", Order=2, GroupName="Parameters")]
		public int Period2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PMO Signal Period", Order=3, GroupName="Parameters")]
		public int Period3
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PMOLine
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PMOSignal
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
		private PMO[] cachePMO;
		public PMO PMO(int period1, int period2, int period3)
		{
			return PMO(Input, period1, period2, period3);
		}

		public PMO PMO(ISeries<double> input, int period1, int period2, int period3)
		{
			if (cachePMO != null)
				for (int idx = 0; idx < cachePMO.Length; idx++)
					if (cachePMO[idx] != null && cachePMO[idx].Period1 == period1 && cachePMO[idx].Period2 == period2 && cachePMO[idx].Period3 == period3 && cachePMO[idx].EqualsInput(input))
						return cachePMO[idx];
			return CacheIndicator<PMO>(new PMO(){ Period1 = period1, Period2 = period2, Period3 = period3 }, input, ref cachePMO);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PMO PMO(int period1, int period2, int period3)
		{
			return indicator.PMO(Input, period1, period2, period3);
		}

		public Indicators.PMO PMO(ISeries<double> input , int period1, int period2, int period3)
		{
			return indicator.PMO(input, period1, period2, period3);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PMO PMO(int period1, int period2, int period3)
		{
			return indicator.PMO(Input, period1, period2, period3);
		}

		public Indicators.PMO PMO(ISeries<double> input , int period1, int period2, int period3)
		{
			return indicator.PMO(input, period1, period2, period3);
		}
	}
}

#endregion
