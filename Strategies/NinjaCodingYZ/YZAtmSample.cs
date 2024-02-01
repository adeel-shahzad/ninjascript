using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace NinjaTrader.NinjaScript.Strategies.YZ
{
    public class YZAtmSample : Strategy
    {
        private const string SystemVersion = "";
        private const string StrategyName = "YZ Atm Sample";
        private const string StrategyDesc = "";


        private string _atmStrategyId = ""; // unique ID; used for referencing the ATM strategy weh ave created at run-time
        private string _atmStrategyOrderId = ""; // unique ID; this is used for entry order within the ATM Strategy
        private bool _isAtmStrategyCreated; // check have we already created an ATM startegy or not
        private bool _enterLong; // long signal i.e. when our green bar's close has crossed the line on y-axis
        private bool _enterShort; // short signal i.e. when our red bar's close has crossed the line on y-axis
        private bool _attemptedToInitializeAtm; // a double protection for if we have ATM Strategy already created or not
        private string _modeInfo;
        private bool _lineOnChart; // should be the horizontal ray we created 
        private double _stop;
        private double _currentLineVal;
        private double _prevLineVal;
        private bool _isModeLong;
        private bool _isModeShort;
        private bool _replacedStopTarget;

        #region Params

        [NinjaScriptProperty]
        [Display(Name = "ATM Name", GroupName = "Entry Module", Order = 0)]
        public string ATMName { get; set; }

        [XmlIgnore]
        [NinjaScriptProperty]
        [Display(Name = "Color Long", GroupName = "Entry Module", Order = 1)]
        public System.Windows.Media.SolidColorBrush ColorLong { get; set; }

        [XmlIgnore]
        [NinjaScriptProperty]
        [Display(Name = "Color Short", GroupName = "Entry Module", Order = 2)]
        public System.Windows.Media.SolidColorBrush ColorShort { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Risk Multiplier", GroupName = "Entry Module", Order = 3)]
        public double RiskMultiplier { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Tick Offset", GroupName = "Entry Module", Order = 4)]
        public int TickOffset { get; set; }

        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                SetDefaults();
                SetParameters();

            }
            else if (State == State.Configure)
            {
                Initialize();

            }
            else if (State == State.Historical)
            {

                AddCustomControls();

                if (ChartControl != null)
                    ChartControl.PreviewKeyDown += KeyDown;
            }
            else if (State == State.Terminated)
            {

                RemoveCustomControls();

                if (ChartControl != null)
                    ChartControl.PreviewKeyDown -= KeyDown;
            }
        }

        private void Initialize()
        {
            ClearOutputWindow();
        }

        private void SetDefaults()
        {
            Description = StrategyDesc;
            Name = StrategyName + " " + SystemVersion;
            Calculate = Calculate.OnEachTick;
            BarsRequiredToTrade = 0;
            Slippage = 0;
            StartBehavior = StartBehavior.WaitUntilFlat;
            IsExitOnSessionCloseStrategy = false;
            ExitOnSessionCloseSeconds = 30;
            TraceOrders = false;
            //EntriesPerDirection = 1;
            //EntryHandling = EntryHandling.AllEntries;
            //IsInstantiatedOnEachOptimizationIteration = false;

        }

        private void SetParameters()
        {
            ATMName = "Input ATM Name !!!";
            ColorLong = Brushes.Green;
            ColorShort = Brushes.Red;
            RiskMultiplier = 1;
            TickOffset = 0;
        }

        #region UI

        private void KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                var keyPressed = e.Key.ToString().ToLower();

                if (keyPressed == "tab")
                {
                    ToggleMode();
                }
            }
            catch (Exception exception)
            {

            }


        }

        private System.Windows.Controls.Button _toggle;
        private System.Windows.Controls.Grid _myGrid;

        private void AddCustomControls()
        {
            ChartControl.Dispatcher.InvokeAsync((() =>
            {
                if (UserControlCollection.Contains(_myGrid))
                    return;

                CreateCustomGrid();
                CreateColumns();
                CreateButton();
                UserControlCollection.Add(_myGrid);

            }));
        }

        private void RemoveCustomControls()
        {
            if (ChartControl == null)
                return;

            // Again, we need to use a Dispatcher to interact with the UI elements
            ChartControl.Dispatcher.InvokeAsync((() =>
            {
                if (_myGrid != null)
                {
                    if (_toggle != null)
                    {
                        _myGrid.Children.Remove(_toggle);
                        _toggle.Click -= OnToggleClick;
                        _toggle = null;
                    }

                }
            }));
        }

        private void CreateCustomGrid()
        {
            _myGrid = new System.Windows.Controls.Grid
            {
                Name = "MyCustomGrid",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
            };

        }

        private void CreateColumns()
        {
            System.Windows.Controls.ColumnDefinition column0 = new System.Windows.Controls.ColumnDefinition();
            System.Windows.Controls.RowDefinition row0 = new System.Windows.Controls.RowDefinition();
            _myGrid.ColumnDefinitions.Add(column0);
            _myGrid.RowDefinitions.Add(row0);

        }

        private void CreateButton()
        {

            _toggle = CreateToggleButton(NoneMode, "MyBuyButton", Brushes.White, Brushes.Blue);
            _toggle.Click += OnToggleClick;

            var margin = _toggle.Margin;
            margin.Right = 50;
            margin.Top = 45;
            _toggle.Margin = margin;

            System.Windows.Controls.Grid.SetColumn(_toggle, 0);
            System.Windows.Controls.Grid.SetRow(_toggle, 0);
            _myGrid.Children.Add(_toggle);

        }

        private System.Windows.Controls.Button CreateToggleButton(string content, string name, System.Windows.Media.Brush foreground,
            System.Windows.Media.Brush background)
        {
            return new System.Windows.Controls.Button
            {
                Name = name,
                Content = content,
                Foreground = foreground,
                Background = background,
                Padding = new Thickness(),
                Opacity = 0.5,
                FontSize = 25


            };
        }

        private void OnToggleClick(object sender, RoutedEventArgs rea)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;


            if (button != null)
            {
                try
                {
                    ToggleMode();

                }
                catch (Exception exception)
                {
                    Print("Exception whilst changing calculation type via toggle button" + exception);
                }
            }
        }



        private string LongMode = "LONG";
        private string ShortMode = "SHORT";
        private string NoneMode = "NONE";

        private string _globalModeName;

        private void ToggleMode()
        {
            SolidColorBrush color = null;

            if (_globalModeName == LongMode)
            {
                _globalModeName = ShortMode;
                color = Brushes.DarkRed;


            }
            else if (_globalModeName == ShortMode || _globalModeName == null)
            {
                _globalModeName = LongMode;
                color = Brushes.Green;

            }


            _toggle.Content = _globalModeName;
            _toggle.Background = color;
            GetLine();
            ForceRefresh();
        }

        #endregion


        protected override void OnBarUpdate()
        {
            if (State != State.Realtime)
                return;

            _prevLineVal = _currentLineVal;
            GetLine();

            if (_lineOnChart)
                Draw.Ray(this, "Crossing", true, 5, _currentLineVal, 0, _currentLineVal, Brushes.DarkRed, DashStyleHelper.Dash, 1);

            DrawBox();


            if (IsFirstTickOfBar)
            {
                if (_isModeLong)
                {
                    _enterLong = Close[1] > _prevLineVal;
                }
                if (_isModeShort)
                {
                    _enterShort = Close[1] < _prevLineVal;
                }
            }

            if (!_isAtmStrategyCreated)
            {
                if (_enterLong)
                {
                    GetStopLong();
                    InitializeAtm(OrderAction.Buy);

                }
                else if (_enterShort)
                {
                    GetStopShort();
                    InitializeAtm(OrderAction.SellShort);
                }

            }
            if (_isAtmStrategyCreated && !_replacedStopTarget)
            {

                if (_atmStrategyId.Length > 0)
                {

                    if (GetAtmStrategyMarketPosition(_atmStrategyId) == MarketPosition.Long)
                    {
                        var entryPrice = GetAtmStrategyPositionAveragePrice(_atmStrategyId);
                        double pointsToStop = 0;
                        double profit = 0;

                        pointsToStop = entryPrice - _stop;
                        profit = entryPrice + (pointsToStop * RiskMultiplier) + (TickOffset * TickSize);
                        _stop = _stop - (TickOffset * TickSize);


                        Print(GetAtmStrategyMarketPosition(_atmStrategyId));
                        Print(GetAtmStrategyStopTargetOrderStatus("Stop1", _atmStrategyId));
                        Print(GetAtmStrategyStopTargetOrderStatus("Target1", _atmStrategyId));

                        AtmStrategyChangeStopTarget(0, _stop, "Stop1", _atmStrategyId);
                        AtmStrategyChangeStopTarget(profit, 0, "Target1", _atmStrategyId);
                        _replacedStopTarget = true;
                    }
                    else if (GetAtmStrategyMarketPosition(_atmStrategyId) == MarketPosition.Short)
                    {
                        var entryPrice = GetAtmStrategyPositionAveragePrice(_atmStrategyId);
                        double pointsToStop = 0;
                        double profit = 0;

                        pointsToStop = _stop - entryPrice;
                        profit = entryPrice - (pointsToStop * RiskMultiplier) - (TickOffset * TickSize);
                        _stop = _stop + (TickOffset * TickSize);

                        Print(GetAtmStrategyMarketPosition(_atmStrategyId));
                        Print(GetAtmStrategyStopTargetOrderStatus("Stop1", _atmStrategyId));
                        Print(GetAtmStrategyStopTargetOrderStatus("Target1", _atmStrategyId));

                        AtmStrategyChangeStopTarget(0, _stop, "Stop1", _atmStrategyId);
                        AtmStrategyChangeStopTarget(profit, 0, "Target1", _atmStrategyId);
                        _replacedStopTarget = true;
                    }


                }

            }
        }

        private void InitializeAtm(OrderAction orderAction)
        {

            if (_attemptedToInitializeAtm)
                return;

            _attemptedToInitializeAtm = true;
            _atmStrategyId = GetAtmStrategyUniqueId();
            _atmStrategyOrderId = GetAtmStrategyUniqueId();

            AtmStrategyCreate(orderAction, OrderType.Market, 0, 0, TimeInForce.Gtc,
                _atmStrategyOrderId, ATMName, _atmStrategyId, (atmCallbackErrorCode, atmCallbackId) =>
                {

                    Print(atmCallbackErrorCode.ToString() + "  Call Back ID" + atmCallbackId + " Strat ID" +
                          _atmStrategyId);

                    Print("STATUS " + GetAtmStrategyMarketPosition(_atmStrategyId));

                    if (atmCallbackErrorCode == ErrorCode.NoError && atmCallbackId == _atmStrategyId)
                    {
                        _isAtmStrategyCreated = true;

                    }


                });
        }

        private void GetStopLong()
        {
            for (int i = 1; i < Bars.Count - 2; i++)
            {
                if (Close[i] < Open[i])
                {
                    _stop = Low[i - 1];
                    break;
                }
            }
        }

        private void GetStopShort()
        {
            for (int i = 1; i < Bars.Count - 2; i++)
            {
                if (Close[i] > Open[i])
                {
                    _stop = High[i - 1];
                    break;
                }
            }
        }

        private void GetLine()
        {
            foreach (var draw in DrawObjects.ToList())
            {
                if (draw is NinjaTrader.NinjaScript.DrawingTools.Line)
                {

                    var line = draw as NinjaTrader.NinjaScript.DrawingTools.Line;

                    if (_globalModeName == LongMode)
                    {
                        _isModeLong = true;
                        _isModeShort = false;
                        line.Stroke.Brush = ColorLong;

                    }
                    else if (_globalModeName == ShortMode)
                    {
                        _isModeLong = false;
                        _isModeShort = true;
                        line.Stroke.Brush = ColorShort;
                    }
                    else if (_globalModeName == NoneMode)
                    {
                        _isModeLong = false;
                        _isModeShort = false;
                        line.Stroke.Brush = Brushes.Blue;

                    }

                    var x3 = ChartControl.GetXByBarIndex(ChartBars, Bars.Count - 1);

                    var x1 = ChartControl.GetXByTime(line.StartAnchor.Time);
                    var x2 = ChartControl.GetXByTime(line.EndAnchor.Time);



                    if (x1 < x2)
                    {
                        var y1 = line.StartAnchor.Price;
                        var y2 = line.EndAnchor.Price;

                        var slope = (y1 - y2) / (x1 - x2);
                        var y3 = y1 - slope * (x1 - x3);

                        //Print("Start Anchor Time: " + line.StartAnchor.Time + " Time Now: " + Time[0]);
                        // Print("y1: " +y1 + " y2:" + y2 + " y3: " +y3 + " x1 " + x1+ " x2" + x2 + " x3: " + x3);

                        _currentLineVal = Math.Round(y3, 2);
                        _lineOnChart = true;
                    }
                    if (x1 > x2)
                    {
                        _globalModeName = "Re-draw from left to right!";
                    }


                }
            }
        }

        private void DrawBox()
        {


            Draw.TextFixed(this, "Information", "Current live value: " + _currentLineVal + Environment.NewLine +
                                                "Mode: " + _globalModeName,


                TextPosition.BottomRight, Brushes.Black,
                new SimpleFont("Arial", 12), Brushes.Black, Brushes.Wheat, 100);

            ForceRefresh();
        }





    }
}
