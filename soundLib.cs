#nullable disable
using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;
using System.Runtime.InteropServices;

namespace WSharp
{
    
    public class SoundLib : ILibrary
    {
        public Dictionary<string, Func<List<object>, object>> GetFunctions()
        {
            return new Dictionary<string, Func<List<object>, object>>
            {

                { "wea_audio_pulse", args => {
                    try {
                        if (args.Count >= 2)
                        {
                            int f = Convert.ToInt32(args[0]);
                            int d = Convert.ToInt32(args[1]);
   
                            Console.Beep(Math.Clamp(f, 37, 32767), d);
                        }
                        else Console.Beep();
                        return true;
                    } catch { return false; }
                }},


                { "wea_audio_alert", args => {
                    if (args.Count == 0) return false;
                    string type = args[0].ToString().ToLower();

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        if (type == "wea_fail") SystemSounds.Hand.Play();
                        else if (type == "wea_info") SystemSounds.Asterisk.Play();
                        else if (type == "wea_warn") SystemSounds.Exclamation.Play();
                        else SystemSounds.Beep.Play();
                        return true;
                    }
                    return false;
                }},


                { "wea_audio_tone", args => {
                    try {
                        string note = args[0].ToString().ToUpper();
                        int duration = args.Count > 1 ? Convert.ToInt32(args[1]) : 300;
                        
                        int freq = note switch {
                            "C" => 261, "C#" => 277, "D" => 294, "D#" => 311,
                            "E" => 329, "F" => 349, "F#" => 370, "G" => 392,
                            "G#" => 415, "A" => 440, "A#" => 466, "B" => 493,
                            "C2" => 523, _ => 440
                        };

                        Console.Beep(freq, duration);
                        return true;
                    } catch { return false; }
                }},

                { "wea_audio_gap", args => {
                    try {
                        int ms = args.Count > 0 ? Convert.ToInt32(args[0]) : 100;
                        Thread.Sleep(ms);
                        return true;
                    } catch { return false; }
                }}
            };
        }
    }
}
