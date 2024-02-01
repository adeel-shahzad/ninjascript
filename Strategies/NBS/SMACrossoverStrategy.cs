using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.NinjaScript.Strategies.NBS
{
    public class SMACrossoverStrategy : Strategy
    {
        // Define strategy parameters
        private double smaPeriod = 20;  // Period for the SMA
        private double lotSize = 1;     // Number of contracts or shares to trade

        // Initialize the strategy
        protected override void OnStateChange()
        {
            if (State == NinjaTrader.NinjaScript.State.SetDefaults)
            {
                Description = @"Simple Moving Average Crossover Strategy";
                Calculate = NinjaTrader.NinjaScript.Calculate.OnEachTick;
                AddPlot(BarBrush, "SMA");
            }
            else if (State == NinjaTrader.NinjaScript.State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Minute, 1);  // You can customize the data series
            }
        }

        // OnBarUpdate method is called for each incoming bar
        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < smaPeriod)
                return;

            // Calculate the SMA
            double smaValue = SMA(Closes[0], Convert.ToInt32(smaPeriod))[0];

            // Check for a crossover
            if (Closes[0][0] > smaValue && Closes[0][1] <= smaValue)
            {
                // Generate a buy signal
                EnterLong("BuySignal");
            }
            // Check for a crossunder
            else if (Closes[0][0] < smaValue && Closes[0][1] >= smaValue)
            {
                // Generate a sell signal
                EnterShort("SellSignal");
            }

            // Plot the SMA on the chart
            Values[0][0] = smaValue;
        }
    }
}