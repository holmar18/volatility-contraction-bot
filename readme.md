# Volatility Contraction Pattern Detection Bot

## Overview

This repository contains a test implementation of an automated trading system designed to detect volatility contraction patterns in stock prices. The bot identifies these patterns and determines if the price breaks the high or low of the pattern within the next three bars, with the volume exceeding 80% of the average volume for the current day. The bot is currently tailored for lower timeframes and is focused on stock trading.

## Features

- **Volatility Contraction Pattern Detection**: Identifies periods where stock price volatility decreases and looks for subsequent price breaks.
- **Volume Analysis**: Ensures that the volume during the breakout exceeds 80% of the average volume for the current day.
- **Stock Trading Focus**: Optimized for trading stocks using lower timeframe data (intraday).
- **Real-time Analysis**: Utilizes real-time market data to identify and react to patterns.

## How It Works

1. **Calculate Average Volume**: The bot calculates the average volume of the current trading day.
2. **Pattern Detection**: Identifies volatility contraction patterns based on historical bar data.
3. **Volume Check**: Compares the volume of the current bar to 80% of the calculated average volume.
4. **Breakout Detection**: Checks if the price breaks the high or low of the identified pattern within the next three bars.
5. **Logging**: Prints relevant information about the detected patterns and volume checks for analysis.

## Version Information

- **Version**: 1.0.0
- **Release Date**: June 18, 2024
- **Status**: Beta

## Requirements

- **cTrader Platform**: This bot is designed to run on the cTrader platform.
- **C# Knowledge**: Basic understanding of C# programming is necessary to understand and modify the code.
- **Market Data Access**: Real-time market data access is required for the bot to function correctly.


## Contributions

Contributions are welcome! If you have any ideas, suggestions, or improvements, feel free to open an issue or create a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.