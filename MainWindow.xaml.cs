using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NinjaTrader.Client;

namespace V1_R
{
    // Represents an account.
    public class Account
    {
        public string AccountName { get; set; }
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


        public MainWindow()
        {
            InitializeComponent();


            // Initialize the collection.
            Accounts = new ObservableCollection<Account>();

            // Bind the collection to the AccountsItemsControl.
            AccountsItemsControl.ItemsSource = Accounts;

            // Add accounts dynamically.
            Accounts.Add(new Account { AccountName = "Sim101" });
            Accounts.Add(new Account { AccountName = "APEX2926700000001" });
            Accounts.Add(new Account { AccountName = "MFFUEVST214695004" });

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
                UpdateAccountInfo();
            }
        }

        // Event handler when an account CheckBox is unchecked.
        private void AccountCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Account account)
            {
                account.IsSelected = false;
                UpdateAccountInfo();
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
    }
}
