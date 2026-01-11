using System;
using System.IO;
using System.Linq;

namespace WSharp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var engine = new Interpreter();

            if (args.Length > 0)
            {
                string path = args[0];
                if (File.Exists(path))
                {
                    BootFromFile(path, engine);
                    Console.WriteLine("\n[WEA_SYS] Islem tamamlandi. Cikis icin ENTER...");
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
            Console.WriteLine("    WEA-Sharp (W#) Terminal v2.1 - [OPTIMIZED BUILD]        ");
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
                    if (cmd == "wea_clean()") { Console.Clear(); fullBuffer = ""; break; }
                    if (cmd == "wea_help()") { ShowHelp(); fullBuffer = ""; break; }

                    if (cmd.StartsWith("run"))
                    {
                        string target = input.Substring(3).Trim().Trim('"');
                        BootFromFile(target, engine);
                        fullBuffer = ""; break;
                    }

                    fullBuffer += input + System.Environment.NewLine;
                    scopeDepth += input.Count(c => c == '{');
                    scopeDepth -= input.Count(c => c == '}');
                    isSegmented = scopeDepth > 0;
                    if (!isSegmented) break;
                }

                if (!string.IsNullOrEmpty(fullBuffer)) ProcessPulse(fullBuffer, engine);
            }
        }

        public static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n--- WSHARP CHEAT SHEET ---");
            Console.WriteLine(" wea_emit(val)      -> Ekrana yazdirir.");
            Console.WriteLine(" wea_unit x = 10    -> Degisken tanimlar.");
            Console.ResetColor();
        }

        private static void ProcessPulse(string rawCode, Interpreter engine)
        {
            try
            {
                var tokens = new Lexer(rawCode).Tokenize();
                var parser = new Parser(tokens);
                var statements = parser.Parse();
                if (statements != null) engine.Interpret(statements);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[WEA_ERROR]: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void BootFromFile(string fileName, Interpreter engine)
        {
            if (!File.Exists(fileName) && File.Exists(fileName + ".we")) fileName += ".we";

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"[WEA_FAIL] Dosya bulunamadi: {fileName}");
                return;
            }

            try
            {
                ProcessPulse(File.ReadAllText(fileName), engine);
            }
            catch (Exception ex) { Console.WriteLine($"[WEA_LOAD_ERROR] {ex.Message}"); }
        }

        static void LaunchSplash()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("WEA-Sharp v2.1 (Optimized)");
            Console.ResetColor();
        }
    }
}
