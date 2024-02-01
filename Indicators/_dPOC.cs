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
	public class _dPOC : Indicator
	{
        #region Propertis

        private Dictionary<double, long> cacheDictionary = new Dictionary<double, long>();

        #endregion

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "_dPOC";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "DPOC_Line");
				UpPoc = Brushes.Red;
				DnPoc = Brushes.Green;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnMarketData(MarketDataEventArgs e)
		{
            if (Bars.Count <= 0)
                return;

            if (e.MarketDataType == MarketDataType.Last && e.Price != 0)
            {

                double price = e.Price;
                long volume = e.Volume;

                if (!cacheDictionary.ContainsKey(price))
                {
                    cacheDictionary[price] = volume;
                }
                else
                {
                    cacheDictionary[price] += volume;
                }
                

            }

        }

		protected override void OnBarUpdate()
		{
            if (Bars.IsFirstBarOfSession)
            {
               	cacheDictionary.Clear();
              	//Plots[0].Brush = Brushes.Transparent;
				return;
            }



            using (var enumerator = cacheDictionary.GetEnumerator())
            {
                KeyValuePair<double, long> max = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    var kv = enumerator.Current;
                    if (kv.Value > max.Value)
                        max = kv;
                }
                Values[0][0] = max.Key;
				
            }
			
				if(Close[0] < Values[0][0])
				{
					PlotBrushes[0][0] = UpPoc;
				}
				else if(Close[0] > Values[0][0])
				{
					PlotBrushes[0][0] = DnPoc;	
				}
			
		}
	
		#region Properties
		
		[NinjaScriptProperty]
		[XmlIgnore, Display(Name = "Brush from Un Poc", GroupName = "Color", Order = 1)] 
		[Browsable(true)]
		public Brush UpPoc
		{
			get; set;
		}
		
		[Browsable(false)]
		public string UpPocSerialize
		{
			get{return Serialize.BrushToString(UpPoc);}
			set{UpPoc = Serialize.StringToBrush(value);}
			
		}
		
		[NinjaScriptProperty]
		[XmlIgnore, Display(Name = "Brush from Dn Poc", GroupName = "Color", Order = 2)] 
		[Browsable(true)]
		public Brush DnPoc
		{
			get; set;
		}
		
		[Browsable(false)]
		public string DnPocSerialize
		{
			get{return Serialize.BrushToString(DnPoc);}
			set{DnPoc = Serialize.StringToBrush(value);}
			
		}
		
			
		#endregion
	
	}
	
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private _dPOC[] cache_dPOC;
		public _dPOC _dPOC(Brush upPoc, Brush dnPoc)
		{
			return _dPOC(Input, upPoc, dnPoc);
		}

		public _dPOC _dPOC(ISeries<double> input, Brush upPoc, Brush dnPoc)
		{
			if (cache_dPOC != null)
				for (int idx = 0; idx < cache_dPOC.Length; idx++)
					if (cache_dPOC[idx] != null && cache_dPOC[idx].UpPoc == upPoc && cache_dPOC[idx].DnPoc == dnPoc && cache_dPOC[idx].EqualsInput(input))
						return cache_dPOC[idx];
			return CacheIndicator<_dPOC>(new _dPOC(){ UpPoc = upPoc, DnPoc = dnPoc }, input, ref cache_dPOC);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators._dPOC _dPOC(Brush upPoc, Brush dnPoc)
		{
			return indicator._dPOC(Input, upPoc, dnPoc);
		}

		public Indicators._dPOC _dPOC(ISeries<double> input , Brush upPoc, Brush dnPoc)
		{
			return indicator._dPOC(input, upPoc, dnPoc);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators._dPOC _dPOC(Brush upPoc, Brush dnPoc)
		{
			return indicator._dPOC(Input, upPoc, dnPoc);
		}

		public Indicators._dPOC _dPOC(ISeries<double> input , Brush upPoc, Brush dnPoc)
		{
			return indicator._dPOC(input, upPoc, dnPoc);
		}
	}
}

#endregion
