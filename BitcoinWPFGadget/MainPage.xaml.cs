﻿using System;
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

        public MainPage()
        {
            InitializeComponent();

            this.btcguild_apikey.Text = Properties.Settings.Default.btcguild_apikey;

            LastUpdate = DateTime.Now;

            // start a timer to update the stats
            updateTimer = new Timer(UpdateTimerTick, null, TimeSpan.Zero, timerUpdateRate);

            // start a timer to update the update countdown display
            displayCountdownTimer = new Timer(UpdateCountdownDisplay, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void UpdateTimerTick(object state)
        {
            try
            {
                UpdateGUI(() =>
                {
                    // don't update without a valid api key
                    if (this.btcguild_apikey.Text == string.Empty || this.btcguild_apikey.Text.Length != btcguild_apikey_length) return;

                    // grab stats from json api
                    BTCGuild.Stats stats = BTCGuild.GetStats(this.btcguild_apikey.Text);

                    // bind user stats
                    this.User = new ObservableCollection<BTCGuild.User>();
                    this.User.Add(stats.user);
                    this.userData.ItemsSource = this.User;

                    // bind pool stats
                    this.PoolStats = new ObservableCollection<BTCGuild.Pool>();
                    this.PoolStats.Add(stats.pool);
                    this.poolDataTotals.ItemsSource = this.PoolStats;

                    // bind worker stats
                    var workerstats = stats.workers.Values.ToList();
                    this.Workers = new ObservableCollection<BTCGuild.Worker>();
                    workerstats.ForEach(worker => this.Workers.Add(worker));
                    this.workerData.ItemsSource = this.Workers;

                    // calculate totals
                    var totals = new BTCGuild.WorkerTotals();
                    this.WorkerTotals = new ObservableCollection<BTCGuild.WorkerTotals>();
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

                    // grab current difficulty
                    double currentDifficulty = Utility.Deserialize<double>("http://blockexplorer.com/q/getdifficulty");
                    //"echo The average time to generate a block at $1 Khps, given current difficulty of [bc,diff],
                    //is [time elapsed [math calc 1/((2**224-1)/[bc,diff]*$1*1000/2**256)]]".
                    //totals.btc_per_day = 1 /* day */ * 1 / (Math.Pow(2, 224) - 1) / currentDifficulty * totals.total_hash_rate * 1000 / Math.Pow(2, 256);

                    //The expected generation output, at $1 Khps, given current difficulty of [bc,diff
                    //is [math calc 50*24*60*60 / (1/((2**224-1)/[bc,diff]*$1*1000/2**256))]
                    //BTC per day and [math calc 50*60*60 / (1/((2**224-1)/[bc,diff]*$1*1000/2**256))] BTC per hour.".
                    totals.btc_per_day = 50 * TimeSpan.FromDays(1).TotalSeconds / (1 / (Math.Pow(2, 224) - 1)) / currentDifficulty * totals.total_hash_rate * 1000000 / Math.Pow(2, 256);
                    totals.btc_per_day_stats = totals.btc_per_day.ToString("0.00");

                    this.WorkerTotals.Add(totals);
                    this.workerDataTotals.ItemsSource = this.WorkerTotals;

                    LastUpdate = DateTime.Now;
                });
            }
            catch (Exception)
            {
                // probably timed out on json webrequest
                UpdateGUI(() =>
                {
                    this.test.Text = "Error updating!";
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
            // perform initial update after they paste in a new key
            if (this.btcguild_apikey.Text.Length == btcguild_apikey_length)
                UpdateTimerTick(null);
        }
    }
}
