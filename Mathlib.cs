using System;
using System.Collections.Generic;

namespace WSharp
{
    public class MathLib : ILibrary
    {
        private static readonly Random _rng = new Random();

        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
            
                { "wea_math_abs", args => new WNumber(Math.Abs(args[0].AsNumber())) },
                { "wea_math_pow", args => new WNumber(Math.Pow(args[0].AsNumber(), args[1].AsNumber())) },
                { "wea_math_sqrt", args => new WNumber(Math.Sqrt(args[0].AsNumber())) },
                { "wea_math_sign", args => new WNumber(Math.Sign(args[0].AsNumber())) },

               
                { "wea_math_round", args => new WNumber(Math.Round(args[0].AsNumber())) },
                { "wea_math_floor", args => new WNumber(Math.Floor(args[0].AsNumber())) },
                { "wea_math_ceil", args => new WNumber(Math.Ceiling(args[0].AsNumber())) },
                { "wea_math_min", args => new WNumber(Math.Min(args[0].AsNumber(), args[1].AsNumber())) },
                { "wea_math_max", args => new WNumber(Math.Max(args[0].AsNumber(), args[1].AsNumber())) },

              
                { "wea_math_clamp", args => {
                    double val = args[0].AsNumber();
                    double min = args[1].AsNumber();
                    double max = args[2].AsNumber();
                    return new WNumber(Math.Max(min, Math.Min(max, val)));
                }},

              
                { "wea_math_sin", args => new WNumber(Math.Sin(args[0].AsNumber())) },
                { "wea_math_cos", args => new WNumber(Math.Cos(args[0].AsNumber())) },
                { "wea_math_tan", args => new WNumber(Math.Tan(args[0].AsNumber())) },
                { "wea_math_asin", args => new WNumber(Math.Asin(args[0].AsNumber())) },
                { "wea_math_acos", args => new WNumber(Math.Acos(args[0].AsNumber())) },

              
                { "wea_math_atan2", args => new WNumber(Math.Atan2(args[0].AsNumber(), args[1].AsNumber())) },

               
                { "wea_math_lerp", args => {
                    double start = args[0].AsNumber();
                    double end = args[1].AsNumber();
                    double t = args[2].AsNumber();
                    return new WNumber(start + (end - start) * t);
                }},

             
                { "wea_math_deg2rad", args => new WNumber(args[0].AsNumber() * (Math.PI / 180.0)) },
                { "wea_math_rad2deg", args => new WNumber(args[0].AsNumber() * (180.0 / Math.PI)) },

                
                { "wea_math_log", args => new WNumber(Math.Log(args[0].AsNumber())) },
                { "wea_math_log10", args => new WNumber(Math.Log10(args[0].AsNumber())) },

                
                { "wea_math_rand", args => new WNumber(_rng.NextDouble()) },
                
               
                { "wea_math_rand_range", args => new WNumber(_rng.Next((int)args[0].AsNumber(), (int)args[1].AsNumber())) },

                { "wea_math_pi", args => new WNumber(Math.PI) },
                { "wea_math_e", args => new WNumber(Math.E) }
            };
        }
    }
}
}
