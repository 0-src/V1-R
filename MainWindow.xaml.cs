using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.Defaults;
using NinjaTrader.Client;
using V1_R.Classes;
using System.Drawing;
using System.Windows.Media;
using LiveChartsCore.SkiaSharpView.Painting;

namespace V1_R
{
    // Represents an account.
    public class Account
    {
        public string AccountName { get; set; }
        public string Strategy { get; set; }
        public bool IsSelected { get; set; }
    }

    // MainWindow code-behind.
    public partial class MainWindow : Window
    {
        // Observable collection for binding to the AccountsItemsControl.
        public ObservableCollection<Account> Accounts { get; set; }

        // Instance of our client wrapper.
        private ClientWrapper clientWrapper;

        // DispatcherTimer to update the live price every 1 second.
        private DispatcherTimer priceUpdateTimer;

        string Instrument = "NQ 03-25";

        // For trade log filtering/charting
        private string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "trade_logs.json");
        private List<TradeLog> allTradeLogs = new();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the accounts collection.
            Accounts = new ObservableCollection<Account>();
            AccountsItemsControl.ItemsSource = Accounts;
            LoadAccounts();

            // Instantiate and set up the client wrapper.
            clientWrapper = new ClientWrapper(ExecutionLogListBox);
            clientWrapper.SetUp("127.0.0.1", 36973);
            clientWrapper.UnSubscribeData(Instrument);
            Task.Delay(100);
            clientWrapper.SubscribeData(Instrument);
            MarketStatusBlock.Text = $"Connected to {Instrument}";

            // Set up a DispatcherTimer to update the live price every 1 second.
            priceUpdateTimer = new DispatcherTimer();
            priceUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            priceUpdateTimer.Tick += PriceUpdateTimer_Tick;
            priceUpdateTimer.Start();

            // Load trade data for the Analysis tab.
            LoadTradeData();
        }

        // Update the live price every second.
        private void PriceUpdateTimer_Tick(object sender, EventArgs e)
        {
            double livePrice = clientWrapper.GetLivePrice("NQ 03-25");
            MarketStatus.Text = $"{Instrument} : {livePrice:##,###0.00}";
            UpdateAccountInfo();
        }

        // Event handler when an account CheckBox is checked.
        private void AccountCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Account account)
            {
                account.IsSelected = true;
                UpdateSelectedAccountsInClient();
                UpdateAccountInfo();
            }
        }

        // Event handler when an account CheckBox is unchecked.
        private void AccountCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Account account)
            {
                account.IsSelected = false;
                UpdateSelectedAccountsInClient();
                UpdateAccountInfo();
            }
        }

        private void UpdateSelectedAccountsInClient()
        {
            var selectedAccounts = Accounts.Where(a => a.IsSelected).ToList();
            clientWrapper.UpdateSelectedAccounts(selectedAccounts);
        }

        // Load accounts directly in MainWindow from config.json.
        private void LoadAccounts()
        {
            try
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var configPath = Path.Combine(documentsPath, "config.json");

                if (!File.Exists(configPath))
                    throw new FileNotFoundException("Config file not found in Documents folder.");

                var configContent = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<Config>(configContent);

                if (config?.Accounts?.Any() == true)
                {
                    Accounts.Clear();
                    foreach (var account in config.Accounts)
                    {
                        Accounts.Add(account);
                        Console.WriteLine($"Loaded account: {account.AccountName}");
                    }
                }
                else
                {
                    Console.WriteLine("No accounts defined in config file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load accounts: {ex.Message}");
            }
        }

        // Aggregates the selected accounts and updates the UI.
        private void UpdateAccountInfo()
        {
            var selectedAccounts = Accounts.Where(a => a.IsSelected).ToList();
            if (selectedAccounts.Any())
            {
                string names = string.Join(", ", selectedAccounts.Select(a => a.AccountName));
                AccountStatusText.Text = "Connected: " + names;

                var balances = selectedAccounts.Select(a => $"{clientWrapper.GetCashValue(a.AccountName):C}");
                AccountBalanceText.Text = "Account Balance: " + string.Join(", ", balances);
            }
            else
            {
                AccountStatusText.Text = "Not Connected";
                AccountBalanceText.Text = "Not Connected to an Account";
            }
        }

        // Allows the custom title bar to be used for dragging the window.
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        // Closes the application.
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            clientWrapper.UnSubscribeData(Instrument);
            Task.Delay(100);
            priceUpdateTimer.Stop();
            clientWrapper?.StopNgrok();
            clientWrapper?.Dispose();
            Application.Current.Shutdown();
        }

        // ------------------- New Functions for Trade Data and Charting -------------------

        // Load trade data from the JSON file.
        private void LoadTradeData()
        {
            try
            {
                if (!File.Exists(logFilePath))
                {
                    Console.WriteLine("[LoadTradeData] No trade log file found.");
                    return;
                }

                string json = File.ReadAllText(logFilePath);
                var trades = JsonSerializer.Deserialize<List<TradeLog>>(json);

                if (trades == null || trades.Count == 0)
                {
                    Console.WriteLine("[LoadTradeData] No trade data available.");
                    return;
                }

                // Order the trades chronologically and assign to the global collection.
                allTradeLogs = trades.OrderBy(t => t.Time).ToList();
                PopulateFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadTradeData] Error loading trade data: {ex.Message}");
            }
        }

        // Populate account and strategy filters based on the loaded trade logs.
        private void PopulateFilters()
        {
            var accounts = allTradeLogs.Select(t => t.AccountName).Distinct().ToList();
            var strategies = allTradeLogs.Select(t => t.Strategy).Distinct().ToList();

            // Add default filter selections.
            accounts.Insert(0, "All Accounts");
            strategies.Insert(0, "All Strategies");

            AccountFilter.ItemsSource = accounts;
            StrategyFilter.ItemsSource = strategies;

            AccountFilter.SelectedIndex = 0;
            StrategyFilter.SelectedIndex = 0;
        }

        // Called when a filter selection changes.
        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadPnL_Click(null, null);
        }

        // Load the PnL chart based on the current filters.
        private void LoadPnL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (allTradeLogs.Count == 0)
                {
                    MessageBox.Show("No trade data available!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string selectedAccount = AccountFilter.SelectedItem as string;
                string selectedStrategy = StrategyFilter.SelectedItem as string;

                var filteredTrades = allTradeLogs;

                if (!string.IsNullOrEmpty(selectedAccount) && selectedAccount != "All Accounts")
                    filteredTrades = filteredTrades.Where(t => t.AccountName == selectedAccount).ToList();

                if (!string.IsNullOrEmpty(selectedStrategy) && selectedStrategy != "All Strategies")
                    filteredTrades = filteredTrades.Where(t => t.Strategy == selectedStrategy).ToList();

                var pnlData = CalculatePnL(filteredTrades);
                UpdatePnLChart(pnlData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading PnL data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Calculate cumulative PnL from trade logs using a FIFO matching approach.
        private List<ObservablePoint> CalculatePnL(List<TradeLog> trades)
        {
            List<ObservablePoint> pnlPoints = new();
            double cumulativePnL = 0;
            int position = 0;
            double avgEntryPrice = 0;
            const double instrumentValue = 20;

            // Ensure trades are processed in chronological order.
            trades = trades.OrderBy(t => t.Time).ToList();

            foreach (var trade in trades)
            {
                double tradePnL = 0;
                // Use a local variable to track remaining quantity to process.
                int qtyRemaining = trade.Quantity;

                if (trade.Action.ToLower() == "buy")
                {
                    // If currently short, try to cover the short position first.
                    if (position < 0)
                    {
                        int closingQuantity = Math.Min(Math.Abs(position), qtyRemaining);
                        tradePnL = (avgEntryPrice - trade.Price) * instrumentValue * closingQuantity;
                        position += closingQuantity; // Covering short reduces the negative position.
                        qtyRemaining -= closingQuantity;
                        if (position == 0)
                            avgEntryPrice = 0;
                    }
                    // Any leftover quantity opens (or increases) a long position.
                    if (qtyRemaining > 0)
                    {
                        int currentLong = (position > 0) ? position : 0;
                        avgEntryPrice = ((avgEntryPrice * currentLong) + (trade.Price * qtyRemaining)) / (currentLong + qtyRemaining);
                        position += qtyRemaining;
                    }
                }
                else if (trade.Action.ToLower() == "sell")
                {
                    // If currently long, try to close the long position first.
                    if (position > 0)
                    {
                        int closingQuantity = Math.Min(position, qtyRemaining);
                        tradePnL = (trade.Price - avgEntryPrice) * instrumentValue * closingQuantity;
                        position -= closingQuantity;
                        qtyRemaining -= closingQuantity;
                        if (position == 0)
                            avgEntryPrice = 0;
                    }
                    // Any leftover quantity opens (or increases) a short position.
                    if (qtyRemaining > 0)
                    {
                        int currentShort = (position < 0) ? Math.Abs(position) : 0;
                        avgEntryPrice = ((avgEntryPrice * currentShort) + (trade.Price * qtyRemaining)) / (currentShort + qtyRemaining);
                        position -= qtyRemaining;
                    }
                }

                cumulativePnL += tradePnL;
                // Bucket the trade time into 15-minute intervals.
                DateTime bucketTime = new DateTime(
                    trade.Time.Year,
                    trade.Time.Month,
                    trade.Time.Day,
                    trade.Time.Hour,
                    (trade.Time.Minute / 15) * 15,
                    0);

                pnlPoints.Add(new ObservablePoint
                {
                    X = bucketTime.ToOADate(),
                    Y = cumulativePnL
                });
            }

            return pnlPoints;
        }

        // Update the PnL chart with the calculated data using a LineSeries with gradient fill.
        // Update the PnL chart with the calculated data using a LineSeries with gradient fill.
        private void UpdatePnLChart(List<ObservablePoint> pnlData)
        {
            // Calculate Y-axis range to determine the normalized zero offset.
            double yAxisMin = pnlData.Min(p => p.Y).Value;
            double yAxisMax = pnlData.Max(p => p.Y).Value;
            double zeroOffset = 0;
            if (yAxisMax != yAxisMin)
            {
                // Normalize the zero level (0 - yAxisMin) over the total range.
                zeroOffset = (0 - yAxisMin) / (yAxisMax - yAxisMin);
            }

            // Create the LineSeries with a gradient fill using LiveCharts2's SkiaSharp paints.
            var lineSeries = new LineSeries<ObservablePoint>
            {
                Values = new ObservableCollection<ObservablePoint>(pnlData),
                Stroke = new SolidColorPaint(SKColors.Black)
                {
                    StrokeThickness = 1
                },
                Fill = new LinearGradientPaint(
                    new SKColor[]
                    {
                // Red for losses.
                SKColors.Red,
                SKColors.Red,
                // Green for profits.
                SKColors.Green,
                SKColors.Green
                    },
                    new SKPoint(0, 1),   // startPoint
                    new SKPoint(0, 0),   // endPoint
                    new float[] { 0f, (float)zeroOffset, (float)zeroOffset, 1f }  // positions
                ),
                GeometrySize = 3,
                LineSmoothness = 0,
                Name = "PnL"
            };

            PnLChart.Series = new ISeries[] { lineSeries };

            // Configure X-axis with time formatting.
            PnLChart.XAxes = new Axis[]
            {
        new Axis
        {
            Labeler = value => DateTime.FromOADate(value).ToString("HH:mm"),
            LabelsRotation = 15,
            MinLimit = pnlData.Min(p => p.X),
            MaxLimit = pnlData.Max(p => p.X)
        }
            };

            // Configure Y-axis.
            PnLChart.YAxes = new Axis[]
            {
        new Axis
        {
            Name = "PnL ($)"
        }
            };
        }
    }
}
