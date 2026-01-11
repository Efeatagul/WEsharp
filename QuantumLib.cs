using System;
using System.Collections.Generic;

namespace WSharp
{
    public class QuantumLib : ILibrary
    {
        
        private const double H = 6.626e-34; 
        private const double H_BAR = 1.054e-34; 
        private const double C = 299792458; 

        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
                

                { "wea_quant_photon_e", args => new WNumber(H * args[0].AsNumber()) },

                
                { "wea_quant_photon_e_lambda", args => new WNumber((H * C) / args[0].AsNumber()) },

               
                { "wea_quant_debroglie", args => new WNumber(H / args[0].AsNumber()) },

               
                { "wea_quant_uncertainty_p", args => {
                    double deltaX = args[0].AsNumber();
                    return new WNumber(H_BAR / (2 * deltaX));
                }},

                
                { "wea_quant_tunneling", args => {
                    double m = args[0].AsNumber();
                    double L = args[1].AsNumber();
                    double V = args[2].AsNumber(); 
                    double E = args[3].AsNumber(); 

                    if (E >= V) return new WNumber(1); 

                    double K = Math.Sqrt(2 * m * (V - E)) / H_BAR;
                    double probability = Math.Exp(-2 * K * L);
                    return new WNumber(probability);
                }},

               
                { "wea_quant_prob", args => {
                    double amp = args[0].AsNumber();
                    return new WNumber(amp * amp);
                }},

              
                { "wea_quant_spin", args => {
                    double s = args[0].AsNumber();
                    return new WNumber(H_BAR * Math.Sqrt(s * (s + 1)));
                }}
            };
        }
    }
}
