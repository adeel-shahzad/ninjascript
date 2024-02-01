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
	public class DMIStochasticExtreme : Indicator
	{
		
		private int 	OverSoldStartBar;
		private int 	OverBoughtStartBar;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Stocks and Commodities - January 2013 - The DMI Stochastic";
				Name										= "DMIStochasticExtreme";
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
				ExtremeHighLevel					= 90;
				ExtremeLowLevel					= 10;
				PeriodD					= 10;
				PeriodK					= 3;
				PeriodOsc					= 10;
				Smoothing					= 3;
				ShowArrows					= true;
				ShowDiamonds					= true;
				ShowLines					= true;
				DrawingObjectOffset					= 1;
				AddPlot(Brushes.Orange, "DMIStochExtreme");
				AddLine(Brushes.DarkViolet, ExtremeLowLevel, "Lower");
				AddLine(Brushes.YellowGreen, ExtremeHighLevel, "Upper");
			}
		}

		protected override void OnBarUpdate()
		{
			DMIStochExtreme[0] = (StochasticsInput(DMIOscillator(PeriodOsc, Brushes.Blue, Brushes.Red), PeriodD, PeriodK, Smoothing).K[0]);
			
			if (ShowLines)
			{
				if (CrossAbove(DMIStochExtreme, ExtremeHighLevel, 1))
					OverSoldStartBar = CurrentBar;
			
				if (DMIStochExtreme[0] > ExtremeHighLevel)
				{
					double OverSoldLine = MIN(Low, (CurrentBar - OverSoldStartBar + 1))[0];
					Draw.Line(this, "OverSold" + OverSoldStartBar, CurrentBar - OverSoldStartBar, OverSoldLine - TickSize * DrawingObjectOffset, 0, OverSoldLine - TickSize * DrawingObjectOffset, Brushes.Blue);
				}
				
				if (CrossBelow(DMIStochExtreme, ExtremeLowLevel, 1))
					OverBoughtStartBar = CurrentBar;
											
				if (DMIStochExtreme[0] < ExtremeLowLevel)
				{
					double OverBoughtLine = MAX(High, (CurrentBar - OverBoughtStartBar + 1))[0];
					Draw.Line(this, "OverBought" + OverBoughtStartBar, CurrentBar - OverBoughtStartBar, OverBoughtLine + TickSize * DrawingObjectOffset, 0, OverBoughtLine + TickSize * DrawingObjectOffset, Brushes.Blue);
				}
			}
			
			if (ShowArrows)
			{
				if (CrossAbove(DMIStochExtreme, ExtremeLowLevel, 1))
				{	
					DrawOnPricePanel = false;
					Draw.ArrowUp(this, "ArrowUp" + CurrentBar, true, 0, ExtremeLowLevel - 5, Brushes.Green);
					DrawOnPricePanel = true;
					
					if (DMIOscillator(PeriodOsc, Brushes.Blue, Brushes.Red)[0] < 0)
						Draw.Text(this, "NoEntry" + CurrentBar, "X", 0, High[0] + TickSize * DrawingObjectOffset, Brushes.Black);
				}
				
				if (CrossBelow(DMIStochExtreme, ExtremeHighLevel, 1))
				{
					DrawOnPricePanel = false;
					Draw.ArrowDown(this, "ArrowDown" + CurrentBar, true, 0, ExtremeHighLevel + 5, Brushes.Red);
					DrawOnPricePanel = true;
					
					if (DMIOscillator(PeriodOsc, Brushes.Blue, Brushes.Red)[0] < 0)
						Draw.Text(this, "NoEntry" + CurrentBar, "X", 0, High[0] + TickSize * DrawingObjectOffset, Brushes.Black);
				}
			}
			
			if (ShowDiamonds)
			{
				if (CrossAbove(DMIStochExtreme, SMA(DMIStochExtreme, 3), 1))
					Draw.Diamond(this, "DiamondBelow" + CurrentBar, true, 0, Low[0] - TickSize * DrawingObjectOffset, Brushes.Green);
						
				if (CrossBelow(DMIStochExtreme, SMA(DMIStochExtreme, 3), 1))
				{	
					if (DMIOscillator(PeriodOsc, Brushes.Blue, Brushes.Red)[0] < 0)
						Draw.Diamond(this, "DiamondAbove" + CurrentBar, true, 0, High[0] + TickSize * DrawingObjectOffset * 2, Brushes.Red); //In case X and Diamond are drawn on same bar.
					
					else
						Draw.Diamond(this, "DiamondAbove" + CurrentBar, true, 0, High[0] + TickSize * DrawingObjectOffset, Brushes.Red);
				}
			}
		}

		#region Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="ExtremeHighLevel", Order=1, GroupName="Parameters")]
		public int ExtremeHighLevel
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="ExtremeLowLevel", Order=2, GroupName="Parameters")]
		public int ExtremeLowLevel
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="PeriodD", Order=3, GroupName="Parameters")]
		public int PeriodD
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="PeriodK", Order=4, GroupName="Parameters")]
		public int PeriodK
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="PeriodOsc", Order=5, GroupName="Parameters")]
		public int PeriodOsc
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Smoothing", Order=6, GroupName="Parameters")]
		public int Smoothing
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowArrows", Order=7, GroupName="Parameters")]
		public bool ShowArrows
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowDiamonds", Order=8, GroupName="Parameters")]
		public bool ShowDiamonds
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowLines", Order=9, GroupName="Parameters")]
		public bool ShowLines
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="DrawingObjectOffset", Order=10, GroupName="Parameters")]
		public int DrawingObjectOffset
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DMIStochExtreme
		{
			get { return Values[0]; }
		}


		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DMIStochasticExtreme[] cacheDMIStochasticExtreme;
		public DMIStochasticExtreme DMIStochasticExtreme(int extremeHighLevel, int extremeLowLevel, int periodD, int periodK, int periodOsc, int smoothing, bool showArrows, bool showDiamonds, bool showLines, int drawingObjectOffset)
		{
			return DMIStochasticExtreme(Input, extremeHighLevel, extremeLowLevel, periodD, periodK, periodOsc, smoothing, showArrows, showDiamonds, showLines, drawingObjectOffset);
		}

		public DMIStochasticExtreme DMIStochasticExtreme(ISeries<double> input, int extremeHighLevel, int extremeLowLevel, int periodD, int periodK, int periodOsc, int smoothing, bool showArrows, bool showDiamonds, bool showLines, int drawingObjectOffset)
		{
			if (cacheDMIStochasticExtreme != null)
				for (int idx = 0; idx < cacheDMIStochasticExtreme.Length; idx++)
					if (cacheDMIStochasticExtreme[idx] != null && cacheDMIStochasticExtreme[idx].ExtremeHighLevel == extremeHighLevel && cacheDMIStochasticExtreme[idx].ExtremeLowLevel == extremeLowLevel && cacheDMIStochasticExtreme[idx].PeriodD == periodD && cacheDMIStochasticExtreme[idx].PeriodK == periodK && cacheDMIStochasticExtreme[idx].PeriodOsc == periodOsc && cacheDMIStochasticExtreme[idx].Smoothing == smoothing && cacheDMIStochasticExtreme[idx].ShowArrows == showArrows && cacheDMIStochasticExtreme[idx].ShowDiamonds == showDiamonds && cacheDMIStochasticExtreme[idx].ShowLines == showLines && cacheDMIStochasticExtreme[idx].DrawingObjectOffset == drawingObjectOffset && cacheDMIStochasticExtreme[idx].EqualsInput(input))
						return cacheDMIStochasticExtreme[idx];
			return CacheIndicator<DMIStochasticExtreme>(new DMIStochasticExtreme(){ ExtremeHighLevel = extremeHighLevel, ExtremeLowLevel = extremeLowLevel, PeriodD = periodD, PeriodK = periodK, PeriodOsc = periodOsc, Smoothing = smoothing, ShowArrows = showArrows, ShowDiamonds = showDiamonds, ShowLines = showLines, DrawingObjectOffset = drawingObjectOffset }, input, ref cacheDMIStochasticExtreme);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DMIStochasticExtreme DMIStochasticExtreme(int extremeHighLevel, int extremeLowLevel, int periodD, int periodK, int periodOsc, int smoothing, bool showArrows, bool showDiamonds, bool showLines, int drawingObjectOffset)
		{
			return indicator.DMIStochasticExtreme(Input, extremeHighLevel, extremeLowLevel, periodD, periodK, periodOsc, smoothing, showArrows, showDiamonds, showLines, drawingObjectOffset);
		}

		public Indicators.DMIStochasticExtreme DMIStochasticExtreme(ISeries<double> input , int extremeHighLevel, int extremeLowLevel, int periodD, int periodK, int periodOsc, int smoothing, bool showArrows, bool showDiamonds, bool showLines, int drawingObjectOffset)
		{
			return indicator.DMIStochasticExtreme(input, extremeHighLevel, extremeLowLevel, periodD, periodK, periodOsc, smoothing, showArrows, showDiamonds, showLines, drawingObjectOffset);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DMIStochasticExtreme DMIStochasticExtreme(int extremeHighLevel, int extremeLowLevel, int periodD, int periodK, int periodOsc, int smoothing, bool showArrows, bool showDiamonds, bool showLines, int drawingObjectOffset)
		{
			return indicator.DMIStochasticExtreme(Input, extremeHighLevel, extremeLowLevel, periodD, periodK, periodOsc, smoothing, showArrows, showDiamonds, showLines, drawingObjectOffset);
		}

		public Indicators.DMIStochasticExtreme DMIStochasticExtreme(ISeries<double> input , int extremeHighLevel, int extremeLowLevel, int periodD, int periodK, int periodOsc, int smoothing, bool showArrows, bool showDiamonds, bool showLines, int drawingObjectOffset)
		{
			return indicator.DMIStochasticExtreme(input, extremeHighLevel, extremeLowLevel, periodD, periodK, periodOsc, smoothing, showArrows, showDiamonds, showLines, drawingObjectOffset);
		}
	}
}

#endregion
