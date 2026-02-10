#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WSharp
{
    public class BioMethodology
    {
        public static string TestHypothesis(string observation, string hypothesis, bool experimentSuccess)
        {
            if (!experimentSuccess)
                return $"SONUÇ: Hipotez ({hypothesis}) reddedildi. Deney sonuçları gözlemle ({observation}) uyuşmuyor. Yeni hipotez kur.";
            return $"SONUÇ: Hipotez ({hypothesis}) desteklendi. Teoriye dönüşme potansiyeli var.";
        }

        public static string CheckLifeSigns(bool metabolism, bool reproduction, bool reaction, bool homeostasis)
        {
            if (metabolism && reproduction && reaction && homeostasis)
                return "DURUM: Canlılık belirtileri tam. Organizma aktif.";
            if (!metabolism) return "DURUM: Cansız veya Virüs (Metabolizma yok).";
            return "DURUM: Canlılık şüpheli veya dormant (uyku) halinde.";
        }
    }

    public class BioTaxonomy
    {
        public static string ClassifyOrganism(string name)
        {
            name = name.ToLower();
            if (name == "e.coli" || name == "siyanobakteri") return "DOMAIN: Bakteri | TÜR: Prokaryot | DUYARLILIK: Antibiyotik.";
            if (name == "metanojen" || name == "termofil") return "DOMAIN: Arke | TÜR: Prokaryot | ORTAM: Ekstrem koşullar.";
            if (name == "amip" || name == "paramesyum") return "DOMAIN: Ökaryot | ALEM: Protista";
            if (name == "maya" || name == "küf") return "DOMAIN: Ökaryot | ALEM: Mantar (Fungi) | BESLENME: Heterotrof";
            if (name == "papatya" || name == "çam") return "DOMAIN: Ökaryot | ALEM: Bitki (Plantae) | BESLENME: Ototrof";
            if (name == "insan" || name == "kedi") return "DOMAIN: Ökaryot | ALEM: Hayvan (Animalia) | BESLENME: Heterotrof";
            return "VERİTABANI: Bilinmeyen organizma.";
        }
    }

    public class BioCell
    {
        public static string TransportSubstance(double cellConcentration, double envConcentration, bool isActiveTransport)
        {
            if (isActiveTransport)
                return "MEKANİZMA: Aktif Taşıma. ATP harcandı. (Az yoğun -> Çok yoğun)";

            if (cellConcentration > envConcentration)
                return "MEKANİZMA: Ozmoz (Su hücreye girer). Hücre şişer (Deplazmoliz/Turgor).";
            else if (cellConcentration < envConcentration)
                return "MEKANİZMA: Ozmoz (Su dışarı çıkar). Hücre büzülür (Plazmoliz).";

            return "MEKANİZMA: Denge hali (İzotonik). Net geçiş yok.";
        }
    }

    public class BioEnergetics
    {
        public static double PhotosynthesisRate(double light, double co2, double h2o, double temp)
        {
            double tempFactor = (temp > 45 || temp < 0) ? 0 : 100 - Math.Abs(30 - temp) * 3;
            if (tempFactor < 0) tempFactor = 0;
            double rate = Math.Min(Math.Min(light, co2), Math.Min(h2o, tempFactor));
            return rate;
        }

        public static string Respiration(int glucose, string type)
        {
            type = type.ToLower();
            if (type == "oksijenli" || type == "aerobic")
                return $"GİRDİ: {glucose} Glikoz -> ÇIKTI: {glucose * 32} ATP + CO2 + H2O (Mitokondride)";
            if (type == "laktik")
                return $"GİRDİ: {glucose} Glikoz -> ÇIKTI: {glucose * 2} ATP + Laktik Asit (Yorgunluk)";
            if (type == "etilalkol")
                return $"GİRDİ: {glucose} Glikoz -> ÇIKTI: {glucose * 2} ATP + Etanol + CO2";
            return "HATA: Geçersiz solunum türü.";
        }
    }

    public class HumanPhysiology
    {
        public static string ImmuneResponse(string threatType, bool isVaccinated)
        {
            if (isVaccinated)
                return $"YANIT: Hafıza hücreleri (B-Lenfosit) aktif. {threatType} antikorlarla anında yok edildi. (İkincil Bağışıklık)";
            return $"YANIT: Doğal savunma (Makrofajlar) devrede. İnflamasyon başladı. {threatType} tanınmaya çalışılıyor. (Birincil Bağışıklık)";
        }

        public static string GasTransport(string location, double pO2)
        {
            if (location == "alveol" && pO2 > 100) return "Hb + O2 -> HbO2 (Oksihemoglobin oluştu).";
            if (location == "doku" && pO2 < 40) return "HbO2 -> Hb + O2 (Oksijen dokuya bırakıldı).";
            return "Stabil taşıma.";
        }

        public static string MenstrualCycle(int day)
        {
            if (day >= 1 && day <= 5) return "EVRE: Menstruasyon (Kanama). Progesteron düşük.";
            if (day >= 6 && day <= 13) return "EVRE: Folikül Evresi. Östrojen artıyor.";
            if (day == 14) return "EVRE: Ovulasyon (Yumurtlama). LH Hormonu Tavan Yaptı!";
            if (day >= 15 && day <= 28) return "EVRE: Korpus Luteum. Progesteron yüksek.";
            return "HATA: Döngü 1-28 gün arasındadır.";
        }
    }

    public class MolecularGenetics
    {
        private static readonly Dictionary<string, string> CodonTable = new Dictionary<string, string>()
        {
            {"AUG", "Metiyonin (START)"}, {"UUU", "Fenilalanin"}, {"UUC", "Fenilalanin"},
            {"UUA", "Lösin"}, {"UUG", "Lösin"}, {"GUU", "Valin"}, {"GUC", "Valin"},
            {"UCU", "Serin"}, {"CCU", "Prolin"}, {"ACU", "Treonin"}, {"GCU", "Alanin"},
            {"UAA", "DUR"}, {"UAG", "DUR"}, {"UGA", "DUR"}
        };

        public static string Transcribe(string dna)
        {
            StringBuilder mrna = new StringBuilder();
            foreach (char baseP in dna.ToUpper())
            {
                if (baseP == 'A') mrna.Append('U');
                else if (baseP == 'T') mrna.Append('A');
                else if (baseP == 'G') mrna.Append('C');
                else if (baseP == 'C') mrna.Append('G');
            }
            return mrna.ToString();
        }

        public static string Translate(string mrna)
        {
            StringBuilder protein = new StringBuilder();
            for (int i = 0; i < mrna.Length - 2; i += 3)
            {
                string codon = mrna.Substring(i, 3);
                if (CodonTable.ContainsKey(codon))
                {
                    string aa = CodonTable[codon];
                    if (aa == "DUR") break;
                    protein.Append(aa + "-");
                }
                else protein.Append("X-");
            }
            return protein.ToString().TrimEnd('-');
        }
    }

    public class Ecology
    {
        public static double TrophicTransfer(double energy, int level)
        {
            return energy * Math.Pow(0.10, level - 1);
        }
    }

    public class PlantBiology
    {
        public static string ApplyHormone(string hormone)
        {
            hormone = hormone.ToLower();
            if (hormone == "oksin") return "ETKİ: Uç büyümesi (Apikal dominans), Işığa yönelim.";
            if (hormone == "etilen") return "ETKİ: Meyve olgunlaşması, yaprak dökümü.";
            return "Bilinmeyen hormon.";
        }
    }

    public class Biochemistry
    {
        public static string EnzymeKinetics(double Vmax, double Km, double SubstrateConc)
        {
            double velocity = (Vmax * SubstrateConc) / (Km + SubstrateConc);
            return $"Tepkime Hızı (V0): {velocity:F4} uM/s | Enzim Kapasitesi: %{(velocity / Vmax) * 100:F1}";
        }

        public static string ProteinFolding(double deltaH, double deltaS, double tempKelvin)
        {
            double deltaG = deltaH - (tempKelvin * deltaS);
            if (deltaG < 0) return $"ΔG: {deltaG:F2} kJ/mol -> SPONTAN. Protein başarıyla 3B yapısını (Native State) aldı.";
            return $"ΔG: {deltaG:F2} kJ/mol -> NON-SPONTAN. Protein denatüre (bozuk) durumda. Şaperon desteği gerekiyor.";
        }
    }

    public class MolecularCell
    {
        public static string GPCR_Signaling(string ligand, string gProteinType)
        {
            gProteinType = gProteinType.ToLower();
            if (gProteinType == "gs") return $"LİGAND: {ligand} -> Adenilat Siklaz Aktif -> cAMP arttı -> PKA Aktif. (Uyarılma)";
            if (gProteinType == "gi") return $"LİGAND: {ligand} -> Adenilat Siklaz İnhibe -> cAMP düştü. (Baskılanma)";
            if (gProteinType == "gq") return $"LİGAND: {ligand} -> Fosfolipaz C Aktif -> IP3/DAG arttı -> Kalsiyum (Ca2+) salındı.";
            return "Bilinmeyen G-Protein yolağı.";
        }

        public static string CancerCheck(double dnaDamagePercent, bool isP53Active)
        {
            if (dnaDamagePercent < 10) return "DNA hasarı tolere edilebilir. Onarım mekanizmaları çalışıyor.";
            if (dnaDamagePercent >= 10 && isP53Active) return "KRİTİK HASAR! p53 geni Apoptosom'u tetikledi. Hücre İntiharı (Apoptoz) başladı.";
            return "⚠️ TEHLİKE! p53 inaktif. Hasarlı hücre bölünüyor. KANSER (Tümörijenez) başladı!";
        }
    }

    public class GeneticEngineering
    {
        public static string CrisprCut(string genome, string guideRNA, string pamSite = "NGG")
        {
            string targetDNA = MolecularGenetics.Transcribe(guideRNA).Replace("U", "T");
            if (genome.Contains(targetDNA))
            {
                return $"BAŞARILI: Cas9 proteini '{targetDNA}' bölgesini buldu ve PAM ({pamSite}) bitişiğinden çift zincir kesiği (DSB) oluşturdu. ";
            }
            return "BAŞARISIZ: Rehber RNA (gRNA) genomda hedef bölgeyle eşleşmedi. Off-target yok.";
        }

        public static string RNA_Silencing(double siRNA_Conc)
        {
            if (siRNA_Conc > 10.0) return "RISC Kompleksi aktif. Hedef mRNA tamamen parçalandı. Gen %100 susturuldu.";
            return $"RISC Kompleksi kısmi aktif. Gen %{siRNA_Conc * 10} oranında susturuldu (Knock-down).";
        }
    }

    public class AdvBioenergetics
    {
        public static string OxPhos(int NADH, int FADH2)
        {
            double totalATP = (NADH * 2.5) + (FADH2 * 1.5);
            double totalH_pumped = (NADH * 10) + (FADH2 * 6);
            return $"Pompalanan Proton (H+): {totalH_pumped} | ATP Sentaz Çıktısı: {totalATP} ATP";
        }
    }

    public class BioClassifyFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(BioTaxonomy.ClassifyOrganism(a[0].AsString()));
        public override string ToString() => "<native fn bio_classify>";
    }

    public class BioTransportFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(BioCell.TransportSubstance(a[0].AsNumber(), a[1].AsNumber(), a[2].AsBoolean()));
        public override string ToString() => "<native fn bio_transport>";
    }

    public class BioPhotoFunc : IWCallable
    {
        public int Arity() => 4;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(BioEnergetics.PhotosynthesisRate(a[0].AsNumber(), a[1].AsNumber(), a[2].AsNumber(), a[3].AsNumber()));
        public override string ToString() => "<native fn bio_photosynthesis>";
    }

    public class BioRespireFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(BioEnergetics.Respiration((int)a[0].AsNumber(), a[1].AsString()));
        public override string ToString() => "<native fn bio_respire>";
    }

    public class GenTranscribeFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(MolecularGenetics.Transcribe(a[0].AsString()));
        public override string ToString() => "<native fn gen_transcribe>";
    }

    public class GenTranslateFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(MolecularGenetics.Translate(a[0].AsString()));
        public override string ToString() => "<native fn gen_translate>";
    }

    public class EcoTransferFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Ecology.TrophicTransfer(a[0].AsNumber(), (int)a[1].AsNumber()));
        public override string ToString() => "<native fn eco_transfer>";
    }

    public class PhysioCheckFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a)
        {
            string sys = a[0].AsString().ToLower();
            if (sys == "cycle" || sys == "dongu") return new WValue(HumanPhysiology.MenstrualCycle((int)a[1].AsNumber()));
            if (sys == "immunity") return new WValue(HumanPhysiology.ImmuneResponse(a[1].AsString(), false));
            return new WValue("Bilinmeyen sistem.");
        }
        public override string ToString() => "<native fn physio_check>";
    }

    public class PlantHormoneFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(PlantBiology.ApplyHormone(a[0].AsString()));
        public override string ToString() => "<native fn plant_hormone>";
    }

    // --- İLERİ (PHD) BİYOLOJİ WRAPPERLARI ---
    public class BioEnzymeFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Biochemistry.EnzymeKinetics(a[0].AsNumber(), a[1].AsNumber(), a[2].AsNumber()));
        public override string ToString() => "<native fn bio_enzyme>";
    }

    public class BioProteinFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Biochemistry.ProteinFolding(a[0].AsNumber(), a[1].AsNumber(), a[2].AsNumber()));
        public override string ToString() => "<native fn bio_protein>";
    }

    public class BioGPCRFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(MolecularCell.GPCR_Signaling(a[0].AsString(), a[1].AsString()));
        public override string ToString() => "<native fn bio_gpcr>";
    }

    public class BioCancerFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(MolecularCell.CancerCheck(a[0].AsNumber(), a[1].AsBoolean()));
        public override string ToString() => "<native fn bio_cancer>";
    }

    public class BioCrisprFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(GeneticEngineering.CrisprCut(a[0].AsString(), a[1].AsString()));
        public override string ToString() => "<native fn bio_crispr>";
    }

    public class BioOxPhosFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(AdvBioenergetics.OxPhos((int)a[0].AsNumber(), (int)a[1].AsNumber()));
        public override string ToString() => "<native fn bio_oxphos>";
    }
}
