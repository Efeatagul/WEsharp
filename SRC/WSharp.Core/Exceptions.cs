/* ======================================================================
 * WSHARP (We#) NEURO-ENGINE - WEAGW Ecosystem
 * Copyright (c) 2026 Efe Ata Gul. All rights reserved.
 * * This file is part of the WSharp project.
 * * OPEN SOURCE: Licensed under the GNU AGPL v3.0. You may use this 
 * file freely in open-source/academic projects provided you give 
 * clear attribution to "WSharp by Efe Ata Gul".
 * * COMMERCIAL: If you wish to use WSharp in closed-source, proprietary, 
 * or commercial products, you must purchase a WEAGW Commercial License.
 * ====================================================================== */
// ═══════════════════════════════════════════════════════════════
//  Exceptions.cs — WSharp Professional Exception Hierarchy
// ═══════════════════════════════════════════════════════════════
//
//  Instead of throwing generic Exception("...") everywhere, WSharp
//  uses a structured exception hierarchy. This enables:
//
//  ✓ Targeted catch blocks in the IDE/REPL
//  ✓ Color-coded error messages (syntax → yellow, runtime → red)
//  ✓ Source location tracking (line, column if available)
//  ✓ Error code classification for diagnostics panel
//  ✓ AI fixer can inspect exception type for better suggestions
//
//  HIERARCHY:
//  ──────────
//  WSharpException (abstract base)
//    ├─ WSharpSyntaxException    (Lexer / Parser errors)
//    ├─ WSharpRuntimeException   (Interpreter / variable / logic errors)
//    │    ├─ WSharpTypeException       (type mismatch)
//    │    ├─ WSharpIndexException      (out-of-bounds)
//    │    ├─ WSharpDivisionException   (divide by zero)
//    │    ├─ WSharpArgumentException   (wrong arg count/type)
//    │    └─ WSharpImportException     (module import failure)
//    └─ NeuroEngineException     (Python bridge / math / neuro errors)
//
// ═══════════════════════════════════════════════════════════════

using System;

namespace WSharp
{
  
    public abstract class WSharpException : Exception
    {
       
        public int Line { get; }

       
        public int Column { get; }

        
        public string ErrorCode { get; }

        protected WSharpException(string message, string errorCode = "WS-000",
                                   int line = 0, int column = 0, Exception inner = null)
            : base(FormatMessage(message, errorCode, line), inner)
        {
            Line = line;
            Column = column;
            ErrorCode = errorCode;
        }

        private static string FormatMessage(string message, string code, int line)
        {
            if (line > 0)
                return $"[{code}] Satır {line}: {message}";
            return $"[{code}] {message}";
        }

       
        public string RawMessage => base.Message.Contains("] ")
            ? base.Message.Substring(base.Message.IndexOf("] ") + 2)
            : base.Message;
    }

   
    public class WSharpSyntaxException : WSharpException
    {
        public string TokenValue { get; }

        public WSharpSyntaxException(string message, int line = 0, int column = 0,
                                      string tokenValue = null)
            : base(message, "WS-SYN", line, column)
        {
            TokenValue = tokenValue;
        }
    }

   
    public class WSharpRuntimeException : WSharpException
    {
        public WSharpRuntimeException(string message, int line = 0, int column = 0,
                                       string errorCode = "WS-RT", Exception inner = null)
            : base(message, errorCode, line, column, inner)
        {
        }
    }

    
    public class WSharpTypeException : WSharpRuntimeException
    {
        public string ExpectedType { get; }
        public string ActualType { get; }

        public WSharpTypeException(string message, string expectedType = null,
                                    string actualType = null, int line = 0)
            : base(message, line, 0, "WS-TYP")
        {
            ExpectedType = expectedType;
            ActualType = actualType;
        }
    }

    
    public class WSharpIndexException : WSharpRuntimeException
    {
        public int Index { get; }
        public int CollectionSize { get; }

        public WSharpIndexException(string message, int index = -1,
                                     int collectionSize = -1, int line = 0)
            : base(message, line, 0, "WS-IDX")
        {
            Index = index;
            CollectionSize = collectionSize;
        }
    }

   
    public class WSharpDivisionException : WSharpRuntimeException
    {
        public WSharpDivisionException(string message = "Sifira bolme hatasi!", int line = 0)
            : base(message, line, 0, "WS-DIV")
        {
        }
    }

    
    public class WSharpArgumentException : WSharpRuntimeException
    {
        public int ExpectedArity { get; }
        public int ActualArity { get; }

        public WSharpArgumentException(string message, int expected = -1,
                                        int actual = -1, int line = 0)
            : base(message, line, 0, "WS-ARG")
        {
            ExpectedArity = expected;
            ActualArity = actual;
        }
    }

    
    public class WSharpImportException : WSharpRuntimeException
    {
        public string FilePath { get; }

        public WSharpImportException(string message, string filePath = null, int line = 0)
            : base(message, line, 0, "WS-IMP")
        {
            FilePath = filePath;
        }
    }

    
    public class NeuroEngineException : WSharpException
    {
        public string Component { get; }

        public NeuroEngineException(string message, string component = "NeuroEngine",
                                     int line = 0, Exception inner = null)
            : base(message, $"WS-NEU", line, 0, inner)
        {
            Component = component;
        }
    }
}
