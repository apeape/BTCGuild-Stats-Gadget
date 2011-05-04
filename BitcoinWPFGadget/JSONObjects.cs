using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BitcoinWPFGadget
{
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
