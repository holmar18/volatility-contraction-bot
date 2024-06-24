using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;


public static class Extensions
{
    /// <param name="AccountBalance">Account.balance value</param>
    /// <param name="RiskPercentage">% of capital to risk per trade</param>
    /// <param name="_StopLoss">SL difference from Stoploss & Entry</param>
    /// <returns>double : Lot size for Forex/Stocks</returns>
    public static double CalculateLotSize(this Symbol thisSymbol, double AccountBalance, double RiskPercentage, double _StopLoss)
    {
        var amount_to_risk_per_trade = AccountBalance * (RiskPercentage / 100);
        var PipScale = thisSymbol.PipValue;
        var trade_volume = amount_to_risk_per_trade / (_StopLoss * PipScale);
        var truncation_factor = thisSymbol.LotSize * PipScale * 100;
        var trade_volume_truncated = ((int)(trade_volume / truncation_factor)) * truncation_factor;

        return thisSymbol.TickSize == 0.01 ? thisSymbol.NormalizeVolumeInUnits(trade_volume) : thisSymbol.NormalizeVolumeInUnits(trade_volume_truncated);
    }

    /// <param name="tradeSize">SL difference from Stoploss,Entry Or Atr value or Any simular</param>
    /// <param name="StopLossMultiplier">Value to multiply the stoploss with default in settings = 1</param>
    /// <returns>double : stoploss size</returns>
    public static double CalculateStopLoss(this Symbol thisSymbol, double tradeSize, double StopLossMultiplier)
    {
        return (tradeSize * (thisSymbol.TickSize / thisSymbol.PipSize * Math.Pow(10, thisSymbol.Digits))) * StopLossMultiplier;
    }

    /// <param name="tradeSize">SL difference from Stoploss,Entry Or Atr value or Any simular</param>
    /// <param name="StopLossMultiplier">Value to multiply the stoploss with default in settings = 1<</param>
    /// <param name="TakeProfit">TP 2 = 2 tp 1 sl</param>
    /// <returns>double : takeprofit size</returns>
    public static double CalcTakeProfit(this Symbol thisSymbol, double tradeSize, double StopLossMultiplier, double TakeProfit)
    {
        var atrInPips = tradeSize * (thisSymbol.TickSize / thisSymbol.PipSize * Math.Pow(10, thisSymbol.Digits));
        return (atrInPips * StopLossMultiplier) * TakeProfit;
    }
}


public static class ContractionPattern
{
    /// <summary>
    /// Determines if the volatility is contracting.
    /// </summary>
    /// <param name="currentVolatility">The current volatility.</param>
    /// <param name="index">The current index of the bar.</param>
    /// <param name="bollingerBands">The Bollinger Bands indicator.</param>
    /// <param name="PeriodsBack">The number of periods to look back for previous volatility.</param>
    /// <returns>True if the current volatility is less than the previous volatility; otherwise, false.</returns>
    private static bool IsVolatilityContraction(double currentVolatility, int index, BollingerBands bollingerBands, int PeriodsBack)
    {
        if (index < PeriodsBack * 2)
            return false;

        var bbTop = bollingerBands.Top.ToArray();
        var bbBot = bollingerBands.Bottom.ToArray();
        double previousVolatility = bbTop[index - PeriodsBack] - bbBot[index - PeriodsBack];

        return currentVolatility < previousVolatility;
    }

    /// <summary>
    /// Checks if the market is in a volatility contraction phase.
    /// </summary>
    /// <param name="bars">The bars (candlesticks) data.</param>
    /// <param name="ColorBar">The action to color the bar if contraction is detected.</param>
    /// <param name="bollingerBands">The Bollinger Bands indicator.</param>
    /// <param name="PeriodsBack">The number of periods to look back for volatility comparison.</param>
    /// <returns>True if the market is contracting; otherwise, false.</returns>
    public static bool ContractionCheck(Bars bars, Action<int> ColorBar, BollingerBands bollingerBands, int PeriodsBack)
    {
        int index = bars.Count - 1;

        double upperBand = bollingerBands.Top.LastValue;
        double lowerBand = bollingerBands.Bottom.LastValue;

        double volatility = upperBand - lowerBand;

        bool isContraction = IsVolatilityContraction(volatility, index, bollingerBands, PeriodsBack);

        if (isContraction)
        {
            ColorBar(index);
            return isContraction;
        }
        return isContraction;
    }
}


public static class ContractionPhase
{
    /// <summary>
    /// A list to hold consolidation values.
    /// </summary>
    public static List<double> consolidation = new();


    /// <summary>
    /// Handles finding entries by adding the high and low prices of the current bar to the consolidation list.
    /// </summary>
    /// <param name="bars">The bars (candlesticks) data.</param>
    public static void HandleFindEntries(Bars bars)
    {
        int index = bars.Count - 1;

        double high = bars.HighPrices[index];
        double low = bars.LowPrices[index];

        consolidation.Add(high);
        consolidation.Add(low);
    }

    /// <summary>
    /// Returns the count of items in the consolidation list.
    /// </summary>
    /// <returns>The count of items in the consolidation list.</returns>
    public static int Count()
    {
        return consolidation.Count;
    }

    /// <summary>
    /// Clears the consolidation list.
    /// </summary>
    public static void Clear()
    {
        consolidation.Clear();
    }

    /// <summary>
    /// Returns the maximum value in the consolidation list.
    /// </summary>
    /// <returns>The maximum value in the consolidation list.</returns>
    public static double Max()
    {
        return consolidation.Max();
    }

    /// <summary>
    /// Returns the minimum value in the consolidation list.
    /// </summary>
    /// <returns>The minimum value in the consolidation list.</returns>
    public static double Min()
    {
        return consolidation.Min();
    }
}


public static class PricePercentageChange
{
    /// <summary>
    /// The percentage change in price.
    /// </summary>
    public static double priceChange;

    /// <summary>
    /// Calculates the price percentage change for the current day.
    /// </summary>
    /// <param name="bars">The bars (candlesticks) data.</param>
    /// <param name="Print">The action to print messages.</param>
    public static void Calculate(Bars bars, Action<string> Print)
    {
        int index = bars.Count - 1;

        double firstBar = 0;
        int currentDay = bars.OpenTimes[index].Day;
        int newDay = bars.OpenTimes[index].Day;
        int searchIndex = index;

        while (newDay == currentDay)
        {
            firstBar = bars.OpenPrices[searchIndex];
            searchIndex -= 1;
            newDay = bars.OpenTimes[searchIndex].Day;
        }

        //Print("Current bar: " + bars.ClosePrices[index]);
        //Print("First bar " + firstBar);
        priceChange = (((bars.ClosePrices[index] - firstBar) / firstBar) * 100);
    }

    /// <summary>
    /// Gets the calculated price percentage change.
    /// </summary>
    /// <returns>The calculated price percentage change.</returns>
    public static double GetChange()
    {
        return priceChange;
    }
}


/// <summary>
/// Represents the different states of the trading strategy.
/// </summary>
public enum TradingState
{
    /// <summary>
    /// The state where the strategy is looking for volatility contraction.
    /// </summary>
    LookingForContraction,

    /// <summary>
    /// The state where the strategy is waiting for volume confirmation and breakout.
    /// </summary>
    WaitingForVolumeAndBreak
}


/// <summary>
/// Manages the current state of the trading strategy.
/// </summary>
public static class StateMachine
{

    private static TradingState _currentState = TradingState.LookingForContraction;

    /// <summary>
    /// Toggles the current state to the specified new state.
    /// </summary>
    /// <param name="newState">The new state to set.</param>
    public static void ToggleState(TradingState newState) => _currentState = newState;

    /// <summary>
    /// Gets the current state of the trading strategy.
    /// </summary>
    public static TradingState CurrentState => _currentState;
}


/// <summary>
/// Stores information about the positions to enter.
/// </summary>
public static class PositionInfo
{
    /// <summary>
    /// The price level to enter a long position.
    /// </summary>
    public static double LongEntry { get; private set; }

    /// <summary>
    /// The price level to enter a short position.
    /// </summary>
    public static double ShortEntry { get; private set; }


    public static TradeDirection _tradedirection;

    /// <summary>
    /// Sets the entry prices for long and short positions.
    /// </summary>
    /// <param name="buy">The price level to enter a long position.</param>
    /// <param name="sell">The price level to enter a short position.</param>
    public static void SetEntries(double buy, double sell)
    {
        LongEntry = buy;
        ShortEntry = sell;
    }

    public static void SetTradeDirection(TradeDirection direction) => _tradedirection = direction;
}



/// <summary>
/// Class that provides methods to check volume conditions in trading.
/// </summary>
public static class VolumeCheck
{
    /// <summary>
    /// Stores the current volume.
    /// </summary>
    public static int CurreVolume = 0;

    /// <summary>
    /// Calculates the average volume of the bars.
    /// </summary>
    /// <param name="bars">The bars to calculate the average volume from.</param>
    /// <returns>The average volume of the specified bars.</returns>
    private static double CalculateAverageVolume(Bars bars, Action<string> Print)
    {
        double sumVolume = 0;
        int index = bars.Count - 2;
        int currentDay = bars.OpenTimes[index].Day;
        int newDay = currentDay;
        int searchIndex = 0;

        while (newDay == currentDay)
        {
            sumVolume += bars.TickVolumes[index];
            newDay = bars.OpenTimes[index].Day;

            if (newDay != currentDay)
            {
                break;
            }
            index -= 1;
            searchIndex += 1;
        }
        //Print("Last bar volume: " + bars.TickVolumes[bars.Count - 2 ]);
        //Print("Current average: " + sumVolume / searchIndex);
        return sumVolume / searchIndex;
    }

    /// <summary>
    /// Checks if the current bar's volume is above the average volume.
    /// </summary>
    /// <param name="averageVolume">The average volume to compare against.</param>
    /// <param name="currentBarVolume">The current bar's volume.</param>
    /// <returns>True if the current bar's volume is above the average volume, otherwise false.</returns>
    private static bool IsCurrentBarVolumeAboveAverage(double averageVolume, double currentBarVolume)
    {
        return currentBarVolume > (averageVolume * 0.8);
    }

    /// <summary>
    /// Determines if the current bar's volume is above the average volume and prints a message if it is.
    /// </summary>
    /// <param name="bars">The bars to analyze.</param>
    /// <param name="Print">The action to print messages.</param>
    /// <returns>True if the current bar's volume is above the average volume, otherwise false.</returns>
    public static bool AboveAverage(Bars bars, Action<string> Print)
    {
        double averageVolume = CalculateAverageVolume(bars, Print);
        double currentBarVolume = bars.TickVolumes[bars.Count - 2];
        Print("Average Volume: " + averageVolume);
        Print("Volume: " + currentBarVolume);
        if (IsCurrentBarVolumeAboveAverage(averageVolume, currentBarVolume))
        {
            Print("Current bar volume " + currentBarVolume + " is above the average volume " + averageVolume);
            return true;
        }
        return false;
    }
}


public enum TradeDirection
{
    LONG,
    SHORT,
    NONE
}


namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None, TimeZone = TimeZones.GMTStandardTime)]
    public class VolatilityContractionBreakOut : Robot
    {
        [Parameter("Price change %", DefaultValue = 2, MinValue = 0.5, MaxValue = 40, Step = 0.1, Group = "Basic")]
        public double PRICE_CHANGE { get; set; }

        [Parameter("Contraction Check back periods", DefaultValue = 10, MinValue = 1, MaxValue = 40, Step = 1, Group = "Basic")]
        public int CONTRACTION_CHECK_BACK_PERIODS { get; set; }

        [Parameter("Contraction BB periods", DefaultValue = 10, MinValue = 1, MaxValue = 40, Step = 1, Group = "Basic")]
        public int BB_PERIODS { get; set; }

        [Parameter("Trade size % of account", DefaultValue = 1, MinValue = 0.1, Step = 0.1, Group = "Risk Settings")]
        public double Risk_Percentage { get; set; }

        [Parameter("TakeProfit (Multiplied with ATR)", DefaultValue = 2, MinValue = 1, Step = 1, Group = "Risk Settings")]
        public double TakeProfit { get; set; }

        [Parameter("ATR multiplier for SL/TP", DefaultValue = 2, MinValue = 1, Step = 1, Group = "Risk Settings")]
        public int AtrMultiplier { get; set; }


        protected override void OnStart()
        {
        }

        protected override void OnTick()
        {
        }

        protected override void OnBar()
        {
            CalculateATR();
            Strategy();
        }

        protected override void OnStop()
        {
        }


        void Strategy()
        {
            if(Filters())
            {
                switch (StateMachine.CurrentState)
                {
                    case TradingState.LookingForContraction:
                        LookingForContractionPhase();
                        break;
                    case TradingState.WaitingForVolumeAndBreak:
                        VolumeChecker();
                        break;
                    default:
                        Print("Error in State management");
                        break;
                }
            }
        }
        
        
        bool Filters()
        {
            return Positions.Find("VOLATILITY_CONTRACTION") == null;
        }


        void LookingForContractionPhase()
        {
            bool isContracting = Contraction();
            if (isContracting)
            {
                ContractionPhase.HandleFindEntries(Bars);
            }
            else if (ContractionPhase.Count() != 0)
            {
                PriceChange();
                MarkEntries();
            }
        }


        bool Contraction()
        {
            BollingerBands bollingerBands = Indicators.BollingerBands(Bars.ClosePrices, BB_PERIODS, 2.0, MovingAverageType.Simple);

            bool isContracting = ContractionPattern.ContractionCheck(Bars, ColorBar, bollingerBands, CONTRACTION_CHECK_BACK_PERIODS);
            return isContracting;
        }


        void PriceChange()
        {
            PricePercentageChange.Calculate(Bars, PrintMessage);
        }


        void MarkEntries()
        {
            // If contraction was only 1 candle.
            if (ContractionPhase.Count() == 1)
            {
                ContractionPhase.Clear();
                return;
            }
            Print("Change: ", Math.Abs(PricePercentageChange.GetChange()));
            Print("Change needed: ", Math.Abs(PRICE_CHANGE));
            if (Math.Abs(PricePercentageChange.GetChange()) >= PRICE_CHANGE)
            {
                PositionInfo.SetEntries(ContractionPhase.Max(), ContractionPhase.Min());
                StateMachine.ToggleState(TradingState.WaitingForVolumeAndBreak);
            }
        }


        int VolumeBarCounter = 0;
        void VolumeChecker()
        {
            if (VolumeBarCounter == 3)
            {
                VolumeBarCounter = 0;
                StateMachine.ToggleState(TradingState.LookingForContraction);
            }

            bool IsVolumeAboveAverage = VolumeCheck.AboveAverage(Bars, PrintMessage);
            if (VolumeEntryConfirmed(IsVolumeAboveAverage))
            {
                ExecuteTrade();
            }
            else
            {
                VolumeBarCounter += 1;
            }
        }

        bool VolumeEntryConfirmed(bool IsVolumeAboveAverage)
        {
            double CurrentPrice = Bars.ClosePrices[Bars.Count - 1];
            Print("1");
            if (CurrentPrice >= ContractionPhase.Max())
            {
                Print("2");
                PositionInfo.SetTradeDirection(TradeDirection.LONG);
            }
            else if (CurrentPrice <= ContractionPhase.Min())
            {
                Print("3");
                PositionInfo.SetTradeDirection(TradeDirection.SHORT);
            }
            else 
            {
                Print("4");
                PositionInfo.SetTradeDirection(TradeDirection.NONE);
            }
            Print(IsVolumeAboveAverage);
            return IsVolumeAboveAverage && PositionInfo._tradedirection != TradeDirection.NONE;
        }


        void ExecuteTrade()
        {
            double SL = Symbol.CalculateStopLoss(_ATR.Result.Last(1), AtrMultiplier);
            double TP = Symbol.CalcTakeProfit(_ATR.Result.Last(1), AtrMultiplier, TakeProfit);
            double SIZE = Symbol.CalculateLotSize(Account.Balance, Risk_Percentage, SL);
            TradeType tradetype = PositionInfo._tradedirection == TradeDirection.LONG ? TradeType.Buy : TradeType.Sell;
            
            ExecuteMarketOrder(tradetype, Symbol.Name, SIZE, "VOLATILITY_CONTRACTION", SL, TP);
            
            StateMachine.ToggleState(TradingState.LookingForContraction);
        }


        private AverageTrueRange _ATR;
        public void CalculateATR()
        {
            _ATR = Indicators.AverageTrueRange(14, MovingAverageType.Simple);
        }



        void PrintMessage(string text)
        {
            Print(text);
        }


        void ColorBar(int index)
        {
            Chart.SetBarFillColor(index, Color.Blue);
        }
    }
}