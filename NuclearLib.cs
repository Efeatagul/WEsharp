using System;
using System.Collections.Generic;

namespace WSharp
{
    public class NuclearLib : ILibrary
    {
       
        private const double C = 299792458; 
        private const double AVOGADRO = 6.022e23; 

        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
                
                { "wea_nuc_energy", args => {
                    double m = args[0].AsNumber();
                    return new WNumber(m * C * C);
                }},

            
                { "wea_nuc_binding_energy", args => {
                    double massDefect = args[0].AsNumber();
                    return new WNumber(massDefect * 931.5);
                }},

                
                { "wea_nuc_decay", args => {
                    double N0 = args[0].AsNumber();
                    double lambda = args[1].AsNumber();
                    double t = args[2].AsNumber();
                    return new WNumber(N0 * Math.Exp(-lambda * t));
                }},

               
                { "wea_nuc_half_life_remain", args => {
                    double N0 = args[0].AsNumber();
                    double t_half = args[1].AsNumber();
                    double t = args[2].AsNumber();
                    return new WNumber(N0 * Math.Pow(0.5, t / t_half));
                }},

                
                { "wea_nuc_activity", args => {
                    double lambda = args[0].AsNumber();
                    double N = args[1].AsNumber(); 
                    return new WNumber(lambda * N);
                }},

               
                { "wea_nuc_atoms", args => {
                    double mass = args[0].AsNumber();
                    double molarMass = args[1].AsNumber();
                    return new WNumber((mass / molarMass) * AVOGADRO);
                }}
            };
        }
    }
}
