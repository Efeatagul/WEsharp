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
                if (MatchKeyword("wea_flow")) return Function("function");
                if (MatchKeyword("wea_unit")) return VarDeclaration();
                return Statement();
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

        private Stmt Function(string kind)
        {
            Token name = Consume(TokenType.wea_sign_name, $"Beklenen: {kind} ismi.");
            Consume(TokenType.wea_sign_mark, "(");

            List<Token> parameters = new List<Token>();
            if (!Check(TokenType.wea_sign_mark))
            {
                do { parameters.Add(Consume(TokenType.wea_sign_name, "Parametre adi bekleniyor.")); }
                while (MatchTokenValue(","));
            }

            ConsumeTokenValue(")", "Parametre parantezi kapanmadi ')'");
            ConsumeTokenValue("{", "Fonksiyon govdesi acilmadi '{'");

            return new FunctionStmt(name, parameters, Block());
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.wea_sign_name, "Değişken adı bekleniyor.");
            Expr initializer = MatchTokenValue("=") ? Expression() : null;
            return new VarStmt(name, initializer);
        }

        private Stmt Statement()
        {
            if (MatchKeyword("wea_verify")) return IfStatement();
            if (MatchKeyword("wea_emit")) return PrintStatement();
            if (MatchKeyword("wea_read")) return ReadStatement();
            if (MatchKeyword("wea_cycle")) return WhileStatement();
            if (MatchKeyword("wea_eman")) return TryStatement();
            if (MatchTokenValue("{")) return new BlockStmt(Block());

            return ExpressionStatement();
        }

        private Stmt ReadStatement()
        {
            Expr msg = Check(TokenType.wea_sign_text) ? Expression() : null;
            return new ExpressionStmt(new CallExpr(
                new VariableExpr(new Token(TokenType.wea_sign_name, "wea_read", 0)),
                new Token(TokenType.wea_sign_mark, ")", 0),
                msg != null ? new List<Expr> { msg } : new List<Expr>()));
        }

        private Stmt IfStatement()
        {
            Expr condition = Expression();
            ConsumeTokenValue("{", "IF blogu acilmadi '{'");
            return new IfStmt(condition, new BlockStmt(Block()), null);
        }

        private Stmt PrintStatement() => new EmitStmt(Expression());

        private Stmt WhileStatement()
        {
            Expr condition = Expression();
            ConsumeTokenValue("{", "Dongu (Cycle) blogu acilmadi '{'");
            return new WhileStmt(condition, new BlockStmt(Block()));
        }

        private Stmt TryStatement()
        {
            ConsumeTokenValue("{", "Try (Eman) blogu acilmadi '{'");
            List<Stmt> tryBlock = Block();
            List<Stmt> catchBlock = new List<Stmt>();

            if (MatchKeyword("wea_fail"))
            {
                ConsumeTokenValue("{", "Catch (Fail) blogu acilmadi '{'");
                catchBlock = Block();
            }
            return new TryStmt(tryBlock, catchBlock);
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!CheckTokenValue("}") && !IsAtEnd())
            {
                statements.Add(Declaration());
            }
            ConsumeTokenValue("}", "Blok kapanmadi '}' eksik.");
            return statements;
        }

        private Stmt ExpressionStatement() => new ExpressionStmt(Expression());

        private Expr Expression() => Assignment();

        private Expr Assignment()
        {
            Expr expr = Equality();
            if (MatchTokenValue("="))
            {
                Expr value = Assignment();
                if (expr is VariableExpr v) return new AssignExpr(v.Name, value);
                throw new Exception("Gecersiz atama hedefi.");
            }
            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();
            while (MatchTokenValue("==") || MatchTokenValue("!="))
                expr = new BinaryExpr(expr, Previous(), Comparison());
            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Term();
            while (MatchTokenValue(">") || MatchTokenValue(">=") || MatchTokenValue("<") || MatchTokenValue("<="))
                expr = new BinaryExpr(expr, Previous(), Term());
            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();
            while (MatchTokenValue("+") || MatchTokenValue("-"))
                expr = new BinaryExpr(expr, Previous(), Factor());
            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();
            while (MatchTokenValue("*") || MatchTokenValue("/"))
                expr = new BinaryExpr(expr, Previous(), Unary());
            return expr;
        }

        private Expr Unary()
        {
            if (MatchTokenValue("-") || MatchTokenValue("!"))
                return new BinaryExpr(new LiteralExpr(new WNumber(0)), Previous(), Unary());
            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();
            while (true)
            {
                if (MatchTokenValue("(")) expr = FinishCall(expr);
                else break;
            }
            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> args = new List<Expr>();
            if (!CheckTokenValue(")"))
            {
                do { args.Add(Expression()); } while (MatchTokenValue(","));
            }
            Token paren = ConsumeTokenValue(")", "Fonksiyon cagrisi kapanmadi ')'");
            return new CallExpr(callee, paren, args);
        }

        private Expr Primary()
        {
            if (Match(TokenType.wea_sign_val)) return new LiteralExpr(new WNumber(double.Parse(Previous().Value, CultureInfo.InvariantCulture)));
            if (Match(TokenType.wea_sign_text)) return new LiteralExpr(new WString(Previous().Value));
            if (Match(TokenType.wea_sign_name)) return new VariableExpr(Previous());

            if (MatchTokenValue("("))
            {
                Expr expr = Expression();
                ConsumeTokenValue(")", "Grup parantezi kapanmadi ')'");
                return new GroupingExpr(expr);
            }

            throw new Exception($"Beklenmeyen sembol veya ifade: {Peek().Value}");
        }

        private bool Match(TokenType type) { if (Check(type)) { Advance(); return true; } return false; }
        private bool MatchTokenValue(string val) { if (CheckTokenValue(val)) { Advance(); return true; } return false; }
        private bool MatchKeyword(string key) { if (Check(TokenType.wea_sign_keyword) && Peek().Value == key) { Advance(); return true; } return false; }
        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
        private bool CheckTokenValue(string val) => !IsAtEnd() && Peek().Value == val;
        private Token Advance() { if (!IsAtEnd()) _current++; return Previous(); }
        private bool IsAtEnd() => Peek().Type == TokenType.wea_sign_halt;
        private Token Peek() => _tokens[_current];
        private Token Previous() => _tokens[_current - 1];
        private Token Consume(TokenType type, string m) { if (Check(type)) return Advance(); throw new Exception(m); }
        private Token ConsumeTokenValue(string v, string m) { if (CheckTokenValue(v)) return Advance(); throw new Exception(m); }

        private void Synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Previous().Value == ";") return;
                Advance();
            }
        }
    }
}
