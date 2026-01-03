#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace WSharp
{
    public class DrawLib : ILibrary
    {
        private static Form _window;
        private static Bitmap _canvas;
        private static Graphics _graphics;
        private static PictureBox _pictureBox;

        public Dictionary<string, Func<List<object>, object>> GetFunctions()
        {
            return new Dictionary<string, Func<List<object>, object>>
            {
                { "win_open", args => {
                    try {
                        int w = int.Parse(args[0].ToString());
                        int h = int.Parse(args[1].ToString());
                        string title = args[2].ToString();

                        if (_window != null) return false;

                        Thread t = new Thread(() => {
                            // DEBUG: Eğer bu yazı terminale düşerse kod buraya kadar gelmiş demektir.
                            Console.WriteLine($"\n[SISTEM] Pencere Hazirlaniyor: {w}x{h}...");

                            _window = new Form {
                                Width = w,
                                Height = h,
                                Text = title,
                                StartPosition = FormStartPosition.CenterScreen,
                                FormBorderStyle = FormBorderStyle.FixedSingle,
                                TopMost = true, // Her zaman en üstte tutar
                                Visible = true  // Görünürlüğü garanti eder
                            };

                            _pictureBox = new PictureBox {
                                Dock = DockStyle.Fill,
                                BackColor = Color.White
                            };

                            _canvas = new Bitmap(w, h);
                            _graphics = Graphics.FromImage(_canvas);
                            _graphics.Clear(Color.White);
                            _pictureBox.Image = _canvas;
                            _window.Controls.Add(_pictureBox);

                            _window.FormClosed += (s, e) => {
                                _window = null;
                                _graphics = null;
                                _canvas?.Dispose();
                                Console.WriteLine("[SISTEM] Grafik Penceresi Kapatildi.");
                            };

                            Application.EnableVisualStyles();
                            // Show() yerine Run() kullanarak thread'i canlı tutuyoruz
                            Application.Run(_window);
                        });

                        t.SetApartmentState(ApartmentState.STA);
                        t.Start();
                        return true;
                    } catch (Exception ex) {
                        Console.WriteLine("Grafik Hatasi: " + ex.Message);
                        return false;
                    }
                }},

                { "win_clear", args => {
                    if (_graphics == null || _pictureBox == null) return false;
                    _pictureBox.Invoke((MethodInvoker)delegate {
                        _graphics.Clear(Color.FromName(args[0].ToString()));
                        _pictureBox.Invalidate();
                    });
                    return true;
                }},

                { "draw_circle", args => {
                    if (_graphics == null || _pictureBox == null) return false;
                    _pictureBox.Invoke((MethodInvoker)delegate {
                        int x = int.Parse(args[0].ToString());
                        int y = int.Parse(args[1].ToString());
                        int r = int.Parse(args[2].ToString());
                        Color c = Color.FromName(args[3].ToString());
                        using (Brush b = new SolidBrush(c)) {
                            _graphics.FillEllipse(b, x, y, r, r);
                        }
                        _pictureBox.Invalidate();
                    });
                    return true;
                }}
            };
        }
    }
}
