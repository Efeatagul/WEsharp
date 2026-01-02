#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
    public enum TokenType
    {
        Keyword,    
        Identifier, 
        Number,     
        Operator,   
        Punctuation,
        String,    
        EOF        
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }

        public override string ToString() => $"[{Type}: {Value}]";
    }

    public class Lexer
    {
        private readonly string _input;
        private int _pos = 0;
        private int _line = 1;

        public Lexer(string input) => _input = input;

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_pos < _input.Length)
            {
                char current = _input[_pos];

             
                if (char.IsWhiteSpace(current))
                {
                    if (current == '\n') _line++;
                    _pos++;
                    continue;
                }

        
                if (current == '#')
                {
                   
                    while (_pos < _input.Length && _input[_pos] != '\n')
                    {
                        _pos++;
                    }
                    continue;
                }

             
                if (char.IsDigit(current))
                {
                    string value = "";
                    while (_pos < _input.Length && char.IsDigit(_input[_pos]))
                    {
                        value += _input[_pos++];
                    }
                    tokens.Add(new Token { Type = TokenType.Number, Value = value, Line = _line });
                    continue;
                }

           
                if (char.IsLetter(current))
                {
                    string value = "";
                    while (_pos < _input.Length && (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_'))
                    {
                        value += _input[_pos++];
                    }

                
                    var keywords = new List<string> {
                        "task", "out", "check", "loop", "eman", "fail", "and", "or"
                    };

                    TokenType type = keywords.Contains(value) ? TokenType.Keyword : TokenType.Identifier;
                    tokens.Add(new Token { Type = type, Value = value, Line = _line });
                    continue;
                }

             
                if (current == '"')
                {
                    _pos++;
                    string value = "";
                    while (_pos < _input.Length && _input[_pos] != '"')
                    {
                        value += _input[_pos++];
                    }
                    if (_pos < _input.Length) _pos++;
                    tokens.Add(new Token { Type = TokenType.String, Value = value, Line = _line });
                    continue;
                }

                if ("+-*/%=(){},;><![].".Contains(current))
                {
                    string value = current.ToString();

                    
                    if (_pos + 1 < _input.Length)
                    {
                        string next = value + _input[_pos + 1];
                        if (next == "==" || next == "<=" || next == ">=" || next == "!=")
                        {
                            value = next;
                            _pos++;
                        }
                    }

                    TokenType type = (value == "=" || "+-*/%".Contains(value) || value == "==")
                                     ? TokenType.Operator
                                     : TokenType.Punctuation;

                    tokens.Add(new Token { Type = type, Value = value, Line = _line });
                    _pos++;
                    continue;
                }

                throw new Exception($"WE# Lexer Hatasi: Beklenmeyen karakter '{current}' Satir: {_line}");
            }

            tokens.Add(new Token { Type = TokenType.EOF, Value = "EOF", Line = _line });
            return tokens;
        }
    }
}