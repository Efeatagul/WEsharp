using System;
using System.Collections.Generic;

namespace WSharp
{
    public static class AIFixer
    {
        
        private static readonly Dictionary<string, string> TypoDatabase = new Dictionary<string, string>
        {
            { "wea_emitt", "wea_emit" },
            { "wea_print", "wea_emit" }, 
            { "wea_unity", "wea_unit" },
            { "wea_plo", "wea_plot" },
            { "wea_plt", "wea_plot" },
            { "wea_cyle", "wea_cycle" },
            { "wea_cylce", "wea_cycle" },
            { "wea_iff", "wea_if" },
            { "wea_retun", "wea_return" },
            { "wea_eman", "wea_eman" }, 
            { "wea_fail", "wea_fail" }
        };

        public static string AnalyzeAndFix(string code, string errorMessage)
        {
           
            if (errorMessage.Contains("Bilinmeyen") || errorMessage.Contains("tanımlı değil") || errorMessage.Contains("Unexpected"))
            {
                foreach (var typo in TypoDatabase)
                {
                    if (code.Contains(typo.Key))
                    {
                        return $"🔍 **TANI:** Yazım hatası tespit edildi.\n❌ Yanlış: '{typo.Key}'\n✅ Doğru: '{typo.Value}'\n\n💡 **ÖNERİLEN DÜZELTME:**\nKomutu '{typo.Value}' olarak düzeltin.";
                    }
                }
            }

          
            int openBrace = code.Split('{').Length - 1;
            int closeBrace = code.Split('}').Length - 1;
            if (openBrace > closeBrace)
            {
                return $"🔍 **TANI:** Kod bloğu kapatılmamış.\n❌ Eksik: '}}' karakteri.\n\n💡 **ÖNERİLEN DÜZELTME:**\nKodun sonuna veya ilgili bloğun altına '}}' ekleyin.";
            }

            int openParen = code.Split('(').Length - 1;
            int closeParen = code.Split(')').Length - 1;
            if (openParen > closeParen)
            {
                return $"🔍 **TANI:** Parantez hatası.\n❌ Eksik: ')' karakteri.\n\n💡 **ÖNERİLEN DÜZELTME:**\nFonksiyon çağrısını ')' ile kapatmayı unutmayın.";
            }

           

            
            if (errorMessage.Contains("Tanimsiz degisken") || errorMessage.Contains("Variable"))
            {
                return $"🔍 **TANI:** Tanımsız değişken kullanıldı.\n\n💡 **ÖNERİLEN DÜZELTME:**\nBu değişkeni kullanmadan önce şöyle tanımlayın:\nwea_unit degisken_adi = 0";
            }

            
            if (errorMessage.Contains("expected") || errorMessage.Contains("bekleniyor"))
            {
                return "💡 **İPUCU:** Satır sonuna ';' koymayı veya bir parantezi kapatmayı unutmuş olabilirsiniz.";
            }

            
            if (errorMessage.Contains("wea_plot"))
            {
                return "💡 **İPUCU:** Grafik çizdirmek (wea_plot) için sayısal değerler gerekir. Örn: wea_plot(x)";
            }

            
            if (errorMessage.Contains("|>") || errorMessage.Contains("Pipe"))
            {
                return "💡 **İPUCU:** Pipe operatörü '|>' sadece fonksiyon zincirlerinde kullanılır. Örn: 16 |> sqrt() |> print()";
            }

            
            return $"🤖 **AI ANALİZİ:**\nSistem hatayı tam çözümleyemedi ancak şunlara dikkat edin:\n1. Satır sonlarında ';' var mı?\n2. Parantezlerin hepsi kapalı mı?\n3. WSharp komutları (wea_...) doğru yazıldı mı?\n\nOrijinal Hata: {errorMessage}";
        }
    }
}
