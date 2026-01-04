#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace WSharp
{
    
    public class AdvancedLibrary : ILibrary
    {
        private static readonly HttpClient _client = new HttpClient();

        static AdvancedLibrary()
        {
            
            _client.Timeout = TimeSpan.FromSeconds(15);
            _client.DefaultRequestHeaders.Add("User-Agent", "WEA-Prime-Engine/2.0 (Master-Build)");
        }

        public Dictionary<string, Func<List<object>, object>> GetFunctions()
        {
            return new Dictionary<string, Func<List<object>, object>>
            {
               
                { "wea_file_write", args => {
                    try { File.WriteAllText(args[0].ToString(), args[1].ToString()); return true; } catch { return false; }
                }},
                { "wea_file_read", args => {
                    string p = args[0].ToString();
                    return File.Exists(p) ? File.ReadAllText(p) : "wea_fail: null_target";
                }},
                { "wea_file_push", args => {
                    try { File.AppendAllText(args[0].ToString(), args[1].ToString()); return true; } catch { return false; }
                }},
                { "wea_file_check", args => File.Exists(args[0].ToString()) },
                { "wea_file_delete", args => {
                    try { if(File.Exists(args[0].ToString())) File.Delete(args[0].ToString()); return true; } catch { return false; }
                }},
                { "wea_file_path", args => Directory.GetCurrentDirectory() },

               
                { "wea_vault_store", args => {
                    try {
                        string path = args[0].ToString();
                        string entry = $"{args[1]}|{args[2]}{Environment.NewLine}";
                        File.AppendAllText(path, entry);
                        return true;
                    } catch { return false; }
                }},
                { "wea_vault_fetch", args => {
                    try {
                        string path = args[0].ToString();
                        string key = args[1].ToString();
                        if (!File.Exists(path)) return "wea_fail";
                        
                        var lines = File.ReadLines(path).Reverse();
                        foreach (var line in lines) {
                            var parts = line.Split('|');
                            if (parts.Length >= 2 && parts[0] == key) return parts[1];
                        }
                        return "wea_fail";
                    } catch { return "wea_error"; }
                }},

            
                { "wea_net_get", args => {
                    try { return _client.GetStringAsync(args[0].ToString()).GetAwaiter().GetResult(); }
                    catch (Exception ex) { return "wea_net_fail: " + ex.Message; }
                }},
                { "wea_net_post", args => {
                    try {
                        var content = new StringContent(args[1].ToString(), Encoding.UTF8, "application/json");
                        var response = _client.PostAsync(args[0].ToString(), content).GetAwaiter().GetResult();
                        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    } catch (Exception ex) { return "wea_net_fail: " + ex.Message; }
                }},
                { "wea_net_json", args => {
                    try {
                        using var doc = JsonDocument.Parse(args[0].ToString());
                        return doc.RootElement.GetProperty(args[1].ToString()).ToString();
                    } catch { return "wea_json_fail"; }
                }},

                
                { "wea_sys_run", args => {
                    try {
                        Process.Start(new ProcessStartInfo {
                            FileName = args[0].ToString(),
                            Arguments = args.Count > 1 ? args[1].ToString() : "",
                            UseShellExecute = true
                        });
                        return true;
                    } catch { return false; }
                }},
                { "wea_sys_env", args => Environment.GetEnvironmentVariable(args[0].ToString()) ?? "wea_null" },
                
            
                { "wea_str_slice", args => {
                    try {
                        string text = args[0].ToString();
                        string s = args[1].ToString();
                        string e = args[2].ToString();
                        int p1 = text.IndexOf(s);
                        if (p1 == -1) return "wea_null";
                        p1 += s.Length;
                        int p2 = text.IndexOf(e, p1);
                        return (p2 == -1) ? "wea_null" : text.Substring(p1, p2 - p1);
                    } catch { return "wea_error"; }
                }}
            };
        }
    }
}
