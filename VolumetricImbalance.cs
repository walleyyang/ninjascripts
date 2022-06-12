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
	Example on how to retrieve and calculate the imbalances from previous bars.
*/

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
  public class VolumetricImbalance : Strategy
  {
    private int volumetricBar = 1;
    private int imbalanceRatio = 4;

    protected override void OnStateChange()
    {
      if (State == State.SetDefaults)
      {
        Description = @"Enter the description for your new custom Strategy here.";
        Name = "VolumetricImbalance";
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
        AddVolumetric(Instrument.FullName, BarsPeriodType.Range, 15, VolumetricDeltaType.BidAsk, 1);
      }
    }

    protected override void OnBarUpdate()
    {
      if (CurrentBars[volumetricBar] < BarsRequiredToTrade)
        return;

      // Display imbalances from previous bar.
      displayImbalances();
    }

    private void displayImbalances()
    {
      NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType barsType = BarsArray[volumetricBar].BarsType as
        NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;

      if (barsType != null)
      {
        long bidHighVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetBidVolumeForPrice(Highs[volumetricBar][1]);
        long askHighVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetAskVolumeForPrice(Highs[volumetricBar][1]);

        long bidLowVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetBidVolumeForPrice(Lows[volumetricBar][1]);
        long askLowVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetAskVolumeForPrice(Lows[volumetricBar][1]);

        int totalTicks = Convert.ToInt32((Highs[volumetricBar][1] - Lows[volumetricBar][1]) / TickSize);
        long[] bidVolumePerTick = new long[totalTicks + 1];
        long[] askVolumePerTick = new long[totalTicks + 1];

        LinkedList<long> bidImbalances = new LinkedList<long>();
        LinkedList<long> askImbalances = new LinkedList<long>();

        // Gets volume per tick for previous bar
        for (int i = 0; i <= totalTicks; i++)
        {
          long bidVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetBidVolumeForPrice(Lows[volumetricBar][1] + (TickSize * i));
          long askVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetAskVolumeForPrice(Lows[volumetricBar][1] + (TickSize * i));

          bidVolumePerTick[i] = bidVolume;
          askVolumePerTick[i] = askVolume;
        }

        // Calculates imbalances
        for (int i = 0; i <= totalTicks; i++)
        {
          // Check imbalance for bid
          if (i != totalTicks)
          {
            long bidVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetBidVolumeForPrice(Lows[volumetricBar][1] + (TickSize * i));
            long askVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetAskVolumeForPrice((Lows[volumetricBar][1] + (TickSize * i) + TickSize));

            long ratio = askVolume == 0 ? bidVolume : bidVolume / askVolume;
            bool ratioGreaterThanImblanceRatio = ratio >= imbalanceRatio;

            if (ratioGreaterThanImblanceRatio)
              bidImbalances.AddLast(bidVolume);
          }

          // Check imbalance for ask
          if (i != 0)
          {
            long bidVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetBidVolumeForPrice((Lows[volumetricBar][1] + (TickSize * i) - TickSize));
            long askVolume = barsType.Volumes[CurrentBars[volumetricBar] - 1].GetAskVolumeForPrice(Lows[volumetricBar][1] + (TickSize * i));

            long ratio = bidVolume == 0 ? askVolume : askVolume / bidVolume;
            bool ratioGreaterThanImblanceRatio = ratio >= imbalanceRatio;

            if (ratioGreaterThanImblanceRatio)
              askImbalances.AddLast(askVolume);
          }
        }

        // Prints twice for some reason and not really looking into it. I think it should only print once on 
        // bar after calculation due to being after bar close setting.

        // Print bid imbalances
        if (bidImbalances.Count > 0)
        {
          Print("***");
          Print(string.Format("Current Bar: {0} | {1}", ToDay(Time[0]), ToTime(Time[0])));

          foreach (long item in bidImbalances)
          {
            Print(item);
          }
          Print("***");
        }

        /*
				// Print ask imbalances
				if (askImbalances.Count > 0)
				{
					Print("***");
				    Print(string.Format("Current Bar: {0} | {1}", ToDay(Time[0]), ToTime(Time[0])));
					
					foreach (long item in askImbalances)
				    {
				        Print(item);
				    }
					Print("***");	
				}
				*/
      }
    }
  }
}
