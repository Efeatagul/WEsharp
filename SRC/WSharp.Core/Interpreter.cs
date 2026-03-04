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
#nullable disable
using System;
using System.IO;
using System.Collections.Generic;

namespace WSharp
{
    public class Interpreter : IVisitor<WValue>, IStmtVisitor<object>
    {
        public readonly Environment Globals = new Environment();
        private Environment _environment;
        public string BasePath { get; set; } = "";
        private readonly HashSet<string> _importedFiles = new HashSet<string>();

        
        public event Action<string> OnOutput;

        
        public void Notify(string message)
        {
            if (OnOutput != null)
                OnOutput.Invoke(message);
            else
                Console.WriteLine(message); 
        }
        

        public Interpreter()
        {
            _environment = Globals;
        }

        public Environment GetEnvironment() => Globals;

       
        public static void RegisterStdLib(Interpreter engine)
        {
            engine.Globals.Define("clock", new WValue(new ClockFunc()));
            engine.Globals.Define("sqrt", new WValue(new SqrtFunc()));
            engine.Globals.Define("input", new WValue(new InputFunc()));
            engine.Globals.Define("print", new WValue(new PrintFunc()));
            engine.Globals.Define("len", new WValue(new LenFunc()));
            engine.Globals.Define("push", new WValue(new PushFunc()));
            engine.Globals.Define("pop", new WValue(new PopFunc()));
            engine.Globals.Define("test_json", new WValue(new TestJsonFunc()));
            engine.Globals.Define("test_serialize", new WValue(new TestSerializeFunc()));
            engine.Globals.Define("map", new WValue(new MapFunc()));
            engine.Globals.Define("filter", new WValue(new FilterFunc()));
            engine.Globals.Define("avg", new WValue(new AvgFunc()));
            engine.Globals.Define("max", new WValue(new MaxFunc()));
            engine.Globals.Define("min", new WValue(new MinFunc()));
            engine.Globals.Define("std_dev", new WValue(new StdDevFunc()));
            engine.Globals.Define("file_write", new WValue(new FileWriteFunc()));
            engine.Globals.Define("file_read", new WValue(new FileReadFunc()));
            engine.Globals.Define("file_write_logged", new WValue(new FileWriteLoggedFunc()));
            engine.Globals.Define("export_csv", new WValue(new ExportCsvFunc()));

            
            engine.Globals.Define("creare_cortex", new WValue(new NeuroCreateCustomFunc()));               
            engine.Globals.Define("simulare_gradus", new WValue(new NeuroStepFunc()));                     
            engine.Globals.Define("nectere_temere", new WValue(new NeuroConnectRandomFunc()));             
            engine.Globals.Define("nectere_directe", new WValue(new NeuroConnectOneFunc()));                
            engine.Globals.Define("legere_tensio", new WValue(new NeuroGetVoltageFunc()));                 
            engine.Globals.Define("legere_pulsus", new WValue(new NeuroGetSpikesFunc()));                   
            engine.Globals.Define("injicere_stimulus", new WValue(new NeuroSetInputFunc()));                
            engine.Globals.Define("configurare_cortex", new WValue(new NeuroConfigureFunc()));              
            engine.Globals.Define("numerare_synapsis", new WValue(new NeuroSynapseCountFunc()));            
            engine.Globals.Define("legere_data", new WValue(new NeuroGetDataFunc()));                       
            engine.Globals.Define("resettere_cortex", new WValue(new NeuroResetFunc()));                    
            engine.Globals.Define("activare_stdp", new WValue(new NeuroEnableStdpFunc()));                  
            engine.Globals.Define("creare_circulus", new WValue(new NeuroConnectRingFunc()));               
            engine.Globals.Define("creare_parva_mundus", new WValue(new NeuroConnectSmallWorldFunc()));    

          
            engine.Globals.Define("bio_classify", new WValue(new BioClassifyFunc()));
            engine.Globals.Define("bio_transport", new WValue(new BioTransportFunc()));
            engine.Globals.Define("bio_photosynthesis", new WValue(new BioPhotoFunc()));
            engine.Globals.Define("bio_respire", new WValue(new BioRespireFunc()));
            engine.Globals.Define("gen_transcribe", new WValue(new GenTranscribeFunc()));
            engine.Globals.Define("gen_translate", new WValue(new GenTranslateFunc()));
            engine.Globals.Define("eco_transfer", new WValue(new EcoTransferFunc()));
            engine.Globals.Define("physio_check", new WValue(new PhysioCheckFunc()));
            engine.Globals.Define("plant_hormone", new WValue(new PlantHormoneFunc()));
            engine.Globals.Define("bio_enzyme", new WValue(new BioEnzymeFunc()));
            engine.Globals.Define("bio_protein", new WValue(new BioProteinFunc()));
            engine.Globals.Define("bio_gpcr", new WValue(new BioGPCRFunc()));
            engine.Globals.Define("bio_cancer", new WValue(new BioCancerFunc()));
            engine.Globals.Define("bio_crispr", new WValue(new BioCrisprFunc()));
            engine.Globals.Define("bio_oxphos", new WValue(new BioOxPhosFunc()));

          
            engine.Globals.Define("chem_element", new WValue(new ChemElementFunc()));
            engine.Globals.Define("chem_mass", new WValue(new ChemMassFunc()));
            engine.Globals.Define("chem_gas", new WValue(new ChemGasFunc()));
            engine.Globals.Define("chem_bond", new WValue(new ChemBondFunc()));
            engine.Globals.Define("chem_gibbs", new WValue(new ChemGibbsFunc()));
            engine.Globals.Define("chem_organic", new WValue(new ChemOrganicFunc()));
            engine.Globals.Define("chem_quantum", new WValue(new ChemQuantumFunc()));
            engine.Globals.Define("chem_spectra", new WValue(new ChemSpectraFunc()));
            engine.Globals.Define("chem_mech", new WValue(new ChemMechFunc()));

          
            engine.Globals.Define("phys_mech", new WValue(new PhysMechFunc()));
            engine.Globals.Define("phys_thermo", new WValue(new PhysThermoFunc()));
            engine.Globals.Define("phys_em", new WValue(new PhysEMFunc()));
            engine.Globals.Define("phys_modern", new WValue(new PhysModernFunc()));

            
            engine.Globals.Define("nuc_decay", new WValue(new NucDecayFunc()));
            engine.Globals.Define("nuc_energy", new WValue(new NucEnergyFunc()));
            engine.Globals.Define("nuc_reactor", new WValue(new NucReactorFunc()));
            engine.Globals.Define("nuc_dose", new WValue(new NucDoseFunc()));

         
            engine.Globals.Define("math_basic", new WValue(new MathBasicFunc()));
            engine.Globals.Define("math_calc", new WValue(new MathCalcFunc()));
            engine.Globals.Define("math_matrix", new WValue(new MathMatrixFunc()));

          
            engine.Globals.Define("core_file", new WValue(new CoreFileFunc()));
            engine.Globals.Define("core_string", new WValue(new CoreStringFunc()));
            engine.Globals.Define("core_system", new WValue(new CoreSysFunc()));
            engine.Globals.Define("core_network", new WValue(new CoreNetFunc()));

      
            engine.Globals.Define("connexio_http", new WValue(new ConnexioHttpFunc()));
            engine.Globals.Define("json_parse", new WValue(new JsonParseFunc()));

       
            engine.Globals.Define("to_number", new WValue(new ToNumberFunc()));
            engine.Globals.Define("typeof", new WValue(new TypeOfFunc()));
            engine.Globals.Define("range", new WValue(new RangeFunc()));
            engine.Globals.Define("sort", new WValue(new SortFunc()));
            engine.Globals.Define("reverse", new WValue(new ReverseFunc()));
            engine.Globals.Define("join", new WValue(new JoinFunc()));
            engine.Globals.Define("split", new WValue(new SplitFunc()));
            engine.Globals.Define("abs", new WValue(new AbsFunc()));
            engine.Globals.Define("pow", new WValue(new PowFunc()));
            engine.Globals.Define("floor", new WValue(new FloorFunc()));
            engine.Globals.Define("ceil", new WValue(new CeilFunc()));
            engine.Globals.Define("round", new WValue(new RoundFunc()));

            
            engine.Globals.Define("to_string", new WValue(new ToStringFunc()));
            engine.Globals.Define("str_upper", new WValue(new StrUpperFunc()));
            engine.Globals.Define("str_lower", new WValue(new StrLowerFunc()));
            engine.Globals.Define("str_contains", new WValue(new StrContainsFunc()));
            engine.Globals.Define("str_replace", new WValue(new StrReplaceFunc()));
            engine.Globals.Define("str_trim", new WValue(new StrTrimFunc()));
            engine.Globals.Define("str_starts_with", new WValue(new StrStartsWithFunc()));
            engine.Globals.Define("str_ends_with", new WValue(new StrEndsWithFunc()));
            engine.Globals.Define("str_index_of", new WValue(new StrIndexOfFunc()));
            engine.Globals.Define("str_sub", new WValue(new StrSubFunc()));

            engine.Globals.Define("plot_live", new WValue(new PlotLiveFunc()));
            engine.Globals.Define("plot_live_named", new WValue(new PlotLiveNamedFunc()));
            engine.Globals.Define("plot_title", new WValue(new PlotTitleFunc()));
            engine.Globals.Define("plot_close", new WValue(new PlotCloseFunc()));
            engine.Globals.Define("plot_raster", new WValue(new PlotRasterFunc()));                       

        
            engine.Globals.Define("py_analyze", new WValue(new PyAnalyzeFunc()));
            engine.Globals.Define("py_plot_save", new WValue(new PyPlotSaveFunc()));
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (Exception ex)
            {
                Notify($"[RUNTIME ERROR] {ex.Message}");
            }
        }

        public void Execute(Stmt stmt) => stmt.Accept(this);

        public WValue Evaluate(Expr expr) => expr.Accept(this);

       
        public WValue EvaluateIn(Expr expr, Environment environment)
        {
            Environment previous = _environment;
            try
            {
                _environment = environment;
                return Evaluate(expr);
            }
            finally
            {
                _environment = previous;
            }
        }

        

        public object Visit(ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);
            return null;
        }

        public object Visit(EmitStmt stmt)
        {
            WValue value = Evaluate(stmt.Expr);
            string output = Stringify(value);
            Notify(output); 
            return null;
        }

        public object Visit(VarStmt stmt)
        {
            WValue value = new WValue(null);
            if (stmt.Initializer != null) value = Evaluate(stmt.Initializer);
            _environment.Define(stmt.Name.Value, value);
            return null;
        }

        public object Visit(BlockStmt stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return null;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = _environment;
            try
            {
                _environment = environment;
                foreach (Stmt statement in statements) Execute(statement);
            }
            finally
            {
                _environment = previous;
            }
        }

        public object Visit(IfStmt stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition))) Execute(stmt.ThenBranch);
            else if (stmt.ElseBranch != null) Execute(stmt.ElseBranch);
            return null;
        }

        public object Visit(WhileStmt stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition))) 
            {
                try
                {
                    Execute(stmt.Body);
                }
                catch (ContinueException) { continue; }
                catch (BreakException) { break; }
            }
            return null;
        }

        public object Visit(FunctionStmt stmt)
        {
            WFunction function = new WFunction(stmt, _environment);
            _environment.Define(stmt.Name.Value, new WValue(function));
            return null;
        }

        public object Visit(ForeachStmt stmt)
        {
            var iterable = Evaluate(stmt.Iterable);
            if (iterable.Type == WType.List)
            {
                var list = iterable.AsList();
                foreach (var item in list)
                {
                    Environment loopEnv = new Environment(_environment);
                    WValue loopVar = new WValue(item);
                    loopEnv.Define(stmt.Name.Value, loopVar);
                    try
                    {
                        ExecuteBlock(((BlockStmt)stmt.Body).Statements, loopEnv);
                    }
                    catch (ContinueException) { continue; }
                    catch (BreakException) { break; }
                }
            }
            else if (iterable.Type == WType.Dict)
            {
                var dict = iterable.AsDict();
                foreach (var key in dict.Keys)
                {
                    Environment loopEnv = new Environment(_environment);
                    loopEnv.Define(stmt.Name.Value, new WValue(key));
                    try
                    {
                        ExecuteBlock(((BlockStmt)stmt.Body).Statements, loopEnv);
                    }
                    catch (ContinueException) { continue; }
                    catch (BreakException) { break; }
                }
            }
            else if (iterable.Type == WType.String)
            {
                string str = iterable.AsString();
                foreach (char c in str)
                {
                    Environment loopEnv = new Environment(_environment);
                    loopEnv.Define(stmt.Name.Value, new WValue(c.ToString()));
                    try
                    {
                        ExecuteBlock(((BlockStmt)stmt.Body).Statements, loopEnv);
                    }
                    catch (ContinueException) { continue; }
                    catch (BreakException) { break; }
                }
            }
            else { throw new WSharpTypeException("Foreach sadece liste, sozluk veya yazi uzerinde calisabilir.", line: stmt.Line); }
            return null;
        }

        public object Visit(BreakStmt stmt) { throw new BreakException(); }
        public object Visit(ContinueStmt stmt) { throw new ContinueException(); }

        public object Visit(TryStmt stmt)
        {
            try
            {
                ExecuteBlock(stmt.TryBlock, new Environment(_environment));
            }
            catch (ReturnException) { throw; }
            catch (Exception)
            {
                if (stmt.CatchBlock != null)
                    ExecuteBlock(stmt.CatchBlock, new Environment(_environment));
            }
            return null;
        }

        public object Visit(ReturnStmt stmt)
        {
            WValue value = new WValue(null);
            if (stmt.Value != null) value = Evaluate(stmt.Value);
            throw new ReturnException(value);
        }

        

        public WValue Visit(AssignExpr expr)
        {
            WValue value = Evaluate(expr.Value);
            _environment.Assign(expr.Name, value);
            return value;
        }

        public WValue Visit(LiteralExpr expr) => expr.Value;

        public WValue Visit(VariableExpr expr) => _environment.Get(expr.Name);

        public WValue Visit(GroupingExpr expr) => Evaluate(expr.Expression);

        public WValue Visit(LambdaExpr expr)
        {
            return new WValue(new WLambda(expr, _environment));
        }

        public WValue Visit(UnaryExpr expr)
        {
            WValue right = Evaluate(expr.Right);

            switch (expr.Operator.Value)
            {
                case "!":
                    return new WValue(!IsTruthy(right));
                case "-":
                    CheckNumberOperand(expr.Operator, right);
                    return new WValue(-right.AsNumber());
            }

            return new WValue(null);
        }

        public WValue Visit(BinaryExpr expr)
        {
            WValue left = Evaluate(expr.Left);
            WValue right = Evaluate(expr.Right);

            switch (expr.Operator.Value)
            {
                case "+":
                    if (left.Type == WType.Number && right.Type == WType.Number)
                        return new WValue(left.AsNumber() + right.AsNumber());
                    if (left.Type == WType.String && right.Type == WType.String)
                        return new WValue(left.AsString() + right.AsString());
                    if (left.Type == WType.String) return new WValue(left.AsString() + Stringify(right));
                    if (right.Type == WType.String) return new WValue(Stringify(left) + right.AsString());

                    
                    if (left.Type == WType.List && right.Type == WType.List)
                    {
                        var listA = left.AsList();
                        var listB = right.AsList();

                        if (listA.Count != listB.Count)
                            throw new WSharpTypeException($"Liste uzunluklari esit olmali: {listA.Count} != {listB.Count}", line: expr.Line);

                        var resultList = new List<object>();
                        for (int i = 0; i < listA.Count; i++)
                        {
                            
                            WValue valA = new WValue(listA[i]);
                            WValue valB = new WValue(listB[i]);
                            
                            
                            if (valA.Type == WType.Number && valB.Type == WType.Number)
                            {
                                resultList.Add(valA.AsNumber() + valB.AsNumber());
                            }
                            else
                            {
                                throw new WSharpTypeException("Vektor toplama sadece sayisal listelerde gecerlidir.", line: expr.Line);
                            }
                        }
                        return new WValue(resultList);
                    }
                   

                    throw new WSharpTypeException("Toplama islemi sadece sayilar, yazilar veya esit uzunluklu listeler arasinda yapilabilir.", line: expr.Line);

                case "-":
                   
                    if (left.Type == WType.List && right.Type == WType.List)
                    {
                        var listA = left.AsList();
                        var listB = right.AsList();
                        if (listA.Count != listB.Count) throw new WSharpTypeException("Vector sizes must match.", line: expr.Line);
                        var res = new List<object>();
                        for (int i = 0; i < listA.Count; i++) res.Add(Convert.ToDouble(listA[i]) - Convert.ToDouble(listB[i]));
                        return new WValue(res);
                    }
                    CheckNumberOperands(expr.Operator, left, right);
                    return new WValue(left.AsNumber() - right.AsNumber());
                case "/":
                    CheckNumberOperands(expr.Operator, left, right);
                    if (right.AsNumber() == 0) throw new WSharpDivisionException(line: expr.Line);
                    return new WValue(left.AsNumber() / right.AsNumber());
                case "%":
                    CheckNumberOperands(expr.Operator, left, right);
                    if (right.AsNumber() == 0) throw new WSharpDivisionException("Sifira bolme (mod) hatasi!", line: expr.Line);
                    return new WValue(left.AsNumber() % right.AsNumber());
                case "*":
                  
                    if (left.Type == WType.Number && right.Type == WType.List)
                    {
                        var res = new List<object>();
                        foreach (var item in right.AsList()) res.Add(left.AsNumber() * Convert.ToDouble(item));
                        return new WValue(res);
                    }
                    
                    if (left.Type == WType.List && right.Type == WType.Number)
                    {
                        var res = new List<object>();
                        foreach (var item in left.AsList()) res.Add(Convert.ToDouble(item) * right.AsNumber());
                        return new WValue(res);
                    }
                    
                    if (left.Type == WType.List && right.Type == WType.List)
                    {
                        var listA = left.AsList();
                        var listB = right.AsList();
                        if (listA.Count != listB.Count) throw new WSharpTypeException("Vector sizes must match.", line: expr.Line);
                        double dot = 0.0;
                        for (int i = 0; i < listA.Count; i++) dot += Convert.ToDouble(listA[i]) * Convert.ToDouble(listB[i]);
                        return new WValue(dot);
                    }
                    CheckNumberOperands(expr.Operator, left, right);
                    return new WValue(left.AsNumber() * right.AsNumber());
                case ">": CheckNumberOperands(expr.Operator, left, right); return new WValue(left.AsNumber() > right.AsNumber());
                case "<": CheckNumberOperands(expr.Operator, left, right); return new WValue(left.AsNumber() < right.AsNumber());
                case ">=": CheckNumberOperands(expr.Operator, left, right); return new WValue(left.AsNumber() >= right.AsNumber());
                case "<=": CheckNumberOperands(expr.Operator, left, right); return new WValue(left.AsNumber() <= right.AsNumber());
                case "!=": return new WValue(!IsEqual(left, right));
                case "==": return new WValue(IsEqual(left, right));
            }
            return new WValue(null);
        }

        public WValue Visit(ListExpr expr)
        {
            List<object> values = new List<object>();
            foreach (var element in expr.Elements)
            {
                
                 WValue val = Evaluate(element);
                 values.Add(val.Value);
            }
            return new WValue(values);
        }

        public WValue Visit(CallExpr expr)
        {
            WValue callee = Evaluate(expr.Callee);

            List<WValue> args = new List<WValue>();
            foreach (Expr arg in expr.Args)
            {
                args.Add(Evaluate(arg));
            }

            if (!callee.IsCallable())
            {
                throw new WSharpTypeException("Sadece fonksiyonlar cagrilabilir.", line: expr.Line);
            }

            IWCallable function = callee.AsCallable();
            if (function.Arity() != -1 && args.Count != function.Arity())
            {
                throw new WSharpArgumentException($"Beklenen arguman: {function.Arity()}, Gelen: {args.Count}.", function.Arity(), args.Count, line: expr.Line);
            }

            return function.Call(this, args);
        }

        public WValue Visit(DictExpr expr)
        {
            var dict = new Dictionary<string, WValue>();
            for (int i = 0; i < expr.Keys.Count; i++)
            {
                WValue key = Evaluate(expr.Keys[i]);
                WValue val = Evaluate(expr.Values[i]);
                
                if (key.Type != WType.String)
                {
                    throw new WSharpTypeException("Sozluk anahtarlari sadece yazi (string) olabilir.", line: expr.Line);
                }
                
                dict[key.AsString()] = val;
            }
            return new WValue(dict);
        }

        public WValue Visit(GetExpr expr)
        {
            WValue obj = Evaluate(expr.Object);
            if (obj.Type == WType.Dict)
            {
                var dict = obj.AsDict();
                string key = expr.Name.Value;
                if (dict.ContainsKey(key))
                {
                    return dict[key];
                }
                throw new WSharpIndexException($"Sozlukte '{key}' anahtari bulunamadi.", line: expr.Line);
            }
            throw new WSharpTypeException("Sadece sozluklerde (dict) nokta ile erisim yapilabilir.", line: expr.Line);
        }

        public WValue Visit(SetExpr expr)
        {
            WValue obj = Evaluate(expr.Object);
            if (obj.Type == WType.Dict)
            {
                var dict = obj.AsDict();
                string key = expr.Name.Value;
                WValue value = Evaluate(expr.Value);
                dict[key] = value;
                return value;
            }
            throw new WSharpTypeException("Sadece sozluklerde (dict) nokta ile atama yapilabilir.", line: expr.Line);
        }

        public WValue Visit(IndexExpr expr)
        {
            WValue obj = Evaluate(expr.Object);
            WValue start = Evaluate(expr.Start);
            
            
            if (obj.Type == WType.List)
            {
                var list = obj.AsList();
                int index = (int)start.AsNumber();
                
                
                if (expr.End != null)
                {
                    WValue endVal = Evaluate(expr.End);
                    int end = (int)endVal.AsNumber();
                    
                 
                    if (index < 0) index = 0;
                    if (end > list.Count) end = list.Count;
                    if (index >= end) return new WValue(new List<object>()); 

                    var range = list.GetRange(index, end - index);
                    return new WValue(range);
                }
                
                else
                {
                    if (index < 0 || index >= list.Count) throw new WSharpIndexException($"Liste indeksi sinirlar disinda: {index}", index, list.Count, line: expr.Line);
                    return new WValue(list[index]);
                }
            }
    
            else if (obj.Type == WType.Dict)
            {
                if (expr.End != null) throw new WSharpTypeException("Sozluklerde dilimleme (slicing) kullanilamaz.", line: expr.Line);
                
                var dict = obj.AsDict();
                string key = start.AsString(); 

                if (dict.ContainsKey(key)) return dict[key];
                throw new WSharpIndexException($"Sozlukte '{key}' anahtari bulunamadi.", line: expr.Line);
            }
            
            else if (obj.Type == WType.String)
            {
                 string str = obj.AsString();
                 int index = (int)start.AsNumber();
                 
                 
                 if (expr.End != null)
                 {
                    WValue endVal = Evaluate(expr.End);
                    int end = (int)endVal.AsNumber();
                    
                    if (index < 0) index = 0;
                    if (end > str.Length) end = str.Length;
                    if (index >= end) return new WValue("");

                    return new WValue(str.Substring(index, end - index));
                 }
                
                 else
                 {
                     if (index < 0 || index >= str.Length) throw new WSharpIndexException($"String indeksi sinirlar disinda: {index}", index, str.Length, line: expr.Line);
                     return new WValue(str[index].ToString());
                 }
            }
            
            throw new WSharpTypeException("Indeksleme sadece liste, sozluk veya yazilar icin gecerlidir.", line: expr.Line);
        }

       

        private bool IsTruthy(WValue obj)
        {
            if (obj.Value == null) return false;
            if (obj.Type == WType.Bool) return (bool)obj.Value;
            if (obj.Type == WType.Number) return obj.AsNumber() != 0;
            return true;
        }

        private bool IsEqual(WValue a, WValue b)
        {
            if (a.Value == null && b.Value == null) return true;
            if (a.Value == null) return false;
            return a.Value.Equals(b.Value);
        }

        private void CheckNumberOperand(Token op, WValue operand)
        {
            if (operand.Type == WType.Number) return;
            throw new WSharpTypeException($"'{op.Value}' operatoru sadece sayilarla calisir.", line: op.Line);
        }

        private void CheckNumberOperands(Token op, WValue left, WValue right)
        {
            if (left.Type == WType.Number && right.Type == WType.Number) return;
            throw new WSharpTypeException($"'{op.Value}' operatoru icin iki taraf da sayi olmalidir.", line: op.Line);
        }

        private string Stringify(WValue value)
        {
            if (value.Type == WType.Null) return "null";
            if (value.Type == WType.Bool) return (bool)value.Value ? "true" : "false";
            return value.ToString();
        }

        
        public WValue Visit(LogicalExpr expr)
        {
            WValue left = Evaluate(expr.Left);

            if (expr.Operator.Value == "||")
            {
                if (IsTruthy(left)) return left; 
            }
            else 
            {
                if (!IsTruthy(left)) return left; 
            }

            return Evaluate(expr.Right);
        }

        
        public WValue Visit(IsKeyExpr expr)
        {
            WValue obj = Evaluate(expr.Object);
            WValue key = Evaluate(expr.Key);

            if (obj.Type != WType.Dict)
                throw new WSharpTypeException("is_key sadece sozluk (dict) uzerinde calisir.", line: expr.Line);

            return new WValue(obj.AsDict().ContainsKey(key.AsString()));
        }

       
        public object Visit(ImportStmt stmt)
        {
            string rawPath = stmt.Path.Value;
            string filePath;
            string source;

            
            bool isRemote = rawPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                         || rawPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            if (isRemote)
            {
                
                string cacheDir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "wpm_cache");

                if (!Directory.Exists(cacheDir))
                    Directory.CreateDirectory(cacheDir);

                
                string urlHash = ComputeUrlHash(rawPath);
                string cachedFile = Path.Combine(cacheDir, urlHash + ".we");

              
                if (_importedFiles.Contains(rawPath))
                    return null;

                if (File.Exists(cachedFile))
                {
                    
                    source = File.ReadAllText(cachedFile);
                }
                else
                {
               
                    source = NetOps.HttpGet(rawPath);

                    if (string.IsNullOrEmpty(source) || source.StartsWith("AĞ HATASI:"))
                    {
                        throw new WSharpImportException(
                            $"Import hatasi: Uzak modül indirilemedi.\n" +
                            $"  URL: {rawPath}\n" +
                            $"  Hata: {source}",
                            rawPath, line: stmt.Line);
                    }

                   
                    try { File.WriteAllText(cachedFile, source); }
                    catch { /* Cache yazma hatası kritik değil, devam et */ }
                }

                _importedFiles.Add(rawPath);
                filePath = cachedFile;   
            }
            else
            {
                
                filePath = rawPath;

                if (!Path.IsPathRooted(filePath))
                    filePath = Path.Combine(BasePath, filePath);

                filePath = Path.GetFullPath(filePath);

                if (_importedFiles.Contains(filePath))
                    return null;

                if (!File.Exists(filePath))
                {
                    throw new WSharpImportException(
                        $"Import hatasi: Dosya bulunamadi -> {filePath}",
                        filePath, line: stmt.Line);
                }

                _importedFiles.Add(filePath);
                source = File.ReadAllText(filePath);
            }

            
            var tokens = new Lexer(source).Tokenize();
            var parser = new Parser(tokens);
            var statements = parser.Parse();

            if (statements == null)
            {
                throw new WSharpImportException(
                    $"Import hatasi: '{rawPath}' derlenemedi.", rawPath, line: stmt.Line);
            }

           
            Environment moduleEnv = new Environment(Globals);
            string previousBasePath = BasePath;
            BasePath = isRemote
                ? Path.GetDirectoryName(filePath)    
                : Path.GetDirectoryName(filePath);   

            Environment previousEnv = _environment;
            try
            {
                _environment = moduleEnv;
                foreach (Stmt s in statements)
                    Execute(s);
            }
            finally
            {
                _environment = previousEnv;
                BasePath = previousBasePath;
            }

            
            var moduleDict = moduleEnv.GetAllDefinitions();
            _environment.Define(stmt.Alias, new WValue(moduleDict));

            return null;
        }

        
        private static string ComputeUrlHash(string url)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(url);
                byte[] hash = sha.ComputeHash(bytes);
                
                return BitConverter.ToString(hash, 0, 8).Replace("-", "").ToLowerInvariant();
            }
        }
    }
    public class BreakException : Exception { }
    public class ContinueException : Exception { }
}