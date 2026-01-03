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
           
            var libraries = new List<ILibrary>
            {
                new AdvancedLibrary(),
                new DrawLib()
            };

  
            foreach (var lib in libraries)
            {
                var funcs = lib.GetFunctions();
                foreach (var func in funcs)
                {
                    if (!StandardLibrary.Functions.ContainsKey(func.Key))
                    {
                        StandardLibrary.Functions.Add(func.Key, func.Value);
                    }
                }
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
    *************************************************
    * *
    * WE# (WeSharp) SMART ENGINE v1.4          *
    * 'GRAFIK MOTORU VE WEB DESTEGI'           *
    * *
    *************************************************");
            Thread.Sleep(800);
            Console.Clear();

            var interpreter = new Interpreter();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine("          WE# BY EFEATAGUL - SMART TERMINAL v1.4            ");
            Console.WriteLine("      'help()' | 'exit()' | 'run dosya.we' | 'we_core()'    ");
            Console.WriteLine("============================================================");
            Console.ResetColor();

          
            while (true)
            {
                string fullCode = "";
                bool isMultiLine = false;
                int bracketCount = 0;

                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(isMultiLine ? "      | " : "user > ");
                    Console.ResetColor();

                    string line = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (!string.IsNullOrEmpty(fullCode) && bracketCount <= 0) break;
                        else if (bracketCount > 0) { fullCode += Environment.NewLine; continue; }
                        else continue;
                    }

                    string trimLine = line.Trim().ToLower();

                    if (trimLine == "exit()") return;

                    if (trimLine == "help()")
                    {
                        ShowHelpMenu();
                        fullCode = "";
                        break;
                    }

                    if (trimLine.StartsWith("run "))
                    {
                        string fileName = line.Trim().Substring(4).Replace("\"", "");
                        RunFromFile(fileName, interpreter);
                        fullCode = "";
                        break;
                    }

                    fullCode += line + Environment.NewLine;

                  
                    if (line.Contains("{") || line.Contains("[")) { bracketCount++; isMultiLine = true; }
                    if (line.Contains("}") || line.Contains("]"))
                    {
                        bracketCount--;
                        if (bracketCount <= 0) { bracketCount = 0; isMultiLine = false; }
                    }

                    if (!isMultiLine) break;
                }

                if (string.IsNullOrEmpty(fullCode)) continue;

                try
                {
                    var lexer = new Lexer(fullCode);
                    var tokens = lexer.Tokenize();
                    var parser = new Parser(tokens);
                    var programStructure = parser.Parse();

                    if (programStructure == null || programStructure.Count == 0) continue;

              
                    interpreter.ExecuteStatements(programStructure);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Hata: " + ex.Message);
                    Console.ResetColor();
                }
            }
        }

        static void RunFromFile(string fileName, Interpreter interpreter)
        {
            try
            {
                if (!File.Exists(fileName) && File.Exists(fileName + ".we")) fileName += ".we";

                if (!File.Exists(fileName))
                {
                    Console.WriteLine($"[!] Hata: {fileName} bulunamadi.");
                    return;
                }

                string code = File.ReadAllText(fileName);
                var lexer = new Lexer(code);
                var tokens = lexer.Tokenize();
                var parser = new Parser(tokens);
                var statements = parser.Parse();

                interpreter.ExecuteStatements(statements);
            }
            catch (Exception ex) { Console.WriteLine("Dosya yürütme hatası: " + ex.Message); }
        }

        static void ShowHelpMenu()
        {
            Console.WriteLine("\n--- WE# YARDIM MENUSU ---");
            Console.WriteLine("win_open(500, 500, \"Baslik\") -> Pencere acar");
            Console.WriteLine("win_clear(\"Red\")            -> Arkaplani boyar");
            Console.WriteLine("draw_text(10, 10, \"Selam\", 20, \"White\") -> Yazi yazar");
            Console.WriteLine("draw_circle(100, 100, 50, \"Blue\") -> Daire cizer");
            Console.WriteLine("--------------------------\n");
        }
    }
}
