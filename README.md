# V1-R Project

## Overview
V1-R is a trading automation project integrating **NinjaTrader Client DLL** with **TradingView** for automated position entries/exits, webhook handling, and execution logic. This project allows traders to automate their strategies and streamline trading operations.

## Features
- Automated trade execution using NinjaTrader Client DLL
- Webhook handling from TradingView
- Configuration-driven setup stored in Windows `Documents` folder
- Logging and monitoring of trade executions
- Scalable and maintainable architecture

## Installation
1. Clone this repository:
   ```sh
   git clone https://github.com/your-repo/V1-R.git
   ```
2. Open `V1-R.sln` in Visual Studio.
3. Restore dependencies:
   ```sh
   dotnet restore
   ```
4. Build and run the project.

## Configuration
Before running the application, configure the `config.json` file, which is located in your Windows `Documents` folder. Below is the expected structure:

```json
{
  "AuthToken": "NgrokAUTH", 
  "NgrokUrl": "URL", 
  "Accounts": [
    { "AccountName": "Account1", "Strategy":"Strategy1" }, 
    { "AccountName": "Account2", "Strategy":"Strategy2"} 
  ]
}
```

## Project Structure
```
V1-R/
├── Classes/
│   ├── Client.cs       # Handles webhook execution logic and trade management
├── Docs/
│   ├── NTApi.md        # Documentation for NinjaTrader Client DLL integration
├── Properties/
│   ├── AssemblyInfo.cs # Assembly metadata
├── MainWindow.xaml     # Main UI structure
├── MainWindow.xaml.cs  # Main application logic for handling trades
├── App.xaml           # Application entry settings
├── App.xaml.cs        # Application startup logic
├── config.json        # Configuration file (stored in Windows Documents folder)
├── V1-R.csproj        # Project file
├── V1-R.sln           # Solution file
├── README.md          # Project documentation (this file)
├── LICENSE.txt        # License details
.gitignore             # Git ignored files
```

## Functions Overview
### `Client.cs`
- **private Dictionary<string, string> selectedAccounts**: Stores selected accounts for trading.
- **private readonly object lockObj**: Ensures thread safety.
- **public ClientWrapper(ListBox logListBox)**: Initializes the client wrapper with logging.
- **private void LogExecution(string message)**: Logs trade executions.
- **private void LoadConfig()**: Loads configurations from `config.json`.
- **private void StartWebhookServer()**: Starts the webhook listener.
- **public void SetUp(string host, int port)**: Initializes the connection to NinjaTrader.
- **public double GetCashValue(string account)**: Retrieves account cash balance.
- **public double GetBuyingPower(string account)**: Retrieves buying power for an account.
- **public double GetRealizedPnL(string account)**: Retrieves realized profit/loss.
- **public void UpdateSelectedAccounts(List<Account> selectedAccountsList)**: Updates selected accounts.
- **public int ProcessTradeInstruction(string json)**: Processes incoming trade instructions.
- **public void SubscribeData(string instrument)**: Subscribes to market data.
- **public void UnSubscribeData(string instrument)**: Unsubscribes from market data.
- **public double GetLivePrice(string instrument)**: Retrieves the latest price for an instrument.
- **public void Dispose()**: Releases resources.
- **public void StopNgrok()**: Stops the Ngrok tunnel.

### `MainWindow.xaml.cs`
- **public MainWindow()**: Initializes the main application window.
- **private void PriceUpdateTimer_Tick(object sender, EventArgs e)**: Handles price update intervals.
- **private void AccountCheckBox_Checked(object sender, RoutedEventArgs e)**: Handles account selection.
- **private void AccountCheckBox_Unchecked(object sender, RoutedEventArgs e)**: Handles account deselection.
- **private void UpdateSelectedAccountsInClient()**: Updates the selected accounts in `Client.cs`.
- **private void LoadAccounts()**: Loads account details into the UI.
- **private void UpdateAccountInfo()**: Updates account information in the UI.
- **private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)**: Enables window dragging.
- **private void CloseButton_Click(object sender, RoutedEventArgs e)**: Closes the application.

## Usage
1. Set up your `config.json` file in your Windows `Documents` folder as per the above structure.
2. Ensure NinjaTrader 8 is Open 
3. Start the application and monitor the logs for execution updates.
4. Ensure your NinjaTrader Client DLL is correctly installed and configured.


## License
This project is licensed under the MIT License. See `LICENSE.txt` for details.

