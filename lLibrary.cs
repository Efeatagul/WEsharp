#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
   
    public interface ILibrary
    {
        
        Dictionary<string, Func<List<object>, object>> GetFunctions();
    }
}
