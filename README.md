# V1-R – NinjaTrader Automation via TradingView Webhooks

**V1-R** is an advanced trading automation tool that connects the NinjaTrader 8 platform with TradingView alerts to enable hands-free trade execution. It leverages the NinjaTrader Client API (via the `NinjaTrader.Client.dll` library) and a custom webhook listener to enter or exit positions automatically based on TradingView signals. Designed for experienced developers and traders, V1-R provides a configurable, extensible framework for executing trades, monitoring account status, and integrating strategy alerts in real-time.

## Key Features

- **Automated Trade Execution:** Places and manages trades on NinjaTrader 8 using the official NinjaTrader Client DLL interface. This allows external control of NinjaTrader for order placement, position management, and data retrieval.  
- **TradingView Webhook Integration:** Accepts webhook alerts from TradingView strategies. When a TradingView alert is triggered, V1-R receives the JSON payload and processes it into trade instructions, enabling seamless strategy automation across platforms.  
- **Configurable Setup:** Uses a JSON config file to map TradingView strategy identifiers to NinjaTrader account names and to store integration settings (such as Ngrok tunnel info). This configuration-driven design lets you easily adapt the tool to different accounts or environments without code changes.  
- **Real-Time Logging & UI Monitoring:** Provides a WPF desktop interface that logs trade execution steps and displays live account data (balances, PnL, prices) for transparency. All actions are logged for audit and debugging, helping advanced users monitor and verify automated trades.  
- **Scalable Architecture:** Built with a modular structure and thread-safe design (using locks for concurrency) to ensure reliable operation. The architecture cleanly separates concerns (data handling, execution logic, UI), making it maintainable and extensible for further development.

## Installation Instructions

### Prerequisites

- **Operating System & SDK:** Windows 10/11 with [.NET 8.0 SDK](https://dotnet.microsoft.com/download) installed (V1-R targets .NET 8.0 WPF on Windows).  
- **Development Tools:** Visual Studio 2022 (or newer) with .NET desktop development workload, or the `dotnet` CLI, to build and run the project.  
- **NinjaTrader 8 Platform:** NinjaTrader 8 must be installed on the system. V1-R relies on NinjaTrader’s Client API DLL, which is typically located at `C:\Program Files\NinjaTrader 8\bin\NinjaTrader.Client.dll`. Ensure this file exists (the project includes a reference to it) and that NinjaTrader 8 is **running** when you use V1-R.  
- **Ngrok (for Webhooks):** An Ngrok account (free or paid) is recommended if you plan to receive webhooks from TradingView over the internet. Obtain an auth token and (optionally) reserve a custom subdomain for a stable webhook URL. Install the Ngrok agent on your machine or have the binary accessible in your PATH for V1-R to launch.  

### Setup and Build

1. **Clone the Repository:** Download the V1-R source code from GitHub. You can use the command: 

   ```bash
   git clone https://github.com/0-src/V1-R.git
   ``` 

   This will create a local `V1-R` project directory.

2. **Open the Solution:** Launch **Visual Studio** and open the `V1-R.sln` solution file at the root of the cloned repository. If you prefer CLI, you can navigate into the project directory and proceed with .NET CLI commands.

3. **Restore Dependencies:** V1-R uses the NinjaTrader client DLL (which should be present from your NinjaTrader installation) and standard .NET packages. Use Visual Studio’s package restore or run the following in a terminal to ensure all dependencies are in place:

   ```bash
   dotnet restore
   ```

   > **Note:** The NinjaTrader API DLL reference is set to the default install path. If your NinjaTrader is installed in a custom location, update the reference hint path in the `.csproj` file or place the DLL in the expected path.

4. **Build the Project:** After restoring, build the solution (**Build > Build Solution** in Visual Studio, or `dotnet build` via CLI). The project targets .NET 8.0; ensure your environment supports this. Successful build will produce the executable.

5. **Configure the Application:** Before running V1-R, prepare the configuration file as described in the next section. The app looks for `config.json` in your Windows **Documents** folder by default.

6. **Run V1-R:** Start the application. You can run it through Visual Studio (e.g., press F5 to run with debugger, or Ctrl+F5 without) or launch the compiled `V1-R.exe`. On startup, ensure NinjaTrader 8 is already open and connected (so that the API can interact). The V1-R UI window should appear, and if configured, it will establish the webhook listener and connect to NinjaTrader’s API. Monitor the **Log** panel in the UI for status messages (e.g., “Ngrok tunnel started”, “Connected to NinjaTrader”, etc.).

### Configuration

After installation, V1-R requires a one-time setup via a JSON config file to know how to route TradingView alerts to your NinjaTrader accounts and how to set up the webhook tunnel. By default, the application expects a file named **`config.json`** located in your Windows **Documents** folder (e.g., `C:\Users\<YourName>\Documents\config.json`).

**Sample `config.json`:**

```json
{
  "AuthToken": "NGROK_AUTH_TOKEN",
  "NgrokUrl": "your-ngrok-subdomain", 
  "Accounts": [
    { "AccountName": "Account1", "Strategy": "Strategy1" },
    { "AccountName": "Account2", "Strategy": "Strategy2" }
  ]
}
``` 



- **AuthToken:** Your Ngrok authentication token for programmatic tunnel setup. This is required if you want V1-R to automatically start an Ngrok tunnel for receiving webhooks. (You can find your auth token on your Ngrok dashboard.)  
- **NgrokUrl:** The reserved domain or subdomain for your tunnel (if any). For example, if you have reserved “`mytradeapp`” as a subdomain, you might put `"NgrokUrl": "mytradeapp"`. V1-R will use this to start an HTTP tunnel like `mytradeapp.ngrok.io`. If left blank or not provided, a random Ngrok URL may be assigned for each session.  
- **Accounts:** An array of account mappings. Each entry links a NinjaTrader account to a strategy name. **AccountName** should exactly match the account identifier as it appears in NinjaTrader, and **Strategy** is an arbitrary label you will use in TradingView alerts. You can list multiple account-strategy pairs. For example, as shown above, alerts tagged with `"Strategy":"Strategy1"` will be executed on `Account1`, and `"Strategy":"Strategy2"` on `Account2`. If you have only one account or strategy, you can use a single mapping.

**Config file location:** The config is loaded from the current Windows user’s Documents directory at runtime. Ensure you save the file there. (This avoids needing admin privileges and separates config from code.) You can change this path in the code (`LoadConfig()` in `Client.cs`) if needed for your setup.

## Usage Examples

Once installed and configured, V1-R can be used to automate trades triggered by TradingView alerts. Below are examples of how to integrate it into your trading workflow:

- **Starting the Application:** Launch NinjaTrader 8 and log in to your brokerage/data feed. Then start V1-R. On startup, the application will set up its internal components: it loads the configuration, connects to NinjaTrader’s API, and if Ngrok credentials are provided, initiates an Ngrok tunnel to expose the local webhook server. The UI will display a log message for each step (e.g., "Configuration loaded", "Webhook server listening on port ...", "Ngrok tunnel running at https://<subdomain>.ngrok.io"). If you have multiple accounts configured, use the checkboxes in the UI to select which account(s) should receive trading signals.

- **Setting up a TradingView Alert:** In TradingView, create or edit an alert for your strategy. Set the **Webhook URL** to the public URL of your V1-R instance. This will be your Ngrok address (for example, `https://mytradeapp.ngrok.io` or the random URL Ngrok provides if you didn’t set a subdomain). There is no custom path needed in the URL unless specified by V1-R (by default it listens on the root path, which is covered by the base Ngrok URL). In the alert message body, include a JSON payload that V1-R can interpret. For instance, if you want to go long on a signal using the strategy labeled "Strategy1", you might use a payload like: 

  ```json
  {
    "Strategy": "Strategy1",
    "Action": "BUY",
    "Symbol": "ES",
    "Quantity": 1
  }
  ``` 

  This is an example; you can define the JSON fields as needed, but they should align with what the V1-R `ProcessTradeInstruction` logic expects. Typically, it will at least look for a strategy identifier (to map to an account) and some instruction of what to do. **Strategy** should match one of the strategy names in your config. You may include fields like **Action** (e.g., "BUY" or "SELL"), **Symbol** (e.g., market symbol or instrument code), **Quantity**, **StopLoss**, **TakeProfit**, etc., depending on how you design your alert and parse it in the code. (Refer to the `ProcessTradeInstruction(string json)` implementation in `Client.cs` for the exact expected format.)

- **Trade Execution Flow:** When TradingView triggers the alert, it will send an HTTP POST request to your Ngrok URL with the JSON payload. The Ngrok agent securely tunnels this request to your local V1-R application. V1-R’s webhook server (running inside `Client.StartWebhookServer()`) receives the request and passes the payload to `ProcessTradeInstruction`. The `Client` parses the JSON, logs the received instruction (via `LogExecution`), and determines which NinjaTrader account to use by looking up the strategy name in the config’s Accounts mapping. It then uses NinjaTrader’s API functions to execute the trade. For example, if the JSON indicated a buy for *ES* on *Strategy1*, V1-R will find the corresponding account (e.g., *Account1*) and call the appropriate NinjaTrader.Client methods to place a buy order for the ES instrument on that account. This happens in the background, without further user intervention.

- **Monitoring and Verification:** Immediately after the webhook is processed, check the V1-R application log (within the UI) for an entry confirming the action, such as *"Executed BUY 1 ES on Account1"* or any error messages if something went wrong. The NinjaTrader UI (if open) should also reflect the new position or order. V1-R continuously updates the linked account’s information in its UI – you’ll see changes in account cash value, buying power, or PnL as provided by the API. The `MainWindow` interface uses a timer to periodically fetch live prices and account metrics via the `Client` (e.g., using `GetLivePrice` and other getters) and refresh the display. This allows you to verify in real-time that the strategy signals are executing as expected. 

- **Multiple Accounts or Strategies:** If you have multiple strategies or accounts, you can select which ones are active by toggling the checkboxes in the UI corresponding to each account. Only selected accounts will be updated and used for incoming instructions. For instance, you might run two TradingView alerts with different strategy names; V1-R will route each to the appropriate account as long as those accounts are checked/active. You can update the selected accounts on the fly, and the `UpdateSelectedAccountsInClient()` routine will sync the selection with the backend logic.

- **Stopping the System:** To gracefully shut down, simply close the V1-R application window. This triggers the disposal of resources in `Client.Dispose()` and will also stop the Ngrok tunnel if one was running. The NinjaTrader platform will remain running (V1-R does not shut it down). If you restart V1-R, it will reconnect to NinjaTrader and establish a new webhook tunnel (likely with a new URL if not using a fixed subdomain).

## Technical Overview

Under the hood, V1-R is composed of a WPF application with a background client that orchestrates trading operations. The design separates the concerns of UI interaction, external communications (webhooks and API calls), and core logic. Below is an overview of the architecture and key components:

### Architecture and Data Flow

1. **Webhook Server:** On launch, the `Client` component starts an internal HTTP listener (`StartWebhookServer`) on a specified port to await incoming webhook HTTP requests. By default, this listens on a localhost port (configured in code) for JSON payloads from TradingView alerts. To allow TradingView (cloud) to reach your local machine, V1-R utilizes **Ngrok**. The client will automatically start an Ngrok tunnel using the provided AuthToken and desired subdomain from the config, which exposes the local webhook port to a public URL. (If manual, you would run `ngrok http <port>` yourself.) The result is that TradingView can post to `https://<YourNgrokSubdomain>.ngrok.io` and that request is forwarded to V1-R’s webhook server on your PC.

2. **JSON Instruction Processing:** When a webhook is received, the raw JSON content is passed to `Client.ProcessTradeInstruction(string json)`. This function parses the JSON (using .NET JSON libraries) and interprets it as a trade command. Based on the payload, it determines the target account/strategy and the action to perform (e.g., enter long, exit position, set stop, etc.). The design allows flexible JSON structures as long as the required fields (like the strategy name and trade action) are present and understood by your implementation of `ProcessTradeInstruction`. The method returns an integer status code indicating the result of the instruction (for example, 0 for success or error codes for various failures), which could be used for logging or flow control.

3. **NinjaTrader API Integration:** Upon parsing the instruction, V1-R uses NinjaTrader’s managed API to execute the trade. The `Client.SetUp(host, port)` method is called during initialization to connect to the NinjaTrader application (NinjaTrader’s API typically connects via localhost and a port). Once connected, V1-R can invoke functions from `NinjaTrader.Client.dll` to perform operations like place orders, check positions, or subscribe to market data. For instance, the `Client` class provides methods such as `SubscribeData(symbol)` to start receiving price updates for a given instrument, or `GetCashValue(account)` to query account balance – these likely wrap around NinjaTrader API calls. Key trading functions are implemented in the NinjaTrader API (e.g., to submit orders or modify them), and V1-R’s role is to call those with parameters determined by the webhook. **Important:** The NinjaTrader application must be running and logged in, since the API calls are directed to the live NinjaTrader session. V1-R does not run NinjaTrader itself; it acts as a client to it.

4. **UI Update and Logging:** The WPF **MainWindow** provides a simple interface mainly for visualization and manual oversight. It initializes and holds an instance of the `Client` (often via a wrapper or directly) and passes it a reference to the log display (e.g., a `ListBox` or similar UI element). The `Client` uses this to append log messages (`LogExecution`) for each significant event, such as receiving an instruction or executing a trade. A timer in the UI (`PriceUpdateTimer_Tick`) periodically requests the latest prices and account info via the client’s methods and refreshes the display. When the user interacts with the UI, e.g., checking or unchecking an account in the list, event handlers (`AccountCheckBox_Checked/Unchecked`) call `Client.UpdateSelectedAccounts(...)` to inform the backend which accounts are active. This mechanism ensures that only those accounts will have trades executed on them or data fetched, providing control to the user even while the system is running. The application also employs a thread-safety mechanism using a lock object in `Client` to synchronize access to shared resources (for example, the selected accounts list and NinjaTrader API calls). This prevents race conditions when multiple events (like concurrent webhooks or UI actions) occur simultaneously.

### Key Components

- **Client.cs:** This is the core engine of V1-R. It handles configuration loading, connection to NinjaTrader, and the webhook server. It contains methods to start/stop the Ngrok tunnel, process incoming trade signals, and query account or market data. The `Client` maintains internal state such as the set of selected accounts (from the UI) and uses a lock (`lockObj`) to ensure thread-safe updates. In essence, `Client.cs` is responsible for translating external events (webhook calls) into NinjaTrader API calls and updating the UI/log with the results.

- **MainWindow.xaml & MainWindow.xaml.cs:** This defines the GUI and its behavior. The window provides checkboxes or list elements for each configured account and a log output area. In the code-behind (MainWindow.xaml.cs), it initializes the `Client` (often via a `ClientWrapper` as hinted in the code), sets up event handlers for UI elements, and starts a timer to periodically refresh data. For example, when an account checkbox is toggled, `MainWindow` invokes `UpdateSelectedAccountsInClient()` to sync with the client component. The UI also includes controls for closing the app (with custom window chrome events for a better UX). While most trading logic is in the `Client`, the `MainWindow` ensures the user can monitor and control which strategies are active without editing config or code at runtime.

- **NinjaTrader Client DLL:** This is not part of the repository’s source, but rather an external dependency. It is the official NinjaTrader 8 API library that V1-R calls into for all trading operations. Functions like getting account balances, placing orders, or subscribing to price feeds are provided by this DLL. V1-R includes a reference to this DLL in its project file and expects it to be present on the system (installed with NinjaTrader). Advanced users can refer to NinjaTrader’s ATI (Automated Trading Interface) documentation for details on available functions (for example, order placement functions, subscription handling, etc.). V1-R’s `Docs/NTApi.md` (in the repository) likely summarizes how to use some of these API calls in context.

- **Ngrok Integration:** V1-R automates the creation and teardown of the Ngrok tunnel for convenience. When the application starts, if an `AuthToken` is provided in config, it will attempt to launch a background Ngrok process (via command-line) to create a public URL pointing to the local webhook server. The `NgrokUrl` (subdomain) from config is used so you get a consistent address (otherwise Ngrok assigns a random URL each time). The client code’s `StopNgrok()` method will terminate this tunnel when the app closes, or you can manually stop Ngrok if needed. This integration means you don’t have to start Ngrok manually for each session, simplifying the workflow. (Ensure the Ngrok binary is installed and accessible; the application might assume it can call `ngrok` from the command line.)

By structuring the system into the above components, V1-R achieves a clear separation of duties: the **UI** for user interaction and monitoring, the **Client logic** for handling external inputs and executing trades, and the **external services** (TradingView and NinjaTrader) connected via webhooks and API calls. This modular architecture makes it easier for developers to extend or modify parts of the system – for example, adding new types of instructions (maybe to set stop-loss orders on NinjaTrader) would mostly involve updating `ProcessTradeInstruction` and possibly the JSON format expected, without affecting the UI. Likewise, one could replace the Ngrok integration with another tunneling service by modifying the tunnel start/stop logic, all while keeping the core trading logic intact.

## License

V1-R is open-source software licensed under the **MIT License**. You are free to use, modify, and distribute this project. See the [`LICENSE.txt`](https://github.com/0-src/V1-R/blob/master/LICENSE.txt) file for the full license text.

