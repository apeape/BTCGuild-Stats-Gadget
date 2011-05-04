using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Threading.Tasks;

namespace BitcoinWPFGadget
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private DateTime LastUpdate;
        private List<BitcoinCharts.Market> Markets;
        private BitcoinCharts.MarketSymbol lastSymbol;
        private double lastBid;
        private Timer updateTimer;
        private Timer displayCountdownTimer;
        private TimeSpan timerUpdateRate = TimeSpan.FromMinutes(15);
        private object UpdateLock = new object();

        public MainPage()
        {
            InitializeComponent();

            // fill combo box with all market symbols
            Enum.GetNames(typeof(BitcoinCharts.MarketSymbol)).ToList().ForEach(symbol
                => this.symbols.Items.Add(symbol));
            this.symbols.SelectedIndex = 0;

            // start a timer to update the stats every 30 seconds
            updateTimer = new Timer(UpdateTimerTick, null, TimeSpan.Zero, timerUpdateRate);

            // start a timer to update the update countdown display
            displayCountdownTimer = new Timer(UpdateCountdownDisplay, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void UpdateTimerTick(object state)
        {
            GetMarkets();

            // update history
            UpdateGUI(() =>
                {
                    var currentMarket = GetCurrentMarket();
                    if (lastSymbol == currentMarket.symbol)
                        lastBid = currentMarket.bid;
                    else
                        lastBid = 0;
                    lastSymbol = currentMarket.symbol;
                });

            UpdateMarketDisplay();
        }

        public void UpdateCountdownDisplay(object state)
        {
            UpdateGUI(() =>
                {
                    this.Countdown.Text = (timerUpdateRate - (DateTime.Now - LastUpdate)).ToString(@"mm\:ss");
                });
        }

        /// <summary>
        /// Updates the market display asynchronously
        /// </summary>
        public void UpdateMarketDisplay()
        {
            new Task(() =>
            {
                UpdateGUI(() =>
                {
                    try
                    {
                        // wait for initial market data fetching
                        while (Markets == null)
                            Thread.Sleep(1000);

                        var currentMarket = GetCurrentMarket();

                        this.lastTrade.Text = currentMarket.latestTrade.ToString();
                        this.Bid.Text = currentMarket.bid.ToString();
                        this.Currency.Text = currentMarket.currency.ToString();
                        var change = 100 - ((lastBid / currentMarket.bid) * 100);
                        this.Change.Text = change.ToString("N2") + "%";
                        this.Change.Visibility = change != 0 ? Visibility.Visible : Visibility.Hidden;
                        this.UpArrow.Visibility = change > 0 ? Visibility.Visible : Visibility.Hidden;
                        this.DownArrow.Visibility = change < 0 ? Visibility.Visible : Visibility.Hidden;

                        this.Ask.Text = currentMarket.ask.ToString();
                        this.Volume.Text = currentMarket.volume.ToString();
                        this.Trades.Text = currentMarket.n_trades.ToString();
                        this.Currency.Text = currentMarket.currency.ToString();
                        this.Currency.Text = currentMarket.currency.ToString();
                    }
                    catch (Exception)
                    {
                        // probably timed out on json webrequest
                    }
                });
            }).Start();
        }

        /// <summary>
        /// Get current market data from bitcoincharts
        /// </summary>
        public void GetMarkets()
        {
            lock (UpdateLock) // only one of these should ever run at once
            {
                Markets = BitcoinCharts.GetMarkets();
                LastUpdate = DateTime.Now;
            }
        }

        /// <summary>
        /// Returns the currently chosen (in the combo box) market
        /// </summary>
        /// <returns></returns>
        public BitcoinCharts.Market GetCurrentMarket()
        {
            var currentSymbol = Utility.StringToEnum<BitcoinCharts.MarketSymbol>(
                (string)this.symbols.SelectedItem);

            var currentMarket = Markets.SingleOrDefault(m => m.symbol == currentSymbol);

            return currentMarket;
        }

        /// <summary>
        /// In WPF, only the thread that created a DispatcherObject may access that object. 
        /// For example, a background thread that is spun off from the main UI thread cannot
        /// update the contents of a Button that was created on the UI thread.
        /// In order for the background thread to access the Content property of the Button,
        /// the background thread must delegate the work to the Dispatcher associated with the UI thread.
        /// </summary>
        /// <param name="action"></param>
        public void UpdateGUI(Action action)
        {
            this.Dispatcher.BeginInvoke((ThreadStart)delegate() { action();});
        }

        private void symbols_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Markets != null)
            {
                var currentMarket = GetCurrentMarket();
                lastSymbol = currentMarket.symbol;
                lastBid = currentMarket.bid;
            }
            UpdateMarketDisplay();
        }

        private void Donations_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Donations.SelectAll();
        }
    }
}
