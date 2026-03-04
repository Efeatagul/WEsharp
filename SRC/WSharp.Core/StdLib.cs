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
#nullable disable
using System;
using System.Collections.Generic;

using System.Text.Json;
namespace WSharp
{
    
    public class ClockFunc : IWCallable
    {
        public int Arity() => 0;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            return new WValue((double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000.0);
        }
        public override string ToString() => "<native fn clock>";
    }

    
    public class SqrtFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            return new WValue(Math.Sqrt(arguments[0].AsNumber()));
        }
        public override string ToString() => "<native fn sqrt>";
    }

   
    public class InputFunc : IWCallable
    {
        public int Arity() => -1; 
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            
            if (arguments.Count > 0)
            {
                interpreter.Notify(arguments[0].ToString());
            }
            string line = Console.ReadLine();
            return new WValue(line ?? "");
        }
        public override string ToString() => "<native fn input>";
    }

   
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

    
    public class LenFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var arg = arguments[0];
            if (arg.Type == WType.List) return new WValue((double)arg.AsList().Count);
            if (arg.Type == WType.Dict) return new WValue((double)arg.AsDict().Count);
            if (arg.Type == WType.String) return new WValue((double)arg.AsString().Length);
            throw new WSharpTypeException("Len fonksiyonu sadece liste, sozluk veya yazilar icin kullanilabilir.");
        }
        public override string ToString() => "<native fn len>";
    }

    
    public class PushFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0];
            if (list.Type != WType.List) throw new WSharpTypeException("Push fonksiyonunun ilk argumani liste olmalidir.");
            
            list.AsList().Add(arguments[1].Value);
            return list;
        }
        public override string ToString() => "<native fn push>";
    }

  
    public class PopFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0];
            if (list.Type != WType.List) throw new WSharpTypeException("Pop fonksiyonunun ilk argumani liste olmalidir.");
            var l = list.AsList();
            if (l.Count == 0) throw new WSharpIndexException("Bos listeden eleman cikarilamaz.");
            
            var last = l[l.Count - 1];
            l.RemoveAt(l.Count - 1);
            return new WValue(last);
        }
        public override string ToString() => "<native fn pop>";
    }

    
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
                throw new WSharpRuntimeException($"Dosya yazma hatasi: {ex.Message}"); 
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
                throw new WSharpRuntimeException($"Dosya okuma hatasi: {ex.Message}"); 
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
                throw new WSharpRuntimeException($"Dosya yazma hatasi: {ex.Message}"); 
            }
        }
        public override string ToString() => "<native fn file_write_logged>";
    }

    
    public class ExportCsvFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string filename = arguments[0].AsString();
            var list = arguments[1].AsList(); 

            if (list.Count == 0) 
            {
                 System.IO.File.WriteAllText(filename, "");
                 return new WValue(true);
            }

            
            var headers = new HashSet<string>();
            foreach (var item in list)
            {
                
                if (item is Dictionary<string, WValue> dict)
                {
                    foreach(var key in dict.Keys) headers.Add(key);
                }
                else
                {
                    throw new WSharpTypeException("CSV Export icin veriler 'sozluk listesi' olmalidir.");
                }
            }

            var headerList = new List<string>(headers);
            var sb = new System.Text.StringBuilder();

            
            var now = DateTime.Now;
            sb.AppendLine($"# Date: {now:yyyy-MM-dd}");
            sb.AppendLine($"# Time: {now:HH:mm:ss}");
            sb.AppendLine("#");

            
            sb.AppendLine(string.Join(",", headerList));

          
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
                 throw new WSharpRuntimeException($"CSV yazma hatasi: {ex.Message}"); 
            }
        }
        public override string ToString() => "<native fn export_csv>";
    }

    
    public class ToNumberFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string s = arguments[0].ToString();
            if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
                return new WValue(result);
            throw new WSharpTypeException($"'{s}' sayiya cevrilemez.");
        }
        public override string ToString() => "<native fn to_number>";
    }

    
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
                case WType.Null: return new WValue("nihil");
                default: return new WValue("unknown");
            }
        }
        public override string ToString() => "<native fn typeof>";
    }

    
    public class RangeFunc : IWCallable
    {
        public int Arity() => -1; 
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            if (arguments.Count == 0 || arguments.Count > 3)
                throw new WSharpArgumentException("range(son) veya range(baslangic, son) bekleniyor.", 2, arguments.Count);

            int start = arguments.Count == 1 ? 0 : (int)arguments[0].AsNumber();
            int end   = arguments.Count == 1 ? (int)arguments[0].AsNumber() : (int)arguments[1].AsNumber();

            var list = new List<object>();
            if (start <= end)
                for (int i = start; i < end; i++) list.Add((double)i);
            else
                for (int i = start; i > end; i--) list.Add((double)i);
            return new WValue(list);
        }
        public override string ToString() => "<native fn range>";
    }

    
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

    
    public class AbsFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Abs(a[0].AsNumber()));
        public override string ToString() => "<native fn abs>";
    }

  
    public class PowFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Pow(a[0].AsNumber(), a[1].AsNumber()));
        public override string ToString() => "<native fn pow>";
    }

   
    public class FloorFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Floor(a[0].AsNumber()));
        public override string ToString() => "<native fn floor>";
    }

  
    public class CeilFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Ceiling(a[0].AsNumber()));
        public override string ToString() => "<native fn ceil>";
    }

    
    public class RoundFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Math.Round(a[0].AsNumber()));
        public override string ToString() => "<native fn round>";
    }

    
    public class PyAnalyzeFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var list = arguments[0].AsList();

            
            var data = new List<object>();
            foreach (var item in list)
                data.Add(Convert.ToDouble(item));

           
            string json = System.Text.Json.JsonSerializer.Serialize(new
            {
                command = "analyze",
                data = data
            });

            
            string result = PythonBridge.Instance.SendCommand(json);

            if (result.StartsWith("[") || result.Contains("\"error\""))
                throw new NeuroEngineException(result, "PyAnalyze");

           
            return WNeuraBridge.ParseJsonToWValue(result);
        }
        public override string ToString() => "<native fn py_analyze>";
    }

    
    public class PyPlotSaveFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var xList = arguments[0].AsList();
            var yList = arguments[1].AsList();
            string filename = arguments[2].AsString();

            
            var xData = new List<object>();
            foreach (var item in xList)
                xData.Add(Convert.ToDouble(item));

            var yData = new List<object>();
            foreach (var item in yList)
                yData.Add(Convert.ToDouble(item));

          
            string json = System.Text.Json.JsonSerializer.Serialize(new
            {
                command = "plot_save",
                x = xData,
                y = yData,
                filename = filename
            });

            
            string result = PythonBridge.Instance.SendCommand(json);

            if (result.StartsWith("[") || result.Contains("\"error\""))
                throw new NeuroEngineException(result, "PyPlotSave");

           
            return WNeuraBridge.ParseJsonToWValue(result);
        }
        public override string ToString() => "<native fn py_plot_save>";
    }

    
    public class ToStringFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(a[0].ToString());
        public override string ToString() => "<native fn to_string>";
    }

    
    public class StrUpperFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(a[0].AsString().ToUpperInvariant());
        public override string ToString() => "<native fn str_upper>";
    }

    
    public class StrLowerFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(a[0].AsString().ToLowerInvariant());
        public override string ToString() => "<native fn str_lower>";
    }

    
    public class StrContainsFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(a[0].AsString().Contains(a[1].AsString()));
        public override string ToString() => "<native fn str_contains>";
    }

    
    public class StrReplaceFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter i, List<WValue> a)
            => new WValue(a[0].AsString().Replace(a[1].AsString(), a[2].AsString()));
        public override string ToString() => "<native fn str_replace>";
    }

    
    public class StrTrimFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(a[0].AsString().Trim());
        public override string ToString() => "<native fn str_trim>";
    }

    
    public class StrStartsWithFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a)
            => new WValue(a[0].AsString().StartsWith(a[1].AsString()));
        public override string ToString() => "<native fn str_starts_with>";
    }

    
    public class StrEndsWithFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a)
            => new WValue(a[0].AsString().EndsWith(a[1].AsString()));
        public override string ToString() => "<native fn str_ends_with>";
    }

    
    public class StrIndexOfFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a)
            => new WValue((double)a[0].AsString().IndexOf(a[1].AsString()));
        public override string ToString() => "<native fn str_index_of>";
    }

    
    public class StrSubFunc : IWCallable
    {
        public int Arity() => -1;
        public WValue Call(Interpreter i, List<WValue> a)
        {
            if (a.Count < 2 || a.Count > 3)
                throw new WSharpArgumentException("str_sub(yazi, baslangic) veya str_sub(yazi, baslangic, uzunluk) bekleniyor.", 2, a.Count);
            string s = a[0].AsString();
            int start = Math.Max(0, (int)a[1].AsNumber());
            if (start >= s.Length) return new WValue("");
            if (a.Count == 2) return new WValue(s.Substring(start));
            int len = Math.Min((int)a[2].AsNumber(), s.Length - start);
            return new WValue(s.Substring(start, len));
        }
        public override string ToString() => "<native fn str_sub>";
    }

    
    public class ConnexioHttpFunc : IWCallable
    {
        private static readonly System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient();

        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string url = arguments[0].AsString();
            try
            {
                string response = _client.GetStringAsync(url).Result;
                return new WValue(response);
            }
            catch (Exception ex)
            {
                string innerMsg = ex.InnerException?.Message ?? ex.Message;
                throw new WSharpRuntimeException($"CONNEXIO HATASI: {innerMsg}");
            }
        }
        public override string ToString() => "<native fn connexio_http>";
    }

    
    public class JsonParseFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string json = arguments[0].AsString();
            try
            {
                return WNeuraBridge.ParseJsonToWValue(json);
            }
            catch (Exception ex)
            {
                throw new WSharpRuntimeException($"JSON PARSE HATASI: {ex.Message}");
            }
        }
        public override string ToString() => "<native fn json_parse>";
    }
}