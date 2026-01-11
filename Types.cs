#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
   
    public enum WType
    {
        Null,
        Number,
        String,
        Function
    }

  
    public class WValue
    {
        public object RawValue;
        public WType Type;

       
        public bool IsSystemFunction = false;
        public Func<List<WValue>, WValue> SystemFunction;

        public WValue(object value, WType type)
        {
            RawValue = value;
            Type = type;
        }

        public WValue(Func<List<WValue>, WValue> function)
        {
            Type = WType.Function;
            IsSystemFunction = true;
            SystemFunction = function;
            RawValue = "<native fn>";
        }

        public double AsNumber() => RawValue is double d ? d : 0;
        public string AsString() => RawValue?.ToString() ?? "";
    }

    public class WNumber : WValue
    {
        public WNumber(double value) : base(value, WType.Number) { }
    }

    public class WString : WValue
    {
        public WString(string value) : base(value, WType.String) { }
    }

    public class WNull : WValue
    {
        public WNull() : base(null, WType.Null) { }
    }

    
    public class Scope
    {
        private readonly Dictionary<string, WValue> _values = new Dictionary<string, WValue>();
        public Scope Enclosing; 

        public Scope() { Enclosing = null; }
        public Scope(Scope enclosing) { Enclosing = enclosing; }

        public void Define(string name, WValue value)
        {
            _values[name] = value;
        }

        public WValue Get(Token name)
        {
            if (_values.ContainsKey(name.Value)) return _values[name.Value];
            if (Enclosing != null) return Enclosing.Get(name);

            throw new Exception($"Tanımsız değişken: '{name.Value}'.");
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

            throw new Exception($"Tanımsız değişkene atama yapılamaz: '{name.Value}'.");
        }
    }
}
