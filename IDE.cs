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

     
        private readonly Color Syntax_Keyword = Color.Yellow;
        private readonly Color Syntax_Function = Color.Orange;
        private readonly Color Syntax_String = Color.White;
        private readonly Color Syntax_Comment = Color.DodgerBlue;
        private readonly Color Syntax_Number = Color.LightGreen;

      
        private readonly string[] Keywords = {
            "wea_unit", "wea_if", "wea_flow", "wea_cycle", "wea_verify", "wea_return",
            "wea_emit", "wea_read", "wea_plot", "wea_math_sin", "wea_math_cos", "wea_wait"
        };

        private readonly Dictionary<string, string> AiKnowledgeBase = new Dictionary<string, string>()
        {
            { "döngü", "WSharp Döngü Yapısı:\n\nwea_cycle (kosul) {\n    wea_emit(\"Dönüyor...\")\n}" },
            { "loop", "WSharp Loop:\n\nwea_cycle (i < 10) {\n    i = i + 1\n}" },
            { "grafik", "Grafik Çizimi:\n\nwea_plot(x, y) komutunu kullanarak 'Scientific Plotter' sekmesine veri gönderebilirsin." },
            { "plot", "Plotting:\nUse wea_plot(value) to visualize data in the bottom panel." },
            { "değişken", "Değişken Tanımlama:\n\nwea_unit x = 10\nwea_unit isim = \"WSharp\"" },
            { "variable", "Variable Declaration:\n\nwea_unit myVar = 100" },
            { "merhaba", "Merhaba! Ben WSharp Yerel Asistanıyım. Kodlama konusunda sana nasıl yardımcı olabilirim?" },
            { "selam", "Selamlar! Kod yazmaya hazır mısın?" }
        };

    
        private TabControl editorTabs;
        private TabControl bottomTabs;
        private RichTextBox terminalArea;
        private PictureBox plotterCanvas;
        private TreeView fileExplorer;
        private TreeView variableWatcher;
        private RichTextBox aiInput;
        private RichTextBox aiOutput;
        private ListBox suggestionBox;

        
        private string ProjectPath;

        
        [DllImport("user32.dll")] static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] static extern int GetScrollPos(IntPtr hWnd, int nBar);
        private const int WM_SETREDRAW = 0x000B;

        public IDE()
        {
            ProjectPath = Path.Combine(Application.StartupPath, "MyProject");
            if (!Directory.Exists(ProjectPath)) Directory.CreateDirectory(ProjectPath);

            this.Text = "WSharp Studio - Ultimate Scientific Edition";
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
            SetupAiLogic(); 
            RefreshExplorer();
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
            splitColLeft.Panel1.Controls.Add(CreatePanelWithHeader("DOSYA GEZGİNİ", fileExplorer));

            
            variableWatcher = CreateTree();
            variableWatcher.Nodes.Add(new TreeNode("Bellek Boş") { ImageKey = "mem" });
            splitColLeft.Panel2.Controls.Add(CreatePanelWithHeader("CANLI DEĞİŞKENLER (WATCH)", variableWatcher));

            
            SplitContainer splitColCenter = CreateSplitter(Orientation.Horizontal);
            splitWorkspace.Panel1.Controls.Add(splitColCenter);

            editorTabs = new TabControl { Dock = DockStyle.Fill, Appearance = TabAppearance.FlatButtons, Padding = new Point(15, 5) };
            splitColCenter.Panel1.Controls.Add(editorTabs);

            bottomTabs = new TabControl { Dock = DockStyle.Fill, Alignment = TabAlignment.Bottom };
            TabPage termPage = new TabPage("Terminal") { BackColor = Color.FromArgb(12, 12, 12) };
            terminalArea = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(12, 12, 12), ForeColor = Color.LightGray, Font = new Font("Consolas", 10), BorderStyle = BorderStyle.None, ReadOnly = true };
            termPage.Controls.Add(terminalArea);
            bottomTabs.TabPages.Add(termPage);

            TabPage plotPage = new TabPage("Scientific Plotter") { BackColor = Color.FromArgb(20, 20, 20) };
            plotterCanvas = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 20) };
            plotPage.Controls.Add(plotterCanvas);
            bottomTabs.TabPages.Add(plotPage);

            splitColCenter.Panel2.Controls.Add(CreatePanelWithHeader("ÇIKTI & GRAFİK", bottomTabs));

           
            SplitContainer splitColRight = CreateSplitter(Orientation.Horizontal);
            splitWorkspace.Panel2.Controls.Add(splitColRight);

            aiInput = new RichTextBox { Dock = DockStyle.Fill, BackColor = C_SideBar, ForeColor = C_Text, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10), Text = "" }; // Boş başlasın
            splitColRight.Panel1.Controls.Add(CreatePanelWithHeader("AI GİRDİ (ENTER)", aiInput));

            aiOutput = new RichTextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 25, 26), ForeColor = Color.Cyan, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9), ReadOnly = true, Text = "WSharp AI v1.0 Çevrimiçi...\nSorularınızı buraya yazabilirsiniz." };
            splitColRight.Panel2.Controls.Add(CreatePanelWithHeader("AI CEVAP", aiOutput));

            this.Tag = new object[] { splitMain, splitWorkspace, splitColLeft, splitColCenter, splitColRight };
        }

       
        private void SetupAiLogic()
        {
            aiInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true; 
                    string question = aiInput.Text.Trim().ToLower();
                    if (string.IsNullOrEmpty(question)) return;

                    aiOutput.AppendText($"\n\nSiz: {aiInput.Text}");
                    aiInput.Clear();

                    
                    string response = "Üzgünüm, bu konuda henüz bilgim yok. 'döngü', 'grafik' veya 'değişken' yazmayı dene.";
                    foreach (var key in AiKnowledgeBase.Keys)
                    {
                        if (question.Contains(key))
                        {
                            response = AiKnowledgeBase[key];
                            break;
                        }
                    }

                    
                    Thread t = new Thread(() =>
                    {
                        Thread.Sleep(500); 
                        this.Invoke((MethodInvoker)(() =>
                        {
                            aiOutput.SelectionColor = Color.LimeGreen;
                            aiOutput.AppendText($"\nAI: ");
                            aiOutput.SelectionColor = C_Text;
                            aiOutput.AppendText(response);
                            aiOutput.ScrollToCaret();
                        }));
                    });
                    t.IsBackground = true; t.Start();
                }
            };
        }

      
        private void DrawDynamicPlot(string codeContext)
        {
            Bitmap bmp = new Bitmap(plotterCanvas.Width, plotterCanvas.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(20, 20, 20));
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen gridPen = new Pen(Color.FromArgb(50, 50, 50));

                int w = plotterCanvas.Width; int h = plotterCanvas.Height; int midH = h / 2;
                for (int i = 0; i < w; i += 50) g.DrawLine(gridPen, i, 0, i, h);
                for (int i = 0; i < h; i += 50) g.DrawLine(gridPen, 0, i, w, i);

                List<PointF> points = new List<PointF>();
                Pen signalPen;

               
                if (codeContext.Contains("wea_neuro") || codeContext.Contains("spike"))
                {
                   
                    signalPen = new Pen(Color.Cyan, 2);
                    Random rnd = new Random();
                    float v = midH;
                    for (int x = 0; x < w; x += 2)
                    {
                       
                        if (x % 150 > 130) v -= 30; 
                        else v += (midH - v) * 0.1f; 

                     
                        v += rnd.Next(-2, 3);
                        points.Add(new PointF(x, v));
                    }
                    g.DrawString("WSharp Plot: Neuron Membrane Potential (mV)", new Font("Consolas", 10), Brushes.Cyan, 10, 10);
                }
                else
                {
                    
                    signalPen = new Pen(Color.Orange, 2);
                    for (int x = 0; x < w; x += 4) points.Add(new PointF(x, midH + (float)(Math.Sin(x * 0.05) * 100)));
                    g.DrawString("WSharp Plot: Standard Sine Wave", new Font("Consolas", 10), Brushes.Orange, 10, 10);
                }

                if (points.Count > 1) g.DrawLines(signalPen, points.ToArray());
            }
            plotterCanvas.Image = bmp;
            bottomTabs.SelectedTab = bottomTabs.TabPages[1];
        }

        
        private void RunCode()
        {
            SaveCurrentFile();
            terminalArea.Clear(); terminalArea.AppendText("> Compiling...\n");

            RichTextBox rtb = editorTabs.SelectedTab?.Controls.OfType<RichTextBox>().FirstOrDefault(c => c.Dock == DockStyle.Fill);
            string code = rtb != null ? rtb.Text : "";

            
            variableWatcher.Nodes.Clear();
            TreeNode rootVar = variableWatcher.Nodes.Add("Aktif Değişkenler");

           
            MatchCollection matches = Regex.Matches(code, @"wea_unit\s+(\w+)\s*=\s*([^;]+)");
            foreach (Match m in matches)
            {
                
                TreeNode node = rootVar.Nodes.Add($"{m.Groups[1].Value}: {m.Groups[2].Value.Trim()}");
                node.NodeFont = new Font("Consolas", 9, FontStyle.Bold);
                node.ForeColor = Color.LightGreen;
            }
            if (rootVar.Nodes.Count == 0) rootVar.Nodes.Add("Değişken bulunamadı.");
            rootVar.Expand();

           
            if (code.Contains("wea_plot"))
            {
                terminalArea.AppendText("> Visualizing Data...\n");
                DrawDynamicPlot(code); 
            }

           
            Thread t = new Thread(() => {
                Thread.Sleep(400);
                this.Invoke((MethodInvoker)(() => terminalArea.AppendText("> Execution Finished. (Exit Code 0)\n")));
            });
            t.IsBackground = true; t.Start();
        }

        
        private void SetupIntelliSense()
        {
            suggestionBox = new ListBox { Parent = this, Visible = false, BackColor = Color.FromArgb(37, 37, 38), ForeColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 10), Size = new Size(200, 100), DrawMode = DrawMode.OwnerDrawFixed };
            suggestionBox.DrawItem += (s, e) => {
                if (e.Index < 0) return;
                bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                e.Graphics.FillRectangle(new SolidBrush(sel ? C_Accent : Color.FromArgb(37, 37, 38)), e.Bounds);
                e.Graphics.DrawString(suggestionBox.Items[e.Index].ToString(), e.Font, Brushes.White, e.Bounds.X + 2, e.Bounds.Y + 2);
            };
            suggestionBox.DoubleClick += (s, e) => ApplySuggestion();
            suggestionBox.BringToFront();
        }

        private void CheckIntelliSense(RichTextBox rtb)
        {
            int caret = rtb.SelectionStart;
            int start = caret - 1;
            while (start >= 0 && (char.IsLetterOrDigit(rtb.Text[start]) || rtb.Text[start] == '_')) start--;
            start++;
            string word = rtb.Text.Substring(start, caret - start);
            if (word.Length < 1) { suggestionBox.Visible = false; return; }

            var ms = Keywords.Where(k => k.StartsWith(word)).ToArray();
            if (ms.Length > 0)
            {
                suggestionBox.Items.Clear(); suggestionBox.Items.AddRange(ms); suggestionBox.SelectedIndex = 0;
                Point pt = this.PointToClient(rtb.PointToScreen(rtb.GetPositionFromCharIndex(caret)));
                suggestionBox.Location = new Point(pt.X, pt.Y + 25);
                suggestionBox.Visible = true; suggestionBox.BringToFront(); rtb.Focus();
            }
            else suggestionBox.Visible = false;
        }

        private void ApplySuggestion()
        {
            if (suggestionBox.SelectedItem == null || editorTabs.SelectedTab == null) return;
            RichTextBox rtb = editorTabs.SelectedTab.Controls.OfType<RichTextBox>().FirstOrDefault();
            if (rtb == null) return;
            string sel = suggestionBox.SelectedItem.ToString();
            int caret = rtb.SelectionStart;
            int start = caret - 1;
            while (start >= 0 && (char.IsLetterOrDigit(rtb.Text[start]) || rtb.Text[start] == '_')) start--;
            start++;
            rtb.Select(start, caret - start); rtb.SelectedText = sel; suggestionBox.Visible = false;
        }

      
        private void OpenFileInTab(string f)
        {
            string p = Path.Combine(ProjectPath, f); if (!File.Exists(p)) return;
            foreach (TabPage pg in editorTabs.TabPages) if (pg.Text == f) { editorTabs.SelectedTab = pg; return; }
            TabPage np = new TabPage(f) { BackColor = C_Back };
            RichTextBox n = new RichTextBox { Dock = DockStyle.Left, Width = 45, BackColor = C_Back, ForeColor = Color.Gray, BorderStyle = BorderStyle.None, ScrollBars = RichTextBoxScrollBars.None, ReadOnly = true, Text = "1\n" };
            RichTextBox e = new RichTextBox { Dock = DockStyle.Fill, BackColor = C_Back, ForeColor = C_Text, Font = new Font("Consolas", 12), BorderStyle = BorderStyle.None, WordWrap = false, AcceptsTab = true, Text = File.ReadAllText(p) };

            e.VScroll += (s, ev) => SyncScroll(e, n);
            e.TextChanged += (s, ev) => { UpdateLineNumbers(e, n); HighlightSyntax(e); };
            e.KeyUp += (s, ev) => { if (!IsMoveKey(ev.KeyCode)) CheckIntelliSense(e); };
            e.KeyDown += (s, ev) => HandleIntelliKey(ev, e);
            e.Click += (s, ev) => suggestionBox.Visible = false;

            np.Controls.Add(e); np.Controls.Add(n); editorTabs.TabPages.Add(np); editorTabs.SelectedTab = np;
            UpdateLineNumbers(e, n); HighlightSyntax(e);
        }

        private void HighlightSyntax(RichTextBox rtb)
        {
            SendMessage(rtb.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            int start = rtb.SelectionStart; int len = rtb.SelectionLength;
            rtb.SelectAll(); rtb.SelectionColor = C_Text; rtb.SelectionBackColor = C_Back;
            ApplyColor(rtb, @"""[^""\\]*(?:\\.[^""\\]*)*""", Syntax_String);
            ApplyColor(rtb, @"\b(wea_unit|wea_flow|wea_cycle|wea_verify|wea_return|wea_if)\b", Syntax_Keyword);
            ApplyColor(rtb, @"\b(wea_emit|wea_read|wea_plot)\b", Syntax_Function);
            ApplyColor(rtb, @"\b\d+(\.\d+)?\b", Syntax_Number);
            ApplyColor(rtb, @"//.*$", Syntax_Comment);
            rtb.Select(start, len); rtb.SelectionColor = C_Text; rtb.SelectionBackColor = C_Back;
            SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
        }

        private void ApplyColor(RichTextBox r, string p, Color c) { foreach (Match m in Regex.Matches(r.Text, p, RegexOptions.Multiline)) { r.Select(m.Index, m.Length); r.SelectionColor = c; r.SelectionBackColor = C_Back; } }
        private void UpdateLineNumbers(RichTextBox r, RichTextBox n) { int l = r.GetLineFromCharIndex(r.TextLength) + 1; string t = ""; for (int i = 1; i <= l; i++) t += i + "\n"; n.Text = t; }
        private void SyncScroll(RichTextBox r, RichTextBox n) { int p = GetScrollPos(r.Handle, 1); p <<= 16; SendMessage(n.Handle, 0x0115, (IntPtr)((uint)4 | (uint)p), IntPtr.Zero); }
        private bool IsMoveKey(Keys k) => k == Keys.Down || k == Keys.Up || k == Keys.Enter || k == Keys.Tab || k == Keys.Escape;
        private void HandleIntelliKey(KeyEventArgs e, RichTextBox r) { if (suggestionBox.Visible) { if (e.KeyCode == Keys.Down) { if (suggestionBox.SelectedIndex < suggestionBox.Items.Count - 1) suggestionBox.SelectedIndex++; e.Handled = true; } else if (e.KeyCode == Keys.Up) { if (suggestionBox.SelectedIndex > 0) suggestionBox.SelectedIndex--; e.Handled = true; } else if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter) { ApplySuggestion(); e.Handled = true; e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.Escape) suggestionBox.Visible = false; } }

        private void AddNewFile() { string n = Prompt.ShowDialog("Dosya Adı:", "Yeni Dosya"); if (!string.IsNullOrWhiteSpace(n)) { if (!n.EndsWith(".we")) n += ".we"; string p = Path.Combine(ProjectPath, n); if (!File.Exists(p)) { File.WriteAllText(p, "// WSharp\nwea_unit V = -65\nwea_plot(V)"); RefreshExplorer(); OpenFileInTab(n); } } }
        private void SaveCurrentFile() { if (editorTabs.SelectedTab != null) { var r = editorTabs.SelectedTab.Controls.OfType<RichTextBox>().FirstOrDefault(); if (r != null) { File.WriteAllText(Path.Combine(ProjectPath, editorTabs.SelectedTab.Text), r.Text); terminalArea.AppendText($"> Saved: {editorTabs.SelectedTab.Text}\n"); } } }
        private void RefreshExplorer() { fileExplorer.Nodes.Clear(); TreeNode r = fileExplorer.Nodes.Add("MyProject"); foreach (string f in Directory.GetFiles(ProjectPath, "*.we")) r.Nodes.Add(Path.GetFileName(f)); r.Expand(); }
        private void SetupMenuBar() { MenuStrip ms = new MenuStrip { Dock = DockStyle.Top, BackColor = C_SideBar, ForeColor = C_Text, Renderer = new DarkMenuRenderer() }; var f = new ToolStripMenuItem("File"); f.DropDownItems.Add("New File", null, (s, e) => AddNewFile()); f.DropDownItems.Add("Save All", null, (s, e) => SaveCurrentFile()); ms.Items.Add(f); ms.Items.Add(new ToolStripMenuItem("Edit")); ms.Items.Add(new ToolStripMenuItem("View")); ToolStripMenuItem r = new ToolStripMenuItem("▶ RUN"); r.Alignment = ToolStripItemAlignment.Right; r.ForeColor = Color.LightGreen; r.Click += (s, e) => RunCode(); ms.Items.Add(r); this.Controls.Add(ms); }
        private void SetupStatusBar() { Panel p = new Panel { Dock = DockStyle.Bottom, Height = 24, BackColor = C_Accent }; p.Controls.Add(new Label { Text = "WSharp Ready", AutoSize = true, ForeColor = Color.White, Location = new Point(5, 4) }); this.Controls.Add(p); }
        private void GlobalShortcuts(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.F5) RunCode(); if (e.Control && e.KeyCode == Keys.S) SaveCurrentFile(); }
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
