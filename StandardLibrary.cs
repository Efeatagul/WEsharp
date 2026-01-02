#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WSharp
{
    public static class StandardLibrary
    {
        public static readonly Dictionary<string, Func<List<object>, object>> Functions =
            new Dictionary<string, Func<List<object>, object>>
        {
           
            { "out", args => {
                if(args.Count > 0) Console.WriteLine(args[0]);
                else Console.WriteLine();
                return null;
            }},
            { "ask", args => {
                if(args.Count > 0) Console.Write(args[0]);
                return Console.ReadLine();
            }},
            { "clean", args => { Console.Clear(); return null; }},
            { "exit", args => { Environment.Exit(0); return null; }},
            { "out_color", args => {
                if(args.Count < 2) return null;
                if(Enum.TryParse(args[1].ToString(), true, out ConsoleColor color))
                    Console.ForegroundColor = color;
                Console.WriteLine(args[0]);
                Console.ResetColor();
                return null;
            }},

        
            { "dice", args => {
                Random rng = new Random();
                if (args.Count >= 2) return rng.Next(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]));
                if (args.Count == 1) return rng.Next(0, Convert.ToInt32(args[0]));
                return rng.Next(0, 100);
            }},
            { "abs", args => Math.Abs(Convert.ToDouble(args[0])) },
            { "sqrt", args => Math.Sqrt(Convert.ToDouble(args[0])) },
            { "pow", args => Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[1])) },
            { "round", args => Math.Round(Convert.ToDouble(args[0])) },
            { "min", args => args.Min(x => Convert.ToDouble(x)) },
            { "max", args => args.Max(x => Convert.ToDouble(x)) },

        
            { "range", args => {
                int start = args.Count >= 2 ? Convert.ToInt32(args[0]) : 0;
                int end = args.Count >= 2 ? Convert.ToInt32(args[1]) : Convert.ToInt32(args[0]);
                return Enumerable.Range(start, end - start).Cast<object>().ToList();
            }},
            { "type", args => args[0]?.GetType().Name ?? "null" },
            { "to_str", args => args[0]?.ToString() },
            { "to_int", args => Convert.ToInt32(args[0]) },

      
            { "upper", args => args[0]?.ToString().ToUpper() },
            { "lower", args => args[0]?.ToString().ToLower() },
            { "trim", args => args[0]?.ToString().Trim() },
            { "contains", args => args[0]?.ToString().Contains(args[1].ToString()) },

          
            { "time", args => DateTime.Now.ToString("HH:mm:ss") },
            { "date", args => DateTime.Now.ToString("yyyy-MM-dd") },
            { "wait", args => {
                Thread.Sleep(Convert.ToInt32(args[0]));
                return null;
            }},
            { "beep", args => { Console.Beep(); return null; }},


            { "we_core", args => {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n-------------------------------------------");
                Console.WriteLine(">>>      WE# (WESHARP) CORE SYSTEM      <<<");
                Console.WriteLine(">>>      weagws by efeatagul            <<<");
                Console.WriteLine(">>>      STATUS: VERIFIED & PROTECTED   <<<");
                Console.WriteLine("-------------------------------------------\n");
                Console.ResetColor();
                return "weagws_authorized";
            }}
        };

        public static bool Exists(string name) => Functions.ContainsKey(name);

        public static object Call(string name, List<object> args)
        {
            if (Exists(name)) return Functions[name](args);
            return null;
        }
    }
}