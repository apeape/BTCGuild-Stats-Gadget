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
using System.Collections.ObjectModel;
using System.Globalization;

namespace BitcoinWPFGadget
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        const int btcguild_apikey_length = 32;
        private DateTime LastUpdate;
        private Timer updateTimer;
        private Timer displayCountdownTimer;
        private TimeSpan timerUpdateRate = TimeSpan.FromMinutes(1);

        public ObservableCollection<BTCGuild.User> User { get; set; }
        public ObservableCollection<BTCGuild.Worker> Workers { get; set; }
        public ObservableCollection<BTCGuild.WorkerTotals> WorkerTotals { get; set; }
        public ObservableCollection<BTCGuild.Pool> PoolStats { get; set; }

        string btcguildapikey = String.Empty;
        BTCGuild.Stats stats = default(BTCGuild.Stats);
        double currentDifficulty;

        public static TimeSpan yellowIdleThreshold;
        public static TimeSpan redIdleThreshold;

        public MainPage()
        {
            this.DataContext = this;
            InitializeComponent();

            this.btcguild_apikey.Text = Properties.Settings.Default.btcguild_apikey;

            this.LastUpdate = DateTime.Now;

            this.User = new ObservableCollection<BTCGuild.User>();
            this.Workers = new ObservableCollection<BTCGuild.Worker>();
            this.WorkerTotals = new ObservableCollection<BTCGuild.WorkerTotals>();
            this.PoolStats = new ObservableCollection<BTCGuild.Pool>();

            this.userStats.ItemsSource = this.User;
            this.poolStats.ItemsSource = this.PoolStats;
            this.workerStats.ItemsSource = this.Workers;
            // custom worker list background color based on last share time
            this.workerStats.ItemContainerStyleSelector = new WorkerListStyleSelector();

            this.workerTotalsStats.ItemsSource = this.WorkerTotals;

            // start a timer to update the stats
            this.updateTimer = new Timer(this.UpdateTimerTick, null, TimeSpan.Zero, this.timerUpdateRate);

            // start a timer to update the update countdown display
            this.displayCountdownTimer = new Timer(UpdateCountdownDisplay, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void UpdateTimerTick(object state)
        {
            this.LastUpdate = DateTime.Now;

            // don't update without a valid api key
            if (btcguildapikey == string.Empty || btcguildapikey.Length != btcguild_apikey_length) return;

            try
            {

                // grab stats from json api
                stats = BTCGuild.GetStats(btcguildapikey);
                if (stats == default(BTCGuild.Stats))
                    throw new System.Runtime.Serialization.SerializationException("Failed to deserialize BTCGuild stats!");

                // grab current difficulty
                currentDifficulty = Utility.Deserialize<double>("http://blockexplorer.com/q/getdifficulty");

                UpdateGUI(() =>
                {
                    try
                    {
                        // bind user stats
                        this.User.Clear();
                        this.User.Add(stats.user);

                        // bind pool stats
                        this.PoolStats.Clear();
                        this.PoolStats.Add(stats.pool);

                        // bind worker stats
                        var workerstats = stats.workers.Values.ToList();
                        this.Workers.Clear();
                        workerstats.ForEach(worker => this.Workers.Add(worker));

                        // calculate totals
                        var totals = new BTCGuild.WorkerTotals();
                        workerstats.ForEach(worker =>
                            {
                                totals.total_hash_rate += worker.hash_rate;
                                totals.blocks_found_total += worker.blocks_found;
                                totals.reset_shares_total += worker.reset_shares;
                                totals.reset_stales_total += worker.reset_stales;
                                totals.round_shares_total += worker.round_shares;
                                totals.round_stales_total += worker.round_stales;
                                totals.total_shares_total += worker.total_shares;
                                totals.total_stales_total += worker.total_stales;
                            });

                        if (currentDifficulty == default(double))
                        {
                            // failed to retrieve difficulty, btc per day calculation not possible
                        }
                        else
                        {
                            // from the gribble bot:
                            // The expected generation output, at $1 Khps, given current difficulty of [bc,diff
                            // is [math calc 50*24*60*60 / (1/((2**224-1)/[bc,diff]*$1*1000/2**256))]
                            // BTC per day and [math calc 50*60*60 / (1/((2**224-1)/[bc,diff]*$1*1000/2**256))] BTC per hour.".
                            totals.btc_per_day = 50 * TimeSpan.FromDays(1).TotalSeconds / (1 / (Math.Pow(2, 224) - 1)) / currentDifficulty * totals.total_hash_rate * 1000000 / Math.Pow(2, 256);
                        }

                        this.WorkerTotals.Clear();
                        this.WorkerTotals.Add(totals);

                        this.test.Text = "BTCGuild Stats"; // reset error
                    }
                    catch (Exception e)
                    {
                        // probably an error updating the gui
                        this.test.Text = e.Message;
                    }
                });
            }
            catch (Exception e)
            {
                UpdateGUI(() =>
                {
                    // probably an error deserializing the json, or we timed out trying to read it
                    this.test.Text = e.Message;
                });
            }
        }

        public void UpdateCountdownDisplay(object state)
        {
            UpdateGUI(() =>
                {
                    this.Countdown.Text = (timerUpdateRate - (DateTime.Now - LastUpdate)).ToString(@"mm\:ss");
                });
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

        private void Donations_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Donations.SelectAll();
        }

        private void savebutton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.btcguild_apikey = this.btcguild_apikey.Text;
            Properties.Settings.Default.Save();
        }

        private void btcguild_apikey_TextChanged(object sender, TextChangedEventArgs e)
        {
            // perform initial update after they paste/type in a new key
            if (this.btcguild_apikey.Text.Length == btcguild_apikey_length)
            {
                btcguildapikey = this.btcguild_apikey.Text;
                UpdateTimerTick(null);
            }
        }

        private void numbersOnlyTextbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            UInt32 result;
            if (!UInt32.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }

        private void yellowidlethreshold_TextChanged(object sender, TextChangedEventArgs e)
        {
            yellowIdleThreshold = TimeSpan.FromMinutes(UInt32.Parse(yellowidlethreshold.Text));
        }

        private void redidlethreshold_TextChanged(object sender, TextChangedEventArgs e)
        {
            redIdleThreshold = TimeSpan.FromMinutes(UInt32.Parse(redidlethreshold.Text));
        }
    }
}
