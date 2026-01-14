using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
            { "wea_retun", "wea_return" }
        };

        public static string AnalyzeAndFix(string originalCode, string errorMessage)
        {
          
            if (errorMessage.Contains("Bilinmeyen fonksiyon") || errorMessage.Contains("tanımlı değil"))
            {
                foreach (var typo in TypoDatabase)
                {
                    if (originalCode.Contains(typo.Key))
                    {
                        return $"🔍 **TANI:** Yazım hatası tespit edildi.\n❌ Yanlış: '{typo.Key}'\n✅ Doğru: '{typo.Value}'\n\n💡 **ÖNERİLEN DÜZELTME:**\n{typo.Value}(...); komutunu kullanın.";
                    }
                }
            }

            
            int openBrace = originalCode.Split('{').Length - 1;
            int closeBrace = originalCode.Split('}').Length - 1;

            if (openBrace > closeBrace)
            {
                return $"🔍 **TANI:** Kod bloğu kapatılmamış.\n❌ Eksik: '}}' karakteri.\n\n💡 **ÖNERİLEN DÜZELTME:**\nKodun en sonuna '}}' ekleyin.";
            }

            int openParen = originalCode.Split('(').Length - 1;
            int closeParen = originalCode.Split(')').Length - 1;

            if (openParen > closeParen)
            {
                return $"🔍 **TANI:** Parantez hatası.\n❌ Eksik: ')' karakteri.\n\n💡 **ÖNERİLEN DÜZELTME:**\nFonksiyon çağrısını ')' ile kapatın.";
            }

            
            if (errorMessage.Contains("değişken") || errorMessage.Contains("Variable"))
            {
                return $"🔍 **TANI:** Tanımsız değişken kullanıldı.\n\n💡 **ÖNERİLEN DÜZELTME:**\nKullanmadan önce değişkeni tanımlayın:\nwea_unit degisken_adi = 0";
            }

            
            return $"🤖 **AI ANALİZİ:**\nHata Mesajı: '{errorMessage}'\n\nLütfen sözdizimini (syntax) kontrol edin. Noktalı virgül veya parantez hatası olabilir.";
        }
    }
}
