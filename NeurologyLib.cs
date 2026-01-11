using System;
using System.Collections.Generic;

namespace WSharp
{
    public class NeurologyLib : ILibrary
    {
       
        private const double FARADAY = 96485.33; 
        private const double GAS_CONST = 8.314;  
        private const double TEMP_BODY = 310.15; 

        public Dictionary<string, Func<List<WValue>, WValue>> GetFunctions()
        {
            return new Dictionary<string, Func<List<WValue>, WValue>>
            {
                
                { "wea_neuro_nernst_adv", args => {
                    double co = args[0].AsNumber();
                    double ci = args[1].AsNumber();
                    double z = args[2].AsNumber();
                    double T = args.Count > 3 ? args[3].AsNumber() : TEMP_BODY;

                    if (ci <= 0) return new WNumber(0);
                    double val = ((GAS_CONST * T) / (z * FARADAY)) * Math.Log(co / ci);
                    return new WNumber(val * 1000); 
                }},

                
                { "wea_neuro_ghk_flux", args => {
                    double Vm = args[0].AsNumber() / 1000.0; 
                    double P = args[1].AsNumber();
                    double co = args[2].AsNumber();
                    double ci = args[3].AsNumber();
                    double z = args[4].AsNumber();
                    double T = TEMP_BODY;

                    double phi = (Vm * z * FARADAY) / (GAS_CONST * T);
                    double expPhi = Math.Exp(-phi);
                    
                    
                    double numerator = phi * (ci - (co * expPhi));
                    double denominator = 1 - expPhi;

                    double J = P * z * FARADAY * (numerator / denominator);
                    return new WNumber(J);
                }},

                { "wea_neuro_lif_step", args => {
                    double v = args[0].AsNumber();
                    double v_rest = args[1].AsNumber();
                    double tau = args[2].AsNumber();
                    double R = args[3].AsNumber();   
                    double I = args[4].AsNumber();   
                    double dt = args[5].AsNumber();  

                    double dv = (dt / tau) * (v_rest - v + (R * I));
                    return new WNumber(v + dv);
                }},

                
                { "wea_neuro_izhi_v", args => {
                    double v = args[0].AsNumber();
                    double u = args[1].AsNumber();
                    double I = args[2].AsNumber();
                    double dt = args.Count > 3 ? args[3].AsNumber() : 1.0;

                    double dv = (0.04 * v * v) + (5 * v) + 140 - u + I;
                    return new WNumber(v + (dv * dt));
                }},

               
                { "wea_neuro_izhi_u", args => {
                    double v = args[0].AsNumber();
                    double u = args[1].AsNumber();
                    double a = args[2].AsNumber();
                    double b = args[3].AsNumber();
                    double dt = args.Count > 4 ? args[4].AsNumber() : 1.0;

                    double du = a * ((b * v) - u);
                    return new WNumber(u + (du * dt));
                }},

               
                { "wea_neuro_stdp", args => {
                    double dt = args[0].AsNumber();
                    double tauPos = 20.0; 
                    double tauNeg = 20.0; 
                    double A_pos = 1.0;  
                    double A_neg = 1.0;   

                    if (args.Count > 1) tauPos = args[1].AsNumber();
                    
                   
                    if (dt > 0)
                        return new WNumber(A_pos * Math.Exp(-dt / tauPos));
                    
                   
                    else if (dt < 0)
                        return new WNumber(-A_neg * Math.Exp(dt / tauNeg)); 
                    
                    return new WNumber(0);
                }},

                
                { "wea_neuro_sigmoid", args => {
                    double x = args[0].AsNumber();
                    return new WNumber(1.0 / (1.0 + Math.Exp(-x)));
                }},

                
                { "wea_neuro_relu", args => {
                    return new WNumber(Math.Max(0, args[0].AsNumber()));
                }},

                
               
                { "wea_neuro_entropy", args => {
                    double p = args[0].AsNumber();
                    if (p <= 0 || p >= 1) return new WNumber(0);
                    return new WNumber(-(p * Math.Log(p, 2) + (1-p) * Math.Log(1-p, 2)));
                }},

               
                { "wea_neuro_fano", args => {
                    double variance = args[0].AsNumber();
                    double mean = args[1].AsNumber();
                    if (mean == 0) return new WNumber(0);
                    return new WNumber(variance / mean);
                }}
            };
        }
    }
}
