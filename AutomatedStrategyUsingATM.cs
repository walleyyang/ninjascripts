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
	Executes a created ATM strategy based on this strategy. A default ATM strategy called "BestATMStrategyEver" may be required if 
	no other is added to the ATM Template Name input.

	Derived from https://www.udemy.com/course/ninjatrader
*/

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
  public class AutomatedStrategyUsingATM : Strategy
  {
    private string atmStrategyId;
    private string atmStrategyOrderId;
    private bool isAtmStrategyCreated = false;
    private bool entryLong;
    private bool entryShort;

    [NinjaScriptProperty]
    [Display(Name = "ATM Template Name", Description = "The name of the custom ATM Strategy.", Order = 0, GroupName = "AutomatedStrategyUsingATM")]
    public string ATMTemplateName { get; set; }

    protected override void OnStateChange()
    {
      if (State == State.SetDefaults)
      {
        Description = @"Enter the description for your new custom Strategy here.";
        Name = "AutomatedStrategyUsingATM";
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

        ATMTemplateName = "BestATMStrategyEver";
      }
      else if (State == State.Configure)
      {
      }
    }

    protected override void OnBarUpdate()
    {
      if (CurrentBar < BarsRequiredToTrade)
        return;

      // Checking for ATM Strategy will fail if not in real time.
      if (State != State.Realtime)
        return;

      if (AtmIsFlat())
      {

        entryLong = Close[0] > Close[1];
        entryShort = Close[0] < Close[1];

        if (entryLong)
        {
          atmStrategyId = GetAtmStrategyUniqueId();
          atmStrategyOrderId = GetAtmStrategyUniqueId();

          AtmStrategyCreate(OrderAction.Buy, OrderType.Market, 0, 0, TimeInForce.Day, atmStrategyOrderId, ATMTemplateName, atmStrategyId,
          (atmCallbackErrorCode, atmCallbackId) =>
          {
            if (atmCallbackId == atmStrategyId)
            {
              if (atmCallbackErrorCode == Cbi.ErrorCode.NoError)
              {
                isAtmStrategyCreated = true;
              }
            }
          });
        }
        else if (entryShort)
        {
          atmStrategyId = GetAtmStrategyUniqueId();
          atmStrategyOrderId = GetAtmStrategyUniqueId();

          AtmStrategyCreate(OrderAction.SellShort, OrderType.Market, 0, 0, TimeInForce.Day, atmStrategyOrderId, ATMTemplateName, atmStrategyId,
          (atmCallbackErrorCode, atmCallbackId) =>
          {
            if (atmCallbackId == atmStrategyId)
            {
              if (atmCallbackErrorCode == Cbi.ErrorCode.NoError)
              {
                isAtmStrategyCreated = true;
              }
            }
          });
        }
      }

    }

    private bool AtmIsFlat()
    {
      if (atmStrategyId == null)
        return true;

      return GetAtmStrategyMarketPosition(atmStrategyId) == MarketPosition.Flat;
    }
  }
}
