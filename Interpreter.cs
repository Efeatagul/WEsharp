#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;

namespace WSharp
{
    public class Interpreter
    {
        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();
        private readonly Dictionary<string, List<Statement>> _functions = new Dictionary<string, List<Statement>>();

        public void ExecuteStatements(List<Statement> statements)
        {
            if (statements == null) return;
            foreach (var stmt in statements)
            {
                switch (stmt.Type)
                {
                    case "Out": HandleOut(stmt); break;
                    case "Assignment": HandleAssignment(stmt); break;
                    case "FunctionCall": HandleFunctionCall(stmt); break; // YENİ: win_open vb. için
                    case "TaskDecl":
                        if (stmt.Tokens.Count > 1)
                            _functions[stmt.Tokens[1].Value.ToLower()] = stmt.Body;
                        break;
                    case "Check": HandleCheck(stmt); break;
                    case "Loop": HandleLoop(stmt); break;
                    case "EmanFail": HandleEmanFail(stmt); break;
                    default:
                        if (stmt.Tokens.Count > 0)
                        {
                            int pos = 0;
                            EvaluateExpression(ref pos, stmt.Tokens);
                        }
                        break;
                }
            }
        }

        // YENİ: Doğrudan çağrılan fonksiyonları (win_open, win_clear vb.) yönetir
        private void HandleFunctionCall(Statement stmt)
        {
            int pos = 0;
            EvaluateExpression(ref pos, stmt.Tokens);
        }

        private void HandleOut(Statement stmt)
        {
            int p = 0;
            // 'out' keyword'ünü atla
            if (stmt.Tokens.Count > 0 && stmt.Tokens[0].Value == "out") p++;
            // Parantez varsa atla
            if (stmt.Tokens.Count > p && stmt.Tokens[p].Value == "(") p++;

            var val = EvaluateExpression(ref p, stmt.Tokens);

            if (val is List<object> list)
                Console.WriteLine("[" + string.Join(", ", list) + "]");
            else
                Console.WriteLine(val ?? "null");
        }

        private void HandleAssignment(Statement stmt)
        {
            if (stmt.Tokens.Count < 3) return;
            int nameIndex = (stmt.Tokens[0].Value == "var") ? 1 : 0;
            string name = stmt.Tokens[nameIndex].Value;

            int p = Array.FindIndex(stmt.Tokens.ToArray(), t => t.Value == "=") + 1;
            if (p > 0 && p < stmt.Tokens.Count)
            {
                _variables[name] = EvaluateExpression(ref p, stmt.Tokens);
            }
        }

        private void HandleCheck(Statement stmt)
        {
            int p = 0;
            if (stmt.Tokens[0].Value == "check") p++;
            var res = EvaluateExpression(ref p, stmt.Tokens);
            if (res is bool b && b || res is int i && i > 0) ExecuteStatements(stmt.Body);
        }

        private void HandleLoop(Statement stmt)
        {
            while (true)
            {
                int p = 0;
                if (stmt.Tokens[0].Value == "loop") p++;
                var res = EvaluateExpression(ref p, stmt.Tokens);
                if (!(res is bool b && b || res is int i && i > 0)) break;
                ExecuteStatements(stmt.Body);
            }
        }

        private void HandleEmanFail(Statement stmt)
        {
            try { ExecuteStatements(stmt.Body); }
            catch (Exception ex) { _variables["last_error"] = ex.Message; ExecuteStatements(stmt.CatchBody); }
        }

        public object EvaluateExpression(ref int pos, List<Token> tokens)
        {
            if (pos >= tokens.Count) return null;
            Token current = tokens[pos];

            // --- FONKSİYON ÇAĞIRMA BLOĞU ---
            if (current.Type == TokenType.Identifier && pos + 1 < tokens.Count && tokens[pos + 1].Value == "(")
            {
                string funcName = current.Value.ToLower();
                pos += 2; // isim ve '(' karakterini geç
                List<object> args = new List<object>();

                while (pos < tokens.Count && tokens[pos].Value != ")")
                {
                    args.Add(EvaluateExpression(ref pos, tokens));
                    if (pos < tokens.Count && tokens[pos].Value == ",") pos++;
                }
                if (pos < tokens.Count) pos++; // ')' karakterini geç

                // 1. Standart Kütüphane Kontrolü
                if (StandardLibrary.Exists(funcName))
                {
                    return StandardLibrary.Call(funcName, args);
                }

                // 2. Kullanıcı Tanımlı Fonksiyon Kontrolü
                if (_functions.ContainsKey(funcName))
                {
                    ExecuteStatements(_functions[funcName]);
                    return null;
                }
                throw new Exception($"Komut bulunamadi: {funcName}");
            }

            if (current.Type == TokenType.Number)
            {
                // int.Parse yerine double.Parse kullanarak ondalık desteği sağladık
                double val = double.Parse(tokens[pos++].Value, System.Globalization.CultureInfo.InvariantCulture);
                return CheckForOperator(ref pos, tokens, val);
            }

            if (current.Type == TokenType.String) return tokens[pos++].Value;

            if (current.Type == TokenType.Identifier)
            {
                string name = tokens[pos++].Value;
                if (_variables.ContainsKey(name))
                {
                    var val = _variables[name];
                    return (val is double d) ? CheckForOperator(ref pos, tokens, d) : val;
                }
                return name;
            }

            if (current.Value == "(") // Gruplandırma parantezleri için
            {
                pos++;
                var val = EvaluateExpression(ref pos, tokens);
                if (pos < tokens.Count && tokens[pos].Value == ")") pos++;
                return val;
            }

            return null;
        }

        private object CheckForOperator(ref int pos, List<Token> tokens, object left)
        {
            if (pos < tokens.Count && tokens[pos].Type == TokenType.Operator)
            {
                string op = tokens[pos++].Value;
                var right = EvaluateExpression(ref pos, tokens);

                // Sayısal işlemler için double desteği
                if (left is double l && right is double r)
                {
                    return op switch
                    {
                        "+" => l + r,
                        "-" => l - r,
                        "*" => l * r,
                        "/" => l / r,
                        "==" => l == r,
                        ">" => l > r,
                        "<" => l < r,
                        "!=" => l != r,
                        _ => l
                    };
                }
                // String birleştirme desteği
                if (op == "+" && (left is string || right is string))
                {
                    return left.ToString() + right.ToString();
                }
            }
            return left;
        }
    }
}
