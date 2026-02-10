#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Text.RegularExpressions;

namespace WSharp
{
    public class FileOps
    {
        public static string Read(string path) => File.Exists(path) ? File.ReadAllText(path) : "HATA: Dosya bulunamadı.";
        public static string Write(string path, string content) { File.WriteAllText(path, content); return "Başarılı"; }
        public static string Append(string path, string content) { File.AppendAllText(path, content + "\n"); return "Başarılı"; }
    }

    public class StringOps
    {
        public static double Length(string s) => s.Length;
        public static string Replace(string s, string oldVal, string newVal) => s.Replace(oldVal, newVal);
        public static string Substring(string s, int start, int len) => s.Substring(start, len);
        public static double Contains(string s, string search) => s.Contains(search) ? 1.0 : 0.0;
    }

    public class SysOps
    {
        public static double GetTime() => DateTimeOffset.Now.ToUnixTimeMilliseconds();
        public static string Delay(int ms) { Thread.Sleep(ms); return $"{ms}ms beklendi."; }
        public static string GetMem() => $"Hafıza Kullanımı: {GC.GetTotalMemory(false) / (1024 * 1024)} MB";
    }

    public class NetOps
    {
        private static readonly HttpClient client = new HttpClient();

        public static string HttpGet(string url)
        {
            try { return client.GetStringAsync(url).Result; }
            catch (Exception ex) { return $"AĞ HATASI: {ex.Message}"; }
        }
    }

    // 1. [File] Dosya Okuma/Yazma
    public class CoreFileFunc : IWCallable
    {
        public int Arity() => 3; // (İşlem, Yol, İçerik)
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            string op = args[0].AsString();
            string path = args[1].AsString();
            string data = args[2].AsString();

            if (op == "read") return new WValue(FileOps.Read(path));
            if (op == "write") return new WValue(FileOps.Write(path, data));
            if (op == "append") return new WValue(FileOps.Append(path, data));
            return new WValue("Geçersiz Dosya İşlemi");
        }
        public override string ToString() => "<native fn core_file>";
    }

    // 2. [String] Metin İşleme
    public class CoreStringFunc : IWCallable
    {
        public int Arity() => 4; // (İşlem, Metin, Arg1, Arg2)
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            string op = args[0].AsString();
            string text = args[1].AsString();

            if (op == "len") return new WValue(StringOps.Length(text));
            if (op == "contains") return new WValue(StringOps.Contains(text, args[2].AsString()));
            if (op == "replace") return new WValue(StringOps.Replace(text, args[2].AsString(), args[3].AsString()));
            return new WValue("Geçersiz Metin İşlemi");
        }
        public override string ToString() => "<native fn core_string>";
    }

    // 3. [Sys] Sistem Kontrolü
    public class CoreSysFunc : IWCallable
    {
        public int Arity() => 2; // (İşlem, Arg)
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            string op = args[0].AsString();
            if (op == "time") return new WValue(SysOps.GetTime());
            if (op == "delay") return new WValue(SysOps.Delay((int)args[1].AsNumber()));
            if (op == "memory") return new WValue(SysOps.GetMem());
            return new WValue("Geçersiz Sistem İşlemi");
        }
        public override string ToString() => "<native fn core_sys>";
    }

    // 4. [Net] İnternet Verisi
    public class CoreNetFunc : IWCallable
    {
        public int Arity() => 2; // (İşlem, URL)
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            if (args[0].AsString() == "get") return new WValue(NetOps.HttpGet(args[1].AsString()));
            return new WValue("Geçersiz Ağ İşlemi");
        }
        public override string ToString() => "<native fn core_net>";
    }
}
