#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WSharp
{
    public interface ILibrary
    {
        Dictionary<string, Func<List<WValue>, WValue>> GetFunctions();
    }

    public static class StandardLibrary
    {
        public static Dictionary<string, Func<List<WValue>, WValue>> Functions = new Dictionary<string, Func<List<WValue>, WValue>>();

        static StandardLibrary()
        {
            Functions["wea_print"] = args => { Console.WriteLine(args.Count > 0 ? args[0].AsString() : ""); return new WNull(); };
            Functions["wea_read"] = args => { if (args.Count > 0) Console.Write(args[0].AsString()); return new WString(Console.ReadLine()); };
            Functions["wea_clean"] = args => { Console.Clear(); return new WNull(); };
            Functions["wea_wait"] = args => { Thread.Sleep(args.Count > 0 ? (int)args[0].AsNumber() : 1000); return new WNull(); };
            Functions["wea_exit"] = args => { System.Environment.Exit(0); return new WNull(); };
            Functions["wea_help"] = args => { Program.ShowHelp(); return new WNull(); };
            Functions["wea_beep"] = args => { Console.Beep(); return new WNull(); };

            Functions["wea_str_len"] = args => new WNumber(args[0].AsString().Length);
            Functions["wea_str_upper"] = args => new WString(args[0].AsString().ToUpper());
            Functions["wea_str_lower"] = args => new WString(args[0].AsString().ToLower());
            Functions["wea_to_str"] = args => new WString(args[0].AsString());
            Functions["wea_to_num"] = args => { return double.TryParse(args[0].AsString(), out double d) ? new WNumber(d) : new WNumber(0); };
        }
    }

    public class InputLib : ILibrary
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
                { "wea_key_down", args => {
                    string key = args[0].AsString().ToUpper();
                    int vKey = (key.Length == 1 && char.IsLetterOrDigit(key[0])) ? (int)key[0] : 0;
                    return new WNumber((GetAsyncKeyState(vKey) & 0x8000) != 0 ? 1 : 0);
                }},
                { "wea_mouse_x", args => new WNumber(Cursor.Position.X) },
                { "wea_mouse_y", args => new WNumber(Cursor.Position.Y) },
                { "wea_mouse_click", args => new WNumber((GetAsyncKeyState(0x01) & 0x8000) != 0 ? 1 : 0) }
            };
        }
    }

    public class DrawLib : ILibrary
    {
        private static Form _window;
        private static Graphics _graphics;
        private static readonly object _lock = new object();

        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
                { "wea_view_init", args => {
                    Thread t = new Thread(() => {
                        _window = new Form { Width = 800, Height = 600, BackColor = Color.Black };
                        _graphics = _window.CreateGraphics();
                        Application.Run(_window);
                    });
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    return new WNumber(1);
                }},
                { "wea_view_clear", args => {
                      if (_graphics != null) lock(_lock) { _graphics.Clear(Color.Black); }
                      return new WNumber(1);
                }},
                { "wea_draw_circle", args => {
                    if (_graphics != null) lock(_lock) { _graphics.FillEllipse(Brushes.White, (int)args[0].AsNumber(), (int)args[1].AsNumber(), (int)args[2].AsNumber(), (int)args[2].AsNumber()); }
                    return new WNumber(1);
                }}
            };
        }
    }

    public class SoundLib : ILibrary
    {
        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions() => new Dictionary<string, Func<List<WValue>, WValue>> {
            { "wea_audio_pulse", args => { Console.Beep(); return new WNumber(1); } }
        };
    }

    public class TimeLib : ILibrary
    {
        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions() => new Dictionary<string, Func<List<WValue>, WValue>> {
            { "wea_chrono_now", args => new WString(DateTime.Now.ToString("HH:mm:ss")) },
            { "wea_chrono_ms", args => new WNumber((double)System.Environment.TickCount) },
            { "wea_pause", args => { Thread.Sleep(args.Count > 0 ? (int)args[0].AsNumber() : 100); return new WNumber(1); } }
        };
    }

    public class AdvancedLibrary : ILibrary
    {
        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions() => new Dictionary<string, Func<List<WValue>, WValue>> {
            { "wea_file_write", args => { try { File.WriteAllText(args[0].AsString(), args[1].AsString()); return new WNumber(1); } catch { return new WNumber(0); } } },
            { "wea_file_read", args => { try { return new WString(File.ReadAllText(args[0].AsString())); } catch { return new WNull(); } } },
            { "wea_file_exists", args => { return new WNumber(File.Exists(args[0].AsString()) ? 1 : 0); } },
            { "wea_file_del", args => { try { File.Delete(args[0].AsString()); return new WNumber(1); } catch { return new WNumber(0); } } }
        };
    }

    public class ConsoleLib : ILibrary
    {
        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions() => new Dictionary<string, Func<List<WValue>, WValue>> {
            { "wea_ui_style", args => new WNumber(1) }
        };
    }
}
