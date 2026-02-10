#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace WSharp
{
    public class BasicMath
    {
        public const double PI = 3.14159265359;
        public const double E = 2.71828182846;

        public static double Sin(double angleDeg) => Math.Sin(angleDeg * Math.PI / 180.0);
        public static double Cos(double angleDeg) => Math.Cos(angleDeg * Math.PI / 180.0);
        public static double Tan(double angleDeg) => Math.Tan(angleDeg * Math.PI / 180.0);

        public static double Log(double val, double baseVal = 10) => Math.Log(val, baseVal);
        public static double Ln(double val) => Math.Log(val);

        public static string ComplexAdd(double r1, double i1, double r2, double i2)
        {
            return $"{r1 + r2} + {i1 + i2}i";
        }

        public static string ComplexMult(double r1, double i1, double r2, double i2)
        {
            double real = (r1 * r2) - (i1 * i2);
            double imag = (r1 * i2) + (i1 * r2);
            return $"{real} + {imag}i";
        }
    }

    public class Calculus
    {
        public static string DifferentiatePolynomial(string poly)
        {
            StringBuilder result = new StringBuilder();

            string cleaned = poly.Replace(" ", "").Replace("-", "+-");
            string[] terms = cleaned.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string term in terms)
            {
                if (string.IsNullOrWhiteSpace(term)) continue;

                double coeff = 1;
                double power = 0;

                if (term.Contains("x"))
                {
                    int xIndex = term.IndexOf("x");
                    string cStr = term.Substring(0, xIndex);
                    if (cStr == "") coeff = 1;
                    else if (cStr == "-") coeff = -1;
                    else double.TryParse(cStr, out coeff);

                    if (term.Contains("^"))
                    {
                        string pStr = term.Substring(term.IndexOf("^") + 1);
                        double.TryParse(pStr, out power);
                    }
                    else power = 1;
                }
                else
                {
                    continue;
                }

                double newCoeff = coeff * power;
                double newPower = power - 1;

                if (newCoeff != 0)
                {
                    if (result.Length > 0 && newCoeff > 0) result.Append(" + ");

                    result.Append(newCoeff);
                    if (newPower == 1) result.Append("x");
                    else if (newPower > 1) result.Append($"x^{newPower}");
                }
            }

            return result.Length == 0 ? "0" : result.ToString();
        }

        public static double DefiniteIntegralSimple(double power, double a, double b)
        {
            Func<double, double> F = (x) => Math.Pow(x, power + 1) / (power + 1);
            return F(b) - F(a);
        }
    }

    public class LinearAlgebra
    {
        public static string MatrixMultiply(string matA, string matB)
        {
            var A = ParseMatrix(matA);
            var B = ParseMatrix(matB);

            int r1 = A.GetLength(0);
            int c1 = A.GetLength(1);
            int r2 = B.GetLength(0);
            int c2 = B.GetLength(1);

            if (c1 != r2) return "HATA: Boyut uyuşmazlığı (Sütun1 != Satır2)";

            double[,] C = new double[r1, c2];

            for (int i = 0; i < r1; i++)
            {
                for (int j = 0; j < c2; j++)
                {
                    for (int k = 0; k < c1; k++)
                    {
                        C[i, j] += A[i, k] * B[k, j];
                    }
                }
            }

            return MatrixToString(C);
        }

        public static string Determinant(string matStr)
        {
            var M = ParseMatrix(matStr);
            int n = M.GetLength(0);

            if (n != M.GetLength(1)) return "HATA: Kare matris değil.";

            double det = 0;
            if (n == 2)
            {
                det = (M[0, 0] * M[1, 1]) - (M[0, 1] * M[1, 0]);
            }
            else if (n == 3)
            {
                det = M[0, 0] * (M[1, 1] * M[2, 2] - M[1, 2] * M[2, 1]) -
                      M[0, 1] * (M[1, 0] * M[2, 2] - M[1, 2] * M[2, 0]) +
                      M[0, 2] * (M[1, 0] * M[2, 1] - M[1, 1] * M[2, 0]);
            }
            else
            {
                return "HATA: Şimdilik sadece 2x2 ve 3x3 destekleniyor.";
            }

            return det.ToString();
        }

        private static double[,] ParseMatrix(string s)
        {
            var rows = s.Split(';');
            int r = rows.Length;
            int c = rows[0].Split(',').Length;
            double[,] mat = new double[r, c];

            for (int i = 0; i < r; i++)
            {
                var cols = rows[i].Split(',');
                for (int j = 0; j < c; j++)
                {
                    double.TryParse(cols[j], out mat[i, j]);
                }
            }
            return mat;
        }

        private static string MatrixToString(double[,] mat)
        {
            int r = mat.GetLength(0);
            int c = mat.GetLength(1);
            StringBuilder sb = new StringBuilder();
            sb.Append("[ ");
            for (int i = 0; i < r; i++)
            {
                if (i > 0) sb.Append(" ; ");
                for (int j = 0; j < c; j++)
                {
                    if (j > 0) sb.Append(", ");
                    sb.Append(mat[i, j]);
                }
            }
            sb.Append(" ]");
            return sb.ToString();
        }
    }

    public class MathBasicFunc : IWCallable
    {
        public int Arity() => 2; // (İşlem, Değer)
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            string op = args[0].AsString();
            double val = args[1].AsNumber();
            if (op == "sin") return new WValue(BasicMath.Sin(val));
            if (op == "cos") return new WValue(BasicMath.Cos(val));
            if (op == "log") return new WValue(BasicMath.Log(val));
            if (op == "ln") return new WValue(BasicMath.Ln(val));
            return new WValue(0);
        }
        public override string ToString() => "<native fn math_basic>";
    }

    // [Math] Calculus (Türev/İntegral)
    public class MathCalcFunc : IWCallable
    {
        public int Arity() => 2; // (İşlem, Veri)
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            string op = args[0].AsString();
            if (op == "deriv") return new WValue(Calculus.DifferentiatePolynomial(args[1].AsString()));
            // İntegral için örnek: "integral", "2,0,5" (x^2, 0'dan 5'e)
            if (op == "integral")
            {
                string[] parts = args[1].AsString().Split(',');
                return new WValue(Calculus.DefiniteIntegralSimple(double.Parse(parts[0]), double.Parse(parts[1]), double.Parse(parts[2])));
            }
            return new WValue("Hata");
        }
        public override string ToString() => "<native fn math_calc>";
    }

    // [Math] Lineer Cebir (Matris)
    public class MathMatrixFunc : IWCallable
    {
        public int Arity() => 2; // (İşlem, Veri/Veri2)
        public WValue Call(Interpreter interpreter, List<WValue> args)
        {
            string op = args[0].AsString();
            string data = args[1].AsString(); // "1,0;0,1" gibi

            if (op == "det") return new WValue(LinearAlgebra.Determinant(data));
            // Çarpma için argümanı '|' ile ayırıyoruz. Örn: "1,2;3,4|1,0;0,1"
            if (op == "mult")
            {
                string[] mats = data.Split('|');
                return new WValue(LinearAlgebra.MatrixMultiply(mats[0], mats[1]));
            }
            return new WValue("Matris Hatası");
        }
        public override string ToString() => "<native fn math_matrix>";
    }
}
