#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

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
                try
                {
                    switch (stmt.Type)
                    {
                        case "Out": HandleEmit(stmt); break;         
                        case "Assignment": HandleUnit(stmt); break;  
                        case "FunctionCall": HandleCall(stmt); break;
                        case "TaskDecl": HandleFlowDecl(stmt); break;
                        case "Check": HandleVerify(stmt); break;     
                        case "Loop": HandleCycle(stmt); break;       
                        case "EmanFail": HandleEmanFail(stmt); break;
                        default:
                            if (stmt.Tokens.Count > 0) { int p = 0; EvaluateExpression(ref p, stmt.Tokens); }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[WEA_RUNTIME_ERROR]: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        private void HandleFlowDecl(Statement stmt)
        {
            if (stmt.Tokens.Count > 1)
                _functions[stmt.Tokens[1].Value.ToLower()] = stmt.Body;
        }

        private void HandleCall(Statement stmt)
        {
            int pos = 0;
            EvaluateExpression(ref pos, stmt.Tokens);
        }

        private void HandleEmit(Statement stmt)
        {
            int p = (stmt.Tokens.Count > 0 && stmt.Tokens[0].Value == "wea_emit") ? 1 : 0;
            if (stmt.Tokens.Count > p && stmt.Tokens[p].Value == "(") p++;

            var val = EvaluateExpression(ref p, stmt.Tokens);

            if (val is List<object> list)
                Console.WriteLine($"[WEA_ARRAY: {string.Join(", ", list)}]");
            else
                Console.WriteLine(val?.ToString() ?? "wea_null");
        }

        private void HandleUnit(Statement stmt)
        {
            if (stmt.Tokens.Count < 3) return;
            int nameIndex = (stmt.Tokens[0].Value == "wea_unit") ? 1 : 0;
            string name = stmt.Tokens[nameIndex].Value;

            int p = 0;
            while (p < stmt.Tokens.Count && stmt.Tokens[p].Value != "=") p++;
            p++;

            if (p < stmt.Tokens.Count)
                _variables[name] = EvaluateExpression(ref p, stmt.Tokens);
        }

        private void HandleVerify(Statement stmt)
        {
            int p = (stmt.Tokens[0].Value == "wea_verify") ? 1 : 0;
            var res = EvaluateExpression(ref p, stmt.Tokens);
            if (IsTrue(res)) ExecuteStatements(stmt.Body);
        }

        private void HandleCycle(Statement stmt)
        {
            while (true)
            {
                int p = (stmt.Tokens[0].Value == "wea_cycle") ? 1 : 0;
                var res = EvaluateExpression(ref p, stmt.Tokens);
                if (!IsTrue(res)) break;
                ExecuteStatements(stmt.Body);
            }
        }

        private void HandleEmanFail(Statement stmt)
        {
            try { ExecuteStatements(stmt.Body); }
            catch (Exception ex)
            {
                _variables["wea_last_error"] = ex.Message;
                ExecuteStatements(stmt.CatchBody);
            }
        }

        private bool IsTrue(object val)
        {
            if (val is bool b) return b;
            if (val is double d) return d > 0;
            if (val is int i) return i > 0;
            return val != null && !val.Equals("wea_fail") && !val.Equals("wea_null");
        }

        public object EvaluateExpression(ref int pos, List<Token> tokens)
        {
            if (pos >= tokens.Count) return null;
            Token current = tokens[pos];

           
            if (current.Type == TokenType.wea_sign_name && pos + 1 < tokens.Count && tokens[pos + 1].Value == "(")
            {
                string funcName = current.Value.ToLower();
                pos += 2;
                var args = new List<object>();

                while (pos < tokens.Count && tokens[pos].Value != ")")
                {
                    args.Add(EvaluateExpression(ref pos, tokens));
                    if (pos < tokens.Count && tokens[pos].Value == ",") pos++;
                }
                if (pos < tokens.Count) pos++;

                
                if (StandardLibrary.Exists(funcName))
                {
                    return StandardLibrary.Call(funcName, args);
                }

                
                if (_functions.ContainsKey(funcName))
                {
                    ExecuteStatements(_functions[funcName]);
                    return null;
                }
                throw new Exception($"[WEA_NOT_FOUND]: '{funcName}' fonksiyonu bulunamadı.");
            }

          
            if (current.Type == TokenType.wea_sign_val)
            {
                double val = double.Parse(tokens[pos++].Value, CultureInfo.InvariantCulture);
                return CheckForOperator(ref pos, tokens, val);
            }

            if (current.Type == TokenType.wea_sign_text) return tokens[pos++].Value;

            if (current.Type == TokenType.wea_sign_name)
            {
                string name = tokens[pos++].Value;
                if (_variables.TryGetValue(name, out object val))
                    return (val is double d) ? CheckForOperator(ref pos, tokens, d) : val;
                return name;
            }

            if (current.Value == "(")
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
            if (pos < tokens.Count && tokens[pos].Type == TokenType.wea_sign_action)
            {
                string op = tokens[pos++].Value;
                var right = EvaluateExpression(ref pos, tokens);

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
                        ">=" => l >= r,
                        "<=" => l <= r,
                        _ => l
                    };
                }
                if (op == "+" && (left is string || right is string))
                    return left.ToString() + right.ToString();
            }
            return left;
        }
    }
}
