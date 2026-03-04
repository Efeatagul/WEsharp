/* ======================================================================
 * WSHARP (We#) NEURO-ENGINE - WEAGW Ecosystem
 * Copyright (c) 2026 Efe Ata Gul. All rights reserved.
 * * This file is part of the WSharp project.
 * * OPEN SOURCE: Licensed under the GNU AGPL v3.0. You may use this 
 * file freely in open-source/academic projects provided you give 
 * clear attribution to "WSharp by Efe Ata Gul".
 * * COMMERCIAL: If you wish to use WSharp in closed-source, proprietary, 
 * or commercial products, you must purchase a WEAGW Commercial License.
 * ====================================================================== */
using System;
using System.Collections.Generic;

namespace WSharp
{
    public static class AIFixer
    {
       
        private static readonly Dictionary<string, string> TypoDatabase = new Dictionary<string, string>
        {
           
            { "cellula", "let" },
            { "operatio", "func" },
            { "si", "if" },
            { "aliter", "else" },
            { "cyclus", "while" },
            { "scribo", "print" },
            { "quaero", "input" },
            { "redeo", "return" },
            { "stop", "break" },
            { "pergo", "continue" },
            { "iniectio", "import" },
            { "verum", "true" },
            { "falsum", "false" },
            { "nihil", "null" },
            { "wea_eman", "try" },
            { "wea_fail", "catch" },
            { "wea_unit", "let" },
            { "wea_emit", "print" },
            { "wea_emitt", "print" },
            { "wea_print", "print" },
            { "wea_flow", "func" },
            { "wea_verify", "if" },
            { "wea_if", "if" },
            { "wea_iff", "if" },
            { "wea_else", "else" },
            { "wea_cycle", "while" },
            { "wea_cyle", "while" },
            { "wea_cylce", "while" },
            { "wea_return", "return" },
            { "wea_retun", "return" },
            { "wea_read", "input" },
            { "wea_import", "import" },
            { "wea_unity", "let" },
            { "wea_len", "len" },
            { "wea_push", "push" },
            { "wea_pop", "pop" },
            { "wea_map", "map" },
            { "wea_filter", "filter" },
            { "var", "let" },
            { "function", "func" },
            { "def", "func" },
            { "elif", "else if" },
            { "console.log", "print" },
            { "puts", "print" },
            { "echo", "print" },
            { "wea_neuro_create_custom", "creare_cortex" },
            { "wea_neuro_step", "simulare_gradus" },
            { "wea_neuro_connect_random", "nectere_temere" },
            { "wea_neuro_connect", "nectere_directe" },
            { "wea_neuro_get_voltage", "legere_tensio" },
            { "wea_neuro_get_spikes", "legere_pulsus" },
            { "wea_neuro_set_input", "injicere_stimulus" },
            { "wea_neuro_configure", "configurare_cortex" },
            { "le", "let" },
            { "lett", "let" },
            { "funct", "func" },
            { "fuction", "func" },
            { "retrun", "return" },
            { "retrn", "return" },
            { "pritn", "print" },
            { "pirnt", "print" },
            { "inport", "import" },
            { "improt", "import" }
        };

        public static string AnalyzeAndFix(string code, string errorMessage)
        {
           
            if (errorMessage.Contains("Bilinmeyen") || errorMessage.Contains("tanımlı değil") || errorMessage.Contains("Unexpected") || errorMessage.Contains("Beklenmeyen"))
            {
                foreach (var typo in TypoDatabase)
                {
                    if (code.Contains(typo.Key))
                    {
                        return $"🔍 **TANI:** Yazım hatası veya eski syntax tespit edildi.\n❌ Yanlış: '{typo.Key}'\n✅ Doğru: '{typo.Value}'\n\n💡 **ÖNERİLEN DÜZELTME:**\nKomutu '{typo.Value}' olarak değiştirin.\n\n📖 Referans: let (değişken), func (fonksiyon), if/else (koşul), print (yazdır), while (döngü)";
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
                return $"🔍 **TANI:** Tanımsız değişken kullanıldı.\n\n💡 **ÖNERİLEN DÜZELTME:**\nBu değişkeni kullanmadan önce şöyle tanımlayın:\nlet degisken_adi = 0";
            }

            
            if (errorMessage.Contains("expected") || errorMessage.Contains("bekleniyor"))
            {
                return "💡 **İPUCU:** Satır sonuna ';' koymayı veya bir parantezi kapatmayı unutmuş olabilirsiniz.";
            }

            
            if (errorMessage.Contains("plot"))
            {
                return "💡 **İPUCU:** Grafik çizdirmek için sayısal değerler gerekir. Örn: plot_live(x)";
            }

            
            if (errorMessage.Contains("|>") || errorMessage.Contains("Pipe"))
            {
                return "💡 **İPUCU:** Pipe operatörü '|>' sadece fonksiyon zincirlerinde kullanılır. Örn: 16 |> sqrt() |> print()";
            }

            
            return $"🤖 **AI ANALİZİ:**\nSistem hatayı tam çözümleyemedi ancak şunlara dikkat edin:\n1. Parantezlerin hepsi kapalı mı?\n2. Komutlar doğru yazıldı mı? (let, func, if, print, while...)\n3. Eski 'wea_' veya Latince syntax kullanıyor olabilir misiniz?\n\n📖 Hızlı Referans:\n  let x = 10         (değişken)\n  print x             (yazdır)\n  if x > 5 {{ }}      (koşul)\n  while x < 10 {{ }}  (döngü)\n\nOrijinal Hata: {errorMessage}";
        }
    }
}