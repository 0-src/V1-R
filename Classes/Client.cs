using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NinjaTrader.Client;
using System.ServiceProcess;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Controls;

namespace V1_R
{
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

    public class ClientWrapper : IDisposable
    {
        private NinjaTrader.Client.Client ntClient;
        private string authToken;
        private const int Port = 80;
        private string ngrokUrl;
        private ListBox executionLogListBox;

        public ClientWrapper(ListBox logListBox)
        {
            ntClient = new NinjaTrader.Client.Client();
            executionLogListBox = logListBox;
            LoadConfig();
            StartNgrokHidden();
            StartWebhookServer();
        }

        private void LogExecution(string message)
        {
            executionLogListBox.Dispatcher.Invoke(() =>
            {
                executionLogListBox.Items.Add($"{DateTime.Now}: {message}");
                executionLogListBox.ScrollIntoView(executionLogListBox.Items[^1]);
            });
            Console.WriteLine(message);
        }

        private void LoadConfig()
        {
            try
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var configPath = Path.Combine(documentsPath, "config.json");

                if (!File.Exists(configPath))
                    throw new FileNotFoundException("Config file not found in Documents folder.");

                var configContent = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<Config>(configContent);
                authToken = config?.AuthToken ?? throw new Exception("Auth token missing in config file.");
                ngrokUrl = config?.NgrokUrl ?? throw new Exception("Ngrok URL missing in config file.");
                LogExecution("Config loaded successfully from Documents folder.");
                LogExecution($"Using ngrok URL: {ngrokUrl}");
            }
            catch (Exception ex)
            {
                LogExecution($"Failed to load config: {ex.Message}");
                throw;
            }
        }

        private void StartNgrokHidden()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ngrok.exe",
                        Arguments = "http --url=composed-honeybee-intimate.ngrok.app 80",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                LogExecution("ngrok tunnel started silently in the background.");
            }
            catch (Exception ex)
            {
                LogExecution($"Failed to start ngrok tunnel: {ex.Message}");
            }
        }

        private void StartWebhookServer()
        {
            Task.Run(() =>
            {
                try
                {
                    var builder = WebApplication.CreateBuilder();
                    var app = builder.Build();

                    app.MapGet("/", async (HttpContext context) =>
                    {
                        await context.Response.WriteAsync("Webhook server is running!");
                        LogExecution("Health check request received.");
                    });

                    app.MapPost("/api/TradingView", async (HttpContext context) =>
                    {
                        try
                        {
                            var queryToken = context.Request.Query["auth"];
                            if (queryToken != authToken)
                            {
                                context.Response.StatusCode = 403;
                                await context.Response.WriteAsync("Unauthorized");
                                LogExecution("Unauthorized webhook access attempt.");
                                return;
                            }

                            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();

                            var instruction = JsonSerializer.Deserialize<TradeInstruction>(body);

                            ProcessTradeInstruction(body);

                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync("Trade processed");
                        }
                        catch (Exception ex)
                        {
                            LogExecution($"Error processing webhook: {ex.Message}");
                            context.Response.StatusCode = 500;
                            await context.Response.WriteAsync("Internal Server Error");
                        }
                    });

                    app.Run($"http://0.0.0.0:{Port}");
                }
                catch (Exception ex)
                {
                    LogExecution($"Failed to start webhook server: {ex.Message}");
                }
            });

            LogExecution($"Webhook server running on port {Port}");
            LogExecution($"Accessible via ngrok URL: {ngrokUrl}/api/TradingView?auth={authToken}");
        }

        public void SetUp(string host, int port)
        {
            try
            {
                ntClient.SetUp(host, port);
                LogExecution($"Client set up with host {host} and port {port}");
            }
            catch (Exception ex)
            {
                LogExecution($"Error setting up client: {ex.Message}");
            }
        }

        public double GetCashValue(string account)
        {
            try
            {
                var value = ntClient.CashValue(account);
                return value;
            }
            catch (Exception ex)
            {
                LogExecution($"Error retrieving cash value: {ex.Message}");
                throw;
            }
        }

        public double GetBuyingPower(string account)
        {
            try
            {
                var power = ntClient.BuyingPower(account);
                return power;
            }
            catch (Exception ex)
            {
                LogExecution($"Error retrieving buying power: {ex.Message}");
                throw;
            }
        }

        public double GetRealizedPnL(string account)
        {
            try
            {
                var pnl = ntClient.RealizedPnL(account);
                return pnl;
            }
            catch (Exception ex)
            {
                LogExecution($"Error retrieving realized PnL: {ex.Message}");
                throw;
            }
        }

        public int ExecuteCommand(string command, string account, string instrument, string action, int quantity,
                                  string orderType, double limitPrice, double stopPrice, string timeInForce,
                                  string oco, string orderId, string strategy, string strategyId)
        {
            try
            {
                var result = ntClient.Command(command, account, instrument, action, quantity, orderType,
                                               limitPrice, stopPrice, timeInForce, oco, orderId, strategy, strategyId);
                return result;
            }
            catch (Exception ex)
            {
                LogExecution($"Error executing command: {ex.Message}");
                throw;
            }
        }

        public int ProcessTradeInstruction(string json, string account = "Sim101")
        {
            try
            {
                var instruction = JsonSerializer.Deserialize<TradeInstruction>(json);

                if (instruction == null)
                    throw new ArgumentException("Invalid JSON payload.");

                if (string.IsNullOrWhiteSpace(instruction.Ticker) ||
                    string.IsNullOrWhiteSpace(instruction.Action) ||
                    instruction.Quantity <= 0)
                {
                    throw new ArgumentException("Invalid trade instruction parameters.");
                }

                int result = ExecuteCommand(
                    "PLACE",
                    account,
                    instruction.Ticker,
                    instruction.Action.ToUpper(),
                    instruction.Quantity,
                    "MARKET",
                    instruction.Price,
                    0,
                    "DAY",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty
                );

                LogExecution($"{instruction.Action.ToUpper()} Executed @{account} - {instruction.Time}");
                return result;
            }
            catch (Exception ex)
            {
                LogExecution($"Error processing trade instruction: {ex.Message}");
                throw;
            }
        }


        public void SubscribeData(string instrument)
        {
            try
            {
                ntClient.SubscribeMarketData(instrument);
            }
            catch {
                LogExecution($"Couldn't subscribe to {instrument}");
            }
        }

        public void UnSubscribeData(string instrument)
        {
            try
            {
                ntClient.UnsubscribeMarketData(instrument);
            }
            catch
            {
                LogExecution($"Couldn't unsubscribe to {instrument}");
            }
        }

        public double GetLivePrice(string instrument)
        {
            try
            {
                var price = ntClient.MarketData(instrument,0);                
                return price;
            }
            catch (Exception ex)
            {
                LogExecution($"Error retrieving live price: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                ntClient?.Dispose();
                LogExecution("Client disposed successfully.");
            }
            catch (Exception ex)
            {
                LogExecution($"Error disposing client: {ex.Message}");
            }
        }

        public void StopNgrok()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("ngrok"))
                {
                    process.Kill();
                    LogExecution("ngrok process terminated.");
                }
            }
            catch (Exception ex)
            {
                LogExecution($"Failed to stop ngrok process: {ex.Message}");
            }
        }
    }

    public class Config
    {
        public string AuthToken { get; set; }
        public string NgrokUrl { get; set; }
    }
}
