#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Diagnostics;

namespace WSharp
{
    
    public interface ILibrary
    {
        Dictionary<string, Func<List<object>, object>> GetFunctions();
    }

    public class AdvancedLibrary : ILibrary
    {
        private static readonly HttpClient _client = new HttpClient();

     
        static AdvancedLibrary()
        {
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) WE-Sharp/1.4");
        }

        public Dictionary<string, Func<List<object>, object>> GetFunctions()
        {
            return new Dictionary<string, Func<List<object>, object>>
            {
                
                { "file_write", args => { File.WriteAllText(args[0].ToString(), args[1].ToString()); return true; }},
                { "file_read", args => File.ReadAllText(args[0].ToString()) },
                { "file_append", args => { File.AppendAllText(args[0].ToString(), args[1].ToString()); return true; }},
                { "file_exists", args => File.Exists(args[0].ToString()) },
                { "file_del", args => { if(File.Exists(args[0].ToString())) File.Delete(args[0].ToString()); return true; }},
                { "file_copy", args => { File.Copy(args[0].ToString(), args[1].ToString(), true); return true; }},
                { "file_move", args => { File.Move(args[0].ToString(), args[1].ToString()); return true; }},

                
                { "dir_create", args => { Directory.CreateDirectory(args[0].ToString()); return true; }},
                { "dir_del", args => { if(Directory.Exists(args[0].ToString())) Directory.Delete(args[0].ToString(), true); return true; }},
                { "dir_exists", args => Directory.Exists(args[0].ToString()) },
                { "dir_files", args => Directory.GetFiles(args[0].ToString()).Cast<object>().ToList() },
                { "dir_dirs", args => Directory.GetDirectories(args[0].ToString()).Cast<object>().ToList() },
                { "path_ext", args => Path.GetExtension(args[0].ToString()) },
                { "path_name", args => Path.GetFileName(args[0].ToString()) },
                { "get_cd", args => Directory.GetCurrentDirectory() },

                
                { "http_get", args => {
                    try {
                        return _client.GetStringAsync(args[0].ToString()).GetAwaiter().GetResult();
                    } catch (Exception ex) {
                        return "HTTP_ERROR: " + ex.Message;
                    }
                }},

             
                { "str_between", args => {
                    string text = args[0].ToString();
                    string start = args[1].ToString();
                    string end = args[2].ToString();
                    int p1 = text.IndexOf(start);
                    if (p1 == -1) return "";
                    p1 += start.Length;
                    int p2 = text.IndexOf(end, p1);
                    if (p2 == -1) return "";
                    return text.Substring(p1, p2 - p1);
                }},

                { "str_split", args => args[0].ToString().Split(new[] { args[1].ToString() }, StringSplitOptions.None).Cast<object>().ToList() },
                { "str_has", args => args[0].ToString().Contains(args[1].ToString()) },
                { "str_replace", args => args[0].ToString().Replace(args[1].ToString(), args[2].ToString()) },
                { "str_upper", args => args[0].ToString().ToUpper() },
                { "str_lower", args => args[0].ToString().ToLower() },
                { "str_trim", args => args[0].ToString().Trim() },
                { "str_len", args => args[0].ToString().Length },
                { "str_sub", args => {
                    int start = Convert.ToInt32(args[1]);
                    int len = Convert.ToInt32(args[2]);
                    return args[0].ToString().Substring(start, len);
                }},

               
                { "sys_exec", args => {
                    try {
                        ProcessStartInfo startInfo = new ProcessStartInfo {
                            FileName = args[0].ToString(),
                            Arguments = args.Count > 1 ? args[1].ToString() : "",
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);
                        return true;
                    } catch { return false; }
                }},

                { "get_env", args => Environment.GetEnvironmentVariable(args[0].ToString()) },

                { "sys_print", args => {
                    try {
                        ProcessStartInfo info = new ProcessStartInfo(args[0].ToString()) {
                            Verb = "print",
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                        Process.Start(info);
                        return true;
                    } catch { return false; }
                }}
            };
        }
    }
}
