using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net;

namespace BitcoinWPFGadget
{
    public static class Utility
    {
        /// <summary>
        /// Deserializes a JSON object to a POCO from the provided URL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    return JsonConvert.DeserializeObject<T>(client.DownloadString(url));
                }
            }
            catch (Exception)
            {
                // failed to deserialize, usually happens when the website is screwing up
                return default(T);
            }
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }

        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime DateTimeFromUnixTime(Int64 value)
        {
            return unixEpoch.AddSeconds(value);
        }
    }
}
