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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	// Converted to NT8-b3 by Ninjscript team 8/9/15.
	
	public class DerivativeOscillator : Indicator
	{
		    private int period = 14; // Default setting for Period
            private int smooth1 = 5; // Default setting for Smooth1
            private int smooth2 = 3; // Default setting for Smooth2
            private int smooth3 = 9; // Default setting for Smooth3  

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Constance Brown's Derivative Oscillator as pusblished in 'Technical Analysis for the Trading Professional' p. 293";
				Name						= "DerivativeOscillator";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;

				AddPlot(new Stroke(Brushes.Blue, DashStyleHelper.Dash, 2), PlotStyle.Bar, "DerivativeOscUp");				
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "DerivativeOscDn");
				AddPlot(new Stroke (Brushes.Gold,2), PlotStyle.Line, "DerivativeOsc");
				AddLine(Brushes.DarkGray, 0, "Zero");
			}
			else if (State == State.Configure)
			{
				Plots[0].Min = 0;
				Plots[1].Max = 0;
			}
		}

		protected override void OnBarUpdate()
		{
			double dsmooth1 = EMA(EMA(RSI(Input, Period, 1), Smooth1), Smooth2)[0];
			double dsmooth2 = SMA(EMA(EMA(RSI(Input, Period, 1), Smooth1), Smooth2), Smooth3)[0];
			
			DerivativeOsc[0]	= 	(dsmooth1 - dsmooth2);
            DerivativeOscUp[0] 	= 	(dsmooth1 - dsmooth2);
			DerivativeOscDn[0]	=	(dsmooth1 - dsmooth2);
		}

		#region Properties
		[Range(2, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Period", Description="Number of bars used in calculations", Order=1, GroupName="Parameters")]
		public int Period
		{
            get { return period; }
            set { period = Math.Max(2, value); }
        }
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Smooth1", Description="Length of smoothing EMA 1", Order=2, GroupName="Parameters")]
		public int Smooth1
        {
            get { return smooth1; }
            set { smooth1 = Math.Max(1, value); }
        }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Smooth2", Description="Length of smoothing EMA 2", Order=3, GroupName="Parameters")]
		public int Smooth2
        {
            get { return smooth2; }
            set { smooth2 = Math.Max(1, value); }
        }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Smooth3", Description="Length of final smoothing SMA", Order=4, GroupName="Parameters")]
		public int Smooth3
        {
            get { return smooth3; }
            set { smooth3 = Math.Max(1, value); }
        }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DerivativeOscUp
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DerivativeOscDn
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DerivativeOsc
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DerivativeOscillator[] cacheDerivativeOscillator;
		public DerivativeOscillator DerivativeOscillator(int period, int smooth1, int smooth2, int smooth3)
		{
			return DerivativeOscillator(Input, period, smooth1, smooth2, smooth3);
		}

		public DerivativeOscillator DerivativeOscillator(ISeries<double> input, int period, int smooth1, int smooth2, int smooth3)
		{
			if (cacheDerivativeOscillator != null)
				for (int idx = 0; idx < cacheDerivativeOscillator.Length; idx++)
					if (cacheDerivativeOscillator[idx] != null && cacheDerivativeOscillator[idx].Period == period && cacheDerivativeOscillator[idx].Smooth1 == smooth1 && cacheDerivativeOscillator[idx].Smooth2 == smooth2 && cacheDerivativeOscillator[idx].Smooth3 == smooth3 && cacheDerivativeOscillator[idx].EqualsInput(input))
						return cacheDerivativeOscillator[idx];
			return CacheIndicator<DerivativeOscillator>(new DerivativeOscillator(){ Period = period, Smooth1 = smooth1, Smooth2 = smooth2, Smooth3 = smooth3 }, input, ref cacheDerivativeOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DerivativeOscillator DerivativeOscillator(int period, int smooth1, int smooth2, int smooth3)
		{
			return indicator.DerivativeOscillator(Input, period, smooth1, smooth2, smooth3);
		}

		public Indicators.DerivativeOscillator DerivativeOscillator(ISeries<double> input , int period, int smooth1, int smooth2, int smooth3)
		{
			return indicator.DerivativeOscillator(input, period, smooth1, smooth2, smooth3);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DerivativeOscillator DerivativeOscillator(int period, int smooth1, int smooth2, int smooth3)
		{
			return indicator.DerivativeOscillator(Input, period, smooth1, smooth2, smooth3);
		}

		public Indicators.DerivativeOscillator DerivativeOscillator(ISeries<double> input , int period, int smooth1, int smooth2, int smooth3)
		{
			return indicator.DerivativeOscillator(input, period, smooth1, smooth2, smooth3);
		}
	}
}

#endregion
