#nullable disable
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices; 

namespace WSharp
{
    static class Program
    {
        
        private static readonly Interpreter interpreter = new Interpreter();

        
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new IDE());
            }
            else
            {
                AllocConsole();

                RunConsoleMode(args);
            }
        }

        private static void RunConsoleMode(string[] args)
        {
            try
            {
                Console.WriteLine("WSharp CLI Mode Active...");

                interpreter.Globals.Define("clock", new WValue(new ClockFunc()));
                interpreter.Globals.Define("sqrt", new WValue(new SqrtFunc()));
                interpreter.Globals.Define("wea_read", new WValue(new InputFunc()));
                interpreter.Globals.Define("print", new WValue(new PrintFunc()));
                interpreter.Globals.Define("wea_len", new WValue(new LenFunc()));
                interpreter.Globals.Define("wea_push", new WValue(new PushFunc()));
                interpreter.Globals.Define("wea_pop", new WValue(new PopFunc()));
                interpreter.Globals.Define("wea_test_json", new WValue(new TestJsonFunc()));
                interpreter.Globals.Define("wea_test_serialize", new WValue(new TestSerializeFunc()));
                interpreter.Globals.Define("wea_map", new WValue(new MapFunc()));
                interpreter.Globals.Define("wea_filter", new WValue(new FilterFunc()));
                interpreter.Globals.Define("wea_avg", new WValue(new AvgFunc()));
                interpreter.Globals.Define("wea_max", new WValue(new MaxFunc()));
                interpreter.Globals.Define("wea_min", new WValue(new MinFunc()));
                interpreter.Globals.Define("wea_std_dev", new WValue(new StdDevFunc()));
                interpreter.Globals.Define("wea_file_write", new WValue(new FileWriteFunc()));
                interpreter.Globals.Define("wea_file_read", new WValue(new FileReadFunc()));
                interpreter.Globals.Define("wea_file_write_logged", new WValue(new FileWriteLoggedFunc()));
                interpreter.Globals.Define("wea_export_csv", new WValue(new ExportCsvFunc()));

                
                interpreter.Globals.Define("wea_neuro_create", new WValue(new NeuroCreateFunc()));
                interpreter.Globals.Define("wea_neuro_build_network", new WValue(new NeuroBuildNetworkFunc()));
                interpreter.Globals.Define("wea_neuro_step", new WValue(new NeuroStepFunc()));
                interpreter.Globals.Define("wea_neuro_set_input", new WValue(new NeuroSetInputFunc()));
                interpreter.Globals.Define("wea_neuro_get_voltage", new WValue(new NeuroGetVoltageFunc()));

                
                interpreter.Globals.Define("bio_classify", new WValue(new BioClassifyFunc()));
                interpreter.Globals.Define("bio_transport", new WValue(new BioTransportFunc()));
                interpreter.Globals.Define("bio_photosynthesis", new WValue(new BioPhotoFunc()));
                interpreter.Globals.Define("bio_respire", new WValue(new BioRespireFunc()));
                interpreter.Globals.Define("gen_transcribe", new WValue(new GenTranscribeFunc()));
                interpreter.Globals.Define("gen_translate", new WValue(new GenTranslateFunc()));
                interpreter.Globals.Define("eco_transfer", new WValue(new EcoTransferFunc()));
                interpreter.Globals.Define("physio_check", new WValue(new PhysioCheckFunc()));
                interpreter.Globals.Define("plant_hormone", new WValue(new PlantHormoneFunc()));
                interpreter.Globals.Define("bio_enzyme", new WValue(new BioEnzymeFunc()));
                interpreter.Globals.Define("bio_protein", new WValue(new BioProteinFunc()));
                interpreter.Globals.Define("bio_gpcr", new WValue(new BioGPCRFunc()));
                interpreter.Globals.Define("bio_cancer", new WValue(new BioCancerFunc()));
                interpreter.Globals.Define("bio_crispr", new WValue(new BioCrisprFunc()));
                interpreter.Globals.Define("bio_oxphos", new WValue(new BioOxPhosFunc()));

               
                interpreter.Globals.Define("chem_element", new WValue(new ChemElementFunc()));
                interpreter.Globals.Define("chem_mass", new WValue(new ChemMassFunc()));
                interpreter.Globals.Define("chem_gas", new WValue(new ChemGasFunc()));
                interpreter.Globals.Define("chem_bond", new WValue(new ChemBondFunc()));
                interpreter.Globals.Define("chem_gibbs", new WValue(new ChemGibbsFunc()));
                interpreter.Globals.Define("chem_organic", new WValue(new ChemOrganicFunc()));
                interpreter.Globals.Define("chem_quantum", new WValue(new ChemQuantumFunc()));
                interpreter.Globals.Define("chem_spectra", new WValue(new ChemSpectraFunc()));
                interpreter.Globals.Define("chem_mech", new WValue(new ChemMechFunc()));

                
                interpreter.Globals.Define("phys_mech", new WValue(new PhysMechFunc()));
                interpreter.Globals.Define("phys_thermo", new WValue(new PhysThermoFunc()));
                interpreter.Globals.Define("phys_em", new WValue(new PhysEMFunc()));
                interpreter.Globals.Define("phys_modern", new WValue(new PhysModernFunc()));

                
                interpreter.Globals.Define("nuc_decay", new WValue(new NucDecayFunc()));
                interpreter.Globals.Define("nuc_energy", new WValue(new NucEnergyFunc()));
                interpreter.Globals.Define("nuc_reactor", new WValue(new NucReactorFunc()));
                interpreter.Globals.Define("nuc_dose", new WValue(new NucDoseFunc()));

               
                interpreter.Globals.Define("math_basic", new WValue(new MathBasicFunc()));
                interpreter.Globals.Define("math_calc", new WValue(new MathCalcFunc()));
                interpreter.Globals.Define("math_matrix", new WValue(new MathMatrixFunc()));

               
                interpreter.Globals.Define("core_file", new WValue(new CoreFileFunc()));
                interpreter.Globals.Define("core_string", new WValue(new CoreStringFunc()));
                interpreter.Globals.Define("core_system", new WValue(new CoreSysFunc()));
                interpreter.Globals.Define("core_network", new WValue(new CoreNetFunc()));

                
                interpreter.Globals.Define("to_number", new WValue(new ToNumberFunc()));
                interpreter.Globals.Define("typeof", new WValue(new TypeOfFunc()));
                interpreter.Globals.Define("range", new WValue(new RangeFunc()));
                interpreter.Globals.Define("sort", new WValue(new SortFunc()));
                interpreter.Globals.Define("reverse", new WValue(new ReverseFunc()));
                interpreter.Globals.Define("join", new WValue(new JoinFunc()));
                interpreter.Globals.Define("split", new WValue(new SplitFunc()));
                interpreter.Globals.Define("abs", new WValue(new AbsFunc()));
                interpreter.Globals.Define("pow", new WValue(new PowFunc()));
                interpreter.Globals.Define("floor", new WValue(new FloorFunc()));
                interpreter.Globals.Define("ceil", new WValue(new CeilFunc()));
                interpreter.Globals.Define("round", new WValue(new RoundFunc()));

               
                interpreter.OnOutput += (output) => Console.WriteLine(output);

                
                if (args.Length > 0)
                {
                    RunFile(args[0]);
                }
                else
                {
                    
                    RunPrompt();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nCRITICAL ERROR:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }

        private static void RunFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"Hata: Dosya bulunamadi -> {path}");
                return;
            }
            string source = File.ReadAllText(path);
            Run(source);
        }

        private static void RunPrompt()
        {
            Console.WriteLine("WSharp v1.0 (Pipe |> Edition)");
            while (true)
            {
                Console.Write("we# > ");
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line)) break;
                Run(line);
            }
        }

        private static void Run(string source)
        {
            try
            {
                Lexer lexer = new Lexer(source);
                List<Token> tokens = lexer.Tokenize();

                Parser parser = new Parser(tokens);
                List<Stmt> statements = parser.Parse();

                if (statements == null) return;

                interpreter.Interpret(statements);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Run Error] {e.Message}");
            }
        }
    }
}
