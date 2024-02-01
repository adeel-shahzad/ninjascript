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

#endregion

//This namespace holds Bars types in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.BarsTypes
{
	public class NbsBarType : BarsType
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Used for plotting data for open close";
				Name										= "NbsBarType";
				BarsPeriod									= new BarsPeriod { BarsPeriodType = (BarsPeriodType) 15, Value = 1 };
				BuiltFrom									= BarsPeriodType.Minute;
				DaysToLoad									= 5;
				IsIntraday									= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack)
		{
			return 1; //replace with your bars logic
		}

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			
		}

		public override void ApplyDefaultBasePeriodValue(BarsPeriod period)
		{
			//replace with any default base period value logic
		}

		public override void ApplyDefaultValue(BarsPeriod period)
		{
			//replace with any default value logic
		}

		public override string ChartLabel(DateTime dateTime)
		{
			return "custom label logic here";
		}

		public override double GetPercentComplete(Bars bars, DateTime now)
		{
			return 1.0d; //replace with your bar percent logic
		}

	}
}
