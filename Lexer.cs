    #nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
    public enum TokenType
    {
        wea_sign_keyword,
        wea_sign_name,
        wea_sign_val,
        wea_sign_text,
        wea_sign_mark,
        wea_sign_halt
    }

    public class Token
    {
        public TokenType Type;
        public string Value;
        public int Line;

        public Token(TokenType type, string value, int line)
        {
            Type = type;
            Value = value;
            Line = line;
        }
    }

    public class Lexer
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        public Lexer(string source) => _source = source;

        public List<Token> Tokenize()
        {
            while (!IsAtEnd()) { _start = _current; ScanToken(); }
            _tokens.Add(new Token(TokenType.wea_sign_halt, "", _line));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.wea_sign_mark); break;
                case ')': AddToken(TokenType.wea_sign_mark); break;
                case '{': AddToken(TokenType.wea_sign_mark); break;
                case '}': AddToken(TokenType.wea_sign_mark); break;
                case ',': AddToken(TokenType.wea_sign_mark); break;
                case '-': AddToken(TokenType.wea_sign_mark); break;
                case '+': AddToken(TokenType.wea_sign_mark); break;
                case '*': AddToken(TokenType.wea_sign_mark); break;
                case '%': AddToken(TokenType.wea_sign_mark); break;
                case '[': AddToken(TokenType.wea_sign_mark); break;
                case ']': AddToken(TokenType.wea_sign_mark); break;
                case ':': AddToken(TokenType.wea_sign_mark); break;
                case '.': AddToken(TokenType.wea_sign_mark); break;

                
                case '&': if (Match('&')) AddToken(TokenType.wea_sign_mark, "&&"); break;


                
                case '|': AddToken(TokenType.wea_sign_mark, Match('>') ? "|>" : Match('|') ? "||" : "|"); break;

                case ';': break;
                case '/': if (Match('/')) while (Peek() != '\n' && !IsAtEnd()) Advance(); else AddToken(TokenType.wea_sign_mark); break;
                case '!': AddToken(TokenType.wea_sign_mark, Match('=') ? "!=" : "!"); break;
                case '=': AddToken(TokenType.wea_sign_mark, Match('=') ? "==" : Match('>') ? "=>" : "="); break;
                case '<': AddToken(TokenType.wea_sign_mark, Match('=') ? "<=" : "<"); break;
                case '>': AddToken(TokenType.wea_sign_mark, Match('=') ? ">=" : ">"); break;
                case ' ': case '\r': case '\t': break;
                case '\n': _line++; break;
                case '"': String(); break;
                default:
                    if (IsDigit(c)) Number();
                    else if (IsAlpha(c)) Identifier();
                    break;
            }
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd()) { if (Peek() == '\n') _line++; Advance(); }
            if (IsAtEnd()) return;
            Advance();
            _tokens.Add(new Token(TokenType.wea_sign_text, _source.Substring(_start + 1, _current - _start - 2), _line));
        }

        private void Number()
        {
            while (IsDigit(Peek())) Advance();
            if (Peek() == '.' && IsDigit(PeekNext())) { Advance(); while (IsDigit(Peek())) Advance(); }
            AddToken(TokenType.wea_sign_val);
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();
            string text = _source.Substring(_start, _current - _start);
            TokenType type = TokenType.wea_sign_name;
            switch (text)
            {
                case "wea_emit":
                case "wea_unit":
                case "wea_flow":
                case "wea_verify":
                case "wea_else":
                case "wea_cycle":
                case "wea_eman":
                case "wea_fail":
                case "wea_read":
                case "wea_return":
                case "is_key": 
                case "foreach":
                case "in":
                case "break":
                case "continue":
                case "dogru":
                case "yanlis":
                case "bos": type = TokenType.wea_sign_keyword; break;
            }
            _tokens.Add(new Token(type, text, _line));
        }

        private void AddToken(TokenType type) => AddToken(type, _source.Substring(_start, _current - _start));
        private void AddToken(TokenType type, string literal) => _tokens.Add(new Token(type, literal, _line));
        private bool Match(char expected) { if (IsAtEnd() || _source[_current] != expected) return false; _current++; return true; }
        private char Peek() => IsAtEnd() ? '\0' : _source[_current];
        private char PeekNext() => (_current + 1 >= _source.Length) ? '\0' : _source[_current + 1];
        private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
        private bool IsDigit(char c) => c >= '0' && c <= '9';
        private bool IsAtEnd() => _current >= _source.Length;
        private char Advance() => _source[_current++];
    }
}
