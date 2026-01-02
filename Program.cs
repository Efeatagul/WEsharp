#nullable disable
using System;
using System.Collections.Generic;
using System.Threading;

namespace WSharp
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
    *************************************************
    * *
    * WE# (WeSharp) PROGRAMMING language          *
    * *
    * we# by Efeatagul                            *
    * VERSION: 1.0 (Live Shell)                   *
    * *
    *************************************************
            ");
            Console.WriteLine("\n      WE# Engine is initializing by Efeatagul...");
            Thread.Sleep(2000);
            Console.Clear();


            var interpreter = new Interpreter();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine("          WE# BY EFEATAGUL - INTERACTIVE TERMINAL v1.0      ");
            Console.WriteLine("      (Cikmak icin 'exit()' yazin veya CTRL+C yapin)      ");
            Console.WriteLine("============================================================");
            Console.ResetColor();
            Console.WriteLine();

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

                    if (line.Trim() == "exit()") return;

                    fullCode += line + Environment.NewLine;

                    if (line.Contains("{") || line.Contains("["))
                    {
                        bracketCount++;
                        isMultiLine = true;
                    }

                    if (line.Contains("}") || line.Contains("]"))
                    {
                        bracketCount--;
                        if (bracketCount <= 0)
                        {
                            bracketCount = 0;
                            isMultiLine = false;
                        }
                    }
                }

                try
                {
                    var lexer = new Lexer(fullCode);
                    List<Token> tokens = lexer.Tokenize();
                    var parser = new Parser(tokens);
                    List<Statement> programStructure = parser.Parse();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("reply: ");
                    Console.ResetColor();

                    interpreter.ExecuteStatements(programStructure);

                    Console.WriteLine();
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("reply: Yanlis!\n");
                    Console.ResetColor();
                }

                fullCode = "";
            }
        }
    }
}