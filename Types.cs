#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
    
    public enum WType
    {
        Number,
        String,
        Bool,
        Function,
        List,
        Dict,
        Null
    }

    
    public interface IWCallable
    {
        int Arity(); 
        WValue Call(Interpreter interpreter, List<WValue> arguments);
    }

    
    public class WValue
    {
        public object Value { get; set; }
        public WType Type { get; set; }

        public WValue(object value)
        {
            Value = value;
            if (value == null) Type = WType.Null;
            else if (value is double) Type = WType.Number;
            else if (value is bool) Type = WType.Bool;
            else if (value is string) Type = WType.String;
            else if (value is IWCallable) Type = WType.Function;
            else if (value is List<object>) Type = WType.List;
            else if (value is Dictionary<string, WValue>) Type = WType.Dict;
            else if (value is int || value is float || value is decimal)
            {
                Value = Convert.ToDouble(value);
                Type = WType.Number;
            }
            else throw new Exception($"Bilinmeyen tip: {value.GetType()}");
        }

        public bool AsBoolean()
        {
            if (Value is bool b) return b;
            if (Value is double d) return d != 0;
            return Value != null;
        }

        public double AsNumber() => (double)Value;
        public string AsString() => (string)Value;
        public List<object> AsList() => (List<object>)Value;
        public Dictionary<string, WValue> AsDict() => (Dictionary<string, WValue>)Value;
        public bool IsCallable() => Type == WType.Function;
        public IWCallable AsCallable() => (IWCallable)Value;

        public override string ToString()
        {
            if (Type == WType.Number) return ((double)Value).ToString();
            if (Type == WType.String) return (string)Value;
            
            
            if (Type == WType.List)
            {
                var list = (List<object>)Value;
                return "[" + string.Join(", ", list) + "]";
            }
            
            
            if (Type == WType.Dict)
            {
                var dict = (Dictionary<string, WValue>)Value;
                var pairs = new List<string>();
                foreach (var kvp in dict)
                {
                    pairs.Add($"{kvp.Key}: {kvp.Value}");
                }
                return "{" + string.Join(", ", pairs) + "}";
            }
            
            return Value?.ToString() ?? "null";
        }
    }

    
    public class Environment
    {
        private readonly Dictionary<string, WValue> _values = new Dictionary<string, WValue>();
        public readonly Environment Enclosing; 

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            Enclosing = enclosing;
        }

       
        public void Define(string name, WValue value)
        {
            _values[name] = value;
        }

        
        public WValue Get(Token name)
        {
            if (_values.ContainsKey(name.Value)) return _values[name.Value];

            if (Enclosing != null) return Enclosing.Get(name);

            throw new Exception($"Tanimsiz degisken: '{name.Value}'.");
        }

        
        public void Assign(Token name, WValue value)
        {
            if (_values.ContainsKey(name.Value))
            {
                _values[name.Value] = value;
                return;
            }

            if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new Exception($"Tanimsiz degiskene atama yapilamaz: '{name.Value}'.");
        }
    }

   
    public class WFunction : IWCallable
    {
        private readonly FunctionStmt _declaration;
        private readonly Environment _closure;

        public WFunction(FunctionStmt declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public int Arity() => _declaration.Params.Count;

        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            
            Environment environment = new Environment(_closure);

            for (int i = 0; i < _declaration.Params.Count; i++)
            {
                environment.Define(_declaration.Params[i].Value, arguments[i]);
            }

            
            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (ReturnException returnValue)
            {
                return returnValue.Value;
            }

            return new WValue(null);
        }

        public override string ToString() => $"<fn {_declaration.Name.Value}>";
    }

    
    public class WLambda : IWCallable
    {
        private readonly LambdaExpr _declaration;
        private readonly Environment _closure;

        public WLambda(LambdaExpr declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public int Arity() => _declaration.Parameters.Count;

        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            Environment environment = new Environment(_closure);
            for (int i = 0; i < _declaration.Parameters.Count; i++)
            {
                environment.Define(_declaration.Parameters[i].Value, arguments[i]);
            }

            
            if (_declaration.Body is ExpressionStmt exprStmt)
            {
                return interpreter.Evaluate(exprStmt.Expression);
            }
            
            
            if (_declaration.Body is BlockStmt blockStmt)
            {
                try
                {
                    interpreter.ExecuteBlock(blockStmt.Statements, environment);
                }
                catch (ReturnException returnValue)
                {
                    return returnValue.Value;
                }
            }

            return new WValue(null);
        }

        public override string ToString() => "<fn lambda>";
    }

    
    public class ReturnException : Exception
    {
        public WValue Value;
        public ReturnException(WValue value) { Value = value; }
    }
}
