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
// parsel.cs — WSharp Parser (with AST Line Number Tracking)
// ════════════════════════════════════════════════════════════
//
//  Every AST node gets a .Line = Previous().Line assignment
//  immediately after construction. This enables precise error
//  reporting: "Satır 42: Liste indeksi sınırlar dışında"
//
//  Approach: Property-based (not constructor-based) so that
//  all existing constructors remain unchanged.
//
// ════════════════════════════════════════════════════════════

#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace WSharp
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens) => _tokens = tokens;

        
        private int CurrentLine => Previous().Line;

        public List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                var decl = Declaration();
                if (decl != null) statements.Add(decl);
            }
            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if (MatchKeyword("import")) return ImportDeclaration();
                if (MatchKeyword("func")) return Function("function");
                if (MatchKeyword("let")) return VarDeclaration();
                return Statement();
            }
            catch (WSharpSyntaxException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[WEA_SYNTAX_ERROR] {ex.Message}");
                Console.ResetColor();
                Synchronize();
                return null;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[WEA_SYNTAX_ERROR] {ex.Message}");
                Console.ResetColor();
                Synchronize();
                return null;
            }
        }

        private Stmt ImportDeclaration()
        {
            Token pathToken = Consume(TokenType.wea_sign_text, "import sonrası dosya yolu (string) bekleniyor.");
            string filePath = pathToken.Value;
            string alias = System.IO.Path.GetFileNameWithoutExtension(filePath);
            var stmt = new ImportStmt(pathToken, alias);
            stmt.Line = pathToken.Line;
            return stmt;
        }

        private Stmt Function(string kind)
        {
            Token name = Consume(TokenType.wea_sign_name, $"Beklenen: {kind} ismi.");
            int line = name.Line;
            Consume(TokenType.wea_sign_mark, "(");

            List<Token> parameters = new List<Token>();
            if (!Check(TokenType.wea_sign_mark))
            {
                do { parameters.Add(Consume(TokenType.wea_sign_name, "Parametre adi bekleniyor.")); }
                while (MatchTokenValue(","));
            }

            ConsumeTokenValue(")", "Parametre parantezi kapanmadi ')'");
            ConsumeTokenValue("{", "Fonksiyon govdesi acilmadi '{'");

            var stmt = new FunctionStmt(name, parameters, Block());
            stmt.Line = line;
            return stmt;
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.wea_sign_name, "Değişken adı bekleniyor.");
            Expr initializer = MatchTokenValue("=") ? Expression() : null;
            var stmt = new VarStmt(name, initializer);
            stmt.Line = name.Line;
            return stmt;
        }

        private Stmt Statement()
        {
            if (MatchKeyword("if"))        return IfStatement();
            if (MatchKeyword("print"))    return PrintStatement();
            if (MatchKeyword("input"))    return ReadStatement();
            if (MatchKeyword("while"))    return WhileStatement();
            if (MatchKeyword("try"))      return TryStatement();
            if (MatchKeyword("foreach"))  return ForeachStatement();
            if (MatchKeyword("break"))    return BreakStatement();
            if (MatchKeyword("continue")) return ContinueStatement();
            if (MatchKeyword("return"))   return ReturnStatement();
            if (MatchTokenValue("{"))     return new BlockStmt(Block()) { Line = CurrentLine };

            return ExpressionStatement();
        }

        private Stmt ReadStatement()
        {
            int line = Peek().Line;
            Expr msg = Check(TokenType.wea_sign_text) ? Expression() : null;
            var stmt = new ExpressionStmt(new CallExpr(
                new VariableExpr(new Token(TokenType.wea_sign_name, "input", line)),
                new Token(TokenType.wea_sign_mark, ")", line),
                msg != null ? new List<Expr> { msg } : new List<Expr>()));
            stmt.Line = line;
            return stmt;
        }

        private Stmt IfStatement()
        {
            int line = CurrentLine;
            Expr condition = Expression();
            ConsumeTokenValue("{", "IF blogu acilmadi '{'");
            Stmt thenBranch = new BlockStmt(Block()) { Line = CurrentLine };
            Stmt elseBranch = null;

            if (MatchKeyword("else"))
            {
                if (MatchKeyword("if"))
                {
                    elseBranch = IfStatement();
                }
                else
                {
                    ConsumeTokenValue("{", "ELSE blogu acilmadi '{'");
                    elseBranch = new BlockStmt(Block()) { Line = CurrentLine };
                }
            }

            return new IfStmt(condition, thenBranch, elseBranch) { Line = line };
        }

        private Stmt PrintStatement()
        {
            int line = CurrentLine;
            var stmt = new EmitStmt(Expression());
            stmt.Line = line;
            return stmt;
        }

        private Stmt WhileStatement()
        {
            int line = CurrentLine;
            Expr condition = Expression();
            ConsumeTokenValue("{", "Dongu (Cycle) blogu acilmadi '{'");
            var stmt = new WhileStmt(condition, new BlockStmt(Block()) { Line = CurrentLine });
            stmt.Line = line;
            return stmt;
        }

        private Stmt ForeachStatement()
        {
            int line = CurrentLine;
            ConsumeTokenValue("(", "'foreach'tan sonra '(' bekleniyor.");
            Token name = Consume(TokenType.wea_sign_name, "Döngü değişkeni adı bekleniyor.");

            if (!MatchKeyword("in"))
                throw new WSharpSyntaxException("'in' anahtar kelimesi bekleniyor.", Peek().Line);

            Expr iterable = Expression();
            ConsumeTokenValue(")", "Foreach ')' ile kapanmadi.");
            ConsumeTokenValue("{", "Foreach blogu acilmadi '{'");

            var stmt = new ForeachStmt(name, iterable, new BlockStmt(Block()) { Line = CurrentLine });
            stmt.Line = line;
            return stmt;
        }

        private Stmt BreakStatement()
        {
            int line = CurrentLine;
            ConsumeTokenValue(";", "break komutundan sonra ';' bekleniyor.");
            return new BreakStmt() { Line = line };
        }

        private Stmt ContinueStatement()
        {
            int line = CurrentLine;
            ConsumeTokenValue(";", "continue komutundan sonra ';' bekleniyor.");
            return new ContinueStmt() { Line = line };
        }

        private Stmt ReturnStatement()
        {
            int line = CurrentLine;
            Expr value = null;

            if (!CheckTokenValue("}") && !IsAtEnd())
            {
                value = Expression();
            }
            return new ReturnStmt(value) { Line = line };
        }

        private Stmt TryStatement()
        {
            int line = CurrentLine;
            ConsumeTokenValue("{", "try blogu acilmadi '{'");
            List<Stmt> tryBlock = Block();
            List<Stmt> catchBlock = new List<Stmt>();

            if (MatchKeyword("catch"))
            {
                ConsumeTokenValue("{", "catch blogu acilmadi '{'");
                catchBlock = Block();
            }
            return new TryStmt(tryBlock, catchBlock) { Line = line };
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!CheckTokenValue("}") && !IsAtEnd())
            {
                
                var decl = Declaration();
                if (decl != null) statements.Add(decl);
            }
            ConsumeTokenValue("}", "Blok kapanmadi '}' eksik.");
            return statements;
        }

        private Stmt ExpressionStatement()
        {
            int line = Peek().Line;
            var stmt = new ExpressionStmt(Expression());
            stmt.Line = line;
            return stmt;
        }

        private Expr Expression()
        {
            if (Check(TokenType.wea_sign_name) && NextTokenIsArrow())
            {
               return Lambda();
            }
            if (CheckTokenValue("(") && IsLikelyLambda())
            {
                return Lambda();
            }
            return Assignment();
        }

        private bool NextTokenIsArrow()
        {
            if (_current + 1 >= _tokens.Count) return false;
            return _tokens[_current + 1].Value == "=>";
        }

        private bool IsLikelyLambda()
        {
             int i = _current;
             while (i < _tokens.Count && _tokens[i].Value != ")") i++;
             if (i >= _tokens.Count - 1) return false;
             return _tokens[i + 1].Value == "=>";
        }

        private Expr Lambda()
        {
            int line = Peek().Line;
            List<Token> parameters = new List<Token>();
            if (MatchTokenValue("("))
            {
                if (!CheckTokenValue(")"))
                {
                    do { parameters.Add(Consume(TokenType.wea_sign_name, "Parametre adi bekleniyor.")); }
                    while (MatchTokenValue(","));
                }
                ConsumeTokenValue(")", "Lambda parametreleri kapanmadi.");
            }
            else
            {
                parameters.Add(Consume(TokenType.wea_sign_name, "Parametre adi bekleniyor."));
            }

            ConsumeTokenValue("=>", "Lambda oku '=>' bekleniyor.");

            Stmt body;
            if (MatchTokenValue("{"))
            {
                 body = new BlockStmt(Block()) { Line = CurrentLine };
            }
            else
            {
                 body = new ExpressionStmt(Expression()) { Line = CurrentLine };
            }
            return new LambdaExpr(parameters, body) { Line = line };
        }

        private Expr Assignment()
        {
            Expr expr = Pipeline();
            if (MatchTokenValue("="))
            {
                int line = CurrentLine;
                Expr value = Expression(); 
                if (expr is VariableExpr v) return new AssignExpr(v.Name, value) { Line = line };
                if (expr is GetExpr get) return new SetExpr(get.Object, get.Name, value) { Line = line };
                throw new WSharpSyntaxException("Gecersiz atama hedefi.", CurrentLine);
            }
            return expr;
        }

        private Expr Pipeline()
        {
            Expr expr = LogicalOr();
            while (MatchTokenValue("|>"))
            {
                int line = CurrentLine;
                Expr right = Call();
                if (right is CallExpr call)
                {
                    call.Args.Insert(0, expr);
                    call.Line = line;
                    expr = call;
                }
                else
                {
                    throw new WSharpSyntaxException("Pipe operator '|>' sonrasinda bir fonksiyon cagrisi gelmelidir.", CurrentLine);
                }
            }
            return expr;
        }

        private Expr LogicalOr()
        {
            Expr expr = LogicalAnd();
            while (MatchTokenValue("||"))
            {
                Token oper = Previous();
                Expr right = LogicalAnd();
                expr = new LogicalExpr(expr, oper, right) { Line = oper.Line };
            }
            return expr;
        }

        private Expr LogicalAnd()
        {
            Expr expr = Equality();
            while (MatchTokenValue("&&"))
            {
                Token oper = Previous();
                Expr right = Equality();
                expr = new LogicalExpr(expr, oper, right) { Line = oper.Line };
            }
            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();
            while (MatchTokenValue("==") || MatchTokenValue("!="))
            {
                Token op = Previous();
                expr = new BinaryExpr(expr, op, Comparison()) { Line = op.Line };
            }
            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Term();
            while (MatchTokenValue(">") || MatchTokenValue(">=") || MatchTokenValue("<") || MatchTokenValue("<="))
            {
                Token op = Previous();
                expr = new BinaryExpr(expr, op, Term()) { Line = op.Line };
            }
            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();
            while (MatchTokenValue("+") || MatchTokenValue("-"))
            {
                Token op = Previous();
                expr = new BinaryExpr(expr, op, Factor()) { Line = op.Line };
            }
            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();
            while (MatchTokenValue("*") || MatchTokenValue("/") || MatchTokenValue("%"))
            {
                Token op = Previous();
                expr = new BinaryExpr(expr, op, Unary()) { Line = op.Line };
            }
            return expr;
        }

        private Expr Unary()
        {
            if (MatchTokenValue("-") || MatchTokenValue("!"))
            {
                Token oper = Previous();
                Expr right = Unary();
          
                return new UnaryExpr(oper, right) { Line = oper.Line };
            }
            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();
            while (true)
            {
                if (MatchTokenValue("("))
                {
                    expr = FinishCall(expr);
                }
                else if (MatchTokenValue("."))
                {
                    Token name = Consume(TokenType.wea_sign_name, "Ozellik adi bekleniyor.");
                    expr = new GetExpr(expr, name) { Line = name.Line };
                }
                else if (MatchTokenValue("["))
                {
                    int line = CurrentLine;
                    Expr start = Expression();
                    Expr end = null;

                    if (MatchTokenValue(":"))
                    {
                        end = Expression();
                    }

                    ConsumeTokenValue("]", "Indeks kapama ']' eksik.");
                    expr = new IndexExpr(expr, start, end) { Line = line };
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            int line = CurrentLine;
            List<Expr> args = new List<Expr>();
            if (!CheckTokenValue(")"))
            {
                do { args.Add(Expression()); } while (MatchTokenValue(","));
            }
            Token paren = ConsumeTokenValue(")", "Fonksiyon cagrisi kapanmadi ')'");
            return new CallExpr(callee, paren, args) { Line = line };
        }

        private Expr Primary()
        {
            if (Match(TokenType.wea_sign_val))
                return new LiteralExpr(new WValue(double.Parse(Previous().Value, CultureInfo.InvariantCulture))) { Line = Previous().Line };
            if (Match(TokenType.wea_sign_text))
                return new LiteralExpr(new WValue(Previous().Value)) { Line = Previous().Line };

            if (MatchKeyword("true"))   return new LiteralExpr(new WValue(true))  { Line = CurrentLine };
            if (MatchKeyword("false"))  return new LiteralExpr(new WValue(false)) { Line = CurrentLine };
            if (MatchKeyword("null"))   return new LiteralExpr(new WValue(null))  { Line = CurrentLine };

            if (MatchKeyword("is_key"))
            {
                int line = CurrentLine;
                ConsumeTokenValue("(", "is_key sonrasi '(' bekleniyor.");
                Expr obj = Expression();
                ConsumeTokenValue(",", "is_key icinde ',' bekleniyor.");
                Expr key = Expression();
                ConsumeTokenValue(")", "is_key ')' ile kapanmadi.");
                return new IsKeyExpr(obj, key) { Line = line };
            }

            if (Match(TokenType.wea_sign_name))
                return new VariableExpr(Previous()) { Line = Previous().Line };

            if (MatchTokenValue("("))
            {
                int line = CurrentLine;
                Expr expr = Expression();
                ConsumeTokenValue(")", "Grup parantezi kapanmadi ')'");
                return new GroupingExpr(expr) { Line = line };
            }

            if (MatchTokenValue("[")) return ListLiteral();
            if (MatchTokenValue("{")) return DictLiteral();

            throw new WSharpSyntaxException($"Beklenmeyen sembol veya ifade: '{Peek().Value}'", Peek().Line);
        }

        private Expr ListLiteral()
        {
            int line = CurrentLine;
            List<Expr> elements = new List<Expr>();
            if (!CheckTokenValue("]"))
            {
                do { elements.Add(Expression()); } while (MatchTokenValue(","));
            }
            ConsumeTokenValue("]", "Liste kapama parantezi eksik ']'");
            return new ListExpr(elements) { Line = line };
        }

        private Expr DictLiteral()
        {
            int line = CurrentLine;
            List<Expr> keys = new List<Expr>();
            List<Expr> values = new List<Expr>();
            if (!CheckTokenValue("}"))
            {
                do
                {
                    keys.Add(Expression());
                    ConsumeTokenValue(":", "Anahtar-değer ayırıcı ':' bekleniyor.");
                    values.Add(Expression());
                } while (MatchTokenValue(","));
            }
            ConsumeTokenValue("}", "Sözlük kapama parantezi eksik '}'");
            return new DictExpr(keys, values) { Line = line };
        }

        private bool Match(TokenType type) { if (Check(type)) { Advance(); return true; } return false; }
        private bool MatchTokenValue(string val) { if (CheckTokenValue(val)) { Advance(); return true; } return false; }
        private bool MatchKeyword(string key) { if (Check(TokenType.wea_sign_keyword) && Peek().Value == key) { Advance(); return true; } return false; }
        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
        private bool CheckTokenValue(string val) => !IsAtEnd() && Peek().Value == val && Peek().Type != TokenType.wea_sign_text && Peek().Type != TokenType.wea_sign_val;
        private Token Advance() { if (!IsAtEnd()) _current++; return Previous(); }
        private bool IsAtEnd() => Peek().Type == TokenType.wea_sign_halt;
        private Token Peek() => _tokens[_current];
        private Token Previous() => _tokens[_current - 1];

        private Token Consume(TokenType type, string m)
        {
            if (Check(type)) return Advance();
            throw new WSharpSyntaxException(m, Peek().Line);
        }

        private Token ConsumeTokenValue(string v, string m)
        {
            if (CheckTokenValue(v)) return Advance();
            throw new WSharpSyntaxException(m, Peek().Line);
        }

        private void Synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Peek().Type == TokenType.wea_sign_keyword)
                {
                    string val = Peek().Value;
                    if (val == "let" || val == "func" || val == "if" ||
                        val == "print" || val == "input" || val == "while" ||
                        val == "try" || val == "foreach" || val == "return")
                        return;
                }
                if (Previous().Value == "}") return;
                Advance();
            }
        }
    }
}