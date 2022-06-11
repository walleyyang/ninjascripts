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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
  public class TwoEMAWithinCandle : Strategy
  {
    private int maxCandleLookBack = 3;
    private int fastEMA = 3;
    private int slowEMA = 8;

    protected override void OnStateChange()
    {
      if (State == State.SetDefaults)
      {
        Description = @"Enter the description for your new custom Strategy here.";
        Name = "TwoEMAWithinCandle";
        Calculate = Calculate.OnBarClose;
        EntriesPerDirection = 1;
        EntryHandling = EntryHandling.AllEntries;
        IsExitOnSessionCloseStrategy = true;
        ExitOnSessionCloseSeconds = 30;
        IsFillLimitOnTouch = false;
        MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
        OrderFillResolution = OrderFillResolution.Standard;
        Slippage = 0;
        StartBehavior = StartBehavior.WaitUntilFlat;
        TimeInForce = TimeInForce.Gtc;
        TraceOrders = false;
        RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
        StopTargetHandling = StopTargetHandling.PerEntryExecution;
        BarsRequiredToTrade = 20;
        // Disable this property for performance gains in Strategy Analyzer optimizations
        // See the Help Guide for additional information
        IsInstantiatedOnEachOptimizationIteration = true;
      }
      else if (State == State.Configure)
      {
      }
    }

    protected override void OnBarUpdate()
    {
      if (CurrentBar < BarsRequiredToTrade)
        return;

      Print("***");
      Print(string.Format("Current Bar: {0} / {1}", ToDay(Time[0]), ToTime(Time[0])));
      Print(string.Format("EMAs within last {0} candles: {1}", maxCandleLookBack, emasWithinCandles()));
      Print("***");
    }

    private bool emasWithinCandles()
    {
      bool[] results = new bool[maxCandleLookBack];
      bool withinCandles = true;

      for (int i = 0; i < maxCandleLookBack; i++)
      {
        double openPrice = Open[i + 1];
        double closePrice = Close[i + 1];
        double barFastEMA = EMA(fastEMA)[i + 1];
        double barSlowEMA = EMA(slowEMA)[i + 1];

        bool fastEMAWithinBar = emaWithinCandle(openPrice, closePrice, barFastEMA);
        bool slowEMAWithinBar = emaWithinCandle(openPrice, closePrice, barSlowEMA);

        bool result = fastEMAWithinBar && slowEMAWithinBar ? true : false;

        results[i] = result;
      }

      for (int i = 0; i < maxCandleLookBack; i++)
      {
        if (!results[i])
        {
          withinCandles = false;

          break;
        }
      }

      return withinCandles;
    }

    private bool emaWithinCandle(double openPrice, double closePrice, double ema)
    {
      if (openPrice < closePrice)
      {
        // Green bar
        return ema > openPrice && ema < closePrice;
      }
      else
      {
        // Red bar
        return ema < openPrice && ema > closePrice;
      }
    }
  }
}
