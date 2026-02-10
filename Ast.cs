#nullable disable
using System.Collections.Generic;

namespace WSharp
{
    public interface IVisitor<R>
    {
        R Visit(BinaryExpr expr);
        R Visit(GroupingExpr expr);
        R Visit(LiteralExpr expr);
        R Visit(VariableExpr expr);
        R Visit(AssignExpr expr);
        R Visit(CallExpr expr);
        R Visit(ListExpr expr);
        R Visit(DictExpr expr);
        R Visit(GetExpr expr);
        R Visit(SetExpr expr);
        R Visit(IndexExpr expr);
        R Visit(UnaryExpr expr);
        R Visit(LambdaExpr expr);
        R Visit(LogicalExpr expr);
        R Visit(IsKeyExpr expr);
    }

    public interface IStmtVisitor<R>
    {
        R Visit(EmitStmt stmt);
        R Visit(VarStmt stmt);
        R Visit(BlockStmt stmt);
        R Visit(IfStmt stmt);
        R Visit(WhileStmt stmt);
        R Visit(FunctionStmt stmt);
        R Visit(TryStmt stmt);
        R Visit(ExpressionStmt stmt);
        R Visit(ForeachStmt stmt);
        R Visit(BreakStmt stmt);
        R Visit(ContinueStmt stmt);
        R Visit(ReturnStmt stmt);
    }

    public abstract class Expr
    {
        public abstract R Accept<R>(IVisitor<R> visitor);
    }

    public abstract class Stmt
    {
        public abstract R Accept<R>(IStmtVisitor<R> visitor);
    }

    public class BinaryExpr : Expr
    {
        public Expr Left;
        public Token Operator;
        public Expr Right;
        public BinaryExpr(Expr left, Token op, Expr right) { Left = left; Operator = op; Right = right; }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public class GroupingExpr : Expr
    {
        public Expr Expression;
        public GroupingExpr(Expr expression) => Expression = expression;
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public class LiteralExpr : Expr
    {
        public WValue Value;
        public LiteralExpr(WValue value) => Value = value;
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    
    public class DictExpr : Expr
    {
        public List<Expr> Keys;
        public List<Expr> Values;
        public DictExpr(List<Expr> keys, List<Expr> values) { Keys = keys; Values = values; }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public class GetExpr : Expr
    {
        public Expr Object;
        public Token Name;
        public GetExpr(Expr obj, Token name) { Object = obj; Name = name; }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public class SetExpr : Expr
    {
        public Expr Object;
        public Token Name;
        public Expr Value;
        public SetExpr(Expr obj, Token name, Expr value) { Object = obj; Name = name; Value = value; }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    
    public class IndexExpr : Expr
    {
        public Expr Object;
        public Expr Start;
        public Expr End; 
        
        public IndexExpr(Expr obj, Expr start, Expr end)
        {
            Object = obj;
            Start = start;
            End = end;
        }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }
    
    public class ListExpr : Expr
    {
        public List<Expr> Elements;
        public ListExpr(List<Expr> elements) => Elements = elements;
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public class VariableExpr : Expr
    {
        public Token Name;
        public VariableExpr(Token name) => Name = name;
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public class UnaryExpr : Expr
    {
        public Token Operator;
        public Expr Right;
        public UnaryExpr(Token operatorToken, Expr right)
        {
            Operator = operatorToken;
            Right = right;
        }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public class AssignExpr : Expr
    {
        public Token Name;
        public Expr Value;
        public AssignExpr(Token name, Expr value) { Name = name; Value = value; }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    public class CallExpr : Expr
    {
        public Expr Callee;
        public Token Paren;
        public List<Expr> Args;

        public CallExpr(Expr callee, Token paren, List<Expr> args)
        {
            Callee = callee; Paren = paren; Args = args;
        }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    

    public class EmitStmt : Stmt
    {
        public Expr Expr;
        public EmitStmt(Expr expr) => Expr = expr;
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class VarStmt : Stmt
    {
        public Token Name;
        public Expr Initializer;
        public VarStmt(Token name, Expr initializer) { Name = name; Initializer = initializer; }
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class BlockStmt : Stmt
    {
        public List<Stmt> Statements;
        public BlockStmt(List<Stmt> statements) => Statements = statements;
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class IfStmt : Stmt
    {
        public Expr Condition;
        public Stmt ThenBranch;
        public Stmt ElseBranch;
        public IfStmt(Expr condition, Stmt thenBranch, Stmt elseBranch)
        {
            Condition = condition; ThenBranch = thenBranch; ElseBranch = elseBranch;
        }
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class WhileStmt : Stmt
    {
        public Expr Condition;
        public Stmt Body;
        public WhileStmt(Expr condition, Stmt body) { Condition = condition; Body = body; }
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class FunctionStmt : Stmt
    {
        public Token Name;
        public List<Token> Params;
        public List<Stmt> Body;
        public FunctionStmt(Token name, List<Token> parameters, List<Stmt> body)
        {
            Name = name; Params = parameters; Body = body;
        }
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class TryStmt : Stmt
    {
        public List<Stmt> TryBlock;
        public List<Stmt> CatchBlock;
        public TryStmt(List<Stmt> tryBlock, List<Stmt> catchBlock) { TryBlock = tryBlock; CatchBlock = catchBlock; }
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class ExpressionStmt : Stmt
    {
        public Expr Expression;
        public ExpressionStmt(Expr expression) => Expression = expression;
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class ForeachStmt : Stmt
    {
        public Token Name;
        public Expr Iterable;
        public Stmt Body;
        public ForeachStmt(Token name, Expr iterable, Stmt body) { Name = name; Iterable = iterable; Body = body; }
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class BreakStmt : Stmt
    {
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class ContinueStmt : Stmt
    {
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }

    public class LambdaExpr : Expr
    {
        public List<Token> Parameters;
        public Stmt Body; 
        public LambdaExpr(List<Token> parameters, Stmt body) { Parameters = parameters; Body = body; }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

  
    public class LogicalExpr : Expr
    {
        public Expr Left;
        public Token Operator;
        public Expr Right;
        public LogicalExpr(Expr left, Token op, Expr right) { Left = left; Operator = op; Right = right; }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    
    public class IsKeyExpr : Expr
    {
        public Expr Object; 
        public Expr Key;    
        public IsKeyExpr(Expr obj, Expr key) { Object = obj; Key = key; }
        public override R Accept<R>(IVisitor<R> visitor) => visitor.Visit(this);
    }

    
    public class ReturnStmt : Stmt
    {
        public Expr Value; 
        public ReturnStmt(Expr value) => Value = value;
        public override R Accept<R>(IStmtVisitor<R> visitor) => visitor.Visit(this);
    }
}
