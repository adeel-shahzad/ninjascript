//
// Copyright (C) 2018, NinjaTrader LLC <www.ninjatrader.com>.
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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.AUN_Indi.Ehlers;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.NBS
{
    public class NqSystemEhlersStrategy : Strategy
    {
        private EhlersDecyclerII _ehlersDecyclerII;
        private EhlersMotherOfAdaptiveMovingAverages _ehlersMotherOfAdaptiveMovingAverages;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                SetDefaults();
                Calculate = Calculate.OnBarClose;
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                _ehlersDecyclerII = EhlersDecyclerII(Close, 125, 0.001d, Brushes.Red, Brushes.LimeGreen);
                _ehlersMotherOfAdaptiveMovingAverages = EhlersMotherOfAdaptiveMovingAverages(Close, 0.5d, 0.05d, Brushes.Red, Brushes.LimeGreen);

                _ehlersDecyclerII.Displacement = 1;
                _ehlersMotherOfAdaptiveMovingAverages.Displacement = 1;

                AddChartIndicator(_ehlersDecyclerII);
                AddChartIndicator(_ehlersMotherOfAdaptiveMovingAverages);
            }
        }

        private void SetDefaults()
        {
            Name = "NqSystemEhlersStrategy";
            Description = "Strategy based on Ehlers Indicators";
        }

        protected override void OnBarUpdate()
        {
            if (State == State.Historical) return;

            double ehlerDown = _ehlersDecyclerII.DecycleOffsetDown[0];
            double closingPrice = Close[0];
            
            if (CrossAbove(Close, _ehlersDecyclerII.DecycleOffsetDown, 1))
                Print($"Has crossed above the Ehler Decycler(down): {ehlerDown}, closingPrice: {closingPrice} and Time[0]: {Time[0]}");
            else if (CrossBelow(Close, _ehlersDecyclerII.DecycleOffsetDown, 1))
                Print($"Has crossed below the Ehler Decycler(down): {ehlerDown}, closingPrice: {closingPrice} and Time[0]: {Time[0]}");
        }

        //#region Properties
        //[Range(1, int.MaxValue), NinjaScriptProperty]
        //[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
        //public int Fast
        //{ get; set; }

        //[Range(1, int.MaxValue), NinjaScriptProperty]
        //[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
        //public int Slow
        //{ get; set; }
        //#endregion
    }
}