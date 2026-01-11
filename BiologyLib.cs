using System;
using System.Collections.Generic;

namespace WSharp
{
    public class BiologyLib : ILibrary
    {
        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
             
                { "wea_bio_dna_compl", args => {
                    string dna = args[0].AsString().ToUpper();
                    char[] res = new char[dna.Length];
                    for(int i=0; i<dna.Length; i++) {
                        if (dna[i] == 'A') res[i] = 'T';
                        else if (dna[i] == 'T') res[i] = 'A';
                        else if (dna[i] == 'C') res[i] = 'G';
                        else if (dna[i] == 'G') res[i] = 'C';
                        else res[i] = '?';
                    }
                    return new WString(new string(res));
                }},
            
                { "wea_bio_dna_rna", args => {
                    return new WString(args[0].AsString().ToUpper().Replace('T', 'U'));
                }},
               
                { "wea_bio_codon_count", args => new WNumber(Math.Floor(args[0].AsString().Length / 3.0)) },

              
                { "wea_bio_bmi", args => new WNumber(args[0].AsNumber() / (args[1].AsNumber() * args[1].AsNumber())) },
          
                { "wea_bio_bmr_male", args => {
                    double w = args[0].AsNumber(); 
                    double h = args[1].AsNumber();
                    double a = args[2].AsNumber(); 
                    return new WNumber(88.36 + (13.4 * w) + (4.8 * h) - (5.7 * a));
                }},

              
                { "wea_bio_sa_vol_ratio", args => {
                    double r = args[0].AsNumber();
                    double sa = 4 * Math.PI * r * r;
                    double vol = (4.0/3.0) * Math.PI * r * r * r;
                    return new WNumber(sa / vol);
                }}
            };
        }
    }
}
