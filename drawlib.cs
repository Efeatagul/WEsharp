#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;

namespace WSharp
{
    
    public class DrawLib : ILibrary
    {
        private static Form _window;
        private static Bitmap _canvas;
        private static Graphics _graphics;
        private static PictureBox _pictureBox;
        private static readonly object _lock = new object();

        public Dictionary<string, Func<List<object>, object>> GetFunctions()
        {
            return new Dictionary<string, Func<List<object>, object>>
            {
                // 1. Pencere Yönetimi
                { "wea_view_init", args => {
                    if (_window != null) return false;

                    int w = args.Count > 0 ? Convert.ToInt32(args[0]) : 800;
                    int h = args.Count > 1 ? Convert.ToInt32(args[1]) : 600;
                    string title = args.Count > 2 ? args[2].ToString() : "WEA Visual Engine";

                    Thread t = new Thread(() => StartWindow(w, h, title));
                    t.SetApartmentState(ApartmentState.STA);
                    t.IsBackground = true;
                    t.Start();
                    return true;
                }},

             
                { "wea_view_clear", args => {
                    if (_graphics == null) return false;
                    string colorName = args.Count > 0 ? args[0].ToString() : "Black";
                    Color c = GetValidColor(colorName);

                    lock(_lock) { _graphics.Clear(c); }
                    RefreshDisplay();
                    return true;
                }},

               
                { "wea_draw_circle", args => {
                    if (_graphics == null) return false;
                    try {
                        int x = Convert.ToInt32(args[0]);
                        int y = Convert.ToInt32(args[1]);
                        int r = Convert.ToInt32(args[2]);
                        Color c = args.Count > 3 ? GetValidColor(args[3].ToString()) : Color.White;

                        lock(_lock) {
                            using (Brush b = new SolidBrush(c)) { _graphics.FillEllipse(b, x, y, r, r); }
                        }
                        RefreshDisplay();
                        return true;
                    } catch { return false; }
                }},

              
                { "wea_draw_rect", args => {
                    if (_graphics == null) return false;
                    try {
                        int x = Convert.ToInt32(args[0]);
                        int y = Convert.ToInt32(args[1]);
                        int w = Convert.ToInt32(args[2]);
                        int h = Convert.ToInt32(args[3]);
                        Color c = args.Count > 4 ? GetValidColor(args[4].ToString()) : Color.White;

                        lock(_lock) {
                            using (Brush b = new SolidBrush(c)) { _graphics.FillRectangle(b, x, y, w, h); }
                        }
                        RefreshDisplay();
                        return true;
                    } catch { return false; }
                }}
            };
        }

     
        private static Color GetValidColor(string name)
        {
            Color c = Color.FromName(name);
            return c.IsKnownColor ? c : Color.White;
        }

        private static void StartWindow(int w, int h, string title)
        {
            try
            {
                _window = new Form
                {
                    Width = w,
                    Height = h,
                    Text = title,
                    FormBorderStyle = FormBorderStyle.FixedSingle,
                    StartPosition = FormStartPosition.CenterScreen,
                    BackColor = Color.Black
                };

             
                typeof(Form).InvokeMember("DoubleBuffered",
                    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, _window, new object[] { true });

                _pictureBox = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.Transparent };
                _canvas = new Bitmap(w, h);
                _graphics = Graphics.FromImage(_canvas);
                _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                _pictureBox.Image = _canvas;
                _window.Controls.Add(_pictureBox);

                _window.FormClosed += (s, e) => {
                    lock (_lock) { _window = null; _graphics = null; _canvas?.Dispose(); }
                };

                Application.Run(_window);
            }
            catch { /* Pencere oluşturma hatası yakalama */ }
        }

        private static void RefreshDisplay()
        {
            try
            {
                if (_pictureBox != null && !_pictureBox.IsDisposed && _pictureBox.InvokeRequired)
                {
                    _pictureBox.BeginInvoke(new Action(() => _pictureBox.Invalidate()));
                }
                else _pictureBox?.Invalidate();
            }
            catch { }
        }
    }
}
