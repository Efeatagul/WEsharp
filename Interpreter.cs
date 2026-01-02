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
            foreach (var stmt in statements)
            {
                switch (stmt.Type)
                {
                    case "Out": HandleOut(stmt); break; 
                    case "Assignment": HandleAssignment(stmt); break;
                    case "TaskDecl": _functions[stmt.Tokens[1].Value] = stmt.Body; break; 
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

        private void HandleOut(Statement stmt)
        {
            int p = 1; 
            if (stmt.Tokens.Count > 1 && stmt.Tokens[p].Value == "(") p++;
            var val = EvaluateExpression(ref p, stmt.Tokens);

            if (val is List<object> list)
                Console.WriteLine("[" + string.Join(", ", list) + "]");
            else
                Console.WriteLine(val ?? "null");
        }

        private void HandleAssignment(Statement stmt)
        {
            if (stmt.Tokens.Count < 3) return;
            string name = stmt.Tokens[0].Value;
            int p = 2; 
            _variables[name] = EvaluateExpression(ref p, stmt.Tokens);
        }

        private void HandleCheck(Statement stmt)
        {
            int p = 1; 
            var res = EvaluateExpression(ref p, stmt.Tokens);
            if (res is bool b && b || res is int i && i > 0) ExecuteStatements(stmt.Body);
        }

        private void HandleLoop(Statement stmt)
        {
            while (true)
            {
                int p = 1;
                var res = EvaluateExpression(ref p, stmt.Tokens);
                if (!(res is bool b && b || res is int i && i > 0)) break;
                ExecuteStatements(stmt.Body);
            }
        }

        private void HandleEmanFail(Statement stmt)
        {
            try { ExecuteStatements(stmt.Body); }
            catch (Exception ex)
            {
                _variables["last_error"] = ex.Message;
                ExecuteStatements(stmt.CatchBody);
            }
        }

        private object EvaluateExpression(ref int pos, List<Token> tokens)
        {
            if (pos >= tokens.Count) return null;
            Token current = tokens[pos];

           
            if (current.Type == TokenType.Identifier && pos + 1 < tokens.Count && tokens[pos + 1].Value == "(")
            {
                string funcName = current.Value;
                pos += 2;
                List<object> args = new List<object>();
                while (pos < tokens.Count && tokens[pos].Value != ")")
                {
                    args.Add(EvaluateExpression(ref pos, tokens));
                    if (pos < tokens.Count && tokens[pos].Value == ",") pos++;
                }
                if (pos < tokens.Count) pos++;

                if (StandardLibrary.Exists(funcName)) return StandardLibrary.Call(funcName, args);
                if (_functions.ContainsKey(funcName)) { ExecuteStatements(_functions[funcName]); return null; }
                throw new Exception($"Unknown command: {funcName}");
            }

          
            if (current.Type == TokenType.Identifier && pos + 1 < tokens.Count && tokens[pos + 1].Value == ".")
            {
                string listName = current.Value;
                pos += 2;
                string method = tokens[pos].Value;
                pos++;
                object arg = null;
                if (pos < tokens.Count && tokens[pos].Value == "(")
                {
                    pos++;
                    if (tokens[pos].Value != ")") arg = EvaluateExpression(ref pos, tokens);
                    pos++;
                }
                return HandleListFunction(listName, method, arg);
            }

           
            if (current.Type == TokenType.Number)
            {
                int val = int.Parse(tokens[pos++].Value);
                return CheckForOperator(ref pos, tokens, val);
            }

            if (current.Type == TokenType.String) return tokens[pos++].Value;

            if (current.Value == "[")
            {
                pos++;
                var list = new List<object>();
                while (pos < tokens.Count && tokens[pos].Value != "]")
                {
                    list.Add(EvaluateExpression(ref pos, tokens));
                    if (pos < tokens.Count && tokens[pos].Value == ",") pos++;
                }
                pos++;
                return list;
            }

            
            if (current.Type == TokenType.Identifier)
            {
                string name = tokens[pos++].Value;
                if (pos < tokens.Count && tokens[pos].Value == "[")
                {
                    pos++;
                    int idx = Convert.ToInt32(EvaluateExpression(ref pos, tokens));
                    pos++;
                    var list = _variables[name] as List<object>;
                    return CheckForOperator(ref pos, tokens, list[idx]);
                }

                if (_variables.ContainsKey(name))
                {
                    var val = _variables[name];
                    return (val is int i) ? CheckForOperator(ref pos, tokens, i) : val;
                }
                return name;
            }

            return null;
        }

        private object HandleListFunction(string listName, string funcName, object arg)
        {
            if (!_variables.ContainsKey(listName) || !(_variables[listName] is List<object> list))
                throw new Exception($"Collection not found: {listName}");

            
            switch (funcName)
            {
                case "add": list.Add(arg); return null; 
                case "take": var v = list.Last(); list.RemoveAt(list.Count - 1); return v; 
                case "size": return list.Count; 
                default: throw new Exception($"Unknown method: {funcName}");
            }
        }

        private object CheckForOperator(ref int pos, List<Token> tokens, object left)
        {
            if (pos < tokens.Count && tokens[pos].Type == TokenType.Operator)
            {
                string op = tokens[pos++].Value;
                var right = EvaluateExpression(ref pos, tokens);
                if (left is int l && right is int r)
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
                        _ => l
                    };
                }
                if (left is bool bl && right is bool br)
                {
                    return op switch { "and" => bl && br, "or" => bl || br, _ => bl };
                }
            }
            return left;
        }
    }
}