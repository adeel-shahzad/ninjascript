// Import necessary namespaces
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
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
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;

namespace NinjaTrader.NinjaScript.Strategies.NBS
{
    public class NqSystem1Buttons : Strategy
    {
        private DerivativeOscillator _derivativeOscillator;
        private mahTrendGRaBer2 _mahTrendGRaBer2;
        private AuSuperTrendU11 _auSuperTrendU11;

        private const string LongEntryName1 = "BL1";
        private const string LongEntryName2 = "BL2";
        private const string LongEntryName3 = "BL3";
        private const string LongEntryName4 = "BL4";
        private const string LongEntryName5 = "BL5";
        private const string ShortEntryName1 = "SS1";
        private const string ShortEntryName2 = "SS2";
        private const string ShortEntryName3 = "SS3";
        private const string ShortEntryName4 = "SS4";
        private const string ShortEntryName5 = "SS5";

        private bool longButtonClicked;
        private bool shortButtonClicked;
        private Button longButton;
        private Button shortButton;
        private Grid myGrid;

        // Strategy parameters
        [NinjaScriptProperty]
        [Display(Name = "MA Period", GroupName = "Parameters", Order = 1)]
        public int MaPeriod { get; set; } = 40;
        [NinjaScriptProperty]
        [Display(Name = "MA Period 2", GroupName = "Parameters", Order = 2)]
        public int MaPeriod2 { get; set; } = 1;
        [NinjaScriptProperty]
        [Display(Name = "Jump", GroupName = "Parameters", Order = 3)]
        public int jump { get; set; } = 9;
        [NinjaScriptProperty]
        [Display(Name = "Jump 1", GroupName = "Parameters", Order = 4)]
        public int jump1 { get; set; } = 18;
        [NinjaScriptProperty]
        [Display(Name = "Jump 2", GroupName = "Parameters", Order = 5)]
        public int jump2 { get; set; } = 36;
        [NinjaScriptProperty]
        [Display(Name = "Jump 3", GroupName = "Parameters", Order = 6)]
        public int jump3 { get; set; } = 45;
        [NinjaScriptProperty]
        [Display(Name = "Jump 4", GroupName = "Parameters", Order = 7)]
        public int jump4 { get; set; } = 50;
        [NinjaScriptProperty]
        [Display(Name = "Trailing Stop Distance", GroupName = "Parameters", Order = 8)]
        public double TrailingStopDistance { get; set; } = 19;
        [NinjaScriptProperty]
        [Display(Name = "Profit Trigger", GroupName = "Parameters", Order = 9)]
        public double ProfitTrigger { get; set; } = 150;

        // variables for Limit Orders
        private double CurrentPrice = 0;
        private double LongLimitPrice = 20000;
        private double ShortLimitPrice = 13302;
        private double LineValue = 13302;
        private double FirstTargetLine = 0;
        private double SecondTargetLine = 0;
        private double ThirdTargetLine = 0;
        private double FourthTargetLine = 0;
        private double trailingStopLevel = 0;
        private bool profitReached = false;
        private Order _stopOrder;

        // Variables for EMA Bands and Heikin-Ashi
        private double maOpen = 0;
        private double maClose = 0;
        private double maLow = 0;
        private double maHigh = 0;
        private double haOpen = 0;
        private double haClose = 0;
        private double haLow = 0;
        private double haHigh = 0;
        private double ExtMapBuffer1 = 0;
        private double ExtMapBuffer2 = 0;
        private List<int> priceLineList = new List<int>();
        double stopLossShortPosition, stopLossLongPosition, profitTargetShortPosition, profitTargetLongPosition;
        private Order longEntryOrder = null, shortEntryOrder = null;


        // Initialize the strategy
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
                AddCustomControl();
                DrawPriceLines();
                Print($"Size of priceLineList: {priceLineList.Count}");
            }
            else if (State == State.DataLoaded)
            {
                _mahTrendGRaBer2 = mahTrendGRaBer2(Close, true);
                //_derivativeOscillator = DerivativeOscillator(89, 5, 3, 9);
                //_auSuperTrendU11 = AuSuperTrendU11(Close, AuSuperTrendU11BaseType.Median, AuSuperTrendU11OffsetType.Default, AuSuperTrendU11VolaType.True_Range, false, 1, 3, 22);

                AddChartIndicator(_mahTrendGRaBer2);
                //AddChartIndicator(_derivativeOscillator);
                //AddChartIndicator(_auSuperTrendU11);
            }
            else if (State == State.Terminated)
            {
                RemoveCustomControl();
            }
        }

        private void SetDefaults()
        {
            Description = "NBS Strategy with Long Short buttons";
            Name = "NQ-System_1-With-Buttons";
            Calculate = Calculate.OnBarClose;
            EntriesPerDirection = 5;
            EntryHandling = EntryHandling.UniqueEntries;
            IsExitOnSessionCloseStrategy = false;
            ExitOnSessionCloseSeconds = 30;
            IsFillLimitOnTouch = false;
            MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
            OrderFillResolution = OrderFillResolution.Standard;
            Slippage = 0;
            StartBehavior = StartBehavior.WaitUntilFlat;
            TimeInForce = TimeInForce.Gtc;
            TraceOrders = true;
            RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
            StopTargetHandling = StopTargetHandling.PerEntryExecution;
            BarsRequiredToTrade = 0;
        }

        private void SetParameters()
        {

        }

        private Button _nbsLongBtn;
        private Button _nbsShortBtn;
        private Grid _nbsGrid;

        #region UI
        private void AddCustomControl()
        {
            if (UserControlCollection.Contains(myGrid))
                return;

            Dispatcher.InvokeAsync((() =>
            {
                myGrid = new System.Windows.Controls.Grid
                {
                    Name = "MyCustomGrid",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top
                };

                System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
                System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();

                myGrid.ColumnDefinitions.Add(column1);
                myGrid.ColumnDefinitions.Add(column2);

                longButton = new System.Windows.Controls.Button
                {
                    Name = "LongButton",
                    Content = "LONG",
                    Foreground = Brushes.White,
                    Background = Brushes.Green
                };

                shortButton = new System.Windows.Controls.Button
                {
                    Name = "ShortButton",
                    Content = "SHORT",
                    Foreground = Brushes.Black,
                    Background = Brushes.Red
                };

                longButton.Click += OnButtonClick;
                shortButton.Click += OnButtonClick;

                System.Windows.Controls.Grid.SetColumn(longButton, 0);
                System.Windows.Controls.Grid.SetColumn(shortButton, 1);

                myGrid.Children.Add(longButton);
                myGrid.Children.Add(shortButton);

                UserControlCollection.Add(myGrid);
            }));
        }

        private void RemoveCustomControl()
        {
            Dispatcher.InvokeAsync((() =>
            {
                if (myGrid != null)
                {
                    if (longButton != null)
                    {
                        myGrid.Children.Remove(longButton);
                        longButton.Click -= OnButtonClick;
                        longButton = null;
                    }
                    if (shortButton != null)
                    {
                        myGrid.Children.Remove(shortButton);
                        shortButton.Click -= OnButtonClick;
                        shortButton = null;
                    }
                }
            }));
        }

        private void OnButtonClick(object sender, RoutedEventArgs rea)
        {
            Button button = sender as Button;
            if (button == longButton && button.Name == "LongButton" && button.Content.Equals("LONG"))
            {
                button.Content = "Exit L";
                button.Name = "ExitLongButton";
                longButtonClicked = true;
                return;
            }

            if (button == shortButton && button.Name == "ShortButton" && button.Content.Equals("SHORT"))
            {
                button.Content = "Exit S";
                button.Name = "ExitShortButton";
                shortButtonClicked = true;
                return;
            }

            if (button == longButton && button.Name == "ExitLongButton" && button.Content.Equals("Exit L"))
            {
                button.Content = "LONG";
                button.Name = "LongButton";
                longButtonClicked = false;
                return;
            }

            if (button == shortButton && button.Name == "ExitShortButton" && button.Content.Equals("Exit S"))
            {
                button.Content = "SHORT";
                button.Name = "ShortButton";
                shortButtonClicked = false;
                return;
            }
        }
        #endregion

        // Strategy execution logic
        protected override void OnBarUpdate()
        {
            DrawLines();

            if (CurrentBar < 1 || State == State.Historical) return;

            CalculateEma();
            CalculateHeikenAshi();
            CalculateUpperLowerBand();

            CurrentPrice = Close[0];
            (LongLimitPrice, ShortLimitPrice) = GetPriceLines(CurrentPrice);

            if (Position.MarketPosition == MarketPosition.Flat && CurrentPrice >= ExtMapBuffer1)
            {
                stopLossLongPosition = LongLimitPrice - jump;
                SetLongTargetLines(LongLimitPrice);

                EnterLongLimit(5, LongLimitPrice, "BL");
                //EnterLongLimit(LongLimitPrice, LongEntryName2);
                //EnterLongLimit(LongLimitPrice, LongEntryName3);
                //EnterLongLimit(LongLimitPrice, LongEntryName4);
                //EnterLongLimit(LongLimitPrice, LongEntryName5);

                //profitTargetLongPosition = LongLimitPrice + jump;

                Print("############# LONG ###############");
                Print($"EnterLong order placed with entryPrice: {LongLimitPrice}");
                Print($"LongLimitPrice: {LongLimitPrice}");
                //Print($"ShortLimitPrice: {ShortLimitPrice}, ProfitTargetShortPosition: {profitTargetShortPosition}, StopLossShortPosition: {stopLossShortPosition}");
                Print($"Current market position: {Position.MarketPosition}");
                Print($"First: {FirstTargetLine}, Second: {SecondTargetLine}, Third: {ThirdTargetLine}, Fourth: {FourthTargetLine}");
                Print($"Upper Band: {ExtMapBuffer1}");
                Print($"Lower Band: {ExtMapBuffer2}");
                Print("######## LONG END ###########");
            }

            if (Position.MarketPosition == MarketPosition.Flat && CurrentPrice <= ExtMapBuffer2)
            {
                stopLossShortPosition = ShortLimitPrice + jump;
                SetShortTargetLines(ShortLimitPrice);

                EnterShortLimit(5, ShortLimitPrice, "SS");
                //EnterShortLimit(ShortLimitPrice, ShortEntryName2);
                //EnterShortLimit(ShortLimitPrice, ShortEntryName3);
                //EnterShortLimit(ShortLimitPrice, ShortEntryName4);
                //EnterShortLimit(ShortLimitPrice, ShortEntryName5);

                //profitTargetShortPosition = ShortLimitPrice - jump;

                Print("############# SHORT ###############");
                Print($"EnterLong order placed with entryPrice: {LongLimitPrice}");
                //Print($"LongLimitPrice: {LongLimitPrice}, ProfitTargetLongPosition: {profitTargetLongPosition}, StopLossLongPosition: {stopLossLongPosition}");
                Print($"ShortLimitPrice: {ShortLimitPrice}");
                Print($"Current market position: {Position.MarketPosition}");
                Print($"First: {FirstTargetLine}, Second: {SecondTargetLine}, Third: {ThirdTargetLine}, Fourth: {FourthTargetLine}");
                Print($"Upper Band: {ExtMapBuffer1}");
                Print($"Lower Band: {ExtMapBuffer2}");
                Print("############# SHORT END ###############");
            }

            // Partial Exit Logic
            //if (Position.MarketPosition == MarketPosition.Long)
            //{
            //    if(CurrentPrice >= FirstTargetLine || CrossAbove(Close, FirstTargetLine, 1))
            //        ExitLongLimit(1, FirstTargetLine, "LX1", "BL");

            //    if(CurrentPrice >= SecondTargetLine)
            //        ExitLongLimit(1, SecondTargetLine, "LX2", "BL");

            //    if (CurrentPrice >= ThirdTargetLine)
            //        ExitLongLimit(1, ThirdTargetLine, "LX3", "BL");

            //    if (CurrentPrice >= FourthTargetLine)
            //        ExitLongLimit(1, FourthTargetLine, "LX4", "BL");
            //}

            //if (Position.MarketPosition == MarketPosition.Short)
            //{
            //    if (CurrentPrice <= FirstTargetLine)
            //        ExitShortLimit(1, FirstTargetLine, "SX1", "SS");

            //    if(CurrentPrice <= SecondTargetLine)
            //        ExitShortLimit(1, SecondTargetLine, "SX2", "SS");

            //    if (CurrentPrice <= ThirdTargetLine)
            //        ExitShortLimit(1, ThirdTargetLine, "SX3", "SS");

            //    if (CurrentPrice <= FourthTargetLine)
            //        ExitShortLimit(1, FourthTargetLine, "SX4", "SS");
            //}

            // Implementing Trailing Stop Logic
            //if (Position.MarketPosition == MarketPosition.Long)
            //{
            //    if ((Math.Abs(Close[0] - LongLimitPrice) * 5 * 5) >= ProfitTrigger)
            //        profitReached = true;
            //    if (profitReached)
            //        trailingStopLevel = Math.Max(trailingStopLevel, Close[0] - TrailingStopDistance);
            //    if (CurrentPrice < trailingStopLevel)
            //        ExitLongStopMarket(5, CurrentPrice, "TLX1", "BL");
            //}

            //if (Position.MarketPosition == MarketPosition.Short) {
            //    if((Math.Abs(Close[0] - ShortLimitPrice) * 5 * 5) >= ProfitTrigger)
            //        profitReached = true;
            //    if (profitReached)
            //        trailingStopLevel = Math.Min(trailingStopLevel, CurrentPrice + TrailingStopDistance);
            //    if (CurrentPrice > trailingStopLevel)
            //        ExitShortStopMarket(5, CurrentPrice, "TSX1", "SS");
            //}

        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, 
            double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            if (IsOrderFilled(execution))
            {
                if (marketPosition == MarketPosition.Long)
                {
                    ExitLongLimit(0, true, 1, FirstTargetLine, "LX1", "BL");
                    ExitLongLimit(0, true, 1, SecondTargetLine, "LX2", "BL");
                    ExitLongLimit(0, true, 1, ThirdTargetLine, "LX3", "BL");
                    ExitLongLimit(0, true, 1, FourthTargetLine, "LX4", "BL");
                    Print($"stopLossLongPosition: {stopLossLongPosition} and CurrentPrice: {CurrentPrice}");
                    Print($"Position.MarketPosition: {Position.MarketPosition}, Position.Quantity: {Position.Quantity}");
                    _stopOrder = ExitLongStopMarket(0, true, 5, stopLossLongPosition, "L-STP", "BL");
                }
                else if (marketPosition == MarketPosition.Short)
                {
                    ExitShortLimit(0, true, 1, FirstTargetLine, "SX1", "SS");
                    ExitShortLimit(0, true, 1, SecondTargetLine, "SX2", "SS");
                    ExitShortLimit(0, true, 1, ThirdTargetLine, "SX3", "SS");
                    ExitShortLimit(0, true, 1, FourthTargetLine, "SX4", "SS");
                    Print($"stopLossShortPosition: {stopLossShortPosition} and CurrentPrice: {CurrentPrice}");
                    Print($"Position.MarketPosition: {Position.MarketPosition}, Position.Quantity: {Position.Quantity}");
                    _stopOrder = ExitShortStopMarket(0, true, 5, stopLossShortPosition, "S-STP", "SS");
                }
            }
        }

        private bool IsOrderFilled(Execution execution)
        {
            Order order = execution.Order;
            return order.OrderState == OrderState.Filled && IsEntryOrder(order.Name);
        }

        private bool IsEntryOrder(string signalName)
        {
            //return signalName.Equals(LongEntryName1) || signalName.Equals(LongEntryName2) 
            //    || signalName.Equals(LongEntryName3) || signalName.Equals(LongEntryName4) 
            //    || signalName.Equals(LongEntryName5) || signalName.Equals(ShortEntryName1) 
            //    || signalName.Equals(ShortEntryName2) || signalName.Equals(ShortEntryName3) 
            //    || signalName.Equals(ShortEntryName4) || signalName.Equals(ShortEntryName5);
            return signalName.Equals("BL") || signalName.Equals("SS");
        }

        private void DrawLines()
        {
            if (Bars.IsFirstBarOfSession && State == State.Historical)
            {
                foreach (int line in priceLineList)
                {
                    Draw.HorizontalLine(this, "price-line-" + line, line, Brushes.OrangeRed);
                }
            }
        }

        private void DrawPriceLines()
        {
            //Print($"Before loop start, LineValue: {LineValue}");
            while (LineValue <= 20000)
            {
                // Long Limit Order Condition with EMA Band Filter
                if (CurrentPrice > ExtMapBuffer1 && CurrentPrice < LineValue && LineValue < LongLimitPrice)
                    LongLimitPrice = LineValue;

                // Short Limit Order Condition with EMA Band Filter
                if (CurrentPrice < ExtMapBuffer2 && CurrentPrice > LineValue && LineValue > ShortLimitPrice)
                    ShortLimitPrice = LineValue;

                // Increment the LineValue by jump points
                LineValue = LineValue + jump;
                priceLineList.Add((int)LineValue);
            }
        }

        private (double, double) GetPriceLines(double price)
        {
            double lineAbove, lineBelow;
            int index;
            // get the mod by diving the currentPrice by jump (now 9)
            // if mod is zero then apply BinarySearch method to get the index
            //  that index will be used for finding lineAbove and lineBelow
            //  i.e. lineAbove = index + 1 and lineBelow = index - 1
            // else mode is non-zero then we will subtract mod from divisor
            //  what will get will be added in CurrentPrice
            //  now this will be multiple of jump
            //  apply the BinarySearch and find the liveAbove and lineBelow

            if ((int)price % jump == 0)
            {
                index = priceLineList.BinarySearch((int)price);
                lineAbove = priceLineList[index + 1];
                lineBelow = priceLineList[index - 1];
            }
            else
            {
                int mod = (int)price % jump;
                int addOffet = jump - mod;
                double priceToFind = price + addOffet;
                index = priceLineList.BinarySearch((int)priceToFind);
                lineAbove = priceLineList[index];
                lineBelow = priceLineList[index - 1];
            }
            return (lineAbove, lineBelow);
        }

        private void SetLongTargetLines(double limitPrice)
        {
            FirstTargetLine = limitPrice + 9;
            SecondTargetLine = limitPrice + 18;
            ThirdTargetLine = limitPrice + 27;
            FourthTargetLine = limitPrice + 36;
        }

        private void SetShortTargetLines(double limitPrice)
        {
            FirstTargetLine = limitPrice - 9;
            SecondTargetLine = limitPrice - 18;
            ThirdTargetLine = limitPrice - 27;
            FourthTargetLine = limitPrice - 36;
        }

        private void CalculateEma()
        {
            maOpen = EMA(Open, MaPeriod)[0];
            maClose = EMA(Close, MaPeriod)[0];
            maLow = EMA(Low, MaPeriod)[0];
            maHigh = EMA(High, MaPeriod)[0];
        }

        private void CalculateHeikenAshi()
        {
            haClose = (maOpen + maHigh + maLow + maClose) / 4;
            haOpen = (Open[1] + Close[1]) / 2;
            haHigh = Math.Max(maHigh, Math.Max(haOpen, haClose));
            haLow = Math.Min(maLow, Math.Min(haOpen, haClose));
        }

        private void CalculateUpperLowerBand()
        {
            ExtMapBuffer1 = EMA(MaPeriod2)[0]; // Upper band
            ExtMapBuffer2 = EMA(Low, MaPeriod2)[0]; // Lower band

            Draw.HorizontalLine(this, "extMapBuffer1", true, ExtMapBuffer1, Brushes.Green, DashStyleHelper.DashDot, 2);
            Draw.HorizontalLine(this, "extMapBuffer2", true, ExtMapBuffer2, Brushes.Red, DashStyleHelper.DashDot, 2);
        }
    }
}




