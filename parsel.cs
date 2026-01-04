#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;

namespace WSharp
{
   
    public class Statement
    {
        public string Type { get; set; }
        public List<Token> Tokens { get; set; } = new List<Token>();
        public List<Statement> Body { get; set; } = new List<Statement>();
        public List<Statement> CatchBody { get; set; } = new List<Statement>();
    }

    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos = 0;

        
        private static readonly HashSet<string> ReservedKeywords = new HashSet<string>
        {
            "wea_flow", "wea_verify", "wea_emit", "wea_eman", "wea_cycle", "wea_fail", "wea_unit"
        };

        public Parser(List<Token> tokens) => _tokens = tokens;

        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : new Token { Type = TokenType.wea_sign_halt, Value = "" };
        private Token Peek(int distance) => (_pos + distance < _tokens.Count) ? _tokens[_pos + distance] : new Token { Type = TokenType.wea_sign_halt, Value = "" };

        public List<Statement> Parse()
        {
            var statements = new List<Statement>();
            if (_tokens == null) return statements;

            while (Current.Type != TokenType.wea_sign_halt)
            {
                
                if (Current.Value == "\n" || Current.Value == ";" || string.IsNullOrWhiteSpace(Current.Value))
                {
                    _pos++;
                    continue;
                }
                statements.Add(ParseStatement());
            }
            return statements;
        }

        private Statement ParseStatement()
        {
            var stmt = new Statement();

        
            if (Current.Value == "wea_eman")
            {
                stmt.Type = "EmanFail";
                _pos++;
                stmt.Body = ParseBlock();

                if (Current.Value == "wea_fail")
                {
                    _pos++;
                    stmt.CatchBody = ParseBlock();
                }
                return stmt;
            }

            
            if (Current.Value == "wea_flow")
            {
                stmt.Type = "TaskDecl";
                _pos++; 
                while (Current.Type != TokenType.wea_sign_halt && Current.Value != "{")
                {
                    stmt.Tokens.Add(Current);
                    _pos++;
                }
                stmt.Body = ParseBlock();
                return stmt;
            }

         
            if (Current.Value == "wea_verify" || Current.Value == "wea_cycle")
            {
                stmt.Type = Current.Value == "wea_verify" ? "Check" : "Loop";
                _pos++;
                while (Current.Type != TokenType.wea_sign_halt && Current.Value != "{")
                {
                    stmt.Tokens.Add(Current);
                    _pos++;
                }
                stmt.Body = ParseBlock();
                return stmt;
            }

         
            if (Current.Value == "wea_emit")
            {
                stmt.Type = "Out";
               
                while (Current.Type != TokenType.wea_sign_halt && Current.Value != "\n")
                {
                    stmt.Tokens.Add(Current);
                    _pos++;
                }
                return stmt;
            }

            
            if (Current.Value == "wea_unit")
            {
                stmt.Type = "Assignment";
                while (Current.Type != TokenType.wea_sign_halt && Current.Value != "\n")
                {
                    stmt.Tokens.Add(Current);
                    _pos++;
                }
                return stmt;
            }

            
            if (Current.Type == TokenType.wea_sign_name && Peek(1).Value == "(")
            {
                stmt.Type = "FunctionCall";
                int parenCount = 0;
                while (Current.Type != TokenType.wea_sign_halt && Current.Value != "\n")
                {
                    if (Current.Value == "(") parenCount++;
                    if (Current.Value == ")") parenCount--;

                    stmt.Tokens.Add(Current);
                    _pos++;

                    if (parenCount == 0) break;
                }
                return stmt;
            }

           
            stmt.Type = "Assignment";
            while (Current.Type != TokenType.wea_sign_halt && Current.Value != "\n" && Current.Value != "}")
            {
             
                if (ReservedKeywords.Contains(Current.Value) && stmt.Tokens.Count > 0) break;

                stmt.Tokens.Add(Current);
                _pos++;
            }

            return stmt;
        }

        private List<Statement> ParseBlock()
        {
            var block = new List<Statement>();
            if (Current.Value == "{") _pos++;

            while (Current.Type != TokenType.wea_sign_halt && Current.Value != "}")
            {
                if (Current.Value == "\n" || string.IsNullOrWhiteSpace(Current.Value))
                {
                    _pos++;
                    continue;
                }
                block.Add(ParseStatement());
            }

            if (Current.Value == "}") _pos++;
            return block;
        }
    }
}
