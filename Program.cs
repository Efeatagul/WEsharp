#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;

namespace WSharp
{
   
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            
            RegisterGlobalModules();
            var engine = new Interpreter();

            if (args.Length > 0)
            {
                string path = args[0];
                if (File.Exists(path))
                {
                    BootFromFile(path, engine);
                    Console.WriteLine("\n[WEA_SYS] İşlem bitti. Çıkış için ENTER...");
                    Console.ReadLine();
                    return;
                }
            }

            LaunchSplash();
            StartInteractiveShell(engine);
        }

        private static void StartInteractiveShell(Interpreter engine)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine("   WEA-Sharp Terminal v2.0 - [MASTER BUILD 2026]            ");
            Console.WriteLine("   'wea_help()' | 'wea_exit()' | 'run \"script.we\"'         ");
            Console.WriteLine("============================================================");
            Console.ResetColor();

            while (true)
            {
                string fullBuffer = "";
                bool isSegmented = false;
                int scopeDepth = 0;

                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(isSegmented ? "      | " : "wea_core > ");
                    Console.ResetColor();

                    string input = Console.ReadLine();
                    if (input == null) break; 

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        if (scopeDepth <= 0 && !string.IsNullOrEmpty(fullBuffer)) break;
                        continue;
                    }

                    string cmd = input.Trim().ToLower();

                   
                    if (cmd == "wea_exit()") return;
                    if (cmd == "wea_help()") { ShowHelp(); fullBuffer = ""; break; }

                    if (cmd.StartsWith("run "))
                    {
                        string target = input.Trim().Substring(4).Trim('"');
                        BootFromFile(target, engine);
                        fullBuffer = ""; break;
                    }

                    fullBuffer += input + Environment.NewLine;
                    scopeDepth += input.Count(c => c == '{' || c == '[');
                    scopeDepth -= input.Count(c => c == '}' || c == ']');

                    isSegmented = scopeDepth > 0;
                    if (!isSegmented) break;
                }

                if (!string.IsNullOrEmpty(fullBuffer)) ProcessPulse(fullBuffer, engine);
            }
        }

        public static void ShowHelp(string category = "")
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;

            if (string.IsNullOrEmpty(category))
            {
                Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
                Console.WriteLine("║              WEA-SHARP CORE MANUAL (v2.0)                ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
                Console.WriteLine(" [1] IO & Core         [2] Logic & Control                 ");
                Console.WriteLine(" [3] Data & Types      [4] Mathematics                     ");
                Console.WriteLine(" [5] Visual & UI       [6] Storage (Prime)                 ");
                Console.WriteLine(" [7] Networking        [8] System Info                     ");
                Console.WriteLine("────────────────────────────────────────────────────────────");
                Console.Write(" >> Kategori No (Çıkış için ESC): ");

                var keyPress = Console.ReadKey();
                if (keyPress.Key == ConsoleKey.Escape) { Console.Clear(); return; }
                ShowHelp(keyPress.KeyChar.ToString());
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n------------------------------------------------------------");
            switch (category)
            {
                case "1": Console.WriteLine(" > IO: wea_print(val), wea_read(msg), wea_clean(), wea_exit()"); break;
                case "2": Console.WriteLine(" > LOGIC: wea_unit(var), wea_cycle(loop), wea_verify(if)"); break;
                case "3": Console.WriteLine(" > DATA: wea_int(val), wea_str(val), wea_list_init()"); break;
                case "4": Console.WriteLine(" > MATH: wea_math_sqrt(n), wea_math_rand(a,b)"); break;
                case "5": Console.WriteLine(" > DRAW: wea_view_init(w,h), wea_draw_circle(x,y,r,c)"); break;
                case "6": Console.WriteLine(" > FILE: wea_file_read(p), wea_file_write(p,c), wea_vault_store(k,v)"); break;
                case "7": Console.WriteLine(" > NET: wea_net_get(url), wea_net_json(data,key)"); break;
                case "8": Console.WriteLine(" > SYS: wea_sys_ver(), wea_sys_env(key)"); break;
                default: ShowHelp(""); return;
            }
            Console.WriteLine("------------------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\n[ENTER] Geri Dön | [Herhangi Tuş] Terminale Dön");
            if (Console.ReadKey().Key == ConsoleKey.Enter) ShowHelp("");
            else Console.Clear();
        }

        private static void RegisterGlobalModules()
        {
            
            var modules = new List<ILibrary> { new AdvancedLibrary(), new DrawLib() };
            foreach (var mod in modules)
            {
                var funcs = mod.GetFunctions();
                if (funcs == null) continue;
                foreach (var f in funcs)
                {
                    string key = f.Key.ToLower();
                    if (!StandardLibrary.Functions.ContainsKey(key))
                        StandardLibrary.Functions.Add(key, f.Value);
                }
            }
        }

        private static void ProcessPulse(string rawCode, Interpreter engine)
        {
            try
            {
                var tokens = new Lexer(rawCode).Tokenize();
                var tree = new Parser(tokens).Parse();
                if (tree != null) engine.ExecuteStatements(tree);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[WEA_CORE_ERROR]: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void BootFromFile(string fileName, Interpreter engine)
        {
            if (!File.Exists(fileName) && File.Exists(fileName + ".we")) fileName += ".we";
            if (!File.Exists(fileName)) { Console.WriteLine($"[WEA_FAIL] Kaynak yok: {fileName}"); return; }
            try { ProcessPulse(File.ReadAllText(fileName), engine); }
            catch (Exception ex) { Console.WriteLine($"[WEA_LOAD_ERROR] {ex.Message}"); }
        }

        static void LaunchSplash()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n    ╔═══════════════════════════════════════╗");
            Console.WriteLine("    ║     WEA-Sharp Core Engine v2.0        ║");
            Console.WriteLine("    ╚═══════════════════════════════════════╝");
            Thread.Sleep(300);
            Console.ResetColor();
        }
    }
}
