using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
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

        // Instance of the NinjaTrader client.
        private Client ntClient;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the collection.
            Accounts = new ObservableCollection<Account>();

            // Bind the collection to the AccountsItemsControl (defined in XAML).
            AccountsItemsControl.ItemsSource = Accounts;

            // Add accounts dynamically.
            Accounts.Add(new Account { AccountName = "Sim101" });
            Accounts.Add(new Account { AccountName = "APEX2926700000001" });
            Accounts.Add(new Account { AccountName = "MFFUEVST214695004" });

            // Set up the NinjaTrader client using the documented host and port.
            ntClient = new Client();
            ntClient.SetUp("127.0.0.1", 36973);
        }

        // Event handler when an account CheckBox is checked.
        private void AccountCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Account account)
            {
                double cashValue = ntClient.CashValue(account.AccountName);

                // Update UI controls.
                AccountStatusText.Text = $"Connected - {account.AccountName}";
                AccountBalanceText.Text = $"Account Balance: {cashValue:C}";

                // Log execution details (replace with your actual execution log logic as needed).
                ExecutionLogListBox.Items.Clear();
                // Add Log Stuff Here
            }
        }

        // Event handler when an account CheckBox is unchecked.
        private void AccountCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Clear UI elements when the account is unchecked.
            AccountStatusText.Text = "Disconnected";
            AccountBalanceText.Text = "Not Connected to an Account";
            ExecutionLogListBox.Items.Clear();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allows the window to be dragged when clicking on the custom title bar.
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ntClient?.Dispose();
            Application.Current.Shutdown();
        }
    }
}
