#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace WSharp
{
    public class PeriodicTable
    {
        public class Element
        {
            public int AtomicNumber;
            public string Symbol;
            public string Name;
            public double Mass; // g/mol
            public double Electronegativity;
            public string GroupBlock;
        }

        public static Dictionary<string, Element> Elements = new Dictionary<string, Element>(StringComparer.OrdinalIgnoreCase)
        {
            {"H",  new Element{ AtomicNumber=1,  Symbol="H",  Name="Hidrojen", Mass=1.008,  Electronegativity=2.20, GroupBlock="Ametal"}},
            {"He", new Element{ AtomicNumber=2,  Symbol="He", Name="Helyum",   Mass=4.002,  Electronegativity=0.00, GroupBlock="Soygaz"}},
            {"Li", new Element{ AtomicNumber=3,  Symbol="Li", Name="Lityum",   Mass=6.94,   Electronegativity=0.98, GroupBlock="Metal"}},
            {"C",  new Element{ AtomicNumber=6,  Symbol="C",  Name="Karbon",   Mass=12.011, Electronegativity=2.55, GroupBlock="Ametal"}},
            {"N",  new Element{ AtomicNumber=7,  Symbol="N",  Name="Azot",     Mass=14.007, Electronegativity=3.04, GroupBlock="Ametal"}},
            {"O",  new Element{ AtomicNumber=8,  Symbol="O",  Name="Oksijen",  Mass=15.999, Electronegativity=3.44, GroupBlock="Ametal"}},
            {"F",  new Element{ AtomicNumber=9,  Symbol="F",  Name="Flor",     Mass=18.998, Electronegativity=3.98, GroupBlock="Ametal"}},
            {"Na", new Element{ AtomicNumber=11, Symbol="Na", Name="Sodyum",   Mass=22.990, Electronegativity=0.93, GroupBlock="Metal"}},
            {"Cl", new Element{ AtomicNumber=17, Symbol="Cl", Name="Klor",     Mass=35.45,  Electronegativity=3.16, GroupBlock="Ametal"}},
            {"K",  new Element{ AtomicNumber=19, Symbol="K",  Name="Potasyum", Mass=39.098, Electronegativity=0.82, GroupBlock="Metal"}},
            {"Ca", new Element{ AtomicNumber=20, Symbol="Ca", Name="Kalsiyum", Mass=40.078, Electronegativity=1.00, GroupBlock="Metal"}},
            {"Fe", new Element{ AtomicNumber=26, Symbol="Fe", Name="Demir",    Mass=55.845, Electronegativity=1.83, GroupBlock="Metal"}},
            {"Cu", new Element{ AtomicNumber=29, Symbol="Cu", Name="Bakır",    Mass=63.546, Electronegativity=1.90, GroupBlock="Metal"}},
            {"Ag", new Element{ AtomicNumber=47, Symbol="Ag", Name="Gümüş",    Mass=107.87, Electronegativity=1.93, GroupBlock="Metal"}},
            {"Au", new Element{ AtomicNumber=79, Symbol="Au", Name="Altın",    Mass=196.97, Electronegativity=2.54, GroupBlock="Metal"}}
        };

        public static string GetInfo(string symbol)
        {
            if (Elements.TryGetValue(symbol, out Element e))
            {
                string config = GetElectronConfiguration(e.AtomicNumber);
                return $"[{e.Symbol}] {e.Name} | No: {e.AtomicNumber} | Kütle: {e.Mass} | EN: {e.Electronegativity} | Grup: {e.GroupBlock}\n   -> Elektron Dizilimi: {config}";
            }
            return "Bilinmeyen element.";
        }

        public static string GetElectronConfiguration(int number)
        {
            string[] orbitals = { "1s", "2s", "2p", "3s", "3p", "4s", "3d", "4p", "5s", "4d" };
            int[] capacities = { 2, 2, 6, 2, 6, 2, 10, 6, 2, 10 };
            StringBuilder sb = new StringBuilder();
            int remaining = number;

            for (int i = 0; i < orbitals.Length; i++)
            {
                if (remaining <= 0) break;
                int fill = Math.Min(remaining, capacities[i]);
                sb.Append($"{orbitals[i]}^{fill} ");
                remaining -= fill;
            }
            return sb.ToString().Trim();
        }
    }

    public class Stoichiometry
    {
        public static double CalculateMolarMass(string formula)
        {
            string pattern = @"([A-Z][a-z]*)(\d*)";
            MatchCollection matches = Regex.Matches(formula, pattern);
            double totalMass = 0;

            foreach (Match m in matches)
            {
                string symbol = m.Groups[1].Value;
                string countStr = m.Groups[2].Value;
                int count = string.IsNullOrEmpty(countStr) ? 1 : int.Parse(countStr);

                if (PeriodicTable.Elements.TryGetValue(symbol, out var elem))
                    totalMass += elem.Mass * count;
                else
                    throw new Exception($"Bilinmeyen element sembolü: {symbol}");
            }
            return totalMass;
        }

        public static string IdealGasLaw(double P, double V, double n, double T_Celsius)
        {
            double R = 0.0821;
            double T_Kelvin = T_Celsius + 273.15;

            if (P == 0) return $"Basınç (P) = {(n * R * T_Kelvin) / V} atm";
            if (V == 0) return $"Hacim (V) = {(n * R * T_Kelvin) / P} Litre";
            if (n == 0) return $"Mol (n) = {(P * V) / (R * T_Kelvin)} mol";
            if (T_Celsius == -273.15) return $"Sıcaklık (T) = {(P * V) / (n * R) - 273.15} °C";

            return "Tüm değerler dolu, hesaplanacak değişkeni 0 girin.";
        }
    }

    public class Bonding
    {
        public static string AnalyzeBond(string el1, string el2)
        {
            if (!PeriodicTable.Elements.ContainsKey(el1) || !PeriodicTable.Elements.ContainsKey(el2))
                return "Element bulunamadı.";

            var e1 = PeriodicTable.Elements[el1];
            var e2 = PeriodicTable.Elements[el2];

            double diff = Math.Abs(e1.Electronegativity - e2.Electronegativity);
            string bondType = "";

            if (diff > 1.7) bondType = "İyonik Bağ (Elektron alışverişi)";
            else if (diff > 0.4) bondType = "Polar Kovalent Bağ (Kutuplu paylaşım)";
            else bondType = "Apolar Kovalent Bağ (Eşit paylaşım)";

            return $"Bağ: {e1.Symbol}-{e2.Symbol} | EN Farkı: {diff:F2} | Tür: {bondType}";
        }
    }

    public class PhysicalChem
    {
        public static string GibbsFreeEnergy(double deltaH, double deltaS, double TempCelsius)
        {
            double T = TempCelsius + 273.15;
            double deltaG = deltaH - (T * deltaS / 1000.0);

            string spont = deltaG < 0 ? "İSTEMLİ (Spontane)" : "İSTEMSİZ (Non-spontane)";
            return $"ΔG = {deltaG:F2} kJ/mol | Durum: {spont} | (T={T}K)";
        }
    }

    public class OrganicChem
    {
        public static string NameAlkane(int carbons)
        {
            string[] prefixes = { "", "Met", "Et", "Prop", "Büt", "Pent", "Heks", "Hept", "Okt", "Non", "Dek" };
            if (carbons > 10) return "Polimer/Uzun Zincir";
            return prefixes[carbons] + "an (Alkan) | Formül: C" + carbons + "H" + (2 * carbons + 2);
        }

        public static string AnalyzeFunctionalGroup(string formula)
        {
            if (formula.Contains("COOH")) return "Karboksilik Asit (-COOH) | Asidik özellik gösterir.";
            if (formula.Contains("OH")) return "Alkol (-OH) | Hidrojen bağı yapabilir.";
            if (formula.Contains("NH2")) return "Amin (-NH2) | Bazik özellik gösterir.";
            if (formula.Contains("CHO")) return "Aldehit (-CHO) | Yükseltgenebilir.";
            return "Hidrokarbon veya bilinmeyen grup.";
        }
    }

    public class QuantumChemistry
    {
        // 1D Kuantum Kutusu (Particle in a 1D Box) - McQuarrie
        public static string ParticleInBox(int n, double L, double x)
        {
            if (x < 0 || x > L) return "Elektron kutunun dışında olamaz (Olasılık: 0).";

            double coeff = Math.Sqrt(2.0 / L);
            double waveFunction = coeff * Math.Sin((n * Math.PI * x) / L);
            double probDensity = Math.Pow(waveFunction, 2);

            return $"Kuantum Durumu (n={n}): x={x}nm konumunda Elektron Olasılık Yoğunluğu: {probDensity:F4}";
        }

        // HOMO/LUMO Orbitalleri
        public static string MolecularOrbital(string type)
        {
            if (type.ToUpper() == "HOMO") return "HOMO: En Yüksek Enerjili Dolu Orbital (Elektron Verme Kapasitesi).";
            if (type.ToUpper() == "LUMO") return "LUMO: En Düşük Enerjili Boş Orbital (Elektron Alma Kapasitesi).";
            return "Bilinmeyen Orbital.";
        }
    }

    public class Spectroscopy
    {
        // Kızılötesi (IR) Spektroskopi Zirveleri
        public static string Infrared(string bond)
        {
            bond = bond.ToUpper();
            if (bond == "C=O") return "IR Zirvesi: ~1700 cm^-1 (Güçlü absorbsiyon, Karbonil grubu).";
            if (bond == "O-H") return "IR Zirvesi: 3200-3600 cm^-1 (Geniş bant, Hidrojen Bağı).";
            if (bond == "C-H") return "IR Zirvesi: 2850-3000 cm^-1 (Alifatik gerilme).";
            return "Veritabanında bu bağın IR bilgisi yok.";
        }
    }

    public class AdvancedOrganic
    {
        // SN1 ve SN2 Reaksiyon Mekanizmaları - Carey/Sundberg
        public static string ReactionMechanism(string substrate, string nucleophile)
        {
            substrate = substrate.ToLower();
            nucleophile = nucleophile.ToLower();

            if (substrate.Contains("tersiyer") || substrate.Contains("tert"))
                return $"MEKANİZMA: SN1 (Karbokasyon Ara Ürünü). Nükleofil gücü önemli değil. Rasemizasyon beklenir.";

            if (substrate.Contains("primer") || substrate.Contains("metil"))
                return $"MEKANİZMA: SN2 (Tek basamaklı Geçiş Hali). Konfigürasyon (R->S) ters döner.";

            return "MEKANİZMA: SN1 ve SN2 yarışabilir (Sekonder substrat). Çözücüye bağlı.";
        }
    }

    // =============================================================
    // WSHARP ENTEGRASYON KATMANI (WRAPPER FUNCTIONS)
    // =============================================================


    public class ChemElementFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(PeriodicTable.GetInfo(a[0].AsString()));
        public override string ToString() => "<native fn chem_element>";
    }

    public class ChemMassFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Stoichiometry.CalculateMolarMass(a[0].AsString()));
        public override string ToString() => "<native fn chem_mass>";
    }

    public class ChemGasFunc : IWCallable
    {
        public int Arity() => 4;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Stoichiometry.IdealGasLaw(a[0].AsNumber(), a[1].AsNumber(), a[2].AsNumber(), a[3].AsNumber()));
        public override string ToString() => "<native fn chem_gas>";
    }

    public class ChemBondFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Bonding.AnalyzeBond(a[0].AsString(), a[1].AsString()));
        public override string ToString() => "<native fn chem_bond>";
    }

    public class ChemGibbsFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(PhysicalChem.GibbsFreeEnergy(a[0].AsNumber(), a[1].AsNumber(), a[2].AsNumber()));
        public override string ToString() => "<native fn chem_gibbs>";
    }

    public class ChemOrganicFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a)
        {
            string inp = a[0].AsString();
            if (double.TryParse(inp, out double c)) return new WValue(OrganicChem.NameAlkane((int)c));
            return new WValue(OrganicChem.AnalyzeFunctionalGroup(inp));
        }
        public override string ToString() => "<native fn chem_organic>";
    }

    public class ChemQuantumFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(QuantumChemistry.ParticleInBox((int)a[0].AsNumber(), a[1].AsNumber(), a[2].AsNumber()));
        public override string ToString() => "<native fn chem_quantum>";
    }

    public class ChemSpectraFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(Spectroscopy.Infrared(a[0].AsString()));
        public override string ToString() => "<native fn chem_spectra>";
    }

    public class ChemMechFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter i, List<WValue> a) => new WValue(AdvancedOrganic.ReactionMechanism(a[0].AsString(), a[1].AsString()));
        public override string ToString() => "<native fn chem_mech>";
    }
}
