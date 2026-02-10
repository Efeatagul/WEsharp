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
            if (MatchKeyword("foreach")) return ForeachStatement();
            if (MatchKeyword("break")) return BreakStatement();
            if (MatchKeyword("continue")) return ContinueStatement();
            if (MatchKeyword("wea_return")) return ReturnStatement();
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
            Stmt thenBranch = new BlockStmt(Block());
            Stmt elseBranch = null;

            if (MatchKeyword("wea_else"))
            {
                if (MatchKeyword("wea_verify"))
                {
                    
                    elseBranch = IfStatement();
                }
                else
                {
                    ConsumeTokenValue("{", "ELSE blogu acilmadi '{'");
                    elseBranch = new BlockStmt(Block());
                }
            }

            return new IfStmt(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement() => new EmitStmt(Expression());

        private Stmt WhileStatement()
        {
            Expr condition = Expression();
            ConsumeTokenValue("{", "Dongu (Cycle) blogu acilmadi '{'");
            return new WhileStmt(condition, new BlockStmt(Block()));
        }

        private Stmt ForeachStatement()
        {
            ConsumeTokenValue("(", "'foreach'tan sonra '(' bekleniyor.");
            Token name = Consume(TokenType.wea_sign_name, "Döngü değişkeni adı bekleniyor.");
            
            if (!MatchKeyword("in")) throw new Exception("'in' anahtar kelimesi bekleniyor.");
            
            Expr iterable = Expression();
            ConsumeTokenValue(")", "Foreach ')' ile kapanmadi.");
            
            ConsumeTokenValue("{", "Foreach blogu acilmadi '{'");
            return new ForeachStmt(name, iterable, new BlockStmt(Block()));
        }

        private Stmt BreakStatement()
        {
            ConsumeTokenValue(";", "Break komutundan sonra ';' bekleniyor.");
            return new BreakStmt();
        }

        private Stmt ContinueStatement()
        {
            ConsumeTokenValue(";", "Continue komutundan sonra ';' bekleniyor.");
            return new ContinueStmt();
        }

        private Stmt ReturnStatement()
        {
            Expr value = null;
            
            if (!CheckTokenValue("}") && !IsAtEnd())
            {
                value = Expression();
            }
            return new ReturnStmt(value);
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
                 body = new BlockStmt(Block());
            }
            else
            {
                 
                 body = new ExpressionStmt(Expression()); 
            }
            return new LambdaExpr(parameters, body);
        }

        private Expr Assignment()
        {
            Expr expr = Pipeline();
            if (MatchTokenValue("="))
            {
                Expr value = Assignment();
                if (expr is VariableExpr v) return new AssignExpr(v.Name, value);
                if (expr is GetExpr get) return new SetExpr(get.Object, get.Name, value);
                throw new Exception("Gecersiz atama hedefi.");
            }
            return expr;
        }

        private Expr Pipeline()
        {
            Expr expr = LogicalOr();
            while (MatchTokenValue("|>"))
            {
                Expr right = Call();
                if (right is CallExpr call)
                {
                    call.Args.Insert(0, expr);
                    expr = call;
                }
                else
                {
                    throw new Exception("Pipe operator '|>' sonrasinda bir fonksiyon cagrisi gelmelidir.");
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
                expr = new LogicalExpr(expr, oper, right);
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
                expr = new LogicalExpr(expr, oper, right);
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
            while (MatchTokenValue("*") || MatchTokenValue("/") || MatchTokenValue("%"))
                expr = new BinaryExpr(expr, Previous(), Unary());
            return expr;
        }

        private Expr Unary()
        {
            if (MatchTokenValue("-") || MatchTokenValue("!"))
            {
                Token oper = Previous();
                Expr right = Unary();

                if (oper.Value == "-")
                    return new BinaryExpr(new LiteralExpr(new WValue(0.0)), oper, right);

                return new UnaryExpr(oper, right);
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
                    expr = new GetExpr(expr, name);
                }
                else if (MatchTokenValue("["))
                {
                    Expr start = Expression();
                    Expr end = null;

                    if (MatchTokenValue(":"))
                    {
                        end = Expression();
                    }

                    ConsumeTokenValue("]", "Indeks kapama ']' eksik.");
                    expr = new IndexExpr(expr, start, end);
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
            if (Match(TokenType.wea_sign_val)) return new LiteralExpr(new WValue(double.Parse(Previous().Value, CultureInfo.InvariantCulture)));
            if (Match(TokenType.wea_sign_text)) return new LiteralExpr(new WValue(Previous().Value));

            
            if (MatchKeyword("dogru")) return new LiteralExpr(new WValue(true));
            if (MatchKeyword("yanlis")) return new LiteralExpr(new WValue(false));
            if (MatchKeyword("bos")) return new LiteralExpr(new WValue(null));

            
            if (MatchKeyword("is_key"))
            {
                ConsumeTokenValue("(", "is_key sonrasi '(' bekleniyor.");
                Expr obj = Expression();
                ConsumeTokenValue(",", "is_key icinde ',' bekleniyor.");
                Expr key = Expression();
                ConsumeTokenValue(")", "is_key ')' ile kapanmadi.");
                return new IsKeyExpr(obj, key);
            }

            if (Match(TokenType.wea_sign_name)) return new VariableExpr(Previous());

            if (MatchTokenValue("("))
            {
                Expr expr = Expression();
                ConsumeTokenValue(")", "Grup parantezi kapanmadi ')'");
                return new GroupingExpr(expr);
            }

            if (MatchTokenValue("[")) return ListLiteral();
            if (MatchTokenValue("{")) return DictLiteral();

            throw new Exception($"Beklenmeyen sembol veya ifade: {Peek().Value}");
        }

        private Expr ListLiteral()
        {
            List<Expr> elements = new List<Expr>();
            if (!CheckTokenValue("]"))
            {
                do
                {
                    elements.Add(Expression());
                } while (MatchTokenValue(","));
            }
            ConsumeTokenValue("]", "Liste kapama parantezi eksik ']'");
            return new ListExpr(elements);
        }

        private Expr DictLiteral()
        {
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
            return new DictExpr(keys, values);
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
        private Token Consume(TokenType type, string m) { if (Check(type)) return Advance(); throw new Exception(m); }
        private Token ConsumeTokenValue(string v, string m) { if (CheckTokenValue(v)) return Advance(); throw new Exception(m); }

        private void Synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Peek().Type == TokenType.wea_sign_keyword)
                {
                    string val = Peek().Value;
                    if (val == "wea_unit" || val == "wea_flow" || val == "wea_verify" ||
                        val == "wea_emit" || val == "wea_read" || val == "wea_cycle" ||
                        val == "wea_eman" || val == "foreach" || val == "wea_return")
                        return;
                }
                if (Previous().Value == "}") return;
                Advance();
            }
        }
    }
}
