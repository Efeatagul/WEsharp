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
// ═══════════════════════════════════════════════════════════════
//  MainWindow.xaml.cs — WSharp Studio IDE (Pure WPF)
// ═══════════════════════════════════════════════════════════════
//
//  This is the UNIFIED WPF IDE, replacing the old WinForms IDE.cs.
//
//  FEATURES PORTED FROM IDE.cs:
//  ────────────────────────────
//  ✓ File Explorer (project tree)
//  ✓ Multi-tab code editor (AvalonEdit)
//  ✓ Syntax highlighting (WSharp keywords)
//  ✓ Code execution on background thread
//  ✓ Terminal output with colored messages
//  ✓ Variable watcher (memory inspector)
//  ✓ Error highlighting + diagnostics
//  ✓ Keyboard shortcuts (F5, Ctrl+S, Ctrl+N)
//  ✓ Scientific plotter (ScottPlot WPF)
//  ✓ Python bridge initialization
//
//  ⚠️  ZERO System.Windows.Forms references.
//      This is a pure WPF application.
// ═══════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace WSharp
{
    
    public partial class MainWindow : Window
    {
         
        private string ProjectPath;
        private string CurrentFilePath;

        
        private static readonly string[] Keywords = {
            "let", "func", "if", "else", "while", "return",
            "print", "input", "import", "break", "continue",
            "true", "false", "null", "try", "catch",
            "plot_live", "plot_live_named", "plot_title", "plot_close",
            "foreach", "in"
        };

        
        public MainWindow()
        {
            InitializeComponent();

            
            ProjectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyProject");
            if (!Directory.Exists(ProjectPath)) Directory.CreateDirectory(ProjectPath);

           
            SetupEditor();

           
            SetupPlot();

            
            RefreshExplorer();

           
            this.KeyDown += GlobalShortcuts;

            
            InitializeEngineBridge();

            LogToConsole("✅ WSharp Studio initialized. Ready.");
            StatusText.Text = "Ready";
        }

        

        private void SetupEditor()
        {
            
            CodeEditor.Text =
                "// WSharp — Lingua Medica\n"
              + "// Write your simulation code here,\n"
              + "// then press F5 or ▶ RUN.\n"
              + "\n"
              + "let x = 42\n"
              + "print x\n";

            
            CodeEditor.TextArea.TextView.LineTransformers.Add(
                new WSharpSyntaxHighlighter());

           
            CodeEditor.Options.IndentationSize = 4;
            CodeEditor.Options.ConvertTabsToSpaces = true;
            CodeEditor.Options.HighlightCurrentLine = true;
        }

        

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
           
            string name = ShowInputDialog("File Name:", "New File");
            if (string.IsNullOrWhiteSpace(name)) return;

            if (!name.EndsWith(".we")) name += ".we";
            string path = Path.Combine(ProjectPath, name);

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "// WSharp Science\n\"C6H12O6\" |> chem_mass() |> print()\n");
                RefreshExplorer();
                OpenFile(path);
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentFile();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "WSharp Files (*.we)|*.we|All Files (*.*)|*.*",
                InitialDirectory = ProjectPath
            };

            if (dialog.ShowDialog() == true)
            {
                OpenFile(dialog.FileName);
            }
        }

        private void OpenFile(string path)
        {
            if (!File.Exists(path)) return;
            CurrentFilePath = path;
            CodeEditor.Text = File.ReadAllText(path);

            
            string fileName = Path.GetFileName(path);
            EditorTabBar.Children.Clear();
            EditorTabBar.Children.Add(new TextBlock
            {
                Text = $"  {fileName}",
                Foreground = Brushes.White,
                FontSize = 11,
                FontFamily = new FontFamily("Segoe UI"),
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(8, 4, 8, 4),
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30))
            });

            StatusText.Text = $"Opened: {fileName}";
        }

        private void SaveCurrentFile()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
              
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "WSharp Files (*.we)|*.we",
                    InitialDirectory = ProjectPath,
                    FileName = "script.we"
                };
                if (dialog.ShowDialog() == true)
                {
                    CurrentFilePath = dialog.FileName;
                }
                else return;
            }

            File.WriteAllText(CurrentFilePath, CodeEditor.Text);
            StatusText.Text = $"Saved: {Path.GetFileName(CurrentFilePath)}";
        }

       

        private void RefreshExplorer()
        {
            FileExplorer.Items.Clear();

            var root = new TreeViewItem
            {
                Header = "📁 MyProject",
                Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                IsExpanded = true
            };

            if (Directory.Exists(ProjectPath))
            {
                foreach (string file in Directory.GetFiles(ProjectPath, "*.we"))
                {
                    var item = new TreeViewItem
                    {
                        Header = $"📄 {Path.GetFileName(file)}",
                        Tag = file,
                        Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200))
                    };
                    root.Items.Add(item);
                }
            }

            FileExplorer.Items.Add(root);
        }

        private void FileExplorer_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileExplorer.SelectedItem is TreeViewItem item && item.Tag is string path)
            {
                OpenFile(path);
            }
        }

        
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            RunCode();
        }

        private void RunCode()
        {
           
            SaveCurrentFile();

            
            RunButton.IsEnabled = false;

           
            ConsoleOutput.Clear();
            ErrorLog.Text = "Compiling...\n";

            string code = CodeEditor.Text;

            LogToConsole($"> WSharp Engine Initializing for \"{Path.GetFileName(CurrentFilePath ?? "script.we")}\"...");

           
            Thread t = new Thread(() =>
            {
                Interpreter engine = null;
                try
                {
                    engine = new Interpreter();
                    Interpreter.RegisterStdLib(engine);
                    engine.BasePath = ProjectPath;

                    
                    engine.Globals.Define("neura", new WValue(new NeuroFunc()));

                    
                    engine.OnOutput += (msg) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LogToConsole(msg);
                        });
                    };

                    Dispatcher.Invoke(() => LogToConsole("> Compiling..."));

                    var tokens = new Lexer(code).Tokenize();
                    var parser = new Parser(tokens);
                    var statements = parser.Parse();

                    if (statements != null)
                    {
                        Dispatcher.Invoke(() => LogToConsole("> Running...\n--------------------------"));
                        engine.Interpret(statements);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        LogToConsole("\n> Process exited with code 0.");
                        ErrorLog.Text = "No issues found. ✅";
                        ErrorLog.Foreground = new SolidColorBrush(Color.FromRgb(100, 255, 140));

                       
                        if (engine != null) UpdateVariableWatcher(engine);

                        RunButton.IsEnabled = true;
                        StatusText.Text = "Execution complete";
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LogToConsole($"\n[FATAL ERROR]: {ex.Message}");

                        
                        ErrorLog.Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                        ErrorLog.Text = $"❌ EXCEPTION: {ex.Message}\n\n";

                        
                        string fix = AIFixer.AnalyzeAndFix(code, ex.Message);
                        ErrorLog.Text += fix;

                        
                        if (engine != null) UpdateVariableWatcher(engine);

                        RunButton.IsEnabled = true;
                        StatusText.Text = "Execution failed";
                    });
                }
            });
            t.IsBackground = true;
            t.Start();
        }

  

        private void UpdateVariableWatcher(Interpreter engine)
        {
            VariableWatcher.Items.Clear();
            var root = new TreeViewItem
            {
                Header = "🌐 Global Scope",
                Foreground = Brushes.White,
                IsExpanded = true
            };

            
            var stdlibNames = new HashSet<string>();
            foreach (var kvp in engine.GetEnvironment().Values)
            {
                if (kvp.Value.Type == WType.Function && kvp.Value.Value is IWCallable callable)
                {
                    if (callable.ToString().StartsWith("<native fn"))
                        stdlibNames.Add(kvp.Key);
                }
            }

            foreach (var kvp in engine.GetEnvironment().Values)
            {
                string name = kvp.Key;
                WValue val = kvp.Value;

                if (stdlibNames.Contains(name) && val.Type == WType.Function)
                    continue;

                TreeViewItem node;
                switch (val.Type)
                {
                    case WType.Number:
                        node = new TreeViewItem { Header = $"🔢 {name} = {val}" };
                        node.Foreground = new SolidColorBrush(Color.FromRgb(100, 255, 140));
                        root.Items.Add(node);
                        break;

                    case WType.String:
                        node = new TreeViewItem { Header = $"📝 {name} = \"{val}\"" };
                        node.Foreground = new SolidColorBrush(Color.FromRgb(214, 157, 133));
                        root.Items.Add(node);
                        break;

                    case WType.Bool:
                        node = new TreeViewItem { Header = $"✅ {name} = {val}" };
                        node.Foreground = new SolidColorBrush(Color.FromRgb(86, 156, 214));
                        root.Items.Add(node);
                        break;

                    case WType.List:
                        var list = val.AsList();
                        node = new TreeViewItem { Header = $"📋 {name}  [{list.Count} eleman]" };
                        node.Foreground = new SolidColorBrush(Color.FromRgb(78, 201, 255));
                        for (int i = 0; i < list.Count && i < 50; i++)
                        {
                            var child = new TreeViewItem
                            {
                                Header = $"[{i}] = {new WValue(list[i])}",
                                Foreground = new SolidColorBrush(Color.FromRgb(180, 220, 255))
                            };
                            node.Items.Add(child);
                        }
                        if (list.Count > 50)
                            node.Items.Add(new TreeViewItem
                            {
                                Header = $"... +{list.Count - 50} eleman daha",
                                Foreground = Brushes.Gray
                            });
                        root.Items.Add(node);
                        break;

                    case WType.Dict:
                        var dict = val.AsDict();
                        node = new TreeViewItem { Header = $"📖 {name}  {{{dict.Count} anahtar}}" };
                        node.Foreground = new SolidColorBrush(Color.FromRgb(199, 146, 234));
                        foreach (var entry in dict)
                        {
                            var child = new TreeViewItem
                            {
                                Header = $"{entry.Key}: {entry.Value}",
                                Foreground = new SolidColorBrush(Color.FromRgb(220, 190, 255))
                            };
                            node.Items.Add(child);
                        }
                        root.Items.Add(node);
                        break;

                    case WType.Function:
                        node = new TreeViewItem { Header = $"⚡ {name}()" };
                        node.Foreground = new SolidColorBrush(Color.FromRgb(130, 130, 130));
                        root.Items.Add(node);
                        break;

                    default:
                        node = new TreeViewItem { Header = $"❓ {name} = {val}" };
                        node.Foreground = Brushes.Gray;
                        root.Items.Add(node);
                        break;
                }
            }

            if (root.Items.Count == 0)
                root.Items.Add(new TreeViewItem
                {
                    Header = "(boş — değişken tanımlanmadı)",
                    Foreground = Brushes.Gray
                });

            VariableWatcher.Items.Add(root);
        }

        

        public void LogToConsole(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => LogToConsole(message));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            ConsoleOutput.AppendText($"[{timestamp}]  {message}\n");
            ConsoleOutput.ScrollToEnd();
        }

       

        private void SetupPlot()
        {
            var plt = MainPlot.Plot;
            plt.Clear();

            int pointCount = 200;
            double[] xs = Enumerable.Range(0, pointCount)
                .Select(i => i * 4.0 * Math.PI / pointCount)
                .ToArray();

            double[] sinValues = xs.Select(x => Math.Sin(x)).ToArray();
            double[] cosValues = xs.Select(x => Math.Cos(x)).ToArray();

            var sinSeries = plt.Add.Scatter(xs, sinValues);
            sinSeries.LegendText = "Sin(x)";
            sinSeries.LineWidth = 2;

            var cosSeries = plt.Add.Scatter(xs, cosValues);
            cosSeries.LegendText = "Cos(x)";
            cosSeries.LineWidth = 2;

            
            plt.FigureBackground.Color = ScottPlot.Color.FromHex("#0D1B2A");
            plt.DataBackground.Color = ScottPlot.Color.FromHex("#16213E");
            plt.Title("WSharp — Demo Waveform");
            plt.Axes.Bottom.Label.Text = "Time (t)";
            plt.Axes.Left.Label.Text = "Amplitude";

            var labelColor = ScottPlot.Color.FromHex("#48CAE4");
            var tickColor = ScottPlot.Color.FromHex("#7B8CA3");
            plt.Axes.Title.Label.ForeColor = labelColor;
            plt.Axes.Bottom.Label.ForeColor = labelColor;
            plt.Axes.Left.Label.ForeColor = labelColor;
            plt.Axes.Bottom.TickLabelStyle.ForeColor = tickColor;
            plt.Axes.Left.TickLabelStyle.ForeColor = tickColor;

            plt.ShowLegend();
            MainPlot.Refresh();
        }

        

        private async void InitializeEngineBridge()
        {
            LogToConsole("🔧 Starting WNEURA engine...");

            bool success = PythonBridge.Instance.Initialize();
            if (!success)
            {
                LogToConsole($"⚠️ Engine not available: {PythonBridge.Instance.LastError}");
                LogToConsole("   (Data science commands will not work until engine is configured)");
                return;
            }

            bool alive = await PythonBridge.Instance.PingAsync();
            if (alive)
                LogToConsole("✅ Engine connected (persistent daemon running).");
            else
                LogToConsole("⚠️ Engine process started but ping failed.");
        }

       

        private void GlobalShortcuts(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                RunCode();
                e.Handled = true;
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveCurrentFile();
                e.Handled = true;
            }
            else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                NewFile_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        

        private static string ShowInputDialog(string prompt, string title)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 350,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow
            };

            var stack = new StackPanel { Margin = new Thickness(15) };

            var label = new TextBlock
            {
                Text = prompt,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 8),
                FontFamily = new FontFamily("Segoe UI")
            };

            var textBox = new TextBox
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Consolas"),
                Padding = new Thickness(6, 4, 6, 4),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204))
            };

            var button = new Button
            {
                Content = "OK",
                Width = 80,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                Foreground = Brushes.White,
                Padding = new Thickness(8, 4, 8, 4),
                BorderThickness = new Thickness(0)
            };

            string result = "";
            button.Click += (s, e) => { result = textBox.Text; dialog.DialogResult = true; };
            dialog.Loaded += (s, e) => textBox.Focus();

            stack.Children.Add(label);
            stack.Children.Add(textBox);
            stack.Children.Add(button);
            dialog.Content = stack;

            return dialog.ShowDialog() == true ? result : "";
        }

        

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            PythonBridge.Instance.Dispose();
        }
    }

    
    public class WSharpSyntaxHighlighter : DocumentColorizingTransformer
    {
        
        private static readonly SolidColorBrush KeywordColor = new SolidColorBrush(Color.FromRgb(86, 156, 214));
        private static readonly SolidColorBrush FunctionColor = new SolidColorBrush(Color.FromRgb(220, 220, 170));
        private static readonly SolidColorBrush StringColor = new SolidColorBrush(Color.FromRgb(214, 157, 133));
        private static readonly SolidColorBrush CommentColor = new SolidColorBrush(Color.FromRgb(87, 166, 74));
        private static readonly SolidColorBrush NumberColor = new SolidColorBrush(Color.FromRgb(181, 206, 168));

      
        private static readonly Regex KeywordRegex = new Regex(
            @"\b(let|func|if|else|while|return|import|break|continue|true|false|null|try|catch|foreach|in)\b",
            RegexOptions.Compiled);

        private static readonly Regex FunctionRegex = new Regex(
            @"\b(print|input|connexio_http|json_parse|clock|sqrt|creare_cortex|simulare_gradus|nectere_\w+|legere_\w+|injicere_\w+|configurare_\w+|numerare_\w+|activare_\w+|resettere_\w+|plot_live|plot_live_named|plot_title|plot_close|bio_\w+|gen_\w+|eco_\w+|physio_\w+|plant_\w+|chem_\w+|phys_\w+|math_\w+|core_\w+|nuc_\w+|py_\w+)\b",
            RegexOptions.Compiled);

        private static readonly Regex StringRegex = new Regex(
            @"""[^""\\]*(?:\\.[^""\\]*)*""",
            RegexOptions.Compiled);

        private static readonly Regex NumberRegex = new Regex(
            @"\b\d+(\.\d+)?\b",
            RegexOptions.Compiled);

        private static readonly Regex CommentRegex = new Regex(
            @"//.*$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        protected override void ColorizeLine(DocumentLine line)
        {
            int lineStart = line.Offset;
            string text = CurrentContext.Document.GetText(line);

            
            ApplyColor(text, lineStart, StringRegex, StringColor);
            ApplyColor(text, lineStart, NumberRegex, NumberColor);
            ApplyColor(text, lineStart, KeywordRegex, KeywordColor);
            ApplyColor(text, lineStart, FunctionRegex, FunctionColor);
            ApplyColor(text, lineStart, CommentRegex, CommentColor);
        }

        private void ApplyColor(string text, int lineStart, Regex pattern, SolidColorBrush brush)
        {
            foreach (Match match in pattern.Matches(text))
            {
                int start = lineStart + match.Index;
                int end = start + match.Length;

                ChangeLinePart(start, end, element =>
                {
                    element.TextRunProperties.SetForegroundBrush(brush);
                });
            }
        }
    }
}
