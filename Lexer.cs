#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace WSharp
{
    
    public enum TokenType
    {
        wea_sign_keyword,   
        wea_sign_name,      
        wea_sign_val,       
        wea_sign_action,    
        wea_sign_mark,      
        wea_sign_text,      
        wea_sign_halt       
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public override string ToString() => $"[WEA_TOKEN -> {Type}: {Value}]";
    }

    public class Lexer
    {
        private readonly string _input;
        private int _pos = 0;
        private int _line = 1;

        
        private static readonly HashSet<string> Keywords = new HashSet<string> {
            "wea_flow", "wea_emit", "wea_verify", "wea_cycle",
            "wea_eman", "wea_fail", "wea_join", "wea_split", "wea_unit"
        };

        public Lexer(string input) => _input = input ?? "";

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            var sb = new StringBuilder();

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
                    while (_pos < _input.Length && _input[_pos] != '\n') _pos++;
                    continue;
                }

                
                if (char.IsDigit(current))
                {
                    sb.Clear();
                    while (_pos < _input.Length && (char.IsDigit(_input[_pos]) || _input[_pos] == '.'))
                    {
                        sb.Append(_input[_pos++]);
                    }
                    tokens.Add(CreateToken(TokenType.wea_sign_val, sb.ToString()));
                    continue;
                }

                
                if (char.IsLetter(current) || current == '_')
                {
                    sb.Clear();
                    while (_pos < _input.Length && (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_'))
                    {
                        sb.Append(_input[_pos++]);
                    }

                    string val = sb.ToString();
                    TokenType type = Keywords.Contains(val.ToLower()) ? TokenType.wea_sign_keyword : TokenType.wea_sign_name;
                    tokens.Add(CreateToken(type, val));
                    continue;
                }

                
                if (current == '"')
                {
                    _pos++; 
                    sb.Clear();
                    while (_pos < _input.Length && _input[_pos] != '"')
                    {
                        sb.Append(_input[_pos++]);
                    }
                    if (_pos < _input.Length) _pos++; 
                    tokens.Add(CreateToken(TokenType.wea_sign_text, sb.ToString()));
                    continue;
                }

             
                if ("+-*/%=(){},;><![].".Contains(current))
                {
                    string op = current.ToString();
                    _pos++;

                  
                    if (_pos < _input.Length)
                    {
                        string nextPair = op + _input[_pos];
                        if (nextPair == "==" || nextPair == "!=" || nextPair == "<=" || nextPair == ">=")
                        {
                            op = nextPair;
                            _pos++;
                        }
                    }

                 
                    bool isAction = (op == "=" || "+-*/%".Contains(op) || op == "==" || op == "!=" ||
                                     op == ">" || op == "<" || op == ">=" || op == "<=");

                    tokens.Add(CreateToken(isAction ? TokenType.wea_sign_action : TokenType.wea_sign_mark, op));
                    continue;
                }

                
                throw new Exception($"[WEA_SCANNER_ERROR]: Unexpected character '{current}' at line {_line}");
            }

            tokens.Add(CreateToken(TokenType.wea_sign_halt, "HALT"));
            return tokens;
        }

        private Token CreateToken(TokenType type, string value) => new Token { Type = type, Value = value, Line = _line };
    }
}
