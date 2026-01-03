#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Text.Json;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WSharp
{
    public static class StandardLibrary
    {
 
        private static Form _window;
        private static Bitmap _canvas;
        private static Dictionary<Keys, bool> _keyStates = new Dictionary<Keys, bool>();
        private static Point _mousePos;

        public static Dictionary<string, Func<List<object>, object>> Functions =
            new Dictionary<string, Func<List<object>, object>>
        {
        
            { "out", args => {
                if(args.Count > 0) Console.WriteLine(args[0]);
                else Console.WriteLine();
                return null;
            }},

            { "ask", args => {
                if(args.Count > 0) Console.Write(args[0]);
                return Console.ReadLine();
            }},

            { "ask_int", args => {
                while (true) {
                    if(args.Count > 0) Console.Write(args[0]);
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out int result)) return result;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Hata: Lutfen gecerli bir tam sayi girin.");
                    Console.ResetColor();
                }
            }},

            { "clean", args => { Console.Clear(); return null; }},

            { "out_color", args => {
                if(args.Count < 2) return null;
                if(Enum.TryParse(args[1].ToString(), true, out ConsoleColor color))
                    Console.ForegroundColor = color;
                Console.WriteLine(args[0]);
                Console.ResetColor();
                return null;
            }},

            { "beep", args => {
                if (OperatingSystem.IsWindows()) Console.Beep();
                else Console.Write("\a");
                return null;
            }},

            { "wait", args => {
                int ms = args.Count > 0 ? Convert.ToInt32(args[0]) : 1000;
                Thread.Sleep(ms);
                return null;
            }},

            { "exit", args => { Environment.Exit(0); return null; }},

            { "get_input", args => Console.ReadKey(true).KeyChar.ToString() },

      
            { "str_len", args => args[0]?.ToString().Length ?? 0 },
            { "str_upper", args => args[0]?.ToString().ToUpper() },
            { "str_lower", args => args[0]?.ToString().ToLower() },
            { "str_trim", args => args[0]?.ToString().Trim() },
            { "str_rev", args => {
                char[] charArray = args[0].ToString().ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }},
            { "str_contains", args => args[0]?.ToString().Contains(args[1].ToString()) },
            { "str_replace", args => args[0]?.ToString().Replace(args[1].ToString(), args[2].ToString()) },
            { "str_split", args => args[0]?.ToString().Split(args[1].ToString()).Cast<object>().ToList() },
            { "str_sub", args => {
                string s = args[0].ToString();
                int start = Convert.ToInt32(args[1]);
                int len = Convert.ToInt32(args[2]);
                return s.Substring(start, len);
            }},
            { "str_slug", args => {
                string s = args[0].ToString().ToLower();
                s = s.Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
                s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
                s = Regex.Replace(s, @"\s+", "-").Trim();
                return s;
            }},
            { "str_fmt", args => string.Format(args[0].ToString(), args.Skip(1).ToArray()) },

      
            { "dice", args => {
                Random rng = new Random();
                if (args.Count >= 2) return rng.Next(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]));
                return rng.Next(0, 100);
            }},
            { "abs", args => Math.Abs(Convert.ToDouble(args[0])) },
            { "sqrt", args => Math.Sqrt(Convert.ToDouble(args[0])) },
            { "pow", args => Math.Pow(Convert.ToDouble(args[0]), Convert.ToDouble(args[1])) },
            { "round", args => Math.Round(Convert.ToDouble(args[0])) },

          
            { "list_new", args => new List<object>() },
            { "list_add", args => {
                var list = (List<object>)args[0];
                list.Add(args[1]);
                return list;
            }},
            { "list_len", args => ((List<object>)args[0]).Count },
            { "list_get", args => ((List<object>)args[0])[Convert.ToInt32(args[1])] },
            { "range", args => {
                int start = args.Count >= 2 ? Convert.ToInt32(args[0]) : 0;
                int end = args.Count >= 2 ? Convert.ToInt32(args[1]) : Convert.ToInt32(args[0]);
                return Enumerable.Range(start, Math.Max(0, end - start)).Cast<object>().ToList();
            }},
            { "to_json", args => JsonSerializer.Serialize(args[0]) },
            { "parse_json", args => JsonSerializer.Deserialize<object>(args[0].ToString()) },

           
            { "win_open", args => {
                int w = args.Count > 0 ? Convert.ToInt32(args[0]) : 800;
                int h = args.Count > 1 ? Convert.ToInt32(args[1]) : 600;
                string title = args.Count > 2 ? args[2].ToString() : "WSharp Engine";
                InitWindow(w, h, title);
                return null;
            }},

            { "win_clear", args => {
                if (_canvas == null) return null;
                using (Graphics g = Graphics.FromImage(_canvas)) {
                    var color = args.Count > 0 ? Color.FromName(args[0].ToString()) : Color.Black;
                    g.Clear(color);
                }
                _window?.Invalidate();
                return null;
            }},

            { "draw_circle", args => {
                if (_canvas == null) return null;
                using (Graphics g = Graphics.FromImage(_canvas)) {
                    int x = Convert.ToInt32(args[0]), y = Convert.ToInt32(args[1]), r = Convert.ToInt32(args[2]);
                    var color = args.Count > 3 ? Color.FromName(args[3].ToString()) : Color.White;
                    g.FillEllipse(new SolidBrush(color), x, y, r, r);
                }
                _window?.Invalidate();
                return null;
            }},

            { "draw_rect", args => {
                if (_canvas == null) return null;
                using (Graphics g = Graphics.FromImage(_canvas)) {
                    int x = Convert.ToInt32(args[0]), y = Convert.ToInt32(args[1]), w = Convert.ToInt32(args[2]), h = Convert.ToInt32(args[3]);
                    var color = args.Count > 4 ? Color.FromName(args[4].ToString()) : Color.White;
                    g.FillRectangle(new SolidBrush(color), x, y, w, h);
                }
                _window?.Invalidate();
                return null;
            }},

           
            { "is_key", args => {
                if (args.Count == 0) return false;
                if (Enum.TryParse(args[0].ToString(), true, out Keys key))
                    return _keyStates.ContainsKey(key) && _keyStates[key];
                return false;
            }},
            { "mouse_x", args => _mousePos.X },
            { "mouse_y", args => _mousePos.Y },

           
            { "time", args => DateTime.Now.ToString("HH:mm:ss") },
            { "date", args => DateTime.Now.ToString("yyyy-MM-dd") },
            { "we_ver", args => "1.6.0-Full-Stack" }
        };

      
        public static bool Exists(string name) => Functions.ContainsKey(name);
        public static object Call(string name, List<object> args) =>
            Functions.TryGetValue(name, out var func) ? func(args) : null;

        private static void InitWindow(int w, int h, string title)
        {
            if (_window != null) return;
            Thread t = new Thread(() => {
                _window = new Form
                {
                    Width = w,
                    Height = h,
                    Text = title,
                    FormBorderStyle = FormBorderStyle.FixedSingle,
                    StartPosition = FormStartPosition.CenterScreen
                };

               
                typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(_window, true, null);

                _canvas = new Bitmap(w, h);
                _window.KeyDown += (s, e) => _keyStates[e.KeyCode] = true;
                _window.KeyUp += (s, e) => _keyStates[e.KeyCode] = false;
                _window.MouseMove += (s, e) => _mousePos = e.Location;
                _window.Paint += (s, e) => { lock (_canvas) e.Graphics.DrawImage(_canvas, 0, 0); };

                Application.Run(_window);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }
}
