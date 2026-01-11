using System;
using System.Collections.Generic;

namespace WSharp
{
    public class ChemistryLib : ILibrary
    {
        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
           
                { "wea_chem_mol", args => new WNumber(args[0].AsNumber() / args[1].AsNumber()) },
           
                { "wea_chem_avogadro", args => new WNumber(6.022e23) },

              
                { "wea_chem_gas_p", args => {
                    double n = args[0].AsNumber();
                    double T = args[1].AsNumber();
                    double V = args[2].AsNumber();
                    return new WNumber((n * 0.0821 * T) / V);
                }},
             
                { "wea_chem_to_kelvin", args => new WNumber(args[0].AsNumber() + 273.15) },

             
                { "wea_chem_molarity", args => new WNumber(args[0].AsNumber() / args[1].AsNumber()) },
              
                { "wea_chem_ph", args => new WNumber(-Math.Log10(args[0].AsNumber())) },
              
                { "wea_chem_poh_to_ph", args => new WNumber(14 - args[0].AsNumber()) },
                
             
                { "wea_chem_dilution_v2", args => {
                    double m1 = args[0].AsNumber();
                    double v1 = args[1].AsNumber();
                    double m2 = args[2].AsNumber();
                    return new WNumber((m1 * v1) / m2);
                }}
            };
        }
    }
}
