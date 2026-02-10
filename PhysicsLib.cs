#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
    public static class PhysConsts
    {
        public const double g = 9.80665;
        public const double c = 299792458;
        public const double G = 6.674e-11;
        public const double k_e = 8.987e9;
        public const double h = 6.626e-34;
        public const double e = 1.602e-19;
        public const double R = 8.314;
        public const double N_A = 6.022e23;
    }

    public class Mechanics
    {
        public static string Motion(double v0, double a, double t)
        {
            double x = v0 * t + 0.5 * a * t * t;
            double v = v0 + a * t;
            return $"Konum Değişimi: {x:F2}m | Son Hız: {v:F2}m/s";
        }

        public static string Force(double m, double a)
        {
            return $"Net Kuvvet (F): {m * a:F2} Newton";
        }

        public static string Energy(double m, double v, double h)
        {
            double KE = 0.5 * m * v * v;
            double PE = m * PhysConsts.g * h;
            return $"Kinetik: {KE:F2}J | Potansiyel: {PE:F2}J | Mekanik: {(KE + PE):F2}J";
        }

        public static string Projectile(double v0, double angle)
        {
            double rad = angle * (Math.PI / 180.0);
            double range = (Math.Pow(v0, 2) * Math.Sin(2 * rad)) / PhysConsts.g;
            double height = (Math.Pow(v0 * Math.Sin(rad), 2)) / (2 * PhysConsts.g);
            return $"Menzil: {range:F2}m | Max Yükseklik: {height:F2}m";
        }

        public static string Gravitation(double m1, double m2, double r)
        {
            double F = PhysConsts.G * (m1 * m2) / (r * r);
            return $"Kütle Çekim Kuvveti: {F:E2} N";
        }
    }

    public class ThermoWaves
    {
        public static string IdealGas(double P, double V, double T_Celsius)
        {
            double T_K = T_Celsius + 273.15;
            double n = (P * V) / (PhysConsts.R * T_K);
            return $"Madde Miktarı: {n:F4} mol (Sıcaklık: {T_K}K)";
        }

        public static string WaveSpeed(double freq, double wavelength)
        {
            return $"Dalga Hızı: {freq * wavelength:F2} m/s";
        }
    }

    public class Electromagnetism
    {
        public static string Coulomb(double q1, double q2, double r)
        {
            double F = PhysConsts.k_e * Math.Abs(q1 * q2) / (r * r);
            string type = (q1 * q2 > 0) ? "İtme" : "Çekme";
            return $"Elektriksel Kuvvet: {F:E2} N ({type})";
        }

        public static string Circuit(double V, double R)
        {
            double I = V / R;
            double P = V * I;
            return $"Akım: {I:F2}A | Güç: {P:F2}W";
        }

        public static string Lorentz(double q, double v, double B)
        {
            double F = q * v * B;
            return $"Manyetik Kuvvet: {F:E2} N";
        }
    }

    public class ModernPhysics
    {
        public static string Snell(double n1, double angle1, double n2)
        {
            double rad1 = angle1 * (Math.PI / 180.0);
            double sin2 = (n1 * Math.Sin(rad1)) / n2;
            if (sin2 > 1) return "Tam Yansıma!";
            double angle2 = Math.Asin(sin2) * (180.0 / Math.PI);
            return $"Kırılma Açısı: {angle2:F2} derece";
        }

        public static string Relativity(double mass, double velocity)
        {
            if (velocity >= PhysConsts.c) return "HATA: Işık hızı aşılamaz!";

            double gamma = 1 / Math.Sqrt(1 - Math.Pow(velocity / PhysConsts.c, 2));
            double E_total = gamma * mass * Math.Pow(PhysConsts.c, 2);

            return $"Gamma Faktörü: {gamma:F4} | Toplam Enerji: {E_total:E2} J";
        }

        public static string PhotoElectric(double freq, double workFunc_eV)
        {
            double E_photon = PhysConsts.h * freq;
            double work_J = workFunc_eV * PhysConsts.e;
            double KE = E_photon - work_J;

            if (KE < 0) return "Foton enerjisi yetersiz.";
            return $"Koparılan Elektronun Kinetik Enerjisi: {KE:E2} J";
        }
    }

    public class PhysMechFunc : IWCallable
    {
        public int Arity() => 3; // (Mod, Değer1, Değer2)
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            string mode = args[0].AsString();
            double v1 = args[1].AsNumber();
            double v2 = args[2].AsNumber();

            if (mode == "motion") return new WValue(Mechanics.Motion(0, v1, v2)); // v0=0 kabul
            if (mode == "force") return new WValue(Mechanics.Force(v1, v2));
            if (mode == "projectile") return new WValue(Mechanics.Projectile(v1, v2));
            if (mode == "gravity") return new WValue(Mechanics.Gravitation(v1, v2, 1)); // r=1m test
            return new WValue("Bilinmeyen Mekanik Modu");
        }
        public override string ToString() => "<native fn phys_mech>";
    }

    // 2. Termodinamik Wrapper (phys_thermo)
    public class PhysThermoFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter interpreter, List<WValue> args) => new WValue(ThermoWaves.IdealGas(args[0].AsNumber(), args[1].AsNumber(), args[2].AsNumber()));
        public override string ToString() => "<native fn phys_thermo>";
    }

    // 3. Elektromanyetizma Wrapper (phys_em)
    public class PhysEMFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            string mode = args[0].AsString();
            double v1 = args[1].AsNumber();
            double v2 = args[2].AsNumber();

            if (mode == "coulomb") return new WValue(Electromagnetism.Coulomb(v1, v2, 1)); // r=1m
            if (mode == "ohm") return new WValue(Electromagnetism.Circuit(v1, v2));
            if (mode == "lorentz") return new WValue(Electromagnetism.Lorentz(v1, v2, 1)); // B=1T
            return new WValue("Bilinmeyen EM Modu");
        }
        public override string ToString() => "<native fn phys_em>";
    }

    // 4. Modern Fizik Wrapper (phys_modern)
    public class PhysModernFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            string mode = args[0].AsString();
            double v1 = args[1].AsNumber();

            if (mode == "relativity") return new WValue(ModernPhysics.Relativity(1, v1)); // m=1kg
            if (mode == "photoelectric") return new WValue(ModernPhysics.PhotoElectric(v1, 2.0)); // Work=2eV
            return new WValue("Bilinmeyen Modern Fizik Modu");
        }
        public override string ToString() => "<native fn phys_modern>";
    }
}
