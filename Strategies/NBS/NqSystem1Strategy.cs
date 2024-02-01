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

namespace NinjaTrader.NinjaScript.Strategies.NBS
{
    public class NqSystem1Strategy : Strategy
    {
        private DerivativeOscillator _derivativeOscillator;
        private mahTrendGRaBer2 _mahTrendGRaBer2;
        private AuSuperTrendU11 _auSuperTrendU11;

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
        private double ShortLimitPrice = 13300;
        private double LineValue = 13300;
        private double FirstTargetLine = 0;
        private double SecondTargetLine = 0;
        private double ThirdTargetLine = 0;
        private double FourthTargetLine = 0;
        private double trailingStopLevel = 0;
        private bool profitReached = false;

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
            }
            else if (State == State.DataLoaded)
            {
                _mahTrendGRaBer2 = mahTrendGRaBer2(Close, true);
                _derivativeOscillator = DerivativeOscillator(89, 5, 3, 9);
                _auSuperTrendU11 = AuSuperTrendU11(Close, AuSuperTrendU11BaseType.Median, AuSuperTrendU11OffsetType.Default, AuSuperTrendU11VolaType.True_Range, false, 1, 3, 22);

                AddChartIndicator(_mahTrendGRaBer2);
                AddChartIndicator(_derivativeOscillator);
                AddChartIndicator(_auSuperTrendU11);
            }
            else if(State == State.Terminated)
            {
                RemoveCustomControl();
            }
        }

        private void SetDefaults()
        {
            Description = "EMA Heikin-Ashi Strategy";
            Name = "NQ-System_1-Strategy";
            Calculate = Calculate.OnBarClose;
            //BarsRequiredToTrade = 0;
            //Slippage = 0;
            //StartBehavior = StartBehavior.WaitUntilFlat;
            //IsExitOnSessionCloseStrategy = false;
            //ExitOnSessionCloseSeconds = 30;
            //TraceOrders = false;
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
            ChartControl.Dispatcher.InvokeAsync(new Action(() =>
            {
                if (UserControlCollection.Contains(_nbsGrid))
                    return;
                Print("Going to add grid");
                CreateCustomGrid();
                CreateColumns();
                CreateButtons();
                UserControlCollection.Add(_nbsGrid);
            }));
        }

        private void RemoveCustomControl()
        {
            if (ChartControl == null) return;

            Print("Inside RemoveCustomControl()");
            //ChartControl.Dispatcher?.InvokeAsync(new Action(() =>
            //{
            //    if(_nbsGrid != null)
            //    {
            //        RemoveButton(_nbsLongBtn);
            //        RemoveButton(_nbsShortBtn);
            //    }
            //}));
            ChartControl.Dispatcher.InvokeAsync((() =>
            {
                if (_nbsGrid != null)
                {
                    if (_nbsLongBtn != null)
                    {
                        _nbsGrid.Children.Remove(_nbsLongBtn);
                        //_nbsLongBtn.Click -= OnBtnClick;
                        _nbsLongBtn = null;
                    }
                    if (_nbsShortBtn != null)
                    {
                        _nbsGrid.Children.Remove(_nbsShortBtn);
                        //_nbsShortBtn.Click -= OnBtnClick;
                        _nbsShortBtn = null;
                    }
                }
            }));
        }

        private void RemoveButton(Button button)
        {
            if(button != null)
            {
                _nbsGrid.Children.Remove(button);
                //button.Click -= OnBtnClick;
                button = null;
            }
        }

        private void CreateCustomGrid()
        {
            Print("Inside CreateCustomGrid()");
            _nbsGrid = new Grid
            {
                Name = "NbsCustomGrid",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                VerticalAlignment = System.Windows.VerticalAlignment.Top
            };
            Print("leaving CreateCustomGrid()");
        }

        private void CreateColumns()
        {
            Print("Inside CreateColumns()");
            ColumnDefinition column1 = new ColumnDefinition();
            ColumnDefinition column2 = new ColumnDefinition();
            _nbsGrid.ColumnDefinitions.Add(column1);
            _nbsGrid.ColumnDefinitions.Add(column2);
            Print("Leaving CreateColumns()");
        }

        private void CreateButtons()
        {
            Print("Inside CreateButtons()");
            _nbsLongBtn = ButtonBuilder("longBtn", "Long", Brushes.White, Brushes.Green);
            _nbsShortBtn = ButtonBuilder("shortBtn", "Short", Brushes.White, Brushes.Red);

            // subscribe click events
            //_nbsLongBtn.Click += OnBtnClick;
            //_nbsShortBtn.Click += OnBtnClick;

            // set buttons margins
            var margin = _nbsLongBtn.Margin;
            margin.Right = 50;
            margin.Top = 45;
            _nbsLongBtn.Margin = margin;
            margin = _nbsShortBtn.Margin;
            margin.Right = 50;
            margin.Top = 45;
            _nbsShortBtn.Margin = margin;

            // buttons placement on grid
            Grid.SetColumn(_nbsLongBtn, 0);
            Grid.SetColumn(_nbsShortBtn, 1);
            _nbsGrid.Children.Add(_nbsLongBtn);
            _nbsGrid.Children.Add(_nbsShortBtn);
            Print("button added to grid");

            // inner method for button creation
            Button ButtonBuilder(string name, string content, Brush foreground, Brush background)
            {
                return new Button
                {
                    Name = name,
                    Content = content,
                    Foreground = foreground,
                    Background = background,
                    Padding = new System.Windows.Thickness(0),
                    Opacity = 0.5,
                    FontSize = 25
                };
            };

            Print("Leaving CreateButtons()");
        }

        //private void OnBtnClick(object sender, RoutedEventArgs args)
        //{
        //    Button button = sender as Button;
        //}
        #endregion

        // Strategy execution logic
        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1) return;

            //Print(" High[0]: " + High[0] + ", Low[0]: " + Low[0] 
            //    + ", Close[0]: " + Close[0] + ", Open[0]: " + Open[0] + ", Time[0]: " + Time[0]);
            //return;

            //Print("MahTrendGraber2 > EMA High: " + _mahTrendGRaBer2.EmaHigh[0]);
            //Print("MahTrendGraber2 > EMA Close: " + _mahTrendGRaBer2.EmaClose[0]);
            //Print("MahTrendGraber2 > EMA Low: " + _mahTrendGRaBer2.EmaLow[0]);

            //Print("AuSuperTrendU11 > Stop Dot: " + _auSuperTrendU11.StopDot[0]);
            //Print("AuSuperTrendU11 > Stop Line: " + _auSuperTrendU11.StopLine[0]);
            //Print("AuSuperTrendU11 > Trend: " + _auSuperTrendU11.Trend[0]);
            //Print("AuSuperTrendU11 > Reverse Dot: " + _auSuperTrendU11.ReverseDot[0]);
            //Print("AuSuperTrendU11 > Up Trend: " + _auSuperTrendU11.UpTrend[0]);
            double entryPrice = Position.AveragePrice;
            CalculateEma();
            CalculateHeikenAshi();
            CalculateUpperLowerBand();

            CurrentPrice = Close[0];
            while(LineValue <= 20000) {
                // Long Limit Order Condition with EMA Band Filter
                if(CurrentPrice > ExtMapBuffer1 && CurrentPrice < LineValue && LineValue < LongLimitPrice)
                    LongLimitPrice = LineValue;

                // Short Limit Order Condition with EMA Band Filter
                if(CurrentPrice < ExtMapBuffer2 && CurrentPrice > LineValue && LineValue > ShortLimitPrice)
                    ShortLimitPrice = LineValue;

                // Increment the LineValue by jump points
                LineValue = LineValue + jump;
                //Print("LineValue: " + LineValue + ", At: " + Time[0] + ", CurrentPrice: " + CurrentPrice);
            }

            if (Position.MarketPosition == Cbi.MarketPosition.Long)
            {
                FirstTargetLine = entryPrice + jump1; // EntryPrice calculated correct?
                SecondTargetLine = entryPrice + jump2; // 18 points away from entry
                ThirdTargetLine = entryPrice + jump3; // 45 points away from entry
                FourthTargetLine = entryPrice + jump4; // 80 points away from entry
            }
            else if (Position.MarketPosition == MarketPosition.Short)
            {
                FirstTargetLine = entryPrice - jump1;
                SecondTargetLine = entryPrice - jump2; // 18 points away from entry
                ThirdTargetLine = entryPrice - jump3; // 45 points away from entry
                FourthTargetLine = entryPrice - jump4; // 80 points away from entry
            }

            //Print("CurrentPrice: " + CurrentPrice + ", ExtMapBuffer1: " + ExtMapBuffer1 + ", ExtMapBuffer2: " + ExtMapBuffer2 + ", ShortLimitPrice: " + ShortLimitPrice + ", LongLimitPrice: " + LongLimitPrice);

            if (CurrentPrice >= ExtMapBuffer1)
            {
                EnterLongLimit(5, entryPrice, "BL");
                //Print("EnterLong order placed");
            }

            if (CurrentPrice <= ExtMapBuffer2)
            {
                EnterShortLimit(5, entryPrice, "SS");
                //Print("EnterShort order placed");
            }

            // Partial Exit Logic
            if(Position.MarketPosition == MarketPosition.Long)
            {
                if(CurrentPrice >= FirstTargetLine)
                    EnterShort(1, "LX1");

                if(CurrentPrice >= SecondTargetLine)
                    EnterShort(1, "LX2");

                if(CurrentPrice >= ThirdTargetLine)
                    EnterShort(1, "LX3");

                if (CurrentPrice >= FourthTargetLine)
                    EnterShort(1, "LX4");
                //        If(CurrentPrice >= FourthTargetLine) Then
                //Sell("LX4") 1 contract next bar at market;
            }

            if (Position.MarketPosition == MarketPosition.Short)
            {
                if (CurrentPrice <= FirstTargetLine)
                    ExitShort(1, "X1", "LX1");

                if(CurrentPrice <= SecondTargetLine)
                    ExitShort(1, "X2", "LX2");

                if(CurrentPrice <= ThirdTargetLine)
                    ExitShort(1, "X3", "LX3");

                if(CurrentPrice <= FourthTargetLine)
                    ExitShort(1, "X4", "LX4");
            }

            // Implementing Trailing Stop Logic
            if (Position.MarketPosition == Cbi.MarketPosition.Long)
            {
                if ((Close[0] - entryPrice) >= ProfitTrigger)
                    profitReached = true;
                if (profitReached)
                    trailingStopLevel = Math.Max(trailingStopLevel, Close[0] - TrailingStopDistance);
                if (CurrentPrice < trailingStopLevel)
                    ExitShortStopMarket(CurrentPrice, "TLX1");
            }

            if (Position.MarketPosition == MarketPosition.Short) {
                if((entryPrice - Close[0]) >= ProfitTrigger)
                    profitReached = true;
                if (profitReached)
                    trailingStopLevel = Math.Min(trailingStopLevel, CurrentPrice + TrailingStopDistance);
                if (CurrentPrice > trailingStopLevel)
                    ExitLongStopMarket(CurrentPrice, "TSX1");
            }

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
            haOpen = (Open[1] + Close[1]) / 2; // ask Surender / Tahir; (haOpen[1] + haClose[1]) / 2;
            haHigh = Math.Max(maHigh, Math.Max(haOpen, haClose));
            haLow = Math.Min(maLow, Math.Min(haOpen, haClose));
        }

        private void CalculateUpperLowerBand() // ask Surender / Tahir
        {
            ExtMapBuffer1 = EMA(High, MaPeriod2)[0]; // Upper band
            ExtMapBuffer2 = EMA(Low, MaPeriod2)[0]; // Lower band
        }
    }
}




