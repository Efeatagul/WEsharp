#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WSharp
{
    
    public static class StandardLibrary
    {
        public static Dictionary<string, Func<List<object>, object>> Functions = new Dictionary<string, Func<List<object>, object>>();
        private static readonly Random _rng = new Random();

        static StandardLibrary()
        {
            RegisterIO();
            RegisterStrings();
            RegisterMath();
            RegisterData();
        }

        #region Fonksiyon Kayıt Bölümleri

        private static void RegisterIO()
        {
            
            Functions["wea_print"] = args => { Console.WriteLine(args.Count > 0 ? args[0] : ""); return null; };
            Functions["wea_read"] = args => { if (args.Count > 0) Console.Write(args[0]); return Console.ReadLine(); };
            Functions["wea_clean"] = args => { Console.Clear(); return null; };
            Functions["wea_wait"] = args => { Thread.Sleep(args.Count > 0 ? Convert.ToInt32(args[0]) : 1000); return null; };
            Functions["wea_exit"] = args => { Environment.Exit(0); return null; };
            Functions["wea_beep"] = args => { if (OperatingSystem.IsWindows()) Console.Beep(); return null; };

            
            Functions["wea_help"] = args => {
                string cat = args.Count > 0 ? args[0].ToString() : "";
                Program.ShowHelp(cat);
                return null;
            };

            Functions["wea_print_color"] = args => {
                if (args.Count < 2) return null;
                if (Enum.TryParse(args[1].ToString(), true, out ConsoleColor color)) Console.ForegroundColor = color;
                Console.WriteLine(args[0]);
                Console.ResetColor();
                return null;
            };
        }

        private static void RegisterStrings()
        {
            Functions["wea_str_len"] = args => args[0]?.ToString().Length ?? 0;
            Functions["wea_str_upper"] = args => args[0]?.ToString().ToUpper();
            Functions["wea_str_lower"] = args => args[0]?.ToString().ToLower();
            Functions["wea_str_trim"] = args => args[0]?.ToString().Trim();
            Functions["wea_str_has"] = args => args[0]?.ToString().Contains(args[1].ToString());
            Functions["wea_str_swap"] = args => args[0]?.ToString().Replace(args[1].ToString(), args[2].ToString());

            Functions["wea_str_slug"] = args => {
                try
                {
                    string s = args[0].ToString().ToLower();
                    s = s.Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
                    s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
                    s = Regex.Replace(s, @"\s+", "-").Trim();
                    return s;
                }
                catch { return "wea_error"; }
            };
        }

        private static void RegisterMath()
        {
            Functions["wea_math_rand"] = args => {
                int min = args.Count >= 2 ? Convert.ToInt32(args[0]) : 0;
                int max = args.Count >= 2 ? Convert.ToInt32(args[1]) : 100;
                return _rng.Next(min, max);
            };
            Functions["wea_math_abs"] = args => Math.Abs(Convert.ToDouble(args[0]));
            Functions["wea_math_sqrt"] = args => Math.Sqrt(Convert.ToDouble(args[0]));
            Functions["wea_math_pow"] = args => Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]));
        }

        private static void RegisterData()
        {
            Functions["wea_list_init"] = args => new List<object>();
            Functions["wea_list_add"] = args => {
                if (args[0] is List<object> list) { list.Add(args[1]); return list; }
                return null;
            };
            Functions["wea_json_out"] = args => JsonSerializer.Serialize(args[0]);
            Functions["wea_json_in"] = args => JsonSerializer.Deserialize<object>(args[0].ToString());
            Functions["wea_sys_ver"] = args => "2.0.0-MasterBuild";
        }

        #endregion

        public static bool Exists(string name) => Functions.ContainsKey(name.ToLower());
        public static object Call(string name, List<object> args) =>
            Functions.TryGetValue(name.ToLower(), out var func) ? func(args) : null;
    }
}
