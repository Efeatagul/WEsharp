#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
    
    public class ConsoleLib : ILibrary
    {
        public Dictionary<string, Func<List<object>, object>> GetFunctions()
        {
            return new Dictionary<string, Func<List<object>, object>>
            {
                
                { "wea_ui_style", args => {
                    if (args.Count == 0) return false;

                    string theme = args[0].ToString().ToLower().Trim();
                    switch (theme)
                    {
                        case "wea_ghost": 
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case "wea_void": 
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        case "wea_neon": 
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        case "wea_magma": 
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case "wea_base": 
                            Console.ResetColor();
                            Console.Clear();
                            return true;
                        default:
                            return false;
                    }
                    Console.Clear(); 
                    if (theme == "wea_ghost") Console.WriteLine("[WEA_GHOST MODE ACTIVE]");
                    return true;
                }},

             
                { "wea_ui_label", args => {
                    if (args.Count > 0)
                    {
                        Console.Title = args[0].ToString();
                        return true;
                    }
                    return false;
                }},

              
                { "wea_ui_wipe", args => {
                    Console.Clear();
                    return true;
                }}
            };
        }
    }
} 
