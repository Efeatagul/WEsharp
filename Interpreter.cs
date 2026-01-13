#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;

namespace WSharp
{
    public class Interpreter : IVisitor<WValue>, IStmtVisitor<object>
    {
        private Scope _scope = new Scope();
        private readonly Dictionary<string, FunctionStmt> _functions = new Dictionary<string, FunctionStmt>();
        private bool _breakingLoop = false;

      
        public event Action<string> OnOutput;

        public Interpreter()
        {
          
            LoadLibrary(StandardLibrary.Functions);
           

          
            LoadLibrary(new NeurologyLib().GetFunctions());
        }

        private void LoadLibrary(Dictionary<string, Func<List<WValue>, WValue>> lib)
        {
            foreach (var func in lib) _scope.Define(func.Key, new WValue(func.Value));
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var stmt in statements)
                {
                    if (stmt != null) Execute(stmt);
                }
            }
            catch (Exception ex)
            {
                
                OnOutput?.Invoke($"[WEA_RUNTIME_ERROR] {ex.Message}");
            }
        }

        private void Execute(Stmt stmt) => stmt.Accept(this);
        private WValue Evaluate(Expr expr) => expr.Accept(this);

        
        public object Visit(EmitStmt stmt)
        {
            WValue val = Evaluate(stmt.Expr);
            string output = val.AsString();

          
            if (OnOutput != null) OnOutput.Invoke(output);
            else Console.WriteLine(output); 

            return null;
        }


        public object Visit(VarStmt stmt) { WValue val = stmt.Initializer != null ? Evaluate(stmt.Initializer) : new WNull(); _scope.Define(stmt.Name.Value, val); return null; }
        public object Visit(BlockStmt stmt) { ExecuteBlock(stmt.Statements, new Scope(_scope)); return null; }
        private void ExecuteBlock(List<Stmt> statements, Scope nextScope) { Scope previous = _scope; _scope = nextScope; try { foreach (var stmt in statements) { if (_breakingLoop) break; Execute(stmt); } } finally { _scope = previous; } }
        public object Visit(IfStmt stmt) { if (IsTruthy(Evaluate(stmt.Condition))) Execute(stmt.ThenBranch); else if (stmt.ElseBranch != null) Execute(stmt.ElseBranch); return null; }
        public object Visit(WhileStmt stmt) { while (IsTruthy(Evaluate(stmt.Condition))) { Execute(stmt.Body); if (_breakingLoop) { _breakingLoop = false; break; } } return null; }
        public object Visit(FunctionStmt stmt) { _functions[stmt.Name.Value] = stmt; return null; }
        public object Visit(TryStmt stmt) { try { ExecuteBlock(stmt.TryBlock, new Scope(_scope)); } catch { if (stmt.CatchBlock != null) ExecuteBlock(stmt.CatchBlock, new Scope(_scope)); } return null; }
        public object Visit(ExpressionStmt stmt) { Evaluate(stmt.Expression); return null; }

        public WValue Visit(LiteralExpr expr) => expr.Value;
        public WValue Visit(VariableExpr expr) { if (_functions.ContainsKey(expr.Name.Value)) return new WValue("FUNCTION", WType.String); return _scope.Get(expr.Name); }
        public WValue Visit(AssignExpr expr) { WValue val = Evaluate(expr.Value); _scope.Assign(expr.Name, val); return val; }

        public WValue Visit(BinaryExpr expr)
        {
            WValue left = Evaluate(expr.Left); WValue right = Evaluate(expr.Right);
            switch (expr.Operator.Value)
            {
                case "+": if (left.Type == WType.String || right.Type == WType.String) return new WString(left.AsString() + right.AsString()); return new WNumber(left.AsNumber() + right.AsNumber());
                case "-": return new WNumber(left.AsNumber() - right.AsNumber());
                case "*": return new WNumber(left.AsNumber() * right.AsNumber());
                case "/": return new WNumber(left.AsNumber() / right.AsNumber());
                case ">": return new WNumber(left.AsNumber() > right.AsNumber() ? 1 : 0);
                case "<": return new WNumber(left.AsNumber() < right.AsNumber() ? 1 : 0);
                case ">=": return new WNumber(left.AsNumber() >= right.AsNumber() ? 1 : 0);
                case "<=": return new WNumber(left.AsNumber() <= right.AsNumber() ? 1 : 0);
                case "==": return new WNumber(IsEqual(left, right) ? 1 : 0);
                case "!=": return new WNumber(!IsEqual(left, right) ? 1 : 0);
            }
            return new WNull();
        }

        public WValue Visit(GroupingExpr expr) => Evaluate(expr.Expression);

        public WValue Visit(CallExpr expr)
        {
            try
            {
                WValue callee = Evaluate(expr.Callee);
                List<WValue> args = expr.Arguments.Select(arg => Evaluate(arg)).ToList();
                if (callee.IsSystemFunction) return callee.SystemFunction(args);
            }
            catch { }

            if (expr.Callee is VariableExpr v && _functions.ContainsKey(v.Name.Value))
            {
                FunctionStmt func = _functions[v.Name.Value];
                Scope funcScope = new Scope(_scope);
                for (int i = 0; i < func.Params.Count; i++)
                {
                    WValue argVal = (i < expr.Arguments.Count) ? Evaluate(expr.Arguments[i]) : new WNull();
                    funcScope.Define(func.Params[i].Value, argVal);
                }
                try { ExecuteBlock(func.Body, funcScope); } catch { }
                return new WNull();
            }

           
            return new WNull();
        }

        private bool IsTruthy(WValue val) { if (val.Type == WType.Null) return false; if (val.Type == WType.Number) return val.AsNumber() != 0; if (val.Type == WType.String) return val.AsString() != "false"; return true; }
        private bool IsEqual(WValue a, WValue b) { if (a.Type != b.Type) return false; return a.AsString() == b.AsString(); }
    }
}
