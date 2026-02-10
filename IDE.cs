#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;

namespace WSharp
{
    public class IDE : Form
    {
        private readonly Color C_Back = Color.FromArgb(30, 30, 30);
        private readonly Color C_SideBar = Color.FromArgb(37, 37, 38);
        private readonly Color C_Border = Color.FromArgb(45, 45, 45);
        private readonly Color C_Accent = Color.FromArgb(0, 122, 204);
        private readonly Color C_Text = Color.FromArgb(220, 220, 220);
        private readonly Color C_Error = Color.FromArgb(255, 100, 100);

        private readonly Color Syntax_Keyword = Color.FromArgb(86, 156, 214);
        private readonly Color Syntax_Function = Color.FromArgb(220, 220, 170);
        private readonly Color Syntax_String = Color.FromArgb(214, 157, 133);
        private readonly Color Syntax_Comment = Color.FromArgb(87, 166, 74);
        private readonly Color Syntax_Number = Color.FromArgb(181, 206, 168);

        private readonly string[] Keywords = {
            "wea_unit", "wea_if", "wea_flow", "wea_cycle", "wea_verify", "wea_return",
            "wea_emit", "wea_read", "wea_plot", "wea_eman", "wea_fail"
        };

        private TabControl editorTabs;
        private Panel welcomePanel;
        private TabControl bottomTabs;
        private RichTextBox terminalArea;
        private PictureBox plotterCanvas;
        private TreeView fileExplorer;
        private TreeView variableWatcher;
        private RichTextBox aiInput;
        private RichTextBox errorLog;
        private ListBox suggestionBox;

        private string ProjectPath;

        [DllImport("user32.dll")] static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] static extern int GetScrollPos(IntPtr hWnd, int nBar);
        private const int WM_SETREDRAW = 0x000B;

        public IDE()
        {
            ProjectPath = Path.Combine(Application.StartupPath, "MyProject");
            if (!Directory.Exists(ProjectPath)) Directory.CreateDirectory(ProjectPath);

            this.Text = "WSharp Studio - Scientific Edition (MIT Core)";
            this.Size = new Size(1600, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = C_Back;
            this.ForeColor = C_Text;
            this.KeyPreview = true;
            this.KeyDown += GlobalShortcuts;

            this.Load += (s, e) => ApplyLayoutRatios();
            this.Resize += (s, e) => SyncHorizontalLines();

            SetupMenuBar();
            SetupStatusBar();
            SetupGridSystem();
            SetupIntelliSense();
            RefreshExplorer();
        }

        private void RunCode()
        {
            SaveCurrentFile();

            terminalArea.Clear();
            terminalArea.SelectionColor = Color.Gray;
            terminalArea.AppendText($"> WSharp Engine Initializing for target \"{editorTabs.SelectedTab?.Text ?? "script.we"}\"...\n");

            errorLog.Clear();
            errorLog.ForeColor = Color.LightGreen;
            errorLog.Text = "Ready.";

            RichTextBox rtb = editorTabs.SelectedTab?.Controls.OfType<RichTextBox>().FirstOrDefault(c => c.Dock == DockStyle.Fill);
            string code = rtb != null ? rtb.Text : "";

            UpdateVariableWatcher(code);

            if (code.Contains("wea_plot")) DrawDynamicPlot(code);

            Thread t = new Thread(() => {
                try
                {
                    var engine = new Interpreter();

                    engine.Globals.Define("clock", new WValue(new ClockFunc()));
                    engine.Globals.Define("sqrt", new WValue(new SqrtFunc()));
                    engine.Globals.Define("wea_read", new WValue(new InputFunc()));
                    engine.Globals.Define("print", new WValue(new PrintFunc()));
                    engine.Globals.Define("wea_len", new WValue(new LenFunc()));
                    engine.Globals.Define("wea_push", new WValue(new PushFunc()));
                    engine.Globals.Define("wea_pop", new WValue(new PopFunc()));
                    engine.Globals.Define("wea_test_json", new WValue(new TestJsonFunc()));
                    engine.Globals.Define("wea_test_serialize", new WValue(new TestSerializeFunc()));
                    engine.Globals.Define("wea_map", new WValue(new MapFunc()));
                    engine.Globals.Define("wea_filter", new WValue(new FilterFunc()));
                    engine.Globals.Define("wea_avg", new WValue(new AvgFunc()));
                    engine.Globals.Define("wea_max", new WValue(new MaxFunc()));
                    engine.Globals.Define("wea_min", new WValue(new MinFunc()));
                    engine.Globals.Define("wea_std_dev", new WValue(new StdDevFunc()));
                    engine.Globals.Define("wea_file_write", new WValue(new FileWriteFunc()));
                    engine.Globals.Define("wea_file_read", new WValue(new FileReadFunc()));
                    engine.Globals.Define("wea_file_write_logged", new WValue(new FileWriteLoggedFunc()));
                    engine.Globals.Define("wea_export_csv", new WValue(new ExportCsvFunc()));

                    engine.Globals.Define("wea_neuro_create", new WValue(new NeuroCreateFunc()));
                    engine.Globals.Define("wea_neuro_build_network", new WValue(new NeuroBuildNetworkFunc()));
                    engine.Globals.Define("wea_neuro_step", new WValue(new NeuroStepFunc()));
                    engine.Globals.Define("wea_neuro_set_input", new WValue(new NeuroSetInputFunc()));
                    engine.Globals.Define("wea_neuro_get_voltage", new WValue(new NeuroGetVoltageFunc()));

                    engine.Globals.Define("bio_classify", new WValue(new BioClassifyFunc()));
                    engine.Globals.Define("bio_transport", new WValue(new BioTransportFunc()));
                    engine.Globals.Define("bio_photosynthesis", new WValue(new BioPhotoFunc()));
                    engine.Globals.Define("bio_respire", new WValue(new BioRespireFunc()));
                    engine.Globals.Define("gen_transcribe", new WValue(new GenTranscribeFunc()));
                    engine.Globals.Define("gen_translate", new WValue(new GenTranslateFunc()));
                    engine.Globals.Define("eco_transfer", new WValue(new EcoTransferFunc()));
                    engine.Globals.Define("physio_check", new WValue(new PhysioCheckFunc()));
                    engine.Globals.Define("plant_hormone", new WValue(new PlantHormoneFunc()));
                    
                    engine.Globals.Define("bio_enzyme", new WValue(new BioEnzymeFunc()));
                    engine.Globals.Define("bio_protein", new WValue(new BioProteinFunc()));
                    engine.Globals.Define("bio_gpcr", new WValue(new BioGPCRFunc()));
                    engine.Globals.Define("bio_cancer", new WValue(new BioCancerFunc()));
                    engine.Globals.Define("bio_crispr", new WValue(new BioCrisprFunc()));
                    engine.Globals.Define("bio_oxphos", new WValue(new BioOxPhosFunc()));

                    engine.Globals.Define("chem_element", new WValue(new ChemElementFunc()));
                    engine.Globals.Define("chem_mass", new WValue(new ChemMassFunc()));
                    engine.Globals.Define("chem_gas", new WValue(new ChemGasFunc()));
                    engine.Globals.Define("chem_bond", new WValue(new ChemBondFunc()));
                    engine.Globals.Define("chem_gibbs", new WValue(new ChemGibbsFunc()));
                    engine.Globals.Define("chem_organic", new WValue(new ChemOrganicFunc()));
                    
                    engine.Globals.Define("chem_quantum", new WValue(new ChemQuantumFunc()));
                    engine.Globals.Define("chem_spectra", new WValue(new ChemSpectraFunc()));
                    engine.Globals.Define("chem_mech", new WValue(new ChemMechFunc()));

                    engine.Globals.Define("phys_mech", new WValue(new PhysMechFunc()));
                    engine.Globals.Define("phys_thermo", new WValue(new PhysThermoFunc()));
                    engine.Globals.Define("phys_em", new WValue(new PhysEMFunc()));
                    engine.Globals.Define("phys_modern", new WValue(new PhysModernFunc()));
                    engine.Globals.Define("math_basic", new WValue(new MathBasicFunc()));
                    engine.Globals.Define("math_calc", new WValue(new MathCalcFunc()));
                    engine.Globals.Define("math_matrix", new WValue(new MathMatrixFunc()));

                    engine.Globals.Define("core_file", new WValue(new CoreFileFunc()));
                    engine.Globals.Define("core_string", new WValue(new CoreStringFunc()));
                    engine.Globals.Define("core_sys", new WValue(new CoreSysFunc()));
                    engine.Globals.Define("core_net", new WValue(new CoreNetFunc()));
                    engine.Globals.Define("nuc_decay", new WValue(new NucDecayFunc()));
                    engine.Globals.Define("nuc_energy", new WValue(new NucEnergyFunc()));
                    engine.Globals.Define("nuc_reactor", new WValue(new NucReactorFunc()));
                    engine.Globals.Define("nuc_dose", new WValue(new NucDoseFunc()));

                    engine.Globals.Define("wea_neura", new WValue(new NeuroFunc()));

                   
                    engine.Globals.Define("to_number", new WValue(new ToNumberFunc()));
                    engine.Globals.Define("typeof", new WValue(new TypeOfFunc()));
                    engine.Globals.Define("range", new WValue(new RangeFunc()));
                    engine.Globals.Define("sort", new WValue(new SortFunc()));
                    engine.Globals.Define("reverse", new WValue(new ReverseFunc()));
                    engine.Globals.Define("join", new WValue(new JoinFunc()));
                    engine.Globals.Define("split", new WValue(new SplitFunc()));
                    engine.Globals.Define("abs", new WValue(new AbsFunc()));
                    engine.Globals.Define("pow", new WValue(new PowFunc()));
                    engine.Globals.Define("floor", new WValue(new FloorFunc()));
                    engine.Globals.Define("ceil", new WValue(new CeilFunc()));
                    engine.Globals.Define("round", new WValue(new RoundFunc()));
                    engine.OnOutput += (msg) => this.Invoke((MethodInvoker)(() => {
                        terminalArea.SelectionColor = Color.WhiteSmoke;
                        terminalArea.AppendText(msg + "\n");
                        terminalArea.ScrollToCaret();
                    }));

                    this.Invoke((MethodInvoker)(() => terminalArea.AppendText("> Compiling...\n")));

                    var tokens = new Lexer(code).Tokenize();
                    var parser = new Parser(tokens);
                    var statements = parser.Parse();

                    if (statements != null)
                    {
                        this.Invoke((MethodInvoker)(() => terminalArea.AppendText("> Running...\n--------------------------\n")));
                        engine.Interpret(statements);
                    }

                    this.Invoke((MethodInvoker)(() => {
                        terminalArea.SelectionColor = Color.LightGreen;
                        terminalArea.AppendText("\n> Process exited with code 0.\n");
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        terminalArea.SelectionColor = Color.Red;
                        terminalArea.AppendText($"\n[FATAL ERROR]: {ex.Message}\n");

                        errorLog.Clear();
                        errorLog.BackColor = Color.FromArgb(40, 20, 20);
                        errorLog.SelectionColor = Color.Red;
                        errorLog.SelectionFont = new Font("Consolas", 10, FontStyle.Bold);
                        errorLog.AppendText($"❌ EXCEPTION: {ex.Message}\n\n");

                        string fix = AIFixer.AnalyzeAndFix(code, ex.Message);
                        errorLog.SelectionColor = Color.Yellow;
                        errorLog.AppendText(fix);

                        HighlightErrorInEditor(ex.Message);
                    }));
                }
            });
            t.IsBackground = true; t.Start();
        }

        private void SetupGridSystem()
        {
            SplitContainer splitMain = CreateSplitter(Orientation.Vertical);
            this.Controls.Add(splitMain);
            splitMain.BringToFront();

            SplitContainer splitWorkspace = CreateSplitter(Orientation.Vertical);
            splitMain.Panel2.Controls.Add(splitWorkspace);

            SplitContainer splitColLeft = CreateSplitter(Orientation.Horizontal);
            splitMain.Panel1.Controls.Add(splitColLeft);

            fileExplorer = CreateTree();
            fileExplorer.NodeMouseDoubleClick += (s, e) => OpenFileInTab(e.Node.Text);
            SetupContextMenu(fileExplorer);
            splitColLeft.Panel1.Controls.Add(CreatePanelWithHeader("EXPLORER", fileExplorer));

            variableWatcher = CreateTree();
            variableWatcher.Nodes.Add(new TreeNode("Memory Empty") { ImageKey = "mem" });
            splitColLeft.Panel2.Controls.Add(CreatePanelWithHeader("MEMORY WATCH", variableWatcher));

            SplitContainer splitColCenter = CreateSplitter(Orientation.Horizontal);
            splitWorkspace.Panel1.Controls.Add(splitColCenter);

            editorTabs = new TabControl { Dock = DockStyle.Fill, Appearance = TabAppearance.FlatButtons, Padding = new Point(15, 5), Visible = false };
            splitColCenter.Panel1.Controls.Add(editorTabs);

            welcomePanel = new Panel { Dock = DockStyle.Fill, BackColor = C_Back };
            Label lblTitle = new Label { Text = "WSharp Studio", Font = new Font("Segoe UI Light", 36, FontStyle.Regular), ForeColor = Color.FromArgb(60, 60, 60), AutoSize = false, TextAlign = ContentAlignment.BottomCenter, Dock = DockStyle.Top, Height = splitColCenter.Panel1.Height / 2 };
            Label lblCommands = new Label { Text = "New File: Ctrl+N    |    Save: Ctrl+S    |    Run: F5", Font = new Font("Consolas", 10), ForeColor = Color.FromArgb(80, 80, 80), AutoSize = false, TextAlign = ContentAlignment.TopCenter, Dock = DockStyle.Fill, Padding = new Padding(0, 20, 0, 0) };
            welcomePanel.Controls.Add(lblCommands);
            welcomePanel.Controls.Add(lblTitle);
            splitColCenter.Panel1.Controls.Add(welcomePanel);
            welcomePanel.BringToFront();

            bottomTabs = new TabControl { Dock = DockStyle.Fill, Alignment = TabAlignment.Bottom };
            TabPage termPage = new TabPage("Terminal") { BackColor = Color.FromArgb(12, 12, 12) };
            terminalArea = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(12, 12, 12), ForeColor = Color.LightGray, Font = new Font("Consolas", 9), BorderStyle = BorderStyle.None, ReadOnly = true, WordWrap = false };
            termPage.Controls.Add(terminalArea);
            bottomTabs.TabPages.Add(termPage);

            TabPage plotPage = new TabPage("Scientific Plotter") { BackColor = Color.FromArgb(20, 20, 20) };
            plotterCanvas = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 20) };
            plotPage.Controls.Add(plotterCanvas);
            bottomTabs.TabPages.Add(plotPage);
            splitColCenter.Panel2.Controls.Add(CreatePanelWithHeader("OUTPUT", bottomTabs));

            SplitContainer splitColRight = CreateSplitter(Orientation.Horizontal);
            splitWorkspace.Panel2.Controls.Add(splitColRight);

            aiInput = new RichTextBox { Dock = DockStyle.Fill, BackColor = C_SideBar, ForeColor = C_Text, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10), Text = "// AI Assistant Ready..." };
            splitColRight.Panel1.Controls.Add(CreatePanelWithHeader("AI ASSISTANT", aiInput));

            errorLog = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 20, 20), ForeColor = C_Error, BorderStyle = BorderStyle.None, Font = new Font("Consolas", 9), ReadOnly = true, Text = "No Issues Found." };
            splitColRight.Panel2.Controls.Add(CreatePanelWithHeader("DIAGNOSTICS", errorLog));

            this.Tag = new object[] { splitMain, splitWorkspace, splitColLeft, splitColCenter, splitColRight };
        }

        private void HighlightErrorInEditor(string errorMsg)
        {
            RichTextBox rtb = editorTabs.SelectedTab?.Controls.OfType<RichTextBox>().FirstOrDefault(c => c.Dock == DockStyle.Fill);
            if (rtb == null) return;

            string targetWord = "";
            if (errorMsg.Contains("'"))
            {
                int start = errorMsg.IndexOf("'") + 1;
                int end = errorMsg.LastIndexOf("'");
                if (end > start) targetWord = errorMsg.Substring(start, end - start);
            }
            else if (errorMsg.Contains("wea_"))
            {
                var match = Regex.Match(errorMsg, @"wea_\w+");
                if (match.Success) targetWord = match.Value;
            }

            if (!string.IsNullOrEmpty(targetWord))
            {
                int index = rtb.Text.IndexOf(targetWord);
                if (index != -1)
                {
                    rtb.Select(index, targetWord.Length);
                    rtb.SelectionColor = Color.White;
                    rtb.SelectionBackColor = Color.Red;
                    rtb.ScrollToCaret();
                }
            }
        }

        private void UpdateVariableWatcher(string code)
        {
            variableWatcher.Nodes.Clear();
            TreeNode rootVar = variableWatcher.Nodes.Add("Global Scope");
            MatchCollection matches = Regex.Matches(code, @"wea_unit\s+(\w+)\s*=\s*([^;]+)");
            foreach (Match m in matches) rootVar.Nodes.Add($"{m.Groups[1].Value}: {m.Groups[2].Value.Trim()}").ForeColor = Color.LightGreen;
            rootVar.Expand();
        }

        private void DrawDynamicPlot(string codeContext)
        {
            Bitmap bmp = new Bitmap(plotterCanvas.Width, plotterCanvas.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(20, 20, 20)); g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen gridPen = new Pen(Color.FromArgb(50, 50, 50));
                int w = plotterCanvas.Width; int h = plotterCanvas.Height; int midH = h / 2;
                for (int i = 0; i < w; i += 50) g.DrawLine(gridPen, i, 0, i, h);
                for (int i = 0; i < h; i += 50) g.DrawLine(gridPen, 0, i, w, i);

                List<PointF> points = new List<PointF>();
                for (int x = 0; x < w; x += 4) points.Add(new PointF(x, midH + (float)(Math.Sin(x * 0.05) * 100)));
                if (points.Count > 1) g.DrawLines(new Pen(Color.Cyan, 2), points.ToArray());
            }
            plotterCanvas.Image = bmp;
        }

        private void OpenFileInTab(string f)
        {
            string p = Path.Combine(ProjectPath, f);
            if (!File.Exists(p)) return;

            if (welcomePanel.Visible) { welcomePanel.Visible = false; editorTabs.Visible = true; }

            foreach (TabPage pg in editorTabs.TabPages) if (pg.Text == f) { editorTabs.SelectedTab = pg; return; }
            TabPage np = new TabPage(f) { BackColor = C_Back };
            RichTextBox n = new RichTextBox { Dock = DockStyle.Left, Width = 45, BackColor = C_Back, ForeColor = Color.FromArgb(100, 100, 100), BorderStyle = BorderStyle.None, ScrollBars = RichTextBoxScrollBars.None, ReadOnly = true, Text = "1\n" };
            RichTextBox e = new RichTextBox { Dock = DockStyle.Fill, BackColor = C_Back, ForeColor = C_Text, Font = new Font("Consolas", 12), BorderStyle = BorderStyle.None, WordWrap = false, AcceptsTab = true, Text = File.ReadAllText(p) };
            e.VScroll += (s, ev) => SyncScroll(e, n);
            e.TextChanged += (s, ev) => { UpdateLineNumbers(e, n); HighlightSyntax(e); };
            e.KeyUp += (s, ev) => { if (!IsMoveKey(ev.KeyCode)) CheckIntelliSense(e); };
            e.KeyDown += (s, ev) => HandleIntelliKey(ev, e);
            e.Click += (s, ev) => suggestionBox.Visible = false;
            np.Controls.Add(e); np.Controls.Add(n);
            editorTabs.TabPages.Add(np);
            editorTabs.SelectedTab = np;
            UpdateLineNumbers(e, n);
            HighlightSyntax(e);
        }

        private void HighlightSyntax(RichTextBox rtb) { SendMessage(rtb.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero); int s = rtb.SelectionStart; int l = rtb.SelectionLength; rtb.SelectAll(); rtb.SelectionColor = C_Text; rtb.SelectionBackColor = C_Back; ApplyColor(rtb, @"""[^""\\]*(?:\\.[^""\\]*)*""", Syntax_String); ApplyColor(rtb, @"\b(wea_unit|wea_flow|wea_cycle|wea_verify|wea_return|wea_if)\b", Syntax_Keyword); ApplyColor(rtb, @"\b(wea_emit|wea_read|wea_plot|clock|sqrt|print|bio_\w+|gen_\w+|eco_\w+|physio_\w+|plant_\w+|chem_\w+|phys_\w+|math_\w+|core_\w+|nuc_\w+)\b", Syntax_Function); ApplyColor(rtb, @"\b\d+(\.\d+)?\b", Syntax_Number); ApplyColor(rtb, @"//.*$", Syntax_Comment); rtb.Select(s, l); rtb.SelectionColor = C_Text; rtb.SelectionBackColor = C_Back; SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero); }
        private void ApplyColor(RichTextBox r, string p, Color c) { foreach (Match m in Regex.Matches(r.Text, p, RegexOptions.Multiline)) { r.Select(m.Index, m.Length); r.SelectionColor = c; r.SelectionBackColor = C_Back; } }
        private void UpdateLineNumbers(RichTextBox r, RichTextBox n) { int l = r.GetLineFromCharIndex(r.TextLength) + 1; string t = ""; for (int i = 1; i <= l; i++) t += i + "\n"; n.Text = t; }
        private void SyncScroll(RichTextBox r, RichTextBox n) { int p = GetScrollPos(r.Handle, 1); p <<= 16; SendMessage(n.Handle, 0x0115, (IntPtr)((uint)4 | (uint)p), IntPtr.Zero); }
        private void SetupIntelliSense() { suggestionBox = new ListBox { Parent = this, Visible = false, BackColor = Color.FromArgb(37, 37, 38), ForeColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 10), Size = new Size(200, 100), DrawMode = DrawMode.OwnerDrawFixed }; suggestionBox.DrawItem += (s, e) => { if (e.Index < 0) return; bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected; e.Graphics.FillRectangle(new SolidBrush(sel ? C_Accent : Color.FromArgb(37, 37, 38)), e.Bounds); e.Graphics.DrawString(suggestionBox.Items[e.Index].ToString(), e.Font, Brushes.White, e.Bounds.X + 2, e.Bounds.Y + 2); }; suggestionBox.DoubleClick += (s, e) => ApplySuggestion(); suggestionBox.BringToFront(); }
        private void CheckIntelliSense(RichTextBox rtb) { int c = rtb.SelectionStart; int st = c - 1; while (st >= 0 && (char.IsLetterOrDigit(rtb.Text[st]) || rtb.Text[st] == '_')) st--; st++; string w = rtb.Text.Substring(st, c - st); if (w.Length < 1) { suggestionBox.Visible = false; return; } var ms = Keywords.Where(k => k.StartsWith(w)).ToArray(); if (ms.Length > 0) { suggestionBox.Items.Clear(); suggestionBox.Items.AddRange(ms); suggestionBox.SelectedIndex = 0; Point pt = this.PointToClient(rtb.PointToScreen(rtb.GetPositionFromCharIndex(c))); suggestionBox.Location = new Point(pt.X, pt.Y + 25); suggestionBox.Visible = true; suggestionBox.BringToFront(); rtb.Focus(); } else suggestionBox.Visible = false; }
        private void ApplySuggestion() { if (suggestionBox.SelectedItem == null || editorTabs.SelectedTab == null) return; RichTextBox rtb = editorTabs.SelectedTab.Controls.OfType<RichTextBox>().FirstOrDefault(); if (rtb == null) return; string s = suggestionBox.SelectedItem.ToString(); int c = rtb.SelectionStart; int st = c - 1; while (st >= 0 && (char.IsLetterOrDigit(rtb.Text[st]) || rtb.Text[st] == '_')) st--; st++; rtb.Select(st, c - st); rtb.SelectedText = s; suggestionBox.Visible = false; }
        private bool IsMoveKey(Keys k) => k == Keys.Down || k == Keys.Up || k == Keys.Enter || k == Keys.Tab || k == Keys.Escape;
        private void HandleIntelliKey(KeyEventArgs e, RichTextBox r) { if (suggestionBox.Visible) { if (e.KeyCode == Keys.Down) { if (suggestionBox.SelectedIndex < suggestionBox.Items.Count - 1) suggestionBox.SelectedIndex++; e.Handled = true; } else if (e.KeyCode == Keys.Up) { if (suggestionBox.SelectedIndex > 0) suggestionBox.SelectedIndex--; e.Handled = true; } else if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter) { ApplySuggestion(); e.Handled = true; e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.Escape) suggestionBox.Visible = false; } }
        private void AddNewFile() { string n = Prompt.ShowDialog("Dosya Adı:", "Yeni Dosya"); if (!string.IsNullOrWhiteSpace(n)) { if (!n.EndsWith(".we")) n += ".we"; string p = Path.Combine(ProjectPath, n); if (!File.Exists(p)) { File.WriteAllText(p, "// WSharp Science\n\"C6H12O6\" |> chem_mass() |> print()\n"); RefreshExplorer(); OpenFileInTab(n); } } }
        private void SaveCurrentFile() { if (editorTabs.SelectedTab != null) { var r = editorTabs.SelectedTab.Controls.OfType<RichTextBox>().FirstOrDefault(); if (r != null) { File.WriteAllText(Path.Combine(ProjectPath, editorTabs.SelectedTab.Text), r.Text); } } }
        private void RefreshExplorer() { fileExplorer.Nodes.Clear(); TreeNode r = fileExplorer.Nodes.Add("MyProject"); foreach (string f in Directory.GetFiles(ProjectPath, "*.we")) r.Nodes.Add(Path.GetFileName(f)); r.Expand(); }

        private void SetupMenuBar()
        {
            MenuStrip ms = new MenuStrip { Dock = DockStyle.Top, BackColor = C_SideBar, ForeColor = C_Text, Renderer = new DarkMenuRenderer() };
            var miniLogo = new ToolStripMenuItem("(#)") { ForeColor = C_Accent, Font = new Font("Consolas", 12, FontStyle.Bold) };
            ms.Items.Add(miniLogo);
            var f = new ToolStripMenuItem("File");
            f.DropDownItems.Add("New File", null, (s, e) => AddNewFile());
            f.DropDownItems.Add("Save All", null, (s, e) => SaveCurrentFile());
            ms.Items.Add(f);
            ms.Items.Add(new ToolStripMenuItem("Edit"));
            ms.Items.Add(new ToolStripMenuItem("View"));
            ToolStripMenuItem r = new ToolStripMenuItem("▶ RUN");
            r.Alignment = ToolStripItemAlignment.Right;
            r.ForeColor = Color.LightGreen;
            r.Click += (s, e) => RunCode();
            ms.Items.Add(r);
            this.Controls.Add(ms);
        }

        private void SetupStatusBar() { Panel p = new Panel { Dock = DockStyle.Bottom, Height = 24, BackColor = C_Accent }; p.Controls.Add(new Label { Text = "WSharp Engine Active", AutoSize = true, ForeColor = Color.White, Location = new Point(5, 4) }); this.Controls.Add(p); }

        private void GlobalShortcuts(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5) RunCode();
            if (e.Control && e.KeyCode == Keys.S) SaveCurrentFile();
            if (e.Control && e.KeyCode == Keys.N) AddNewFile();
        }

        private SplitContainer CreateSplitter(Orientation o) => new SplitContainer { Dock = DockStyle.Fill, Orientation = o, BackColor = C_Border, SplitterWidth = 2 };
        private TreeView CreateTree() => new TreeView { Dock = DockStyle.Fill, BackColor = C_SideBar, ForeColor = C_Text, BorderStyle = BorderStyle.None, ShowLines = false };
        private Panel CreatePanelWithHeader(string t, Control c) { Panel p = new Panel { Dock = DockStyle.Fill, BackColor = C_SideBar }; Label l = new Label { Text = t, Dock = DockStyle.Top, Height = 22, BackColor = C_SideBar, ForeColor = Color.Gray, Font = new Font("Segoe UI", 7, FontStyle.Bold), Padding = new Padding(5, 4, 0, 0) }; c.Dock = DockStyle.Fill; p.Controls.Add(c); p.Controls.Add(l); return p; }
        private void SetupContextMenu(Control c) { ContextMenuStrip cm = new ContextMenuStrip(); cm.Items.Add("Yeni Dosya", null, (s, e) => AddNewFile()); cm.Items.Add("Yenile", null, (s, e) => RefreshExplorer()); c.ContextMenuStrip = cm; }
        private void ApplyLayoutRatios() { if (this.Tag is object[] splits) { int w = this.ClientSize.Width; int h = this.ClientSize.Height - 50; ((SplitContainer)splits[0]).SplitterDistance = (int)(w * 0.20); ((SplitContainer)splits[1]).SplitterDistance = (int)((w * 0.80) * 0.75); int sp = (int)(h * 0.70); ((SplitContainer)splits[2]).SplitterDistance = sp; ((SplitContainer)splits[3]).SplitterDistance = sp; ((SplitContainer)splits[4]).SplitterDistance = sp; } }
        private void SyncHorizontalLines() { if (this.Tag is object[] splits) { int dist = ((SplitContainer)splits[3]).SplitterDistance; ((SplitContainer)splits[2]).SplitterDistance = dist; ((SplitContainer)splits[4]).SplitterDistance = dist; } }
    }

    public class DarkMenuRenderer : ToolStripProfessionalRenderer { public DarkMenuRenderer() : base(new DarkColors()) { } }
    public class DarkColors : ProfessionalColorTable { public override Color MenuItemSelected => Color.FromArgb(60, 60, 60); public override Color MenuItemBorder => Color.FromArgb(60, 60, 60); public override Color MenuBorder => Color.FromArgb(30, 30, 30); public override Color ToolStripDropDownBackground => Color.FromArgb(37, 37, 38); }
    public static class Prompt { public static string ShowDialog(string t, string c) { Form p = new Form() { Width = 300, Height = 140, Text = c, StartPosition = FormStartPosition.CenterScreen, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog }; TextBox tb = new TextBox() { Left = 10, Top = 35, Width = 260, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White }; Button b = new Button() { Text = "OK", Left = 190, Top = 70, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White }; p.Controls.Add(new Label() { Left = 10, Top = 10, Text = t, AutoSize = true }); p.Controls.Add(tb); p.Controls.Add(b); p.AcceptButton = b; return p.ShowDialog() == DialogResult.OK ? tb.Text : ""; } }
}
