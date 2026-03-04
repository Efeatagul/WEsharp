/* ======================================================================
 * WSHARP (We#) NEURO-ENGINE - WEAGW Ecosystem
 * Copyright (c) 2026 Efe Ata Gul. All rights reserved.
 * * This file is part of the WSharp project.
 * * OPEN SOURCE: Licensed under the GNU AGPL v3.0. You may use this 
 * file freely in open-source/academic projects provided you give 
 * clear attribution to "WSharp by Efe Ata Gul".
 * * COMMERCIAL: If you wish to use WSharp in closed-source, proprietary, 
 * or commercial products, you must purchase a WEAGW Commercial License.
 * ====================================================================== */
#nullable disable
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows; 
using System.Runtime.InteropServices; 

namespace WSharp
{
    static class Program
    {
        private static readonly Interpreter interpreter = new Interpreter();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [STAThread]
        static void Main(string[] args)
        {
           
            if (args.Length > 0 && File.Exists(args[0]))
            {
                AllocConsole();
                RunConsoleFile(args[0]);
                return;
            }

            
            AllocConsole();
            ShowLauncher();
        }

        
        private static void ShowLauncher()
        {
            Console.Title = "WSharp — Lingua Medica";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            try { Console.Clear(); } catch { /* handle geçersizse (pipe/redirect) yoksay */ }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ╔═══════════════════════════════════════════════╗
  ║                                               ║
  ║     ██╗    ██╗███████╗██╗  ██╗ █████╗         ║
  ║     ██║    ██║██╔════╝██║  ██║██╔══██╗        ║
  ║     ██║ █╗ ██║███████╗███████║███████║        ║
  ║     ██║███╗██║╚════██║██╔══██║██╔══██║        ║
  ║     ╚███╔███╔╝███████║██║  ██║██║  ██║        ║
  ║      ╚══╝╚══╝ ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝        ║
  ║                                               ║
  ║         L I N G U A   M E D I C A             ║
  ╚═══════════════════════════════════════════════╝");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.WriteLine("  Bilim icin tasarlanan programlama dili.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  [1]  Colloquium  — Canli REPL Konsolu");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [2]  IDE         — Gorsel Gelistirme Ortami");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [Q]  Cikis");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Seciminiz > ");

            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == '1')
                {
                    Console.WriteLine("1");
                    RunColloquium();
                    return;
                }
                else if (key.KeyChar == '2')
                {
                    Console.WriteLine("2");
                    
                    try
                    {
                        var app = new App();
                        app.Run(new MainWindow());
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[WPF FATAL CRASH]");
                        Console.WriteLine(ex.ToString());
                        Console.ResetColor();
                        System.Windows.MessageBox.Show(
                            ex.ToString(),
                            "WPF Fatal Crash",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    }
                    return;
                }
                else if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    return;
                }
            }
        }

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

      
        private static void RunColloquium()
        {
            Console.Clear();
            Console.Title = "WSharp Colloquium — REPL";

          
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════╗");
            Console.WriteLine("  ║   COLLOQUIUM — WSharp REPL v1.0      ║");
            Console.WriteLine("  ║   Lingua Medica Interactive Shell     ║");
            Console.WriteLine("  ╚═══════════════════════════════════════╝");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Yardim icin 'auxilium' yazin. Cikmak icin 'vale' yazin.");
            Console.WriteLine();

      
            Interpreter.RegisterStdLib(interpreter);
            interpreter.BasePath = Directory.GetCurrentDirectory();
            interpreter.OnOutput += (output) =>
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(output);
            };

            var history = new List<string>();
            int historyIndex = -1;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  wsharp");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" » ");
                Console.ForegroundColor = ConsoleColor.White;

                string line = ReadLineWithHistory(history, ref historyIndex);

                if (line == null) break; 

                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

               
                if (history.Count == 0 || history[history.Count - 1] != trimmed)
                    history.Add(trimmed);
                historyIndex = history.Count;

           
                if (trimmed == "vale" || trimmed == "exit" || trimmed == "quit")
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n  Vale! (Hoşça kal)\n");
                    Console.ResetColor();
                    break;
                }

                if (trimmed == "auxilium" || trimmed == "help")
                {
                    ShowHelp();
                    continue;
                }

                if (trimmed == "clear" || trimmed == "purga")
                {
                    Console.Clear();
                    continue;
                }

                if (trimmed == "memoria" || trimmed == "vars")
                {
                    ShowVariables();
                    continue;
                }

                if (trimmed.StartsWith("curre ") || trimmed.StartsWith("run "))
                {
                    string filePath = trimmed.Substring(trimmed.IndexOf(' ') + 1).Trim().Trim('"');
                    RunFileInRepl(filePath);
                    continue;
                }

                
                string fullCode = trimmed;
                if (NeedsMoreInput(fullCode))
                {
                    fullCode = ReadMultiLine(fullCode);
                }

                
                ExecuteRepl(fullCode);
            }
        }

        
        private static string ReadLineWithHistory(List<string> history, ref int historyIndex)
        {
            
            var buffer = new List<char>();
            int pos = 0;
            int startLeft = Console.CursorLeft;
            int startTop = Console.CursorTop;

            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return new string(buffer.ToArray());
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (pos > 0)
                    {
                        buffer.RemoveAt(pos - 1);
                        pos--;
                        RedrawLine(buffer, startLeft, startTop, pos);
                    }
                }
                else if (key.Key == ConsoleKey.Delete)
                {
                    if (pos < buffer.Count)
                    {
                        buffer.RemoveAt(pos);
                        RedrawLine(buffer, startLeft, startTop, pos);
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (pos > 0) { pos--; Console.CursorLeft = startLeft + pos; }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (pos < buffer.Count) { pos++; Console.CursorLeft = startLeft + pos; }
                }
                else if (key.Key == ConsoleKey.Home)
                {
                    pos = 0; Console.CursorLeft = startLeft;
                }
                else if (key.Key == ConsoleKey.End)
                {
                    pos = buffer.Count; Console.CursorLeft = startLeft + pos;
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (history.Count > 0 && historyIndex > 0)
                    {
                        historyIndex--;
                        buffer.Clear();
                        buffer.AddRange(history[historyIndex].ToCharArray());
                        pos = buffer.Count;
                        RedrawLine(buffer, startLeft, startTop, pos);
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (historyIndex < history.Count - 1)
                    {
                        historyIndex++;
                        buffer.Clear();
                        buffer.AddRange(history[historyIndex].ToCharArray());
                        pos = buffer.Count;
                        RedrawLine(buffer, startLeft, startTop, pos);
                    }
                    else
                    {
                        historyIndex = history.Count;
                        buffer.Clear();
                        pos = 0;
                        RedrawLine(buffer, startLeft, startTop, pos);
                    }
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    buffer.Clear();
                    pos = 0;
                    RedrawLine(buffer, startLeft, startTop, pos);
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    buffer.Insert(pos, key.KeyChar);
                    pos++;
                    RedrawLine(buffer, startLeft, startTop, pos);
                }
            }
        }

        private static void RedrawLine(List<char> buffer, int startLeft, int startTop, int cursorPos)
        {
            Console.SetCursorPosition(startLeft, startTop);
            string text = new string(buffer.ToArray());
            Console.Write(text + "  "); 
            Console.SetCursorPosition(startLeft + cursorPos, startTop);
        }

        private static bool NeedsMoreInput(string code)
        {
            int open = 0, close = 0;
            foreach (char c in code)
            {
                if (c == '{') open++;
                if (c == '}') close++;
            }
            return open > close;
        }

        private static string ReadMultiLine(string firstLine)
        {
            string code = firstLine + "\n";
            while (NeedsMoreInput(code))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  ...... ");
                Console.ForegroundColor = ConsoleColor.White;
                string next = Console.ReadLine();
                if (next == null) break;
                code += next + "\n";
            }
            return code;
        }

        private static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  ╔═══════════════════════════════════════╗");
            Console.WriteLine("  ║         AUXILIUM — Yardım             ║");
            Console.WriteLine("  ╠═══════════════════════════════════════╣");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("vale        ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("REPL'den cik              ║");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("purga       ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Ekrani temizle            ║");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("memoria     ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Degiskenleri goster       ║");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("curre \"x\"   ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Dosya calistir            ║");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("↑ / ↓       ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Komut gecmisi             ║");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╠═══════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ║  Hızlı Sözdizimi:                    ║");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("let");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" x = 10         ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Degisken          ║");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("print");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" x            ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Yazdir            ║");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("func");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" f(n) {}      ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Fonksiyon         ║");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╚═══════════════════════════════════════╝\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void ShowVariables()
        {
            var defs = interpreter.GetEnvironment().GetAllDefinitions();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  ─── Memoria (Değişkenler) ───");

            int count = 0;
            foreach (var kv in defs)
            {
                
                if (kv.Value.Type == WType.Function) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"  {kv.Key}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" = ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{kv.Value}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  ({kv.Value.Type})");
                count++;
            }

            if (count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  (bos — henuz degisken tanimlanmadi)");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ────────────────────────────\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void RunFileInRepl(string path)
        {
            if (!File.Exists(path))
            {
                
                string altPath = Path.Combine(interpreter.BasePath ?? ".", path);
                if (File.Exists(altPath)) path = altPath;
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ Dosya bulunamadi: {path}");
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  ▶ Calistiriliyor: {Path.GetFileName(path)}");
            Console.ForegroundColor = ConsoleColor.White;

            string prevBase = interpreter.BasePath;
            interpreter.BasePath = Path.GetDirectoryName(Path.GetFullPath(path));

            string source = File.ReadAllText(path);
            ExecuteRepl(source);

            interpreter.BasePath = prevBase;
        }

        private static void ExecuteRepl(string source)
        {
            try
            {
                var tokens = new Lexer(source).Tokenize();
                var stmts = new Parser(tokens).Parse();
                if (stmts != null)
                    interpreter.Interpret(stmts);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("  ✗ ");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e.Message);

             
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string suggestion = AIFixer.AnalyzeAndFix(source, e.Message);
                if (!string.IsNullOrEmpty(suggestion))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  💡 {suggestion.Split('\n')[0]}");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

       
        private static void RunConsoleFile(string path)
        {
            Console.Title = $"WSharp — {Path.GetFileName(path)}";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  ▶ WSharp CLI — {Path.GetFileName(path)}");
            Console.ForegroundColor = ConsoleColor.White;

            Interpreter.RegisterStdLib(interpreter);
            interpreter.OnOutput += (output) => Console.WriteLine(output);
            interpreter.BasePath = Path.GetDirectoryName(Path.GetFullPath(path));

            string source = File.ReadAllText(path);
            ExecuteRepl(source);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n  ENTER ile cik...");
            Console.ResetColor();
            Console.ReadLine();
        }
    }
}