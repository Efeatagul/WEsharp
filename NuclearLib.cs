#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
    public static class NucConsts
    {
        public const double amu_to_MeV = 931.5; // 1 Atomik Kütle Birimi = 931.5 MeV Enerji
        public const double c = 299792458;      // Işık Hızı
        public const double e = 1.602e-19;      // Elementer Yük
    }

    public class Decay
    {
        public static string CalculateDecay(double N0, double halfLife, double time)
        {
            double lambda = Math.Log(2) / halfLife;
            double N_final = N0 * Math.Exp(-lambda * time);
            double activity = lambda * N_final;

            return $"Kalan Çekirdek: {N_final:F2} | Anlık Aktivite: {activity:E2} Bq [Image of radioactive decay graph]";
        }
    }

    public class Fission
    {
        public static string BindingEnergy(double massDefect_amu)
        {
            double energy_MeV = massDefect_amu * NucConsts.amu_to_MeV;
            return $"Açığa Çıkan Enerji: {energy_MeV:F2} MeV";
        }

        public static string ChainReaction(double k_eff)
        {
            if (k_eff < 1) return "Reaktör Sönüyor (Sub-critical)";
            if (k_eff == 1) return "Reaktör Kararlı (Critical) [Image of nuclear fission chain reaction]";
            return "DİKKAT! ERİME RİSKİ (Super-critical - Çernobil Durumu!)";
        }
    }

    public class Dosimetry
    {
        public static string RadiationDose(string rayType, double energy_J, double bodyMass_kg)
        {
            double absorbedDose = energy_J / bodyMass_kg; // Gray (Gy)
            double qualityFactor = 1; // Beta, Gama, X-Ray

            if (rayType.ToLower() == "alpha") qualityFactor = 20; // Alfa çok yıkıcıdır
            if (rayType.ToLower() == "neutron") qualityFactor = 10;

            double equivalentDose = absorbedDose * qualityFactor; // Sievert (Sv)

            string risk = equivalentDose > 1.0 ? "ÖLÜMCÜL" : "Kabul Edilebilir";
            return $"Doz: {equivalentDose:F4} Sv ({equivalentDose * 100:F2} Rem) | Risk: {risk} [Image of radiation shielding penetration]";
        }
    }

    public class NucDecayFunc : IWCallable
    {
        public int Arity() => 3; // (N0, YarıÖmür, Süre)
        public WValue Call(Interpreter interpreter, List<WValue> args) => new WValue(Decay.CalculateDecay(args[0].AsNumber(), args[1].AsNumber(), args[2].AsNumber()));
        public override string ToString() => "<native fn nuc_decay>";
    }

    public class NucEnergyFunc : IWCallable
    {
        public int Arity() => 1; // Kütle Kaybı (amu)
        public WValue Call(Interpreter interpreter, List<WValue> args) => new WValue(Fission.BindingEnergy(args[0].AsNumber()));
        public override string ToString() => "<native fn nuc_energy>";
    }

    public class NucReactorFunc : IWCallable
    {
        public int Arity() => 1; // k_eff
        public WValue Call(Interpreter interpreter, List<WValue> args) => new WValue(Fission.ChainReaction(args[0].AsNumber()));
        public override string ToString() => "<native fn nuc_reactor>";
    }

    public class NucDoseFunc : IWCallable
    {
        public int Arity() => 3; // (IşınTürü, Enerji, Kütle)
        public WValue Call(Interpreter interpreter, List<WValue> args) => new WValue(Dosimetry.RadiationDose(args[0].AsString(), args[1].AsNumber(), args[2].AsNumber()));
        public override string ToString() => "<native fn nuc_dose>";
    }
}
