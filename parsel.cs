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

        public Parser(List<Token> tokens) => _tokens = tokens;

        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : new Token { Type = TokenType.EOF, Value = "" };
        private Token Peek(int distance) => (_pos + distance < _tokens.Count) ? _tokens[_pos + distance] : new Token { Type = TokenType.EOF, Value = "" };

        public List<Statement> Parse()
        {
            var statements = new List<Statement>();
            while (_pos < _tokens.Count && Current.Type != TokenType.EOF)
            {
                statements.Add(ParseStatement());
            }
            return statements;
        }

        private Statement ParseStatement()
        {
            var stmt = new Statement();

          
            if (Current.Value == "eman")
            {
                stmt.Type = "EmanFail";
                _pos++;
                stmt.Body = ParseBlock();

                if (_pos < _tokens.Count && Current.Value == "fail")
                {
                    _pos++;
                    stmt.CatchBody = ParseBlock();
                }
                return stmt;
            }

            if (Current.Value == "task")
            {
                stmt.Type = "TaskDecl";
                while (_pos < _tokens.Count && Current.Value != "{")
                {
                    stmt.Tokens.Add(Current);
                    _pos++;
                }
                stmt.Body = ParseBlock();
                return stmt;
            }

      
            if (Current.Value == "check" || Current.Value == "loop")
            {
                stmt.Type = Current.Value == "check" ? "Check" : "Loop";
                while (_pos < _tokens.Count && Current.Value != "{")
                {
                    stmt.Tokens.Add(Current);
                    _pos++;
                }
                stmt.Body = ParseBlock();
                return stmt;
            }

           
            if (Current.Type == TokenType.Identifier && Peek(1).Value == "(")
            {
                stmt.Type = "FunctionCall";
                while (_pos < _tokens.Count && Current.Value != "\n" && Current.Type != TokenType.EOF)
                {
                    stmt.Tokens.Add(Current);
                    _pos++;
                    if (stmt.Tokens.Last().Value == ")") break; 
                }
                return stmt;
            }

         
            if (Current.Value == "out")
            {
                stmt.Type = "Out";
                while (_pos < _tokens.Count && Current.Value != "\n" && Current.Type != TokenType.EOF)
                {
                    stmt.Tokens.Add(Current);
                    _pos++;
                   
                }
                return stmt;
            }

          
            stmt.Type = "Assignment";
            while (_pos < _tokens.Count && Current.Value != "\n" && Current.Value != "}" && Current.Type != TokenType.EOF)
            {

                if (new[] { "task", "check", "out", "eman", "loop" }.Contains(Current.Value) && stmt.Tokens.Count > 0) break;

                stmt.Tokens.Add(Current);
                _pos++;
            }

            return stmt;
        }

        private List<Statement> ParseBlock()
        {
            var block = new List<Statement>();
            if (Current.Value == "{") _pos++;

            while (_pos < _tokens.Count && Current.Value != "}")
            {
                block.Add(ParseStatement());
            }

            if (_pos < _tokens.Count && Current.Value == "}") _pos++;
            return block;
        }
    }
}
