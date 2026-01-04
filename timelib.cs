#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace WSharp
{
    
    public class TimeLib : ILibrary
    {
        
        private static readonly Stopwatch _uptimeCounter = Stopwatch.StartNew();

        public Dictionary<string, Func<List<object>, object>> GetFunctions()
        {
            return new Dictionary<string, Func<List<object>, object>>
            {
                
                { "wea_chrono_now", args => DateTime.Now.ToString("HH:mm:ss") },
                { "wea_chrono_day", args => DateTime.Now.ToString("dd.MM.yyyy") },
                
               
                { "wea_chrono_life", args => Math.Round(_uptimeCounter.Elapsed.TotalSeconds, 2) },

               
                { "wea_chrono_tag", args => {
                    try {
                        string format = args.Count > 0 ? args[0].ToString() : "yyyyMMddHHmmss";
                        return DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
                    } catch { return "wea_invalid_format"; }
                }},

                
                { "wea_pause", args => {
                    try {
                        if (args.Count > 0 && int.TryParse(args[0].ToString(), out int ms))
                        {
                            System.Threading.Thread.Sleep(Math.Max(0, ms));
                            return true;
                        }
                        return false;
                    } catch { return false; }
                }},

               
                { "wea_chrono_global", args => DateTimeOffset.UtcNow.ToUnixTimeSeconds() },

               
                { "wea_chrono_ms", args => _uptimeCounter.Elapsed.TotalMilliseconds }
            };
        }
    }
}
