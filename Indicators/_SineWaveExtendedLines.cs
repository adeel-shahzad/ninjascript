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
using System.IO;
#endregion

/// QUICK MODIFICATOIN BY PM OF BELOW INDICATOR BY fxstill, ALSO USING THE 
/// MarketStructuresLite SCRIPT ALSO BY fxstill
///+--------------------------------------------------------------------------------------------------+
///|   Site:     https://fxstill.com                                                                  |
///|   Telegram: https://t.me/fxstill (Literature on cryptocurrencies, development and code. )        |
///|                                   Don't forget to subscribe!                                     |
///+--------------------------------------------------------------------------------------------------+

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class _SineWaveExtendedLines : Indicator
	{
		#region CLASS LEVEL VARIABLES
		
			private Series<double> smooth;
			private Series<double> cycle;
			private Series<double> Q1;
			private Series<double> I1;
			private Series<double> deltaPhase;
			private Series<double> InstPeriod;
			private Series<double> per;
			
			private int MINBAR = 7;
			
			private int crossBarUp, crossBarDown;
			
			private double ha1, ha2, ha22, tpi;
			private int length, sz;
		
			#region doOnce Bool Switch Class Scope Variables
			
				private bool doOnce = false; //https://ninjatrader.com/support/forum/forum/ninjatrader-8/indicator-development/105000-how-to-fire-alarm-sound-only-once-per-bar
			
			#endregion
		
		#endregion

		protected override void OnStateChange()
		{
			#region 00. State.SetDefaults
			
				if (State == State.SetDefaults)
				{
					Description									= @"The Sinewave Indicator:John Ehlers, Cybernetic Analysis For Stocks And Futures., pg.154-155";
					Name										= "_SineWaveExtendedLines";
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
					Alpha1					= 0.07;
					Length					= 5;
					AddPlot(Brushes.DodgerBlue, "Sine");
					AddPlot(Brushes.Red, "Lsine");
						
					#region Sounds Alerts Bool Switches
					
						HLinesCUpColorBool						= true;
						HLinesCDownColorBool					= true;
					
					#endregion
					
					
					#region Horizontal Lines Bool Switches
					
						HLinesCUpColorOnOff						= true;
						HLinesCDownColorOnOff					= true;
					
					#endregion
					
					#region Dots Bool Switches
					
						DotsCUpOnOff							= true;
						DotsCDownOnOff							= true;
					
					#endregion


					#region Horizontal Lines Colors Selectors

						HLinesCUpColor							= Brushes.White;
						HLinesCDownColor						= Brushes.Red;
					
					#endregion
					
					#region Dots Colors Selectors

						DotsCUpColor							= Brushes.Cyan;
						DotsCDownColor							= Brushes.Magenta;
					
					#endregion
				}

			#endregion
				
			#region 01. State.Configure
				
				else if (State == State.Configure)
				{
	   				ha1 = Math.Pow(1 - 0.5 * Alpha1, 2);
	   				ha2 = 1 - Alpha1;
	   				ha22 = Math.Pow(ha2, 2);
	   				ha2 *= 2;
	   				length = (Length > 7)? 7: Length;
	   				sz = 0;		
					tpi = 2 * Math.PI;
					
				}

			#endregion
				
			#region 02. State.DataLoaded
				
				else if (State == State.DataLoaded)
				{				
					smooth = new Series<double>(this);
					cycle = new Series<double>(this);
					Q1 = new Series<double>(this);
					I1 = new Series<double>(this);
					deltaPhase = new Series<double>(this);
					InstPeriod = new Series<double>(this);
					per = new Series<double>(this);
				}

			#endregion

		}

		
		protected override void OnBarUpdate()
		{
			#region 00. BARS CHECK
			
				if (CurrentBar <= MINBAR) return;
			
			#endregion

					
			#region 01. SINE WAVE CODE (BY ANDREI60'S - UNTOUCHED)
			
				smooth[0] = (Median[0] + 2 * Median[1] + 2 * Median[2] + Median[3]) / 6;
	   			cycle[0]  =  ha1 * (smooth[0] - 2 * smooth[1] + smooth[2]) + 
	            	         ha2 * cycle[1] - ha22 * cycle[2];
	   			Q1[0]     = (0.0962 * cycle[0] + 0.5769 * cycle[2] - 0.5769 * cycle[4] -
	            	         0.0962 * cycle[6]) * (0.5 + 0.08 * InstPeriod[1] );  
	   			I1[0]     = cycle[3];
	   			if (Q1[0] != 0 && Q1[1] != 0) {
	      			deltaPhase[0] = (I1[0] / Q1[0] - I1[1] / Q1[1]) / 
	                		        (1 + I1[0] * I1[1] / (Q1[0] * Q1[1])  );
	   			} else deltaPhase[0] = 0;
				
	   			if (deltaPhase[0] < 0.1) deltaPhase[0] = 0.1;
	   			if (deltaPhase[0] > 1.1) deltaPhase[0] = 1.1;     
	   			double medianDelta = GetMedian(deltaPhase, length);
	   			double dc = (medianDelta != 0)? 6.28318 / medianDelta + 0.5: 15;
	   			InstPeriod[0] = 0.33 * dc + 0.67 * InstPeriod[1];
	   			per[0] = 0.15 * InstPeriod[0] + 0.85 * per[1];
	   			int dcPeriod = (int)Math.Ceiling(per[0]);
	   			double real = 0.0, imag = 0.0;
				for(int pos = 0;  pos < dcPeriod; pos++) { 
	      			real += Math.Sin(tpi * pos / dcPeriod) * cycle[pos];
	      			imag += Math.Cos(tpi * pos / dcPeriod) * cycle[pos];
	   			}
	   			double dcPhase = (Math.Abs(imag) > 0.001)? Math.Atan(real / imag) * 180 / Math.PI: 90 * Math.Sign(real);
	   			dcPhase += 90;
	   			dcPhase = (imag < 0)? dcPhase + 180: dcPhase;
	   			dcPhase = (dcPhase > 315)? dcPhase - 360: dcPhase;
				
	   			Sine[0]  = Math.Sin( dcPhase * Math.PI / 180);
	   			Lsine[0] = Math.Sin((dcPhase + 45) * Math.PI / 180);
			
			#endregion

			
			#region 02. FONT VARIABLE
			
				// NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Font", 12) { Size = 15, Bold = true };
			
			#endregion
				

			#region 03.0. CURRENTBARS TRAP
				
				if(CrossBelow(Values[0], Values[1], 1))
				{
					crossBarUp = CurrentBar;
					//Print("ax crossBarUp : " + crossBarUp);
				}
				//Print("ZZ :" +crossBarUp);

				#region REFERENCE
				
					// https://forum.ninjatrader.com/forum/ninjatrader-8/strategy-development/1061585-how-to-index-the-last-bar-before-ema-cross-above?p=1061615#post1061615
				
				#endregion
				
				
				if(CrossAbove(Values[0], Values[1], 1))
				{
					crossBarDown = CurrentBar;
					//Print("ax crossBarDown : " + crossBarDown);
				}
				Print("ZZ :" +crossBarDown);
			
			#endregion
					
			#region 03.1. EXTENDED LINES, DOTS AND SOUNDS SNIPPETS
				
				if( CrossAbove(Values[0], Values[1], 1)
					&& (doOnce == false) )
				{
					#region LINE LENGTH BASED ON PRIOR SIGNAL BARS
					
						// Draws the line not till next crossover candle but with minus previous nb of candles ot previous crossover candle
						//Draw.Line(this, "A1"+CurrentBar.ToString(), false, 1, High[0] + (1* TickSize), -(CurrentBar-crossBar), High[0] + (1* TickSize), Brushes.White, DashStyleHelper.Dot, 5);
					
					#endregion
					
					#region UP SIGNAL HORIZONTAL LINES DRAWINGS
					
						if (HLinesCUpColorOnOff)
						{
							
							Draw.Line(this, 
								"A1"+CurrentBar.ToString(), 
								false, 
								Time[CurrentBar-crossBarUp], 
								Low[CurrentBar-crossBarUp], 
								Time[0], 
								Low[CurrentBar-crossBarUp], 
								HLinesCUpColor, 
								DashStyleHelper.Dot, 
								5);
						}
					
					#endregion

					#region UP SIGNAL DOTS DRAWINGS
					
						if (DotsCUpOnOff)
						{
							Draw.Dot(this, "C"+CurrentBar.ToString(), true, 1, High[0], DotsCUpColor);
						}
					
					#endregion

					#region UP SIGNAL SOUND ALERTS
					
						if(HLinesCUpColorBool)
						{
							PlaySound(@"C:\Program Files\NinjaTrader 8\sounds\" + SoundFilesHLinesCUpColor);
							
							doOnce = true; // prevent additional alerts
						}

					#endregion
				

					#region PRINTS
					
						/*
							Print(" ##### ---");
							Print("Time[crossBarUp] : " + Time[crossBarUp]);
							Print("Time[CurrentBar-crossBarUp] : " + Time[CurrentBar-crossBarUp]);
							Print("crossBarUp : " + crossBarUp);
							Print("Low[crossBarUp] - (3 * TickSize) : " + (Low[crossBarUp] - (3 * TickSize)));
							Print("Low[CurrentBar-crossBarUp] - (3 * TickSize) : " + (Low[CurrentBar-crossBarUp] - (3 * TickSize)));
							Print("Time[0] : " + Time[0]);
						
						*/
					
					#endregion
					
					#region CURRENTBAR VS CROSSBAR
					
						/*
						Draw.Text(this, 
							"test1"+CurrentBar, 
							false, 
							CurrentBar.ToString() + " = \n CurrentBar", 
							2, 
							High[0] + (3 * TickSize), 
							0, 
							Brushes.White, 
							myFont, 
							TextAlignment.Left, 
							Brushes.Transparent, 
							null, 
							1);
						
						Draw.Text(this, 
							"test2"+CurrentBar, 
							false, 
							crossBarUp.ToString() + " = \n crossBarUp", 
							2, 
							High[0] + (9 * TickSize), 
							0, 
							Brushes.Magenta, 
							myFont, 
							TextAlignment.Left, 
							Brushes.Transparent, 
							null, 
							1);
						*/
					
					#endregion

				}
				else if ( CrossBelow(Values[0], Values[1], 1)
						&& (doOnce == false) ) 
				{
					#region DOWN SIGNAL HORIZONTAL LINES DRAWINGS
					
						if (HLinesCDownColorOnOff)
						{						
							Draw.Line(this, 
								"B1"+CurrentBar.ToString(), 
								false, 
								Time[CurrentBar-crossBarDown], 
								High[CurrentBar-crossBarDown], 
								Time[0], 
								High[CurrentBar-crossBarDown], 
								HLinesCDownColor, 
								DashStyleHelper.Dot, 
								5);
						}
					
					#endregion

					#region DOWN SIGNAL DOTS DRAWINGS
						
						if (DotsCDownOnOff)
						{
							Draw.Dot(this, "D"+CurrentBar.ToString(), true, 0, Low[0], DotsCDownColor);
						}
					
					#endregion

					#region DOWN SIGNAL SOUND ALERTS

						if(HLinesCDownColorBool)
						{
							PlaySound(@"C:\Program Files\NinjaTrader 8\sounds\" + SoundFilesHLinesCDownColor);
							
							doOnce = true; // prevent additional alerts
						}
					
					#endregion
					

					#region PRINTS
					
						/*
						Print(" ##### ---");
						Print("Time[0] : " + Time[0]);
						Print("Time[CurrentBar-crossBarDown] : " + Time[CurrentBar-crossBarDown]);
						Print("crossBarDown : " + crossBarDown);
						Print("High[crossBarDown] + (1 * TickSize) : " + (High[CurrentBar-crossBarDown] + (1 * TickSize)));
						Print("High[CurrentBar-crossBarDown] + (1 * TickSize) : " + (High[CurrentBar-crossBarDown] + (1 * TickSize)));
						Print("CurrentBar-crossBarDown : " + (CurrentBar-crossBarDown));
						Print("Time[0] : " + Time[0]);
						*/
					
					#endregion
					
					#region CURRENTBAR VS CROSSBAR
					
						/*
						Draw.Text(this, 
							"test3"+CurrentBar, 
							false, 
							CurrentBar.ToString() + " = \n CurrentBar", 
							1, 
							Low[0] - (3 * TickSize), 
							0, 
							Brushes.Red, 
							myFont, 
							TextAlignment.Left, 
							Brushes.Transparent, 
							null, 
							1);
						
						Draw.Text(this, 
							"test4"+CurrentBar, 
							false, 
							crossBarDown.ToString() + " = \n crossBarDown", 
							1, 
							Low[0] - (9 * TickSize), 
							0, 
							Brushes.Pink, 
							myFont, 
							TextAlignment.Left, 
							Brushes.Transparent, 
							null, 
							1);
						*/
					
					#endregion
				}
			
			#endregion   
			
				
			#region 04. RESET doOnce BOOL FLAG
			
				if (IsFirstTickOfBar)
				{
					doOnce = false;
				}
				
			#endregion
		}

		#region Properties
					
			#region 00. Parameters
		
				[NinjaScriptProperty]
				[Range(0.01, double.MaxValue)]
				[Display(Name="Alpha1", Order=1, GroupName="00. Parameters")]
				public double Alpha1
				{ get; set; }

				[NinjaScriptProperty]
				[Range(1, int.MaxValue)]
				[Display(Name="Length", Order=2, GroupName="00. Parameters")]
				public int Length
				{ get; set; }

				[Browsable(false)]
				[XmlIgnore]
				public Series<double> Sine
				{
					get { return Values[0]; }
				}

				[Browsable(false)]
				[XmlIgnore]
				public Series<double> Lsine
				{
					get { return Values[1]; }
				}
			
			#endregion
				
				
			#region 01. Enable Crossover Signal Sounds
			
				[NinjaScriptProperty]
				[Display(Name="Crossover Up Line Sound Switch On/Off", Order=1, GroupName="01. Enable Crossover Signal Sounds")]
				public bool HLinesCUpColorBool
				{ get; set; }
			
				[NinjaScriptProperty]
				[Display(Name="Crossover Up Line Sound Switch On/Off", Order=2, GroupName="01. Enable Crossover Signal Sounds")]
				public bool HLinesCDownColorBool
				{ get; set; }
			
			#endregion	
				
			#region 02. Select Crossover Sounds
			
				[NinjaScriptProperty]
				[Display(Name="Crossover Up Line Sound", Order=1, GroupName="02. Select Crossover Sound")]
				[TypeConverter(typeof(NinjaTrader.NinjaScript.Indicators.SoundConverter_SineWaveExtendedLines))] 
				public string SoundFilesHLinesCUpColor
				{get;set;}
			
				[NinjaScriptProperty]
				[Display(Name="Crossover Down Line Sound", Order=2, GroupName="02. Select Crossover Sound")]
				[TypeConverter(typeof(NinjaTrader.NinjaScript.Indicators.SoundConverter_SineWaveExtendedLines))] 
				public string SoundFilesHLinesCDownColor
				{get;set;}
			
			#endregion


			#region 03. Enable Crossover Lines
				
				[NinjaScriptProperty]
				[Display(Name="Crossover Up Line Switch On/Off", Order=1, GroupName="03. Enable Crossover Lines")]
				public bool HLinesCUpColorOnOff
				{ get; set; }
				
				[NinjaScriptProperty]
				[Display(Name="Crossover Down Line Switch On/Off", Order=2, GroupName="03. Enable Crossover Lines")]
				public bool HLinesCDownColorOnOff
				{ get; set; }
			
			#endregion
				
			#region 04. Crossover Lines Colors

				[NinjaScriptProperty]
				[XmlIgnore]
				[Display(Name="Crossover Up Lines Color", Description="Color of line", Order=1, GroupName="04. Crossover Lines Colors")]
				public Brush HLinesCUpColor
				{ get; set; }

				[Browsable(false)]
				public string HLinesCUpColorSerializable
				{
					get { return Serialize.BrushToString(HLinesCUpColor); }
					set { HLinesCUpColor = Serialize.StringToBrush(value); }
				}
				

				[NinjaScriptProperty]
				[XmlIgnore]
				[Display(Name="Crossover Down Lines Color", Description="Color of line", Order=2, GroupName="04. Crossover Lines Colors")]
				public Brush HLinesCDownColor
				{ get; set; }

				[Browsable(false)]
				public string HLinesCDownColorSerializable
				{
					get { return Serialize.BrushToString(HLinesCDownColor); }
					set { HLinesCDownColor = Serialize.StringToBrush(value); }
				}
				
			#endregion


			#region 05. Enable Crossover Dots
				
				[NinjaScriptProperty]
				[Display(Name="Crossover Up Dots Switch On/Off", Order=1, GroupName="05. Enable Crossover Dots")]
				public bool DotsCUpOnOff
				{ get; set; }
				
				[NinjaScriptProperty]
				[Display(Name="Crossover Down Dots Switch On/Off", Order=2, GroupName="05. Enable Crossover Dots")]
				public bool DotsCDownOnOff
				{ get; set; }
			
			#endregion
				
			#region 06. Crossover Dots Colors

				[NinjaScriptProperty]
				[XmlIgnore]
				[Display(Name="Crossover Up Dots Color", Description="Color of line", Order=1, GroupName="06. Crossover Dots Colors")]
				public Brush DotsCUpColor
				{ get; set; }

				[Browsable(false)]
				public string DotsCUpColorSerializable
				{
					get { return Serialize.BrushToString(DotsCUpColor); }
					set { DotsCUpColor = Serialize.StringToBrush(value); }
				}
				

				[NinjaScriptProperty]
				[XmlIgnore]
				[Display(Name="Crossover Down Dots Color", Description="Color of line", Order=2, GroupName="06. Crossover Dots Colors")]
				public Brush DotsCDownColor
				{ get; set; }

				[Browsable(false)]
				public string DotsCDownColorSerializable
				{
					get { return Serialize.BrushToString(DotsCDownColor); }
					set { DotsCDownColor = Serialize.StringToBrush(value); }
				}
				
			#endregion


		#endregion

	}
	
	#region SoundConverter Class
	
		// using System.IO; https://forum.ninjatrader.com/forum/ninjatrader-8/indicator-development/1172753-sound-files-dropdown-picker?p=1172766#post1172766
		public class SoundConverter_SineWaveExtendedLines : TypeConverter
		{
			
			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				if (context == null)
				{
					return null;
				}
				//List <string> list;
				List <string> list = new List <string> ();
				
				
				DirectoryInfo dir = new DirectoryInfo(NinjaTrader.Core.Globals.InstallDir+ "sounds");
				
				FileInfo[] files= dir.GetFiles("*.wav");
				
				foreach (FileInfo file in files)
				{
					list.Add(file.Name);
				}
					
				
				return new TypeConverter.StandardValuesCollection(list);
			}
			
		

			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
				return true;
			}
		}
	
	#endregion
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private _SineWaveExtendedLines[] cache_SineWaveExtendedLines;
		public _SineWaveExtendedLines _SineWaveExtendedLines(double alpha1, int length, bool hLinesCUpColorBool, bool hLinesCDownColorBool, string soundFilesHLinesCUpColor, string soundFilesHLinesCDownColor, bool hLinesCUpColorOnOff, bool hLinesCDownColorOnOff, Brush hLinesCUpColor, Brush hLinesCDownColor, bool dotsCUpOnOff, bool dotsCDownOnOff, Brush dotsCUpColor, Brush dotsCDownColor)
		{
			return _SineWaveExtendedLines(Input, alpha1, length, hLinesCUpColorBool, hLinesCDownColorBool, soundFilesHLinesCUpColor, soundFilesHLinesCDownColor, hLinesCUpColorOnOff, hLinesCDownColorOnOff, hLinesCUpColor, hLinesCDownColor, dotsCUpOnOff, dotsCDownOnOff, dotsCUpColor, dotsCDownColor);
		}

		public _SineWaveExtendedLines _SineWaveExtendedLines(ISeries<double> input, double alpha1, int length, bool hLinesCUpColorBool, bool hLinesCDownColorBool, string soundFilesHLinesCUpColor, string soundFilesHLinesCDownColor, bool hLinesCUpColorOnOff, bool hLinesCDownColorOnOff, Brush hLinesCUpColor, Brush hLinesCDownColor, bool dotsCUpOnOff, bool dotsCDownOnOff, Brush dotsCUpColor, Brush dotsCDownColor)
		{
			if (cache_SineWaveExtendedLines != null)
				for (int idx = 0; idx < cache_SineWaveExtendedLines.Length; idx++)
					if (cache_SineWaveExtendedLines[idx] != null && cache_SineWaveExtendedLines[idx].Alpha1 == alpha1 && cache_SineWaveExtendedLines[idx].Length == length && cache_SineWaveExtendedLines[idx].HLinesCUpColorBool == hLinesCUpColorBool && cache_SineWaveExtendedLines[idx].HLinesCDownColorBool == hLinesCDownColorBool && cache_SineWaveExtendedLines[idx].SoundFilesHLinesCUpColor == soundFilesHLinesCUpColor && cache_SineWaveExtendedLines[idx].SoundFilesHLinesCDownColor == soundFilesHLinesCDownColor && cache_SineWaveExtendedLines[idx].HLinesCUpColorOnOff == hLinesCUpColorOnOff && cache_SineWaveExtendedLines[idx].HLinesCDownColorOnOff == hLinesCDownColorOnOff && cache_SineWaveExtendedLines[idx].HLinesCUpColor == hLinesCUpColor && cache_SineWaveExtendedLines[idx].HLinesCDownColor == hLinesCDownColor && cache_SineWaveExtendedLines[idx].DotsCUpOnOff == dotsCUpOnOff && cache_SineWaveExtendedLines[idx].DotsCDownOnOff == dotsCDownOnOff && cache_SineWaveExtendedLines[idx].DotsCUpColor == dotsCUpColor && cache_SineWaveExtendedLines[idx].DotsCDownColor == dotsCDownColor && cache_SineWaveExtendedLines[idx].EqualsInput(input))
						return cache_SineWaveExtendedLines[idx];
			return CacheIndicator<_SineWaveExtendedLines>(new _SineWaveExtendedLines(){ Alpha1 = alpha1, Length = length, HLinesCUpColorBool = hLinesCUpColorBool, HLinesCDownColorBool = hLinesCDownColorBool, SoundFilesHLinesCUpColor = soundFilesHLinesCUpColor, SoundFilesHLinesCDownColor = soundFilesHLinesCDownColor, HLinesCUpColorOnOff = hLinesCUpColorOnOff, HLinesCDownColorOnOff = hLinesCDownColorOnOff, HLinesCUpColor = hLinesCUpColor, HLinesCDownColor = hLinesCDownColor, DotsCUpOnOff = dotsCUpOnOff, DotsCDownOnOff = dotsCDownOnOff, DotsCUpColor = dotsCUpColor, DotsCDownColor = dotsCDownColor }, input, ref cache_SineWaveExtendedLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators._SineWaveExtendedLines _SineWaveExtendedLines(double alpha1, int length, bool hLinesCUpColorBool, bool hLinesCDownColorBool, string soundFilesHLinesCUpColor, string soundFilesHLinesCDownColor, bool hLinesCUpColorOnOff, bool hLinesCDownColorOnOff, Brush hLinesCUpColor, Brush hLinesCDownColor, bool dotsCUpOnOff, bool dotsCDownOnOff, Brush dotsCUpColor, Brush dotsCDownColor)
		{
			return indicator._SineWaveExtendedLines(Input, alpha1, length, hLinesCUpColorBool, hLinesCDownColorBool, soundFilesHLinesCUpColor, soundFilesHLinesCDownColor, hLinesCUpColorOnOff, hLinesCDownColorOnOff, hLinesCUpColor, hLinesCDownColor, dotsCUpOnOff, dotsCDownOnOff, dotsCUpColor, dotsCDownColor);
		}

		public Indicators._SineWaveExtendedLines _SineWaveExtendedLines(ISeries<double> input , double alpha1, int length, bool hLinesCUpColorBool, bool hLinesCDownColorBool, string soundFilesHLinesCUpColor, string soundFilesHLinesCDownColor, bool hLinesCUpColorOnOff, bool hLinesCDownColorOnOff, Brush hLinesCUpColor, Brush hLinesCDownColor, bool dotsCUpOnOff, bool dotsCDownOnOff, Brush dotsCUpColor, Brush dotsCDownColor)
		{
			return indicator._SineWaveExtendedLines(input, alpha1, length, hLinesCUpColorBool, hLinesCDownColorBool, soundFilesHLinesCUpColor, soundFilesHLinesCDownColor, hLinesCUpColorOnOff, hLinesCDownColorOnOff, hLinesCUpColor, hLinesCDownColor, dotsCUpOnOff, dotsCDownOnOff, dotsCUpColor, dotsCDownColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators._SineWaveExtendedLines _SineWaveExtendedLines(double alpha1, int length, bool hLinesCUpColorBool, bool hLinesCDownColorBool, string soundFilesHLinesCUpColor, string soundFilesHLinesCDownColor, bool hLinesCUpColorOnOff, bool hLinesCDownColorOnOff, Brush hLinesCUpColor, Brush hLinesCDownColor, bool dotsCUpOnOff, bool dotsCDownOnOff, Brush dotsCUpColor, Brush dotsCDownColor)
		{
			return indicator._SineWaveExtendedLines(Input, alpha1, length, hLinesCUpColorBool, hLinesCDownColorBool, soundFilesHLinesCUpColor, soundFilesHLinesCDownColor, hLinesCUpColorOnOff, hLinesCDownColorOnOff, hLinesCUpColor, hLinesCDownColor, dotsCUpOnOff, dotsCDownOnOff, dotsCUpColor, dotsCDownColor);
		}

		public Indicators._SineWaveExtendedLines _SineWaveExtendedLines(ISeries<double> input , double alpha1, int length, bool hLinesCUpColorBool, bool hLinesCDownColorBool, string soundFilesHLinesCUpColor, string soundFilesHLinesCDownColor, bool hLinesCUpColorOnOff, bool hLinesCDownColorOnOff, Brush hLinesCUpColor, Brush hLinesCDownColor, bool dotsCUpOnOff, bool dotsCDownOnOff, Brush dotsCUpColor, Brush dotsCDownColor)
		{
			return indicator._SineWaveExtendedLines(input, alpha1, length, hLinesCUpColorBool, hLinesCDownColorBool, soundFilesHLinesCUpColor, soundFilesHLinesCDownColor, hLinesCUpColorOnOff, hLinesCDownColorOnOff, hLinesCUpColor, hLinesCDownColor, dotsCUpOnOff, dotsCDownOnOff, dotsCUpColor, dotsCDownColor);
		}
	}
}

#endregion
