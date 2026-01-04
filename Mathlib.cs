#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
    
    public class MathLib : ILibrary
    {
        
        private static readonly Random _rng = new Random();

        public Dictionary<string, Func<List<object>, object>> GetFunctions()
        {
            return new Dictionary<string, Func<List<object>, object>>
            {
                
                { "wea_math_root", args => Math.Sqrt(ToDouble(args[0])) },
                { "wea_math_force", args => Math.Pow(ToDouble(args[0]), ToDouble(args[1])) },
                { "wea_math_pure", args => Math.Abs(ToDouble(args[0])) },
                { "wea_math_fix", args => Math.Round(ToDouble(args[0]), args.Count > 1 ? Convert.ToInt32(args[1]) : 0) }, 
                
                
                
                { "wea_geo_sin", args => Math.Sin(ToRadian(ToDouble(args[0]))) },
                { "wea_geo_cos", args => Math.Cos(ToRadian(ToDouble(args[0]))) },
                { "wea_geo_tan", args => Math.Tan(ToRadian(ToDouble(args[0]))) },

              
                { "wea_const_pi", args => Math.PI },
                { "wea_const_e", args => Math.E },

                
                { "wea_math_chance", args => {
                    if (args.Count >= 2)
                        return _rng.Next(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]));
                    if (args.Count == 1)
                        return _rng.Next(0, Convert.ToInt32(args[0]));
                    return _rng.Next();
                }}
            };
        }

       
        private double ToDouble(object val)
        {
            if (val == null) return 0;
            return double.TryParse(val.ToString(), out double res) ? res : 0;
        }

        private double ToRadian(double degree) => degree * (Math.PI / 180.0);
    }
}
