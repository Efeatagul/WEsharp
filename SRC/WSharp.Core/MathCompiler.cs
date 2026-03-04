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
// MathCompiler.cs — WSharp Math JIT Compiler
// ════════════════════════════════════════════
//
//  Compiles a WSharp FunctionStmt / LambdaExpr that describes
//  neuron physics (e.g. izhikevich(v, u, I) { ... }) into a
//  native CLR delegate via System.Linq.Expressions.
//
//  Output signature:
//      Func<float, float, float, ValueTuple<float, float>>
//      where the three floats are the function's first three
//      parameters (v, u, I) and the tuple is (new_v, new_u).
//
//  Supported AST nodes:
//      ✓ LiteralExpr   (numeric constants → float)
//      ✓ VariableExpr  (parameters + cellula locals)
//      ✓ BinaryExpr    (+, -, *, /)
//      ✓ UnaryExpr     (negation via 0 - x)
//      ✓ GroupingExpr
//      ✓ VarStmt       (cellula x = …)
//      ✓ AssignExpr    (x = …)
//      ✓ ReturnStmt    (redeo [v_expr, u_expr])
//      ✓ BlockStmt / ExpressionStmt
//
//  Hot-path guarantee:
//      Once compiled, no WValue / Interpreter is involved.
//      The resulting delegate runs at native CLR speed.
//
// ════════════════════════════════════════════

#nullable disable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace WSharp
{
        public static class MathCompiler
    {
        
        public static CompiledNeuroFunc Compile(FunctionStmt stmt)
        {
            if (stmt == null) throw new ArgumentNullException(nameof(stmt));

            var ctx = new CompileContext(stmt.Params);
            var body = CompileBlock(stmt.Body, ctx);

            return BuildDelegate(ctx, body, stmt.Name.Value, stmt.Params.Count);
        }

        
        public static CompiledNeuroFunc Compile(LambdaExpr lambda)
        {
            if (lambda == null) throw new ArgumentNullException(nameof(lambda));

            var ctx = new CompileContext(lambda.Parameters);

            
            List<Expression> body;
            if (lambda.Body is BlockStmt block)
                body = CompileBlock(block.Statements, ctx);
            else if (lambda.Body is ExpressionStmt single)
                body = CompileImplicitReturn(single.Expression, ctx);
            else
                throw new MathCompilerException(
                    "Lambda gövdesi bir blok veya ifade olmalıdır.",
                    lambda.Line);

            return BuildDelegate(ctx, body, "<lambda>", lambda.Parameters.Count);
        }

       

        private sealed class CompileContext
        {
            
            public readonly Dictionary<string, ParameterExpression> Params =
                new Dictionary<string, ParameterExpression>(StringComparer.Ordinal);

            
            public readonly Dictionary<string, ParameterExpression> Locals =
                new Dictionary<string, ParameterExpression>(StringComparer.Ordinal);

            
            public ParameterExpression ReturnVar;
            public LabelTarget ReturnLabel;

            public CompileContext(IList<Token> parameters)
            {
                foreach (var p in parameters)
                    Params[p.Value] = Expression.Parameter(typeof(float), p.Value);

                
                ReturnVar = Expression.Variable(typeof(ValueTuple<float, float>), "__result");
                ReturnLabel = Expression.Label(typeof(ValueTuple<float, float>), "__return");
            }

            
            public ParameterExpression Resolve(string name, int sourceLine)
            {
                if (Params.TryGetValue(name, out var p)) return p;
                if (Locals.TryGetValue(name, out var l)) return l;
                throw new MathCompilerException(
                    $"MathCompiler: '{name}' değişkeni tanımlı değil. " +
                    $"Desteklenen: [{string.Join(", ", Params.Keys)}]",
                    sourceLine);
            }

           
            public IEnumerable<ParameterExpression> AllLocals()
            {
                yield return ReturnVar;
                foreach (var l in Locals.Values) yield return l;
            }
        }

    

        private static List<Expression> CompileBlock(List<Stmt> stmts, CompileContext ctx)
        {
            var exprs = new List<Expression>();
            foreach (var stmt in stmts)
                exprs.AddRange(CompileStmt(stmt, ctx));
            return exprs;
        }

        private static IEnumerable<Expression> CompileStmt(Stmt stmt, CompileContext ctx)
        {
            switch (stmt)
            {
                
                case VarStmt varStmt:
                {
                    var local = Expression.Variable(typeof(float), varStmt.Name.Value);
                    ctx.Locals[varStmt.Name.Value] = local;

                    if (varStmt.Initializer != null)
                        yield return Expression.Assign(local, CompileExpr(varStmt.Initializer, ctx));
                    else
                        yield return Expression.Assign(local, Expression.Constant(0f));
                    break;
                }

             
                case ReturnStmt retStmt:
                {
                    if (retStmt.Value == null)
                        throw new MathCompilerException(
                            "MathCompiler: 'return' bir liste ifadesi bekleniyor: return [yeni_v, yeni_u]",
                            retStmt.Line);

                    Expression resultExpr;

                    if (retStmt.Value is ListExpr listExpr)
                    {
  
                        if (listExpr.Elements.Count < 2)
                            throw new MathCompilerException(
                                "MathCompiler: return listesi en az 2 eleman içermelidir: return [yeni_v, yeni_u]",
                                retStmt.Line);

                        var item1 = Expression.Convert(
                            CompileExpr(listExpr.Elements[0], ctx), typeof(float));
                        var item2 = Expression.Convert(
                            CompileExpr(listExpr.Elements[1], ctx), typeof(float));

                        var tupleCtor = typeof(ValueTuple<float, float>)
                            .GetConstructor(new[] { typeof(float), typeof(float) });
                        resultExpr = Expression.New(tupleCtor, item1, item2);
                    }
                    else
                    {
                       
                        var tupleCtor = typeof(ValueTuple<float, float>)
                            .GetConstructor(new[] { typeof(float), typeof(float) });
                        resultExpr = Expression.New(tupleCtor,
                            Expression.Convert(CompileExpr(retStmt.Value, ctx), typeof(float)),
                            Expression.Constant(0f));
                    }

                    yield return Expression.Assign(ctx.ReturnVar, resultExpr);
                    yield return Expression.Goto(ctx.ReturnLabel, ctx.ReturnVar);
                    break;
                }

               
                case BlockStmt blockStmt:
                    foreach (var e in CompileBlock(blockStmt.Statements, ctx))
                        yield return e;
                    break;

                
                case ExpressionStmt exprStmt:
                    yield return CompileExpr(exprStmt.Expression, ctx);
                    break;

                default:
                    throw new MathCompilerException(
                        $"MathCompiler: '{stmt.GetType().Name}' ifade türü desteklenmiyor. " +
                        "Desteklenen: cellula (VarStmt), redeo (ReturnStmt), blok, atama.",
                        stmt.Line);
            }
        }

        
        private static List<Expression> CompileImplicitReturn(Expr expr, CompileContext ctx)
        {
            var compiled = CompileExpr(expr, ctx);
           
            var tupleCtor = typeof(ValueTuple<float, float>)
                .GetConstructor(new[] { typeof(float), typeof(float) });
            var tuple = Expression.New(tupleCtor,
                Expression.Convert(compiled, typeof(float)),
                Expression.Constant(0f));
            var assign = Expression.Assign(ctx.ReturnVar, tuple);
            var go = Expression.Goto(ctx.ReturnLabel, ctx.ReturnVar);
            return new List<Expression> { assign, go };
        }

       

        private static Expression CompileExpr(Expr expr, CompileContext ctx)
        {
            switch (expr)
            {
             
                case LiteralExpr literal:
                {
                    if (literal.Value.Type == WType.Number)
                        return Expression.Constant((float)literal.Value.AsNumber());
                    throw new MathCompilerException(
                        "MathCompiler: Yalnızca sayısal sabitler desteklenir.",
                        literal.Line);
                }

                
                case VariableExpr variable:
                    return ctx.Resolve(variable.Name.Value, variable.Line);

               
                case GroupingExpr grouping:
                    return CompileExpr(grouping.Expression, ctx);

                
                case BinaryExpr binary:
                {
                    var left  = CompileExpr(binary.Left, ctx);
                    var right = CompileExpr(binary.Right, ctx);
                    switch (binary.Operator.Value)
                    {
                        case "+": return Expression.Add(left, right);
                        case "-": return Expression.Subtract(left, right);
                        case "*": return Expression.Multiply(left, right);
                        case "/": return Expression.Divide(left, right);
                        case "%": return Expression.Modulo(left, right);
                        default:
                            throw new MathCompilerException(
                                $"MathCompiler: '{binary.Operator.Value}' operatörü " +
                                "matematiksel derleme için desteklenmiyor.",
                                binary.Line);
                    }
                }

                
                case UnaryExpr unary when unary.Operator.Value == "-":
                    return Expression.Negate(CompileExpr(unary.Right, ctx));

                
                case AssignExpr assign:
                {
                    var target = ctx.Resolve(assign.Name.Value, assign.Line);
                    var value  = CompileExpr(assign.Value, ctx);
                    return Expression.Assign(target, value);
                }

                
                default:
                    throw new MathCompilerException(
                        $"MathCompiler: '{expr.GetType().Name}' ifade türü " +
                        "matematiksel JIT derlemesinde desteklenmiyor. " +
                        "Desteklenen: LiteralExpr, VariableExpr, BinaryExpr, AssignExpr, GroupingExpr.",
                        expr.Line);
            }
        }

        

        private static CompiledNeuroFunc BuildDelegate(
            CompileContext ctx,
            List<Expression> bodyExprs,
            string name,
            int paramCount)
        {
           
            var defaultReturn = Expression.Label(
                ctx.ReturnLabel,
                Expression.Default(typeof(ValueTuple<float, float>)));

      
            var allExprs = new List<Expression>(bodyExprs) { defaultReturn };

        
            var block = Expression.Block(
                typeof(ValueTuple<float, float>),
                ctx.AllLocals(),
                allExprs);

            
            var paramList = new ParameterExpression[3];
            var paramNames = new string[paramCount];

            int i = 0;
            foreach (var kv in ctx.Params)
            {
                if (i < 3) paramList[i] = kv.Value;
                if (i < paramCount) paramNames[i] = kv.Key;
                i++;
            }

            
            for (int j = 0; j < 3; j++)
                if (paramList[j] == null)
                    paramList[j] = Expression.Parameter(typeof(float), $"__unused{j}");

            var lambda = Expression.Lambda<Func<float, float, float, ValueTuple<float, float>>>(
                block,
                paramList[0], paramList[1], paramList[2]);

            var compiled = lambda.Compile();

            return new CompiledNeuroFunc(
                name:       name,
                paramNames: paramNames,
                paramCount: paramCount,
                invoke:     compiled);
        }
    }

    
    public sealed class CompiledNeuroFunc
    {
        
        public string Name { get; }

      
        public string[] ParamNames { get; }

        
        public int ParamCount { get; }

            public Func<float, float, float, ValueTuple<float, float>> Invoke { get; }

        internal CompiledNeuroFunc(
            string name,
            string[] paramNames,
            int paramCount,
            Func<float, float, float, ValueTuple<float, float>> invoke)
        {
            Name       = name;
            ParamNames = paramNames;
            ParamCount = paramCount;
            Invoke     = invoke;
        }

       
        public ValueTuple<float, float> Call(float p0 = 0f, float p1 = 0f, float p2 = 0f)
            => Invoke(p0, p1, p2);

        public override string ToString()
            => $"CompiledNeuroFunc({Name}({string.Join(", ", ParamNames)}) → (float,float))";
    }

    
    public sealed class MathCompilerException : WSharpException
    {
        public MathCompilerException(string message, int line = 0)
            : base(message, "WS-JIT", line) { }
    }
}
