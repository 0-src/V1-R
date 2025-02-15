using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace V1_R
{
    /// <summary>
    /// Represents a trade instruction received via JSON.
    /// </summary>
    public class TradeInstruction
    {
        public string Ticker { get; set; }
        public string Action { get; set; }
        public string Sentiment { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Quantity { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double Price { get; set; }

        public DateTime Time { get; set; }
    }

    /// <summary>
    /// A wrapper around the NinjaTrader Client that abstracts away the underlying API.
    /// This wrapper allows you to call only the methods you need and adds a layer of flexibility.
    /// </summary>
    public class ClientWrapper : IDisposable
    {
        private NinjaTrader.Client.Client ntClient;

        public ClientWrapper()
        {
            // Instantiate the underlying NinjaTrader client.
            ntClient = new NinjaTrader.Client.Client();
        }

        /// <summary>
        /// Configures the underlying client with the specified host and port.
        /// </summary>
        public void SetUp(string host, int port)
        {
            ntClient.SetUp(host, port);
        }

        /// <summary>
        /// Gets the cash value for a given account.
        /// </summary>
        public double GetCashValue(string account)
        {
            return ntClient.CashValue(account);
        }

        /// <summary>
        /// Gets the buying power for a given account.
        /// </summary>
        public double GetBuyingPower(string account)
        {
            return ntClient.BuyingPower(account);
        }

        /// <summary>
        /// Gets the realized profit and loss for a given account.
        /// </summary>
        public double GetRealizedPnL(string account)
        {
            return ntClient.RealizedPnL(account);
        }

        /// <summary>
        /// Executes a trade command using the specified parameters.
        /// </summary>
        /// <param name="command">Command text (e.g., "BUY" or "SELL")</param>
        /// <param name="account">Account identifier</param>
        /// <param name="instrument">Ticker/instrument symbol</param>
        /// <param name="action">Action type</param>
        /// <param name="quantity">Quantity to trade</param>
        /// <param name="orderType">Type of order (e.g., "LIMIT")</param>
        /// <param name="limitPrice">Price for the order</param>
        /// <param name="stopPrice">Stop price (if applicable)</param>
        /// <param name="timeInForce">Time in force (e.g., "DAY")</param>
        /// <param name="oco">OCO (One Cancels Other) grouping (if any)</param>
        /// <param name="orderId">Order ID (if applicable)</param>
        /// <param name="strategy">Strategy name (if applicable)</param>
        /// <param name="strategyId">Strategy identifier (if applicable)</param>
        /// <returns>An integer result code from the API</returns>
        public int ExecuteCommand(string command, string account, string instrument, string action, int quantity,
                                  string orderType, double limitPrice, double stopPrice, string timeInForce,
                                  string oco, string orderId, string strategy, string strategyId)
        {
            return ntClient.Command(command, account, instrument, action, quantity, orderType,
                                      limitPrice, stopPrice, timeInForce, oco, orderId, strategy, strategyId);
        }

        /// <summary>
        /// Processes a trade instruction provided as a JSON string.
        /// The JSON should follow the structure:
        /// {
        ///     "ticker": "TICKER",
        ///     "action": "BUY",
        ///     "sentiment": "LONG",
        ///     "quantity": 1,
        ///     "price": 20000,
        ///     "time": "2025-02-13T14:30:00"
        /// }
        /// </summary>
        /// <param name="json">The JSON payload containing trade instruction data.</param>
        /// <param name="defaultAccount">A default account to use if not provided elsewhere.</param>
        /// <returns>The result code from executing the trade command.</returns>
        public int ProcessTradeInstruction(string json, string defaultAccount = "Sim101")
        {
            // Deserialize the JSON into a TradeInstruction object.
            TradeInstruction instruction = JsonSerializer.Deserialize<TradeInstruction>(json);

            // Validate basic fields.
            if (instruction == null)
                throw new ArgumentException("Invalid JSON payload.");
            if (string.IsNullOrWhiteSpace(instruction.Ticker) ||
                string.IsNullOrWhiteSpace(instruction.Action) ||
                instruction.Quantity <= 0)
            {
                throw new ArgumentException("Invalid trade instruction parameters.");
            }

            // Map the instruction to command parameters.
            // For demonstration, we use:
            // - defaultAccount as the account,
            // - instruction.Ticker as the instrument,
            // - instruction.Action as both command and action,
            // - instruction.Quantity as the quantity,
            // - "LIMIT" as the orderType,
            // - instruction.Price as the limit price,
            // - 0 as the stopPrice,
            // - "DAY" as the timeInForce.
            // Other parameters (oco, orderId, strategy, strategyId) are set to empty strings.
            int result = ExecuteCommand(
                command: instruction.Action.ToUpper(),     // e.g., "BUY" or "SELL"
                account: defaultAccount,
                instrument: instruction.Ticker,
                action: instruction.Action.ToUpper(),
                quantity: instruction.Quantity,
                orderType: "LIMIT",          // Default order type; adjust as needed.
                limitPrice: instruction.Price,
                stopPrice: 0,                // No stop price provided.
                timeInForce: "DAY",          // Default time in force.
                oco: string.Empty,
                orderId: string.Empty,
                strategy: string.Empty,
                strategyId: string.Empty
            );

            return result;
        }

        /// <summary>
        /// Gets the live price (last price) for the specified instrument.
        /// The MarketData method with type 0 returns the last price.
        /// </summary>
        public double GetLivePrice(string instrument)
        {
            return ntClient.MarketData(instrument, 0);
        }

        /// <summary>
        /// Convenience method to get the live price for "NQ MAR25".
        /// </summary>
        public double GetLivePriceNQMar25()
        {
            return GetLivePrice("NQ 03-25");
        }

        /// <summary>
        /// Disposes the underlying client.
        /// </summary>
        public void Dispose()
        {
            if (ntClient != null)
            {
                ntClient.Dispose();
                ntClient = null;
            }
        }
    }
}
