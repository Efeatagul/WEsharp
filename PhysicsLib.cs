using System;
using System.Collections.Generic;

namespace WSharp
{
    public class PhysicsLib : ILibrary
    {
        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
               
                { "wea_phy_vel", args => new WNumber(args[0].AsNumber() / args[1].AsNumber()) },
                
                { "wea_phy_accel", args => new WNumber((args[0].AsNumber() - args[1].AsNumber()) / args[2].AsNumber()) },
                
                { "wea_phy_freefall", args => new WNumber(0.5 * 9.81 * Math.Pow(args[0].AsNumber(), 2)) },

                
                { "wea_phy_force", args => new WNumber(args[0].AsNumber() * args[1].AsNumber()) },
               
                { "wea_phy_momentum", args => new WNumber(args[0].AsNumber() * args[1].AsNumber()) },
                
                { "wea_phy_gravity", args => {
                    double G = 6.674e-11;
                    double m1 = args[0].AsNumber();
                    double m2 = args[1].AsNumber();
                    double r = args[2].AsNumber();
                    return new WNumber((G * m1 * m2) / (r * r));
                }},

                
              
                { "wea_phy_ke", args => new WNumber(0.5 * args[0].AsNumber() * Math.Pow(args[1].AsNumber(), 2)) },
                
                { "wea_phy_pe", args => new WNumber(args[0].AsNumber() * 9.81 * args[1].AsNumber()) },
                
                { "wea_phy_work", args => new WNumber(args[0].AsNumber() * args[1].AsNumber()) },

                
                { "wea_phy_wave_speed", args => new WNumber(args[0].AsNumber() * args[1].AsNumber()) }
            };
        }
    }
}
