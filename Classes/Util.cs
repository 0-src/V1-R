using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace V1_R.Classes
{
    public class TradeLog
    {
        public required string AccountName
            { get; set; }
        public string Ticker 
            { get; set; }
        public string Action 
            { get; set; }
        public string Sentiment 
            { get; set; }
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Quantity 
            { get; set; }
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double Price 
            { get; set; }
        public string Strategy 
            { get; set; }

        public DateTime Time 
            { get; set; } = DateTime.UtcNow;
    }

    public static class Util
    {
        private static readonly string LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "trade_logs.json");

        /// <summary>
        /// Logs a trade execution to a JSON file for later analysis.
        /// </summary>
        public static void LogTrade(TradeLog tradeLog)
        {
            try
            {
                Debug.WriteLine($"[LogTrade] Attempting to log trade: {tradeLog.AccountName} executed {tradeLog.Action} on {tradeLog.Ticker}");

                List<TradeLog> tradeLogs = new();

                // Ensure the log file exists
                if (File.Exists(LogFilePath))
                {
                    string existingData = File.ReadAllText(LogFilePath);
                    if (!string.IsNullOrWhiteSpace(existingData))
                    {
                        tradeLogs = JsonSerializer.Deserialize<List<TradeLog>>(existingData) ?? new List<TradeLog>();
                    }
                }

                // Add the new trade to the list
                tradeLogs.Add(tradeLog);

                // Save updated log list back to file
                string json = JsonSerializer.Serialize(tradeLogs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(LogFilePath, json);

                Debug.WriteLine($"[LogTrade] Trade successfully logged to {LogFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LogTrade] Failed to log trade: {ex.Message}");
            }
        }
    }
}