/*
  Not all required variables are here. This is primarily to hold various methods that may be useful.
*/
public class VariousMethods
{
  // General method to allow entry based on time and max loss resulting in disabling the strategy.
  private bool allowTradeEntry()
  {
    bool isWithinTimeRange = ToTime(Time[0]) >= Start && ToTime(Time[0]) <= End;
    bool isTimeCheckDisabled = DisableStartEndTime ? true : isWithinTimeRange;

    // Disable strategy at end time
    if (ToTime(Time[0]) == End && State == State.Realtime)
    {
      Print("End time reached. Strategy disabled.");

      // Only works in State.Realtime
      CloseStrategy(Name);
    }

    // Disable strategy when max loss hit
    if (Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar) <= (MaxAmountLoss * (-1)) && State == State.Realtime)
    {
      Print("Max loss of $" + MaxAmountLoss + " hit.");
      Print("Strategy disabled.");

      // Only works in State.Realtime
      CloseStrategy(Name);
    }

    return isTimeCheckDisabled;
  }

  // Example on updating a stop. Use whichever you want.
  private void updateToNewStop()
  {
    // Updates the stop above break even for long or below break even for short
    if (Position.MarketPosition == MarketPosition.Long && Close[0] >= (Position.AveragePrice + TriggerStopUpdate * TickSize) && updatedStop == false)
    {
      SetStopLoss(EntryName.LONG.ToString(), CalculationMode.Price, Position.AveragePrice + NewStop * TickSize, false);
      updatedStop = true;
    }

    if (Position.MarketPosition == MarketPosition.Short && Close[0] <= (Position.AveragePrice - TriggerStopUpdate * TickSize) && updatedStop == false)
    {
      SetStopLoss(EntryName.SHORT.ToString(), CalculationMode.Price, Position.AveragePrice - NewStop * TickSize, false);
      updatedStop = true;
    }

    // Updates the stop below break even for long or above break even for short
    if (Position.MarketPosition == MarketPosition.Long && Close[0] >= (Position.AveragePrice + TriggerStopUpdate * TickSize) && updatedStop == false)
    {
      SetStopLoss(EntryName.LONG.ToString(), CalculationMode.Price, Position.AveragePrice - NewStop * TickSize, false);
      updatedStop = true;
    }

    if (Position.MarketPosition == MarketPosition.Short && Close[0] <= (Position.AveragePrice - TriggerStopUpdate * TickSize) && updatedStop == false)
    {
      SetStopLoss(EntryName.SHORT.ToString(), CalculationMode.Price, Position.AveragePrice + NewStop * TickSize, false);
      updatedStop = true;
    }
  }

  // Returns the median for a linkedlist containing double
  private double getMedian(LinkedList<double> data)
  {
    double[] unsortedData = new double[maxCandleLookBack];
    int counter = 0;

    foreach (var item in data)
    {
      unsortedData[counter] = item;
      counter++;
    }

    Array.Sort(unsortedData);

    return maxCandleLookBack % 2 != 0 ? unsortedData[maxCandleLookBack / 2] : (unsortedData[(maxCandleLookBack - 1) / 2] + unsortedData[maxCandleLookBack / 2]) / 2.0;
  }

  // maxCandleLookBack amount fast EMA above slow EMA
  private bool fastEMAsAboveSlowEMAs()
  {
    bool[] results = new bool[maxCandleLookBack];
    bool fastEMAAboveSlowEMA = true;

    for (int i = 0; i < maxCandleLookBack; i++)
    {
      double barFastEMA = EMA(FastEMA)[i + 1];
      double barSlowEMA = EMA(SlowEMA)[i + 1];
      bool result = barFastEMA > barSlowEMA ? true : false;
      results[i] = result;
    }
    for (int i = 0; i < maxCandleLookBack; i++)
    {
      if (results[i] == false)
      {
        fastEMAAboveSlowEMA = false;
        break;
      }
    }
    return fastEMAAboveSlowEMA;
  }

  // maxCandleLookBack amount fast EMA below slow EMA
  private bool fastEMAsBelowSlowEMAs()
  {
    bool[] results = new bool[maxCandleLookBack];
    bool fastEMABelowSlowEMA = true;

    for (int i = 0; i < maxCandleLookBack; i++)
    {
      double barFastEMA = EMA(FastEMA)[i + 1];
      double barSlowEMA = EMA(SlowEMA)[i + 1];
      bool result = barFastEMA < barSlowEMA ? true : false;
      results[i] = result;
    }
    for (int i = 0; i < maxCandleLookBack; i++)
    {
      if (results[i] == false)
      {
        fastEMABelowSlowEMA = false;
        break;
      }
    }
    return fastEMABelowSlowEMA;
  }
}