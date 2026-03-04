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
// ═══════════════════════════════════════════════════════════════
//  PythonBridge.cs — Persistent Process Bridge (Singleton)
// ═══════════════════════════════════════════════════════════════
//
//  ARCHITECTURE:
//  ─────────────
//  This class spawns the bundled WneuraEngine.exe EXACTLY ONCE
//  and keeps it alive for the entire application lifetime.
//  Communication happens over stdin/stdout using a simple
//  line-based JSON protocol:
//
//    C# → writes one JSON line to stdin
//    Py → reads it, processes, prints one JSON line to stdout
//
//  PREVIOUS FLAWS (now fixed):
//  ───────────────────────────
//  ✗ Process.Start per command  → massive overhead
//  ✗ "py", "python" fallbacks   → fragile dependency on local installs
//  ✗ No concurrency control     → race conditions on stdout
//
//  CURRENT DESIGN:
//  ───────────────
//  ✓ Single long-lived process  → spawned once, reused forever
//  ✓ Bundled Engine.exe         → no local Python dependency
//  ✓ SemaphoreSlim              → thread-safe concurrent access
//  ✓ IDisposable                → graceful shutdown
//  ✓ Async API                  → non-blocking UI calls
//
//  ⚠️  NO System.Windows.Forms references anywhere.
// ═══════════════════════════════════════════════════════════════

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WSharp
{
    
    public sealed class PythonBridge : IDisposable
    {
       
        private static readonly Lazy<PythonBridge> _lazy =
            new Lazy<PythonBridge>(() => new PythonBridge());

        
        public static PythonBridge Instance => _lazy.Value;

        

        private Process _process;           
        private StreamWriter _stdin;        
        private StreamReader _stdout;       

      
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private bool _initialized;
        private bool _disposed;

        
        public bool IsRunning =>
            _initialized && _process != null && !_process.HasExited;

       
        public string LastError { get; private set; } = "";

        
        private PythonBridge() { }

        
        public bool Initialize()
        {
            if (_initialized && IsRunning)
                return true;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

               
                string engineExe = Path.Combine(baseDir, "Engine", "WneuraEngine.exe");
                string enginePy  = Path.Combine(baseDir, "wneura_engine.py");

                string fileName;
                string arguments;

                if (File.Exists(engineExe))
                {
                   
                    fileName  = engineExe;
                    arguments = "";
                }
                else if (File.Exists(enginePy))
                {
                    
                    fileName  = "py";
                    arguments = $"\"{enginePy}\"";
                }
                else
                {
                    LastError = $"Engine not found.\n"
                              + $"Searched: {engineExe}\n"
                              + $"Searched: {enginePy}";
                    return false;
                }

                
                var startInfo = new ProcessStartInfo
                {
                    FileName               = fileName,
                    Arguments              = arguments,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,

                    
                    RedirectStandardInput  = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,

                   
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding  = System.Text.Encoding.UTF8,
                };

               
                _process = Process.Start(startInfo);

                if (_process == null || _process.HasExited)
                {
                    LastError = "Engine process failed to start.";
                    return false;
                }

                
                _stdin  = _process.StandardInput;
                _stdout = _process.StandardOutput;

                
                _stdin.AutoFlush = true;

                _initialized = true;
                LastError = "";
                return true;
            }
            catch (Exception ex)
            {
                LastError = $"Engine initialization failed: {ex.Message}";
                _initialized = false;
                return false;
            }
        }

      
        public async Task<string> SendCommandAsync(string jsonPayload)
        {
            
            if (!IsRunning)
            {
                return "{\"status\":\"error\",\"msg\":\"Engine is not running. Call Initialize() first.\"}";
            }

            
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                
                await _stdin.WriteLineAsync(jsonPayload).ConfigureAwait(false);

                
                string response = await _stdout.ReadLineAsync().ConfigureAwait(false);

                if (response == null)
                {
                    
                    LastError = "Engine process closed unexpectedly.";
                    return "{\"status\":\"error\",\"msg\":\"Engine process closed unexpectedly.\"}";
                }

                return response;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return $"{{\"status\":\"error\",\"msg\":\"{EscapeJson(ex.Message)}\"}}";
            }
            finally
            {
                _lock.Release();
            }
        }

        
        public string SendCommand(string jsonPayload)
        {
            return SendCommandAsync(jsonPayload).GetAwaiter().GetResult();
        }

        
        public async Task<bool> PingAsync()
        {
            string response = await SendCommandAsync("{\"command\":\"ping\"}");
            return response != null && response.Contains("\"pong\"");
        }

        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                
                _stdin?.Close();

               
                if (_process != null && !_process.HasExited)
                {
                    if (!_process.WaitForExit(3000))
                    {
                        
                        _process.Kill();
                    }
                }

                _process?.Dispose();
            }
            catch
            {
                
            }

            _lock?.Dispose();
        }

        
        private static string EscapeJson(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r");
        }
    }
}