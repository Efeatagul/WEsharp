using System;
using System.Collections.Generic;

namespace WSharp
{
    public class NeurologyLib : ILibrary
    {
       
        private const double FARADAY = 96485.3321;   
        private const double GAS_CONST = 8.314462;    
        private const double TEMP_BODY = 310.15;     
        private const double E_CHARGE = 1.602e-19;   

        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
               
                { "wea_wneura_run", args => {
                    string script = args[0].AsString();
                    string parametre = args.Count > 1 ? args[1].AsString() : "";
                    
                    
                    string sonuc = PythonBridge.Run(script, parametre);
                    return new WString(sonuc);
                }},

                
                { "wea_neura_create", args => {
                    string netName = args[0].AsString();
                    string layers = args[1].AsString();
                    string result = PythonBridge.Run("create_network.py", $"--name {netName} --layers {layers}");
                    return new WString(result);
                }},

                
                { "wea_neura_train", args => {
                    string netName = args[0].AsString();
                    string dataSet = args[1].AsString();
                    double epochs = args[2].AsNumber();
                    string result = PythonBridge.Run("train_network.py", $"--name {netName} --data {dataSet} --epochs {epochs}");
                    return new WString(result);
                }},

                
                { "wea_neuro_nernst", args => {
                    double co = args[0].AsNumber(); 
                    double ci = args[1].AsNumber();
                    double z = args[2].AsNumber();  
                    double T = args.Count > 3 ? args[3].AsNumber() : TEMP_BODY;

                    if (ci <= 0 || co <= 0) return new WNumber(0);

                    double val = ((GAS_CONST * T) / (z * FARADAY)) * Math.Log(co / ci);
                    return new WNumber(val * 1000);
                }},

                
                { "wea_neuro_ghk_voltage", args => {
                    double Pk = args[0].AsNumber(); double Pna = args[1].AsNumber(); double Pcl = args[2].AsNumber();
                    double Ko = args[3].AsNumber(); double Ki = args[4].AsNumber();
                    double Nao = args[5].AsNumber(); double Nai = args[6].AsNumber();
                    double Clo = args[7].AsNumber(); double Cli = args[8].AsNumber();
                    double T = TEMP_BODY;

                    double num = (Pk * Ko) + (Pna * Nao) + (Pcl * Cli);
                    double den = (Pk * Ki) + (Pna * Nai) + (Pcl * Clo);

                    double val = ((GAS_CONST * T) / FARADAY) * Math.Log(num / den);
                    return new WNumber(val * 1000);
                }},

              
                { "wea_neuro_ghk_current", args => {
                    double Vm = args[0].AsNumber() / 1000.0; 
                    double P = args[1].AsNumber();          
                    double Xo = args[2].AsNumber();          
                    double Xi = args[3].AsNumber();          
                    double z = args[4].AsNumber();           
                    double T = TEMP_BODY;

                    double xi = (z * Vm * FARADAY) / (GAS_CONST * T);
                    
                   
                    if (Math.Abs(1 - Math.Exp(-xi)) < 1e-9) return new WNumber(0);

                    double I = P * z * FARADAY * xi * ((Xi - (Xo * Math.Exp(-xi))) / (1 - Math.Exp(-xi)));
                    return new WNumber(I);
                }},

                
                { "wea_neuro_hh_alpha_n", args => {
                    double V = args[0].AsNumber();
                    double num = 0.01 * (V + 55);
                    double den = 1 - Math.Exp(-(V + 55) / 10.0);
                    return new WNumber(Math.Abs(den) < 1e-9 ? 0.1 : num / den);
                }},

               
                { "wea_neuro_hh_beta_n", args => new WNumber(0.125 * Math.Exp(-(args[0].AsNumber() + 65) / 80.0)) },

              
                { "wea_neuro_hh_alpha_m", args => {
                    double V = args[0].AsNumber();
                    double num = 0.1 * (V + 40);
                    double den = 1 - Math.Exp(-(V + 40) / 10.0);
                    return new WNumber(Math.Abs(den) < 1e-9 ? 1.0 : num / den);
                }},

                
                { "wea_neuro_hh_beta_m", args => new WNumber(4.0 * Math.Exp(-(args[0].AsNumber() + 65) / 18.0)) },

               
                { "wea_neuro_hh_alpha_h", args => new WNumber(0.07 * Math.Exp(-(args[0].AsNumber() + 65) / 20.0)) },

                
                { "wea_neuro_hh_beta_h", args => new WNumber(1.0 / (1.0 + Math.Exp(-(args[0].AsNumber() + 35) / 10.0))) },

                
                { "wea_neuro_syn_ampa", args => {
                    double g = args[0].AsNumber();
                    double V = args[1].AsNumber(); 
                    double E = 0;                 
                    return new WNumber(g * (V - E));
                }},

               
                { "wea_neuro_syn_nmda", args => {
                    double g = args[0].AsNumber();
                    double V = args[1].AsNumber();
                    double Mg = args[2].AsNumber(); 
                    
                    double block = 1.0 / (1.0 + (Mg * Math.Exp(-0.062 * V) / 3.57));
                    return new WNumber(g * block * (V - 0));
                }},

               
                { "wea_neuro_syn_gaba", args => {
                    double g = args[0].AsNumber();
                    double V = args[1].AsNumber();
                    double E_cl = -70.0; 
                    return new WNumber(g * (V - E_cl));
                }},

              
                
                { "wea_neuro_cable_lambda", args => {
                    double Rm = args[0].AsNumber(); 
                    double Ri = args[1].AsNumber(); 
                    double d = args[2].AsNumber();
                    return new WNumber(Math.Sqrt((Rm / Ri) * (d / 4.0)));
                }},

                
                { "wea_neuro_cable_tau", args => {
                    double Rm = args[0].AsNumber();
                    double Cm = args[1].AsNumber(); 
                    return new WNumber(Rm * Cm);
                }},

                
                { "wea_neuro_cable_decay", args => {
                    double V0 = args[0].AsNumber(); 
                    double x = args[1].AsNumber();  
                    double lambda = args[2].AsNumber();
                    return new WNumber(V0 * Math.Exp(-x / lambda));
                }},

               
                { "wea_neuro_bcm", args => {
                    double pre = args[0].AsNumber();  
                    double post = args[1].AsNumber(); 
                    double theta = args[2].AsNumber(); 
                    double rate = 0.01;

                    return new WNumber(rate * pre * post * (post - theta));
                }},

             
                { "wea_neuro_oja", args => {
                    double w = args[0].AsNumber();
                    double x = args[1].AsNumber(); 
                    double y = args[2].AsNumber(); 
                    double rate = 0.01;

                    return new WNumber(w + rate * (y * (x - y * w)));
                }},

              
                { "wea_neuro_rate_sigmoid", args => {
                    double I = args[0].AsNumber();
                    double gain = args[1].AsNumber();
                    double bias = args[2].AsNumber();
                    return new WNumber(1.0 / (1.0 + Math.Exp(-gain * (I - bias))));
                }},

               
                { "wea_neuro_cv_isi", args => {
                    double stdDev = args[0].AsNumber();
                    double mean = args[1].AsNumber();
                    if (mean == 0) return new WNumber(0);
                    return new WNumber(stdDev / mean);
                }},

               
                { "wea_neuro_mutual_info", args => {
                    double hX = args[0].AsNumber();
                    double hY = args[1].AsNumber();
                    double hXY = args[2].AsNumber();
                    return new WNumber(hX + hY - hXY);
                }}
            };
        }
    }
}
