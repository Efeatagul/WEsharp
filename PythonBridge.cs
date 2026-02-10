using System;
using System.Diagnostics;
using System.IO;

namespace WSharp
{
    public static class PythonBridge
    {
        public static string Run(string scriptPath, string args)
        {
            if (!File.Exists(scriptPath))
                return $"[ERROR] File Not Found!\nPath: {scriptPath}";

            
            string[] pythonCommands = { "py", "python", "python3" };
            string lastError = "";

            foreach (var cmd in pythonCommands)
            {
                try
                {
                    ProcessStartInfo start = new ProcessStartInfo();
                    start.FileName = cmd;
                    start.Arguments = $"\"{scriptPath}\" {args}";
                    start.UseShellExecute = false;
                    start.RedirectStandardOutput = true;
                    start.RedirectStandardError = true;
                    start.CreateNoWindow = true;

                    using (Process process = Process.Start(start))
                    {
                        using (StreamReader reader = process.StandardOutput)
                        {
                            string result = reader.ReadToEnd();
                            string error = process.StandardError.ReadToEnd();
                            process.WaitForExit();

                            
                            if (!string.IsNullOrEmpty(error) && string.IsNullOrEmpty(result))
                            {
                                lastError = $"({cmd}): {error}"; 
                                continue; 

                            
                            return result.Trim();
                        }
                    }
                }
                catch (Exception)
                {
                    
                    continue; 
                }
            }

            
            return $"[PYTHON ERROR]: Hiçbir Python komutu çalışmadı.\nSon Hata: {lastError}\nLütfen Python'un kurulu olduğundan emin olun.";
        }
    }
}
