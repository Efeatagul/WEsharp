#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
    public class Interpreter : IVisitor<WValue>, IStmtVisitor<object>
    {
        public readonly Environment Globals = new Environment();
        private Environment _environment;

        
        public event Action<string> OnOutput;

        
        public void Notify(string message)
        {
            if (OnOutput != null)
                OnOutput.Invoke(message);
            else
                Console.WriteLine(message); 
        }
        

        public Interpreter()
        {
            _environment = Globals;
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (Exception ex)
            {
                Notify($"[RUNTIME ERROR] {ex.Message}");
            }
        }

        public void Execute(Stmt stmt) => stmt.Accept(this);

        public WValue Evaluate(Expr expr) => expr.Accept(this);

        

        public object Visit(ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return null;
        }

        public object Visit(EmitStmt stmt)
        {
            WValue value = Evaluate(stmt.Expr);
            string output = Stringify(value);
            Notify(output); 
            return null;
        }

        public object Visit(VarStmt stmt)
        {
            WValue value = new WValue(null);
            if (stmt.Initializer != null) value = Evaluate(stmt.Initializer);
            _environment.Define(stmt.Name.Value, value);
            return null;
        }

        public object Visit(BlockStmt stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return null;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = _environment;
            try
            {
                _environment = environment;
                foreach (Stmt statement in statements) Execute(statement);
            }
            finally
            {
                _environment = previous;
            }
        }

        public object Visit(IfStmt stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition))) Execute(stmt.ThenBranch);
            else if (stmt.ElseBranch != null) Execute(stmt.ElseBranch);
            return null;
        }

        public object Visit(WhileStmt stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition))) 
            {
                try
                {
                    Execute(stmt.Body);
                }
                catch (ContinueException) { continue; }
                catch (BreakException) { break; }
            }
            return null;
        }

        public object Visit(FunctionStmt stmt)
        {
            WFunction function = new WFunction(stmt, _environment);
            _environment.Define(stmt.Name.Value, new WValue(function));
            return null;
        }

        public object Visit(ForeachStmt stmt)
        {
            var iterable = Evaluate(stmt.Iterable);
            if (iterable.Type == WType.List)
            {
                var list = iterable.AsList();
                foreach (var item in list)
                {
                    Environment loopEnv = new Environment(_environment);
                    WValue loopVar = new WValue(item);
                    loopEnv.Define(stmt.Name.Value, loopVar);
                    try
                    {
                        ExecuteBlock(((BlockStmt)stmt.Body).Statements, loopEnv);
                    }
                    catch (ContinueException) { continue; }
                    catch (BreakException) { break; }
                }
            }
            else if (iterable.Type == WType.Dict)
            {
                var dict = iterable.AsDict();
                foreach (var key in dict.Keys)
                {
                    Environment loopEnv = new Environment(_environment);
                    loopEnv.Define(stmt.Name.Value, new WValue(key));
                    try
                    {
                        ExecuteBlock(((BlockStmt)stmt.Body).Statements, loopEnv);
                    }
                    catch (ContinueException) { continue; }
                    catch (BreakException) { break; }
                }
            }
            else if (iterable.Type == WType.String)
            {
                string str = iterable.AsString();
                foreach (char c in str)
                {
                    Environment loopEnv = new Environment(_environment);
                    loopEnv.Define(stmt.Name.Value, new WValue(c.ToString()));
                    try
                    {
                        ExecuteBlock(((BlockStmt)stmt.Body).Statements, loopEnv);
                    }
                    catch (ContinueException) { continue; }
                    catch (BreakException) { break; }
                }
            }
            else { throw new Exception("Foreach sadece liste, sozluk veya yazi uzerinde calisabilir."); }
            return null;
        }

        public object Visit(BreakStmt stmt) { throw new BreakException(); }
        public object Visit(ContinueStmt stmt) { throw new ContinueException(); }

        public object Visit(TryStmt stmt)
        {
            try
            {
                ExecuteBlock(stmt.TryBlock, new Environment(_environment));
            }
            catch (ReturnException) { throw; }
            catch (Exception)
            {
                if (stmt.CatchBlock != null)
                    ExecuteBlock(stmt.CatchBlock, new Environment(_environment));
            }
            return null;
        }

        public object Visit(ReturnStmt stmt)
        {
            WValue value = new WValue(null);
            if (stmt.Value != null) value = Evaluate(stmt.Value);
            throw new ReturnException(value);
        }

        

        public WValue Visit(AssignExpr expr)
        {
            WValue value = Evaluate(expr.Value);
            _environment.Assign(expr.Name, value);
            return value;
        }

        public WValue Visit(LiteralExpr expr) => expr.Value;

        public WValue Visit(VariableExpr expr) => _environment.Get(expr.Name);

        public WValue Visit(GroupingExpr expr) => Evaluate(expr.Expression);

        public WValue Visit(LambdaExpr expr)
        {
            return new WValue(new WLambda(expr, _environment));
        }

        public WValue Visit(UnaryExpr expr)
        {
            WValue right = Evaluate(expr.Right);

            switch (expr.Operator.Value)
            {
                case "!":
                    return new WValue(!IsTruthy(right));
                case "-":
                    CheckNumberOperand(expr.Operator, right);
                    return new WValue(-right.AsNumber());
            }

            return new WValue(null);
        }

        public WValue Visit(BinaryExpr expr)
        {
            WValue left = Evaluate(expr.Left);
            WValue right = Evaluate(expr.Right);

            switch (expr.Operator.Value)
            {
                case "+":
                    if (left.Type == WType.Number && right.Type == WType.Number)
                        return new WValue(left.AsNumber() + right.AsNumber());
                    if (left.Type == WType.String && right.Type == WType.String)
                        return new WValue(left.AsString() + right.AsString());
                    if (left.Type == WType.String) return new WValue(left.AsString() + Stringify(right));
                    if (right.Type == WType.String) return new WValue(Stringify(left) + right.AsString());

                    
                    if (left.Type == WType.List && right.Type == WType.List)
                    {
                        var listA = left.AsList();
                        var listB = right.AsList();

                        if (listA.Count != listB.Count)
                            throw new Exception($"Liste uzunluklari esit olmali: {listA.Count} != {listB.Count}");

                        var resultList = new List<object>();
                        for (int i = 0; i < listA.Count; i++)
                        {
                            
                            WValue valA = new WValue(listA[i]);
                            WValue valB = new WValue(listB[i]);
                            
                            
                            if (valA.Type == WType.Number && valB.Type == WType.Number)
                            {
                                resultList.Add(valA.AsNumber() + valB.AsNumber());
                            }
                            else
                            {
                                throw new Exception("Vektor toplama sadece sayisal listelerde gecerlidir.");
                            }
                        }
                        return new WValue(resultList);
                    }
                   

                    throw new Exception("Toplama islemi sadece sayilar, yazilar veya esit uzunluklu listeler arasinda yapilabilir.");

                case "-":
                   
                    if (left.Type == WType.List && right.Type == WType.List)
                    {
                        var listA = left.AsList();
                        var listB = right.AsList();
                        if (listA.Count != listB.Count) throw new Exception("Vector sizes must match.");
                        var res = new List<object>();
                        for (int i = 0; i < listA.Count; i++) res.Add(Convert.ToDouble(listA[i]) - Convert.ToDouble(listB[i]));
                        return new WValue(res);
                    }
                    CheckNumberOperands(expr.Operator, left, right);
                    return new WValue(left.AsNumber() - right.AsNumber());
                case "/":
                    CheckNumberOperands(expr.Operator, left, right);
                    if (right.AsNumber() == 0) throw new Exception("Sifira bolme hatasi!");
                    return new WValue(left.AsNumber() / right.AsNumber());
                case "%":
                    CheckNumberOperands(expr.Operator, left, right);
                    if (right.AsNumber() == 0) throw new Exception("Sifira bolme (mod) hatasi!");
                    return new WValue(left.AsNumber() % right.AsNumber());
                case "*":
                  
                    if (left.Type == WType.Number && right.Type == WType.List)
                    {
                        var res = new List<object>();
                        foreach (var item in right.AsList()) res.Add(left.AsNumber() * Convert.ToDouble(item));
                        return new WValue(res);
                    }
                    
                    if (left.Type == WType.List && right.Type == WType.Number)
                    {
                        var res = new List<object>();
                        foreach (var item in left.AsList()) res.Add(Convert.ToDouble(item) * right.AsNumber());
                        return new WValue(res);
                    }
                    
                    if (left.Type == WType.List && right.Type == WType.List)
                    {
                        var listA = left.AsList();
                        var listB = right.AsList();
                        if (listA.Count != listB.Count) throw new Exception("Vector sizes must match.");
                        double dot = 0.0;
                        for (int i = 0; i < listA.Count; i++) dot += Convert.ToDouble(listA[i]) * Convert.ToDouble(listB[i]);
                        return new WValue(dot);
                    }
                    CheckNumberOperands(expr.Operator, left, right);
                    return new WValue(left.AsNumber() * right.AsNumber());
                case ">": CheckNumberOperands(expr.Operator, left, right); return new WValue(left.AsNumber() > right.AsNumber());
                case "<": CheckNumberOperands(expr.Operator, left, right); return new WValue(left.AsNumber() < right.AsNumber());
                case ">=": CheckNumberOperands(expr.Operator, left, right); return new WValue(left.AsNumber() >= right.AsNumber());
                case "<=": CheckNumberOperands(expr.Operator, left, right); return new WValue(left.AsNumber() <= right.AsNumber());
                case "!=": return new WValue(!IsEqual(left, right));
                case "==": return new WValue(IsEqual(left, right));
            }
            return new WValue(null);
        }

        public WValue Visit(ListExpr expr)
        {
            List<object> values = new List<object>();
            foreach (var element in expr.Elements)
            {
                
                 WValue val = Evaluate(element);
                 values.Add(val.Value);
            }
            return new WValue(values);
        }

        public WValue Visit(CallExpr expr)
        {
            WValue callee = Evaluate(expr.Callee);

            List<WValue> args = new List<WValue>();
            foreach (Expr arg in expr.Args)
            {
                args.Add(Evaluate(arg));
            }

            if (!callee.IsCallable())
            {
                throw new Exception("Sadece fonksiyonlar cagrilabilir.");
            }

            IWCallable function = callee.AsCallable();
            if (args.Count != function.Arity())
            {
                throw new Exception($"Beklenen arguman: {function.Arity()}, Gelen: {args.Count}.");
            }

            return function.Call(this, args);
        }

        public WValue Visit(DictExpr expr)
        {
            var dict = new Dictionary<string, WValue>();
            for (int i = 0; i < expr.Keys.Count; i++)
            {
                WValue key = Evaluate(expr.Keys[i]);
                WValue val = Evaluate(expr.Values[i]);
                
                if (key.Type != WType.String)
                {
                    throw new Exception("Sozluk anahtarlari sadece yazi (string) olabilir.");
                }
                
                dict[key.AsString()] = val;
            }
            return new WValue(dict);
        }

        public WValue Visit(GetExpr expr)
        {
            WValue obj = Evaluate(expr.Object);
            if (obj.Type == WType.Dict)
            {
                var dict = obj.AsDict();
                string key = expr.Name.Value;
                if (dict.ContainsKey(key))
                {
                    return dict[key];
                }
                throw new Exception($"Sozlukte '{key}' anahtari bulunamadi.");
            }
            throw new Exception("Sadece sozluklerde (dict) nokta ile erisim yapilabilir.");
        }

        public WValue Visit(SetExpr expr)
        {
            WValue obj = Evaluate(expr.Object);
            if (obj.Type == WType.Dict)
            {
                var dict = obj.AsDict();
                string key = expr.Name.Value;
                WValue value = Evaluate(expr.Value);
                dict[key] = value;
                return value;
            }
            throw new Exception("Sadece sozluklerde (dict) nokta ile atama yapilabilir.");
        }

        public WValue Visit(IndexExpr expr)
        {
            WValue obj = Evaluate(expr.Object);
            WValue start = Evaluate(expr.Start);
            
            
            if (obj.Type == WType.List)
            {
                var list = obj.AsList();
                int index = (int)start.AsNumber();
                
                
                if (expr.End != null)
                {
                    WValue endVal = Evaluate(expr.End);
                    int end = (int)endVal.AsNumber();
                    
                 
                    if (index < 0) index = 0;
                    if (end > list.Count) end = list.Count;
                    if (index >= end) return new WValue(new List<object>()); 

                    var range = list.GetRange(index, end - index);
                    return new WValue(range);
                }
                
                else
                {
                    if (index < 0 || index >= list.Count) throw new Exception($"Liste indeksi sinirlar disinda: {index}");
                    return new WValue(list[index]);
                }
            }
    
            else if (obj.Type == WType.Dict)
            {
                if (expr.End != null) throw new Exception("Sozluklerde dilimleme (slicing) kullanilamaz.");
                
                var dict = obj.AsDict();
                string key = start.AsString(); 

                if (dict.ContainsKey(key)) return dict[key];
                throw new Exception($"Sozlukte '{key}' anahtari bulunamadi.");
            }
            
            else if (obj.Type == WType.String)
            {
                 string str = obj.AsString();
                 int index = (int)start.AsNumber();
                 
                 
                 if (expr.End != null)
                 {
                    WValue endVal = Evaluate(expr.End);
                    int end = (int)endVal.AsNumber();
                    
                    if (index < 0) index = 0;
                    if (end > str.Length) end = str.Length;
                    if (index >= end) return new WValue("");

                    return new WValue(str.Substring(index, end - index));
                 }
                
                 else
                 {
                     if (index < 0 || index >= str.Length) throw new Exception($"String indeksi sinirlar disinda: {index}");
                     return new WValue(str[index].ToString());
                 }
            }
            
            throw new Exception("Indeksleme sadece liste, sozluk veya yazilar icin gecerlidir.");
        }

       

        private bool IsTruthy(WValue obj)
        {
            if (obj.Value == null) return false;
            if (obj.Type == WType.Bool) return (bool)obj.Value;
            if (obj.Type == WType.Number) return obj.AsNumber() != 0;
            return true;
        }

        private bool IsEqual(WValue a, WValue b)
        {
            if (a.Value == null && b.Value == null) return true;
            if (a.Value == null) return false;
            return a.Value.Equals(b.Value);
        }

        private void CheckNumberOperand(Token op, WValue operand)
        {
            if (operand.Type == WType.Number) return;
            throw new Exception($"'{op.Value}' operatoru sadece sayilarla calisir.");
        }

        private void CheckNumberOperands(Token op, WValue left, WValue right)
        {
            if (left.Type == WType.Number && right.Type == WType.Number) return;
            throw new Exception($"'{op.Value}' operatoru icin iki taraf da sayi olmalidir.");
        }

        private string Stringify(WValue value)
        {
            if (value.Type == WType.Null) return "bos";
            if (value.Type == WType.Bool) return (bool)value.Value ? "dogru" : "yanlis";
            return value.ToString();
        }

        
        public WValue Visit(LogicalExpr expr)
        {
            WValue left = Evaluate(expr.Left);

            if (expr.Operator.Value == "||")
            {
                if (IsTruthy(left)) return left; 
            }
            else 
            {
                if (!IsTruthy(left)) return left; 
            }

            return Evaluate(expr.Right);
        }

        
        public WValue Visit(IsKeyExpr expr)
        {
            WValue obj = Evaluate(expr.Object);
            WValue key = Evaluate(expr.Key);

            if (obj.Type != WType.Dict)
                throw new Exception("is_key sadece sozluk (dict) uzerinde calisir.");

            return new WValue(obj.AsDict().ContainsKey(key.AsString()));
        }
    }
    public class BreakException : Exception { }
    public class ContinueException : Exception { }
}
