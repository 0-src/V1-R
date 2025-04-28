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
        public string Instrument { get; set; } 

        
        // Instance of our client wrapper.
        private ClientWrapper clientWrapper;

        // DispatcherTimer to update the live price every 1 second.
        private DispatcherTimer priceUpdateTimer;


        private string ConfigPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "V1",
            "config.json"
        );

        private string CurrentInstrument => InstrumentTextBox.Text.Trim();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the accounts collection.
            Accounts = new ObservableCollection<Account>();
            AccountsItemsControl.ItemsSource = Accounts;
            LoadAccounts();
            LoadInstrument();

            AddAccountButton.Click += AddAccountButton_Click;
            EditAccountButton.Click += EditAccountButton_Click;
            DeleteAccountButton.Click += DeleteAccountButton_Click;
            SaveInstrumentButton.Click += SaveInstrumentButton_Click;


            RefreshAccountsListBox(); // Load into ListBox after loading accounts

            string liveInstrument = GetInstrumentFromConfig();

            // Instantiate and set up the client wrapper.
            clientWrapper = new ClientWrapper(ExecutionLogListBox);
            clientWrapper.SetUp("127.0.0.1", 36973);
            clientWrapper.UnSubscribeData(liveInstrument);
            Task.Delay(100);
            clientWrapper.SubscribeData(liveInstrument);
            MarketStatusBlock.Text = $"Connected to {liveInstrument}";

            // Set up a DispatcherTimer to update the live price every 1/2 second.
            priceUpdateTimer = new DispatcherTimer();
            priceUpdateTimer.Interval = TimeSpan.FromMilliseconds(500);
            priceUpdateTimer.Tick += PriceUpdateTimer_Tick;
            priceUpdateTimer.Start();
        }

        private void LoadInstrument()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return;

                var configContent = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<Config>(configContent);

                if (config != null && !string.IsNullOrEmpty(config.Instrument))
                {
                    InstrumentTextBox.Text = config.Instrument;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load instrument: {ex.Message}");
            }
        }

        private void SaveInstrumentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return;

                var configContent = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<Config>(configContent);

                if (config != null)
                {
                    config.Instrument = InstrumentTextBox.Text.Trim();
                    var updatedContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ConfigPath, updatedContent);
                    MessageBox.Show("Instrument saved successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save instrument: {ex.Message}");
            }
        }

        private string GetInstrumentFromConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return null;

                var configContent = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<Config>(configContent);

                return config?.Instrument;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read instrument from config: {ex.Message}");
                return null;
            }
        }

        // Update the live price every second.
        private void PriceUpdateTimer_Tick(object sender, EventArgs e)
        {
            double livePrice = clientWrapper.GetLivePrice(CurrentInstrument);
            MarketStatus.Text = $"{CurrentInstrument} : {livePrice:##,###0.00}";
        }

        // Event handler when an account CheckBox is checked.
        private void AccountCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Account account)
            {
                account.IsSelected = true;
                UpdateSelectedAccountsInClient();
            }
        }

        // Event handler when an account CheckBox is unchecked.
        private void AccountCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Account account)
            {
                account.IsSelected = false;
                UpdateSelectedAccountsInClient();
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

                if (!File.Exists(ConfigPath))
                    throw new FileNotFoundException("Config file not found in Documents folder.");

                var configContent = File.ReadAllText(ConfigPath);
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

        private void RefreshAccountsListBox()
        {
            AccountsListBox.Items.Clear();
            foreach (var account in Accounts)
            {
                AccountsListBox.Items.Add($"{account.AccountName} — {account.Strategy}");
            }
        }

        private void SaveAccounts()
        {
            try
            {
                var configContent = File.ReadAllText(ConfigPath);
                var config = JsonSerializer.Deserialize<Config>(configContent);

                if (config != null)
                {
                    config.Accounts = Accounts.ToList();
                    var updatedContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ConfigPath, updatedContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save accounts: {ex.Message}");
            }
        }


        // Allows the custom title bar to be used for dragging the window.
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AccountNameTextBox.Text) || string.IsNullOrWhiteSpace(StrategyTextBox.Text))
            {
                MessageBox.Show("Please enter both Account Name and Strategy.");
                return;
            }

            Accounts.Add(new Account
            {
                AccountName = AccountNameTextBox.Text.Trim(),
                Strategy = StrategyTextBox.Text.Trim()
            });

            RefreshAccountsListBox();
            SaveAccounts();
        }

        private void EditAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an account to edit.");
                return;
            }

            if (string.IsNullOrWhiteSpace(AccountNameTextBox.Text) || string.IsNullOrWhiteSpace(StrategyTextBox.Text))
            {
                MessageBox.Show("Please enter both Account Name and Strategy.");
                return;
            }

            var account = Accounts[AccountsListBox.SelectedIndex];
            account.AccountName = AccountNameTextBox.Text.Trim();
            account.Strategy = StrategyTextBox.Text.Trim();

            RefreshAccountsListBox();
            SaveAccounts();
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an account to delete.");
                return;
            }

            Accounts.RemoveAt(AccountsListBox.SelectedIndex);
            RefreshAccountsListBox();
            SaveAccounts();
        }


        // Closes the application.
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            clientWrapper.UnSubscribeData(CurrentInstrument);
            Task.Delay(100);
            priceUpdateTimer.Stop();
            clientWrapper?.StopNgrok();
            clientWrapper?.Dispose();
            Application.Current.Shutdown();
        }
    }
}
