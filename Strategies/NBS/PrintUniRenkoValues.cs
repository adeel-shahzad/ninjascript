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
using NinjaTrader.NinjaScript.BarsTypes;

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
            else if (State == State.Configure)
            {
                ClearOutputWindow();
            }
        }

        private void SetDefaults()
        {
            Name = "Print_UniRenko_Value";
            Description = "Access UniRenko Bars and get values from them.";
            Calculate = Calculate.OnEachTick;
            Slippage = 0;
            IsExitOnSessionCloseStrategy = false;
            ExitOnSessionCloseSeconds = 30;
        }

        protected override void OnBarUpdate()
        {
            //if (State == State.Historical) return;

            if (Bars.IsFirstBarOfSession)
            {
                Print($"Instrument.MasterInstrument.TickSize: {Instrument.MasterInstrument.TickSize}, Instrument.MasterInstrument.PointValue: {Instrument.MasterInstrument.PointValue}");
            }

            //Print($"Bars.Count: {Bars.Count} and Bars.LastBarTime: {Bars.LastBarTime}");
            int index = Bars.Count - 2;
            double c_high = Bars.GetHigh(index);
            double c_low = Bars.GetLow(index);
            double c_open = Bars.GetOpen(index);
            double c_close = Bars.GetClose(index);
            double p_high = Bars.GetHigh(index - 1);
            double p_low = Bars.GetLow(index - 1);
            double p_open = Bars.GetOpen(index - 1);
            double p_close = Bars.GetClose(index - 1);
            //Print($"Bars.BarsType.BarsPeriod.Value2: {Bars.BarsType.BarsPeriod.Value2}, Bars.BarsType.BarsPeriod.Value: {Bars.BarsType.BarsPeriod.Value}, Bars.GetType(): {Bars.GetType()}, Bars.BarsType: {Bars.BarsType}, Bars.GetAsk({index}): {Bars.GetAsk(index)}, Bars.GetClose({index}): {Bars.GetClose(index)}");
            double candleSizeValue = c_high - c_low;
            Print($"c_high - c_low -> {c_high - c_low}, (c_high - c_low) * 4 -> {(c_high - c_low) * 4}");
            Print($"Math.Abs(c_open - c_close) -> {Math.Abs(c_open - c_close)}, (Math.Abs(c_open - c_close)) * 4 -> {(Math.Abs(c_open - c_close)) * 4}");

        }
    }
}
