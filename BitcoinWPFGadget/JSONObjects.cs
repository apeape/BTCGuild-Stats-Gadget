using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace BitcoinWPFGadget
{
    public class BTCGuild
    {
        public static BTCGuild.Stats GetStats(string apikey)
        {
            return Utility.Deserialize<BTCGuild.Stats>("http://www.btcguild.com/api.php?api_key=" + apikey);
        }

        public class Stats
        {
            public Pool pool { get; set; }
            public User user { get; set; }
            public Dictionary<String, Worker> workers { get; set; }
        }

        public class Pool
        {
            public UInt64 active_workers { get; set; }
            public double hash_rate { get; set; }
            public UInt64 round_shares { get; set; }
            public TimeSpan round_time { get; set; }

            public string hash_rate_stats { get { return (hash_rate / 1000f).ToString("0.00") + " gH/s"; } }
        }

        public class User
        {
            public double confirmed_rewards { get; set; }
            public double estimated_rewards { get; set; }
            public double payouts { get; set; }
            public double unconfirmed_rewards { get; set; }

            public string confirmed_rewards_stats { get { return confirmed_rewards + " BTC"; } }
            public string estimated_rewards_stats { get { return estimated_rewards + " BTC"; } }
            public string payouts_stats { get { return payouts + " BTC"; } }
            public string unconfirmed_rewards_stats { get { return unconfirmed_rewards + " BTC"; } }
        }

        public class Worker
        {
            public UInt64 blocks_found { get; set; }
            public double hash_rate { get; set; }
            public TimeSpan last_share_timespan
            { 
                get {
                    TimeSpan res = new TimeSpan();
                    if (TimeSpan.TryParse(last_share, out res))
                        return res;
                    else return TimeSpan.MaxValue;
                }
            }
            public string last_share { get; set; }
            public UInt64 reset_shares { get; set; }
            public UInt64 reset_stales { get; set; }
            public UInt64 round_shares { get; set; }
            public UInt64 round_stales { get; set; }
            public UInt64 total_shares { get; set; }
            public UInt64 total_stales { get; set; }
            public string worker_name { get; set; }

            public string hash_rate_stats { get { return hash_rate + " mH/s"; } }
            public string reset_share_stats { get { return reset_shares + " (" + reset_stales + ")"; } }
            public string round_share_stats { get { return round_shares + " (" + round_stales + ")"; } }
            public string total_share_stats { get { return total_shares + " (" + total_stales + ")"; } }

            public SolidColorBrush statusColor
            {
                get
                {
                    if (last_share_timespan > MainPage.redIdleThreshold)
                        return Brushes.LightPink;
                    if (last_share_timespan > MainPage.yellowIdleThreshold)
                        return Brushes.LightYellow;
                    else return Brushes.LightGreen;
                }
            }
        }

        public class WorkerTotals
        {
            public double total_hash_rate { get; set; }
            public UInt64 round_shares_total { get; set; }
            public UInt64 round_stales_total { get; set; }
            public UInt64 reset_shares_total { get; set; }
            public UInt64 reset_stales_total { get; set; }
            public UInt64 total_shares_total { get; set; }
            public UInt64 total_stales_total { get; set; }

            public string hash_rate_total_stats { get { return total_hash_rate + " mH/s"; } }
            public string round_shares_total_stats { get { return round_shares_total + " (" + round_stales_total + ")"; } }
            public string reset_shares_total_stats { get { return reset_shares_total + " (" + reset_stales_total + ")"; } }
            public string total_shares_total_stats { get { return total_shares_total + " (" + total_stales_total + ")"; } }
            public UInt64 blocks_found_total { get; set; }

            public double btc_per_day { get; set; }
            public string btc_per_day_stats { get { return btc_per_day.ToString("0.00"); } }
        }
    }

    public class BitcoinCharts
    {
        public static List<BitcoinCharts.Market> GetMarkets()
        {
            return Utility.Deserialize<List<BitcoinCharts.Market>>("http://bitcoincharts.com/t/markets.json");
        }

        /// <summary>
        /// Get current weighted BTC prices from bitcoincharts
        /// </summary>
        public static BitcoinCharts.WeightedPrices GetWeightedPrices()
        {
            return Utility.Deserialize<BitcoinCharts.WeightedPrices>("http://bitcoincharts.com/t/weighted_prices.json");
        }

        public enum MarketSymbol
        {
            mtgoxUSD,
            bcmPPUSD,
            britcoinGBP,
            bcEUR,
            bcLREUR,
            bcLRUSD,
            btcexJPY,
            btcexWMR,
            bcmLRUSD,
            bcmPXGAU,
            btcexRUB,
            bcPGAU,
            btcexUSD,
            btcexEUR,
            btcexWMZ,
            btcexYAD,
            bcmBMUSD,
            bitomatPLN,
            bitmarketEUR,
            bitmarketGBP,
            bitmarketPLN,
            virwoxSLL,
        }

        public enum CurrencyType
        {
            USD, RUB, GAU, SLL, GBP, PLN, EUR, JPY
        }

        public class Market
        {
            public double high { get; set; }
            [JsonIgnore]
            public DateTime latestTrade
            {
                get { return Utility.DateTimeFromUnixTime(latest_trade_unixtime); }
            }
            [JsonProperty(PropertyName = "latest_trade")]
            public Int64 latest_trade_unixtime { get; set; }
            public double bid { get; set; }
            public double volume { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public CurrencyType currency { get; set; }
            public double low { get; set; }
            public double n_trades { get; set; }
            public double ask { get; set; }
            public double close { get; set; }
            public double open { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public MarketSymbol symbol { get; set; }
            public double currency_volume { get; set; }
        }

        public class WeightedPrices
        {
            public Currency USD { get; set; }
            public Currency RUB { get; set; }
            public Currency GAU { get; set; }
            public Currency SLL { get; set; }
            public Currency GBP { get; set; }
            public Currency PLN { get; set; }
            public Currency EUR { get; set; }
        }

        public class Currency
        {
            [JsonProperty(PropertyName = "7d")]
            public double Week { get; set; }
            [JsonProperty(PropertyName = "30d")]
            public double Month { get; set; }
            [JsonProperty(PropertyName = "24h")]
            public double Day { get; set; }
        }
    }
}
