using NinjaTrader.Cbi;
using NinjaTrader.CQG.ProtoBuf;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.NinjaScript.Strategies.NBS
{
    public class TestStrategy : Strategy
    {
        private const string EnterLongName = "Adeel>BL";
        private const string EnterShortName = "Adeel>SS";
        private const string ExitLongName = "Adeel>LX";
        private const string ExitShortName = "Adeel>SX";

        private bool _entryLong, _entryShort, _traceMyOrders;
        private double _stopTicks, _profitTicks;
        private int _posSize;
        private Cbi.Order _longEntryOrder, _shortEntryOrder;

        [NinjaScriptProperty, Range(100, 500)]
        [Display(Name = "A Period", GroupName = "A Parameters", Order = 0)]
        public int APeriod { get; set; }

        [NinjaScriptProperty, Range(100, 500)]
        [Display(Name = "Z Period", GroupName = "Z Parameters", Order = 0)]
        public int ZPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Position Size", GroupName = "Parameters", Order = 0)]
        public int PositionSize { get; set; }

        [NinjaScriptProperty, Range(10, 20)]
        [Display(Name = "Stop Ticks", GroupName = "Parameters", Order = 0)]
        public int StopTicks { get; set; }

        [NinjaScriptProperty, Range(10, 20)]
        [Display(Name = "Profit Ticks", GroupName = "Parameters", Order = 0)]
        public int ProfitTicks { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trace Orders", GroupName = "Parameters", Order = 0)]
        public bool TraceMyOrders { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                SetDefaults();
                SetParameters();
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();
                _posSize = PositionSize;
                _stopTicks = StopTicks;
                _profitTicks = ProfitTicks;
                TraceOrders = TraceMyOrders;
            }
            else if (State == State.DataLoaded)
            {
            }
        }

        private void SetDefaults()
        {
            Description = @"Trade with profit and stop orders.";
            Name = "TestStrategy";
            TraceOrders = TraceMyOrders;
            APeriod = 100;
            ZPeriod = 100;
            IsExitOnSessionCloseStrategy = false;
            Slippage = 0;
            Calculate = Calculate.OnBarClose;
        }

        private void SetParameters()
        {
            PositionSize = 3;
            StopTicks = 20;
            ProfitTicks = 20;
            TraceMyOrders = false;
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1 || State == State.Historical)
                return;

            CreateEntrySignal();
            TradeOnSignal();
        }

        private void CreateEntrySignal()
        {
            Print($"Open[0]: {Open[0]}, Close[0]: {Close[0]}, High[1]: {High[1]}, Low[1]: {Low[1]}");
            if (Close[0] > Open[0] && Open[0] > High[1])
            {
                _entryLong = true;
            }
            if (Open[0] > Close[0] && Open[0] < Low[1])
            {
                _entryShort = true;
            }
        }

        private void TradeOnSignal()
        {
            if (Position.MarketPosition == MarketPosition.Flat)
            {
                if (_entryLong)
                {
                    Print("Entering long position");
                    _longEntryOrder = EnterLong(_posSize, EnterLongName);
                    _entryLong = false;
                }
                else if (_entryShort)
                {
                    Print("Entering short position");
                    _shortEntryOrder = EnterShort(_posSize, EnterShortName);
                    _entryShort = false;
                }
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            Cbi.Order order = execution.Order;
            if (order.Name.Equals(EnterLongName) && order.OrderState == OrderState.Filled && Position.MarketPosition == MarketPosition.Long)
            {
                Print("Going to place exit order for long position");
                double profit = order.AverageFillPrice + (TickSize * _profitTicks);
                double stop = order.AverageFillPrice - (TickSize * _stopTicks);
                Print($"profit: {profit}, stop: {stop}");
                ExitLongLimit(0, true, _posSize, profit, ExitLongName, EnterLongName);
                ExitLongStopMarket(0, true, _posSize, stop, ExitLongName, EnterLongName);
                _longEntryOrder = null;
            }
            else if (order.Name.Equals(EnterShortName) && order.OrderState == OrderState.Filled && Position.MarketPosition == MarketPosition.Short)
            {
                Print("Going to place exit order for short position");
                double profit = order.AverageFillPrice - (TickSize * _profitTicks);
                double stop = order.AverageFillPrice + (TickSize * _stopTicks);
                Print($"profit: {profit}, stop: {stop}");
                ExitShortLimit(0, true, _posSize, profit, ExitShortName, EnterShortName);
                ExitShortStopMarket(0, true, _posSize, stop, ExitShortName, EnterShortName);
                _shortEntryOrder = null;
            }
        }
    }
}
