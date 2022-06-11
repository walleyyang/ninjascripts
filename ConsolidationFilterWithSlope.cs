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

/*
	Helps filter consolidation on a moving average using slope. Changing positiveSlope and negativeSlope will adjust filtering.
*/

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
  public class ConsolidationFilterWithSlope : Strategy
  {
    private int maxCandleLookBack = 3;
    // Degrees
    private int positiveSlope = 15;
    private int negativeSlope = -15;

    protected override void OnStateChange()
    {
      if (State == State.SetDefaults)
      {
        Description = @"Enter the description for your new custom Strategy here.";
        Name = "ConsolidationFilterWithSlope";
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
      Print(string.Format("Consolidating: {0}", consolidating()));
      Print("***");
    }

    // Filters out some consolidation. 
    private bool consolidating()
    {
      // Get slope of last maxCandleLookBack number of candles from 8 EMA
      double slopeDegree = Math.Atan(Slope(EMA(8), maxCandleLookBack, 0)) * 180 / Math.PI;

      return slopeDegree < positiveSlope && slopeDegree > negativeSlope;
    }
  }
}
