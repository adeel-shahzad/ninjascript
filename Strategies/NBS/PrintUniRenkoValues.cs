using NinjaTrader.Cbi;
using NinjaTrader.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.mah_Indicators;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Collections;
using NinjaTrader.Gui;

namespace NinjaTrader.NinjaScript.Strategies.NBS
{
    public class PrintUniRenkoValues : Strategy
    {
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                SetDefaults();
            }
        }

        private void SetDefaults()
        {
            Name = "Print_UniRenko_Value";
            Description = "Access UniRenko Bars and get values from them.";
            Calculate = Calculate.OnBarClose;
            Slippage = 0;
            IsExitOnSessionCloseStrategy = false;
            ExitOnSessionCloseSeconds = 30;
        }

        protected override void OnBarUpdate()
        {
            if (State == State.Historical) return;

            Print($"Bars.Count: {Bars.Count} and Bars.LastBarTime: {Bars.LastBarTime}");
            int index = Bars.Count - 2;
            Print($"Bars.BarsType.Name: {Bars.BarsType.Name}, Bars.GetAsk({index}): {Bars.GetAsk(index)}, Bars.GetClose({index}): {Bars.GetClose(index)}");
        }
    }
}
