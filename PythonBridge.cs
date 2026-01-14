using System;
using System.Diagnostics;
using System.IO;

namespace WSharp
{
    public static class PythonBridge
    {
        // -----------------------------------------------------------------------------------------
        // DEVELOPER NOTE:
        // To link Python manually:
        // 1. Install Python.
        // 2. Open CMD and type 'where python'.
        // 3. Paste the resulting path below (inside the quotes). 
        // 4. If you leave this as default, the system will try to use the global 'python' command.
        // @weagw-developer-have a nice day
        // -----------------------------------------------------------------------------------------

       
        private static string PythonPath = @"PASTE_YOUR_PYTHON_PATH_HERE";

        public static string Run(string scriptPathRaw, string args)
        {
            try
            {
              
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptPathRaw);

             
                if (!File.Exists(fullPath))
                {
                    return $"[ERROR] File Not Found!\nPath: {fullPath}";
                }

                
                string exeToUse = PythonPath;

              
                if (exeToUse.Contains("PASTE_YOUR_PYTHON_PATH_HERE") || string.IsNullOrEmpty(exeToUse))
                {
                    exeToUse = "python"; 
                }

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = exeToUse;
                
                start.Arguments = $"\"{fullPath}\" {args}";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.CreateNoWindow = true;

                using (Process process = Process.Start(start))
                {
                    string result = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                        return $"[PYTHON ERROR]: {error}";

                    return result.Trim();
                }
            }
            catch (Exception ex)
            {
                return $"[SYSTEM ERROR]: Python could not be started.\nReason: {ex.Message}\nFix: Check 'PythonPath' in PythonBridge.cs or add Python to your System PATH.";
            }
        }
    }
}
