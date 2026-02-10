#nullable disable
using System;
using System.Collections.Generic;

using System.Text.Json;
namespace WSharp
{
    // --- STANDART KÜTÜPHANE (Yerleşik Fonksiyonlar) ---

    // 1. CLOCK
    public class ClockFunc : IWCallable
    {
        public int Arity() => 0;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            return new WValue((double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000.0);
        }
        public override string ToString() => "<native fn clock>";
    }

    // 2. SQRT
    public class SqrtFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            return new WValue(Math.Sqrt(arguments[0].AsNumber()));
        }
        public override string ToString() => "<native fn sqrt>";
    }

    // 3. INPUT (wea_read)
    public class InputFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            // IDE'de Input şimdilik zor olduğu için basit bir workaround
            // Burası ileride "Prompt.ShowDialog" ile değiştirilebilir.
            return new WValue("Kullanıcı Girişi (Mock)");
        }
        public override string ToString() => "<native fn input>";
    }

    // 4. PRINT
    public class PrintFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string text = arguments[0].ToString();
            interpreter.Notify(text);

            return new WValue(null);
        }
        public override string ToString() => "<native fn print>";
    }

    // 5. LEN (Uzunluk)
    public class LenFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var arg = arguments[0];
            if (arg.Type == WType.List) return new WValue((double)arg.AsList().Count);
            if (arg.Type == WType.Dict) return new WValue((double)arg.AsDict().Count);
            if (arg.Type == WType.String) return new WValue((double)arg.AsString().Length);
            throw new Exception("Len fonksiyonu sadece liste, sozluk veya yazilar icin kullanilabilir.");
        }
        public override string ToString() => "<native fn len>";
    }

    // 6. PUSH (Ekle)
    public class PushFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0];
            if (list.Type != WType.List) throw new Exception("Push fonksiyonunun ilk argumani liste olmalidir.");
            
            list.AsList().Add(arguments[1].Value);
            return list;
        }
        public override string ToString() => "<native fn push>";
    }

    // 7. POP (Cikar)
    public class PopFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0];
            if (list.Type != WType.List) throw new Exception("Pop fonksiyonunun ilk argumani liste olmalidir.");
            var l = list.AsList();
            if (l.Count == 0) throw new Exception("Bos listeden eleman cikarilamaz.");
            
            var last = l[l.Count - 1];
            l.RemoveAt(l.Count - 1);
            return new WValue(last);
        }
        public override string ToString() => "<native fn pop>";
    }

    // 8. TEST JSON (Verification)
    public class TestJsonFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
             try 
             {
                 string json = arguments[0].AsString();
                 return WNeuraBridge.ParseJsonToWValue(json); 
             }
             catch(Exception ex) { return new WValue("Error: " + ex.Message); }
        }
        public override string ToString() => "<native fn test_json>";
    }

    // 9. TEST SERIALIZE (Verification)
    public class TestSerializeFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
             try 
             {
                 return new WValue(WNeuraBridge.ConvertWValueToJson(arguments[0])); 
             }
             catch(Exception ex) { return new WValue("Error: " + ex.Message); }
        }
        public override string ToString() => "<native fn test_serialize>";
    }

    // 10. MAP (Functional)
    public class MapFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0].AsList();
            var func = arguments[1].AsCallable();
            var result = new List<object>();
            foreach (var item in list)
            {
                var args = new List<WValue> { new WValue(item) };
                var mapped = func.Call(interpreter, args);
                result.Add(mapped.Value);
            }
            return new WValue(result);
        }
        public override string ToString() => "<native fn map>";
    }

    // 11. FILTER (Functional)
    public class FilterFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0].AsList();
            var func = arguments[1].AsCallable();
            var result = new List<object>();
            foreach (var item in list)
            {
                var args = new List<WValue> { new WValue(item) };
                var condition = func.Call(interpreter, args);
                if (condition.AsBoolean())
                {
                    result.Add(item);
                }
            }
            return new WValue(result);
        }
        public override string ToString() => "<native fn filter>";
    }
    // 12. STATISTICS
    public class AvgFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0].AsList();
            if (list.Count == 0) return new WValue(0.0);
            double sum = 0;
            foreach(var item in list) sum += Convert.ToDouble(item);
            return new WValue(sum / list.Count);
        }
        public override string ToString() => "<native fn avg>";
    }

    public class MaxFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0].AsList();
            if (list.Count == 0) return new WValue(null);
            double max = double.MinValue;
            foreach(var item in list) 
            {
                double val = Convert.ToDouble(item);
                if (val > max) max = val;
            }
            return new WValue(max);
        }
        public override string ToString() => "<native fn max>";
    }

    public class MinFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0].AsList();
            if (list.Count == 0) return new WValue(null);
            double min = double.MaxValue;
            foreach(var item in list) 
            {
                double val = Convert.ToDouble(item);
                if (val < min) min = val;
            }
            return new WValue(min);
        }
        public override string ToString() => "<native fn min>";
    }

    public class StdDevFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0].AsList();
            if (list.Count == 0) return new WValue(0.0);
            
            double sum = 0;
            foreach(var item in list) sum += Convert.ToDouble(item);
            double mean = sum / list.Count;
            
            double sumSqDiff = 0;
            foreach(var item in list) 
            {
                double val = Convert.ToDouble(item);
                sumSqDiff += Math.Pow(val - mean, 2);
            }
            return new WValue(Math.Sqrt(sumSqDiff / list.Count));
        }
        public override string ToString() => "<native fn std_dev>";
        }
    // 13. FILE I/O
    public class FileWriteFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string filename = arguments[0].AsString();
            WValue content = arguments[1];
            
            string dataToWrite;
            if (content.Type == WType.List || content.Type == WType.Dict)
            {
                dataToWrite = WNeuraBridge.ConvertWValueToJson(content);
            }
            else
            {
                dataToWrite = content.ToString();
            }

            try 
            {
                System.IO.File.WriteAllText(filename, dataToWrite);
                return new WValue(true);
            }
            catch(Exception ex) 
            { 
                throw new Exception($"Dosya yazma hatasi: {ex.Message}"); 
            }
        }
        public override string ToString() => "<native fn file_write>";
    }

    public class FileReadFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string filename = arguments[0].AsString();
            try 
            {
                string content = System.IO.File.ReadAllText(filename);
                return new WValue(content);
            }
            catch(Exception ex) 
            { 
                throw new Exception($"Dosya okuma hatasi: {ex.Message}"); 
            }
        }
        public override string ToString() => "<native fn file_read>";
    }

    public class FileWriteLoggedFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string filename = arguments[0].AsString();
            WValue content = arguments[1];
            
            // Generate metadata header
            var now = DateTime.Now;
            string metadata = $"# Date: {now:yyyy-MM-dd}\n# Time: {now:HH:mm:ss}\n# \n";
            
            string dataToWrite;
            if (content.Type == WType.List || content.Type == WType.Dict)
            {
                dataToWrite = metadata + WNeuraBridge.ConvertWValueToJson(content);
            }
            else
            {
                dataToWrite = metadata + content.ToString();
            }

            try 
            {
                System.IO.File.WriteAllText(filename, dataToWrite);
                return new WValue(true);
            }
            catch(Exception ex) 
            { 
                throw new Exception($"Dosya yazma hatasi: {ex.Message}"); 
            }
        }
        public override string ToString() => "<native fn file_write_logged>";
    }

    // 14. EXPORT CSV
    public class ExportCsvFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string filename = arguments[0].AsString();
            var list = arguments[1].AsList(); // Expecting list of dicts

            if (list.Count == 0) 
            {
                 System.IO.File.WriteAllText(filename, "");
                 return new WValue(true);
            }

            // 1. Collect all unique keys (headers)
            var headers = new HashSet<string>();
            foreach (var item in list)
            {
                // We need to cast item (object) to dict. 
                // Since AsList returns List<object>, we rely on it being Dictionary<string, WValue>
                if (item is Dictionary<string, WValue> dict)
                {
                    foreach(var key in dict.Keys) headers.Add(key);
                }
                else
                {
                    throw new Exception("CSV Export icin veriler 'sozluk listesi' olmalidir.");
                }
            }

            var headerList = new List<string>(headers);
            var sb = new System.Text.StringBuilder();

            // Add metadata header (Excel-compatible comments)
            var now = DateTime.Now;
            sb.AppendLine($"# Date: {now:yyyy-MM-dd}");
            sb.AppendLine($"# Time: {now:HH:mm:ss}");
            sb.AppendLine("#");

            // 2. Write Header Row
            sb.AppendLine(string.Join(",", headerList));

            // 3. Write Data Rows
            foreach (var item in list)
            {
                var dict = (Dictionary<string, WValue>)item;
                var rowValues = new List<string>();
                foreach (var header in headerList)
                {
                    string val = "";
                    if (dict.ContainsKey(header))
                    {
                        val = dict[header].ToString();
                        // Simple CSV escaping: if contains comma, quote it.
                        if (val.Contains(",") || val.Contains("\"") || val.Contains("\n"))
                        {
                            val = "\"" + val.Replace("\"", "\"\"") + "\"";
                        }
                    }
                    rowValues.Add(val);
                }
                sb.AppendLine(string.Join(",", rowValues));
            }

            try 
            {
                System.IO.File.WriteAllText(filename, sb.ToString());
                return new WValue(true);
            }
            catch(Exception ex) 
            { 
                 throw new Exception($"CSV yazma hatasi: {ex.Message}"); 
            }
        }
        public override string ToString() => "<native fn export_csv>";
    }

    // ============================================
    // YENI EKLENEN FONKSIYONLAR
    // ============================================

    // to_number: String -> Number
    public class ToNumberFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string s = arguments[0].ToString();
            if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
                return new WValue(result);
            throw new Exception($"'{s}' sayiya cevrilemez.");
        }
        public override string ToString() => "<native fn to_number>";
    }

    // typeof: Any -> String
    public class TypeOfFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            switch (arguments[0].Type)
            {
                case WType.Number: return new WValue("number");
                case WType.String: return new WValue("string");
                case WType.Bool: return new WValue("bool");
                case WType.List: return new WValue("list");
                case WType.Dict: return new WValue("dict");
                case WType.Function: return new WValue("function");
                case WType.Null: return new WValue("null");
                default: return new WValue("unknown");
            }
        }
        public override string ToString() => "<native fn typeof>";
    }

    // range: (start, end) or (end) -> List
    public class RangeFunc : IWCallable
    {
        public int Arity() => 2; // (start, end)
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            int start = (int)arguments[0].AsNumber();
            int end = (int)arguments[1].AsNumber();
            var list = new List<object>();
            if (start <= end)
                for (int i = start; i < end; i++) list.Add((double)i);
            else
                for (int i = start; i > end; i--) list.Add((double)i);
            return new WValue(list);
        }
        public override string ToString() => "<native fn range>";
    }

    // sort: List -> List (ascending)
    public class SortFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = new List<object>(arguments[0].AsList());
            list.Sort((a, b) => Convert.ToDouble(a).CompareTo(Convert.ToDouble(b)));
            return new WValue(list);
        }
        public override string ToString() => "<native fn sort>";
    }

    // reverse: List -> List
    public class ReverseFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = new List<object>(arguments[0].AsList());
            list.Reverse();
            return new WValue(list);
        }
        public override string ToString() => "<native fn reverse>";
    }

    // join: (List, separator) -> String
    public class JoinFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0].AsList();
            string sep = arguments[1].AsString();
            var parts = new List<string>();
            foreach (var item in list) parts.Add(new WValue(item).ToString());
            return new WValue(string.Join(sep, parts));
        }
        public override string ToString() => "<native fn join>";
    }

    // split: (String, separator) -> List
    public class SplitFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string s = arguments[0].AsString();
            string sep = arguments[1].AsString();
            var parts = s.Split(new[] { sep }, StringSplitOptions.None);
            var list = new List<object>();
            foreach (var part in parts) list.Add(part);
            return new WValue(list);
        }
        public override string ToString() => "<native fn split>";
    }

    // abs: Number -> Number
    public class AbsFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Abs(a[0].AsNumber()));
        public override string ToString() => "<native fn abs>";
    }

    // pow: (base, exp) -> Number
    public class PowFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Pow(a[0].AsNumber(), a[1].AsNumber()));
        public override string ToString() => "<native fn pow>";
    }

    // floor: Number -> Number
    public class FloorFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Floor(a[0].AsNumber()));
        public override string ToString() => "<native fn floor>";
    }

    // ceil: Number -> Number
    public class CeilFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Ceiling(a[0].AsNumber()));
        public override string ToString() => "<native fn ceil>";
    }

    // round: Number -> Number
    public class RoundFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Round(a[0].AsNumber()));
        public override string ToString() => "<native fn round>";
    }
}
