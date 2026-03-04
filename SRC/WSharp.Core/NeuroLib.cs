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
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ILGPU;
using ILGPU.Runtime;

namespace WSharp
{
   
    public struct Synapse
    {
        public int    TargetID;
        public double Weight;

        public Synapse(int targetID, double weight)
        {
            TargetID = targetID;
            Weight   = weight;
        }

        public override string ToString()
            => $"Synapse(→{TargetID}, w={Weight:F4})";
    }

    
    public class CustomNeuroKernel : IDisposable
    {
        
        public  int      NeuronCount;
        private int      _paddedCount;

        public  double[] V;
        public  double[] U;
        public  double[] I_ext;
        public  bool[]   Spiked;
        public  double[] PreTrace;         
        public  double[] PostTrace;        

       
        public  double[] ParamA;
        public  double[] ParamB;
        public  double[] ParamC;
        public  double[] ParamD;

        
        public  double   SpikeThreshold = 30.0;
        public  double   InputDecay     = 0.9;

        
        public  bool     StdpEnabled  = false;
        public  double   A_plus       = 0.01;     
        public  double   A_minus      = 0.012;    
        public  double   Tau_plus     = 20.0;   
        public  double   Tau_minus    = 20.0;     
        public  double   MaxWeight    = 50.0;    
        
        public  int      CurrentStep  = 0;

       
        private List<Synapse>[] _outSynapses;
        private List<(int SourceID, int SynapseIdx)>[] _inSynapses;

      
        private CompiledNeuroFunc _compiledFunc;
        private IWCallable        _fallbackFunc;
        private Interpreter       _fallbackInterp;

        
        private static readonly int  VecSize = Vector<double>.Count;
        private static readonly bool HwAccel = Vector.IsHardwareAccelerated;

        
        public  bool         UseCuda { get; private set; }
        private Context      _gpuContext;
        private Accelerator  _gpuAccelerator;
        private MemoryBuffer1D<double, Stride1D.Dense> _d_V;
        private MemoryBuffer1D<double, Stride1D.Dense> _d_U;
        private MemoryBuffer1D<double, Stride1D.Dense> _d_I_ext;
        private MemoryBuffer1D<double, Stride1D.Dense> _d_ParamA;
        private MemoryBuffer1D<double, Stride1D.Dense> _d_ParamB;
        private MemoryBuffer1D<double, Stride1D.Dense> _d_ParamC;
        private MemoryBuffer1D<double, Stride1D.Dense> _d_ParamD;
        private MemoryBuffer1D<int,    Stride1D.Dense> _d_Spiked;  
        private Action<Index1D,
                       ArrayView<double>, ArrayView<double>, ArrayView<double>,
                       ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>,
                       ArrayView<int>, double> _gpuKernelAction;

        
        public CustomNeuroKernel(int count, IWCallable modelFunc, Interpreter interpreter,
                                 bool useCuda = false)
        {
            NeuronCount  = count;
            _paddedCount = ((count + VecSize - 1) / VecSize) * VecSize;

          
            V              = new double[_paddedCount];
            U              = new double[_paddedCount];
            I_ext          = new double[_paddedCount];
            Spiked         = new bool[_paddedCount];
            PreTrace       = new double[_paddedCount];
            PostTrace      = new double[_paddedCount];

            ParamA = new double[_paddedCount];
            ParamB = new double[_paddedCount];
            ParamC = new double[_paddedCount];
            ParamD = new double[_paddedCount];

            for (int i = 0; i < _paddedCount; i++)
            {
                V[i]             = -65.0;
                U[i]             = 0.0;
                I_ext[i]         = 0.0;
                ParamA[i]        = 0.02;
                ParamB[i]        = 0.2;
                ParamC[i]        = -65.0;
                ParamD[i]        = 8.0;
            }

            
            _outSynapses = new List<Synapse>[count];
            _inSynapses  = new List<(int, int)>[count];
            int estimatedSynPerNeuron = Math.Max(16, Math.Min(1000, (int)(count * 0.01)));
            for (int i = 0; i < count; i++)
            {
                _outSynapses[i] = new List<Synapse>(estimatedSynPerNeuron);
                _inSynapses[i]  = new List<(int, int)>(estimatedSynPerNeuron);
            }

          
            UseCuda = false;
            if (useCuda)
            {
                try
                {
                    _gpuContext = Context.CreateDefault();
                   
                    var preferredDevice = _gpuContext.GetPreferredDevice(preferCPU: false);
                    _gpuAccelerator = preferredDevice.CreateAccelerator(_gpuContext);

                    _d_V      = _gpuAccelerator.Allocate1D<double>(count);
                    _d_U      = _gpuAccelerator.Allocate1D<double>(count);
                    _d_I_ext  = _gpuAccelerator.Allocate1D<double>(count);
                    _d_ParamA = _gpuAccelerator.Allocate1D<double>(count);
                    _d_ParamB = _gpuAccelerator.Allocate1D<double>(count);
                    _d_ParamC = _gpuAccelerator.Allocate1D<double>(count);
                    _d_ParamD = _gpuAccelerator.Allocate1D<double>(count);
                    _d_Spiked = _gpuAccelerator.Allocate1D<int>(count);

                  
                    _d_V.CopyFromCPU(V.AsSpan(0, count).ToArray());
                    _d_U.CopyFromCPU(U.AsSpan(0, count).ToArray());
                    _d_I_ext.CopyFromCPU(I_ext.AsSpan(0, count).ToArray());
                    _d_ParamA.CopyFromCPU(ParamA.AsSpan(0, count).ToArray());
                    _d_ParamB.CopyFromCPU(ParamB.AsSpan(0, count).ToArray());
                    _d_ParamC.CopyFromCPU(ParamC.AsSpan(0, count).ToArray());
                    _d_ParamD.CopyFromCPU(ParamD.AsSpan(0, count).ToArray());

                    
                    _gpuKernelAction = _gpuAccelerator
                        .LoadAutoGroupedStreamKernel<Index1D,
                            ArrayView<double>, ArrayView<double>, ArrayView<double>,
                            ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>,
                            ArrayView<int>, double>(IzhikevichGpuKernel);

                    UseCuda = true;
                    Console.WriteLine($"[ILGPU] GPU Acceleration: ENABLED");
                    Console.WriteLine($"[ILGPU] Device: {_gpuAccelerator.Name} ({_gpuAccelerator.AcceleratorType})");
                    Console.WriteLine($"[ILGPU] Neurons on GPU: {count:N0}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ILGPU] GPU init failed: {ex.Message} — falling back to CPU.");
                    CleanupGpu();
                }
            }

           
            if (modelFunc is WFunction wfunc)
            {
                try { _compiledFunc = MathCompiler.Compile(wfunc.Declaration); }
                catch (MathCompilerException)
                {
                    _fallbackFunc   = modelFunc;
                    _fallbackInterp = interpreter;
                }
            }
            else if (modelFunc is WLambda wlambda)
            {
                try { _compiledFunc = MathCompiler.Compile(wlambda.Expr); }
                catch (MathCompilerException)
                {
                    _fallbackFunc   = modelFunc;
                    _fallbackInterp = interpreter;
                }
            }
            else
            {
                _fallbackFunc   = modelFunc;
                _fallbackInterp = interpreter;
            }
        }

       
        static void IzhikevichGpuKernel(
            Index1D index,
            ArrayView<double> v, ArrayView<double> u, ArrayView<double> I_ext,
            ArrayView<double> a, ArrayView<double> b,
            ArrayView<double> c, ArrayView<double> d,
            ArrayView<int> spiked, double threshold)
        {
            double vv = v[index];
            double uu = u[index];
            double ii = I_ext[index];

            
            double v_next = vv + (0.04 * vv * vv + 5.0 * vv + 140.0 - uu + ii);
            double u_next = uu + (a[index] * (b[index] * vv - uu));

           
            if (v_next >= threshold)
            {
                v[index] = c[index];       
                u[index] = u_next + d[index]; 
                spiked[index] = 1;
            }
            else
            {
                v[index] = v_next;
                u[index] = u_next;
                spiked[index] = 0;
            }
        }

        
        public void Configure(double threshold, double resetV, double resetUDelta,
                              int refractorySteps = 0, double inputDecay = 0.9)
        {
            SpikeThreshold = threshold;
            InputDecay     = inputDecay;

            for (int i = 0; i < _paddedCount; i++)
            {
                ParamC[i] = resetV;
                ParamD[i] = resetUDelta;
            }
        }

        
        public void EnableStdp(double aPlus, double aMinus,
                               double tauPlus, double tauMinus,
                               double maxWeight)
        {
            StdpEnabled = true;
            A_plus      = aPlus;
            A_minus     = aMinus;
            Tau_plus    = tauPlus;
            Tau_minus   = tauMinus;
            MaxWeight   = maxWeight;
        }

        
        public void ConnectRandom(double probability, double weight)
        {
            if (probability <= 0) return;
            if (probability >= 1.0) probability = 0.999999;

            var rng = new Random();
            double logNotP = Math.Log(1.0 - probability);
            long maxIndex = (long)NeuronCount * NeuronCount;
            long currentIndex = -1;

            while (true)
            {
                double u = 1.0 - rng.NextDouble();
                long step = (long)(Math.Log(u) / logNotP) + 1;
                currentIndex += step;

                if (currentIndex >= maxIndex) break;

                int source = (int)(currentIndex / NeuronCount);
                int target = (int)(currentIndex % NeuronCount);

                if (source != target)
                {
                    AddSynapse(source, target, weight);
                }
            }
        }

        public void ConnectOne(int source, int target, double weight)
        {
            if (source >= 0 && source < NeuronCount &&
                target >= 0 && target < NeuronCount)
                AddSynapse(source, target, weight);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddSynapse(int source, int target, double weight)
        {
            int idx = _outSynapses[source].Count;
            _outSynapses[source].Add(new Synapse(target, weight));
            _inSynapses[target].Add((source, idx));   
        }

        public int GetSynapseCount()
        {
            int total = 0;
            for (int i = 0; i < NeuronCount; i++)
                total += _outSynapses[i].Count;
            return total;
        }

        public void ConnectRing(double weight)
        {
            for (int i = 0; i < NeuronCount; i++)
            {
                int right = (i + 1) % NeuronCount;
                int left  = (i - 1 + NeuronCount) % NeuronCount;
                AddSynapse(i, right, weight);
                AddSynapse(i, left, weight);
            }
        }

        public void ConnectSmallWorld(int k, double beta, double weight)
        {
            int halfK = k / 2;
            var rng = new Random();

            for (int i = 0; i < NeuronCount; i++)
            {
                for (int v = 1; v <= halfK; v++)
                {
                    int right = (i + v) % NeuronCount;
                    if (rng.NextDouble() < beta)
                    {
                        do { right = rng.Next(NeuronCount); } while (right == i);
                    }
                    AddSynapse(i, right, weight);

                    int left = (i - v + NeuronCount) % NeuronCount;
                    if (rng.NextDouble() < beta)
                    {
                        do { left = rng.Next(NeuronCount); } while (left == i);
                    }
                    AddSynapse(i, left, weight);
                }
            }
        }

        
        public int Step(int steps)
        {
            int totalSpikes = 0;

            for (int t = 0; t < steps; t++)
            {
                if (UseCuda)
                {
                    
                    _gpuKernelAction(NeuronCount,
                        _d_V.View, _d_U.View, _d_I_ext.View,
                        _d_ParamA.View, _d_ParamB.View,
                        _d_ParamC.View, _d_ParamD.View,
                        _d_Spiked.View, SpikeThreshold);
                    _gpuAccelerator.Synchronize();

                    
                    var gpuSpiked = _d_Spiked.GetAsArray1D();
                    for (int i = 0; i < NeuronCount; i++)
                        Spiked[i] = gpuSpiked[i] != 0;

                    
                    if (StdpEnabled)
                        DecayTraces();

                   
                    Array.Fill(I_ext, 0.0, 0, NeuronCount);
                    for (int i = 0; i < NeuronCount; i++)
                    {
                        if (Spiked[i])
                        {
                            totalSpikes++;

                            if (StdpEnabled)
                            {
                                PreTrace[i]  += 1.0;
                                PostTrace[i] += 1.0;
                            }

                            var outList = _outSynapses[i];
                            for (int s = 0; s < outList.Count; s++)
                            {
                                int target = outList[s].TargetID;
                                if (target >= 0 && target < NeuronCount)
                                    I_ext[target] += outList[s].Weight;
                            }

                            if (StdpEnabled)
                                ApplyStdpForSpikedNeuron(i);
                        }
                    }

                   
                    DecayCurrentSIMD();
                    _d_I_ext.CopyFromCPU(I_ext.AsSpan(0, NeuronCount).ToArray());
                }
                else
                {
                 
                    if (_compiledFunc != null)
                        StepPhysicsParallelJIT();
                    else
                        StepPhysicsFallback();

                    DetectSpikes();

                    if (StdpEnabled)
                        DecayTraces();

                    for (int i = 0; i < NeuronCount; i++)
                    {
                        if (Spiked[i])
                        {
                            totalSpikes++;
                            V[i] = ParamC[i];
                            U[i] += ParamD[i];

                            if (StdpEnabled)
                            {
                                PreTrace[i]  += 1.0;
                                PostTrace[i] += 1.0;
                            }

                            var outList = _outSynapses[i];
                            for (int s = 0; s < outList.Count; s++)
                            {
                                int target = outList[s].TargetID;
                                if (target >= 0 && target < NeuronCount)
                                    I_ext[target] += outList[s].Weight;
                            }

                            if (StdpEnabled)
                                ApplyStdpForSpikedNeuron(i);
                        }
                    }

                    DecayCurrentSIMD();
                }

                CurrentStep++;
            }

            
            if (UseCuda)
            {
                var gpuV = _d_V.GetAsArray1D();
                var gpuU = _d_U.GetAsArray1D();
                Array.Copy(gpuV, 0, V, 0, NeuronCount);
                Array.Copy(gpuU, 0, U, 0, NeuronCount);
            }

            return totalSpikes;
        }

      
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void ApplyStdpForSpikedNeuron(int neuron)
        {
   
            var inList = _inSynapses[neuron];
            for (int k = 0; k < inList.Count; k++)
            {
                int src    = inList[k].SourceID;
                int synIdx = inList[k].SynapseIdx;

                double dw = A_plus * PreTrace[src];
                if (dw <= 0.0) continue;

                var syn = _outSynapses[src][synIdx];
                syn.Weight = Math.Min(syn.Weight + dw, MaxWeight);
                _outSynapses[src][synIdx] = syn;
            }

          
            var outList = _outSynapses[neuron];
            for (int s = 0; s < outList.Count; s++)
            {
                int target = outList[s].TargetID;

                double dw = A_minus * PostTrace[target];
                if (dw <= 0.0) continue;

                var syn = outList[s];
                syn.Weight = Math.Max(syn.Weight - dw, 0.0);
                outList[s] = syn;
            }
        }

       
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DecayTraces()
        {
            double decayPre  = Math.Exp(-1.0 / Tau_plus);
            double decayPost = Math.Exp(-1.0 / Tau_minus);

            for (int i = 0; i < NeuronCount; i++)
            {
                PreTrace[i]  *= decayPre;
                PostTrace[i] *= decayPost;
            }
        }

        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void StepPhysicsParallelJIT()
        {
            var compiled = _compiledFunc;
            Parallel.For(0, NeuronCount, i =>
            {
                var result = compiled.Invoke((float)V[i], (float)U[i], (float)I_ext[i]);
                V[i] = result.Item1;
                U[i] = result.Item2;
            });
        }

        
        private void StepPhysicsFallback()
        {
            for (int i = 0; i < NeuronCount; i++)
            {
                var args = new List<WValue>
                {
                    new WValue(V[i]),
                    new WValue(U[i]),
                    new WValue(I_ext[i])
                };

                WValue result = _fallbackFunc.Call(_fallbackInterp, args);

                if (result.Value is List<object> lst && lst.Count >= 2)
                {
                    V[i] = Convert.ToDouble(lst[0]);
                    U[i] = Convert.ToDouble(lst[1]);
                }
                else
                {
                    V[i] = result.AsNumber();
                }
            }
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DetectSpikes()
        {
            double threshold = SpikeThreshold;
            for (int i = 0; i < NeuronCount; i++)
                Spiked[i] = V[i] >= threshold;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DecayCurrentSIMD()
        {
            if (HwAccel)
            {
                var decay = new Vector<double>(InputDecay);
                for (int i = 0; i < _paddedCount; i += VecSize)
                {
                    var current = new Vector<double>(I_ext, i);
                    (current * decay).CopyTo(I_ext, i);
                }
            }
            else
            {
                double decay = InputDecay;
                for (int i = 0; i < NeuronCount; i++)
                    I_ext[i] *= decay;
            }
        }

        public void Reset()
        {
            Array.Fill(V, -65.0, 0, _paddedCount);
            Array.Fill(U, 0.0,   0, _paddedCount);
            Array.Fill(I_ext,  0.0,   0, _paddedCount);
            Array.Fill(Spiked, false, 0, _paddedCount);
            Array.Fill(PreTrace, 0.0, 0, _paddedCount);
            Array.Fill(PostTrace, 0.0, 0, _paddedCount);
            CurrentStep = 0;

            for (int i = 0; i < NeuronCount; i++)
            {
                _outSynapses[i].Clear();
                _inSynapses[i].Clear();
            }

          
            if (UseCuda)
            {
                _d_V.CopyFromCPU(V.AsSpan(0, NeuronCount).ToArray());
                _d_U.CopyFromCPU(U.AsSpan(0, NeuronCount).ToArray());
                _d_I_ext.CopyFromCPU(I_ext.AsSpan(0, NeuronCount).ToArray());
            }
        }

     
        private void CleanupGpu()
        {
            _d_V?.Dispose();      _d_V = null;
            _d_U?.Dispose();      _d_U = null;
            _d_I_ext?.Dispose();  _d_I_ext = null;
            _d_ParamA?.Dispose(); _d_ParamA = null;
            _d_ParamB?.Dispose(); _d_ParamB = null;
            _d_ParamC?.Dispose(); _d_ParamC = null;
            _d_ParamD?.Dispose(); _d_ParamD = null;
            _d_Spiked?.Dispose(); _d_Spiked = null;
            _gpuAccelerator?.Dispose(); _gpuAccelerator = null;
            _gpuContext?.Dispose();     _gpuContext = null;
            UseCuda = false;
        }

        public void Dispose()
        {
            CleanupGpu();
            GC.SuppressFinalize(this);
        }

        public override string ToString()
            => $"CustomNeuroKernel(N={NeuronCount}, CUDA={UseCuda}, SIMD={HwAccel}, STDP={StdpEnabled}, Step={CurrentStep})";
    }

   

    public class NeuroCreateCustomFunc : IWCallable
    {
        public int Arity() => -1;   
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            if (arguments.Count < 2)
                throw new Exception("creare_cortex: En az 2 argüman gerekli (model, nöron_sayısı).");

            var physicsFunc = arguments[0].Value as IWCallable;
            if (physicsFunc == null)
                throw new Exception(
                    "creare_cortex: Birinci argüman bir operatio olmalı.\n" +
                    "Örnek: operatio modelim(v, u, I) { redeo [v + dv, u + du] }");

            int count = (int)arguments[1].AsNumber();
            if (count <= 0) throw new Exception("creare_cortex: Nöron sayısı pozitif olmalı.");

          
            bool useCuda = false;
            if (arguments.Count >= 3)
            {
                var cudaArg = arguments[2];
                useCuda = (cudaArg.Type == WType.Bool)
                    ? (bool)cudaArg.Value
                    : cudaArg.AsNumber() != 0;
            }

            var kernel = new CustomNeuroKernel(count, physicsFunc, interpreter, useCuda);

            if (useCuda)
                interpreter.Notify($"[ILGPU] NVIDIA CUDA Acceleration: {(kernel.UseCuda ? "ENABLED" : "FALLBACK → CPU")}");

            return new WValue(kernel);
        }
        public override string ToString() => "<native fn creare_cortex>";
    }

    public class NeuroStepFunc : IWCallable
    {
        public int Arity() => -1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            if (arguments.Count < 1) throw new Exception("wea_neuro_step: Kernel argümanı gerekli.");
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) throw new Exception("wea_neuro_step: Birinci argüman CustomNeuroKernel olmalı.");

            int steps = (arguments.Count >= 2) ? (int)arguments[1].AsNumber() : 1;
            return new WValue((double)kernel.Step(steps));
        }
        public override string ToString() => "<native fn wea_neuro_step>";
    }

    public class NeuroConnectRandomFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) throw new Exception("wea_neuro_connect_random: Kernel gerekli.");
            kernel.ConnectRandom(arguments[1].AsNumber(), arguments[2].AsNumber());
            return new WValue((double)kernel.GetSynapseCount());
        }
        public override string ToString() => "<native fn wea_neuro_connect_random>";
    }

    public class NeuroConnectOneFunc : IWCallable
    {
        public int Arity() => 4;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) throw new Exception("wea_neuro_connect: Kernel gerekli.");
            kernel.ConnectOne((int)arguments[1].AsNumber(), (int)arguments[2].AsNumber(), arguments[3].AsNumber());
            return new WValue(true);
        }
        public override string ToString() => "<native fn wea_neuro_connect>";
    }

    public class NeuroConnectRingFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) throw new Exception("creare_circulus: Kernel gerekli.");
            kernel.ConnectRing(arguments[1].AsNumber());
            return new WValue((double)kernel.GetSynapseCount());
        }
        public override string ToString() => "<native fn creare_circulus>";
    }

    public class NeuroConnectSmallWorldFunc : IWCallable
    {
        public int Arity() => 4;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) throw new Exception("creare_parva_mundus: Kernel gerekli.");
            kernel.ConnectSmallWorld((int)arguments[1].AsNumber(), arguments[2].AsNumber(), arguments[3].AsNumber());
            return new WValue((double)kernel.GetSynapseCount());
        }
        public override string ToString() => "<native fn creare_parva_mundus>";
    }

    public class NeuroGetVoltageFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) throw new Exception("wea_neuro_get_voltage: Kernel gerekli.");
            int id = (int)arguments[1].AsNumber();
            return new WValue(id >= 0 && id < kernel.NeuronCount ? kernel.V[id] : 0.0);
        }
        public override string ToString() => "<native fn wea_neuro_get_voltage>";
    }

    public class NeuroGetSpikesFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) return new WValue(new List<object>());
            var list = new List<object>();
            for (int i = 0; i < kernel.NeuronCount; i++)
                if (kernel.Spiked[i]) list.Add((double)i);
            return new WValue(list);
        }
        public override string ToString() => "<native fn wea_neuro_get_spikes>";
    }

    public class NeuroSetInputFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) throw new Exception("wea_neuro_set_input: Kernel gerekli.");
            int id = (int)arguments[1].AsNumber();
            if (id >= 0 && id < kernel.NeuronCount)
                kernel.I_ext[id] = arguments[2].AsNumber();
            return new WValue(true);
        }
        public override string ToString() => "<native fn wea_neuro_set_input>";
    }

    public class NeuroConfigureFunc : IWCallable
    {
        public int Arity() => -1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            if (arguments.Count < 4)
                throw new Exception("wea_neuro_configure: En az 4 argüman gerekli.");
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) throw new Exception("wea_neuro_configure: Kernel gerekli.");

            double threshold   = arguments[1].AsNumber();
            double resetV      = arguments[2].AsNumber();
            double resetUDelta = arguments[3].AsNumber();
            int refractory     = (arguments.Count >= 5) ? (int)arguments[4].AsNumber() : 0;
            double decay       = (arguments.Count >= 6) ? arguments[5].AsNumber() : 0.9;

            kernel.Configure(threshold, resetV, resetUDelta, refractory, decay);
            return new WValue(true);
        }
        public override string ToString() => "<native fn wea_neuro_configure>";
    }

    public class NeuroSynapseCountFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            return new WValue(kernel != null ? (double)kernel.GetSynapseCount() : 0.0);
        }
        public override string ToString() => "<native fn wea_neuro_synapse_count>";
    }

    public class NeuroGetDataFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) return new WValue(new List<object>());
            var list = new List<object>(kernel.NeuronCount);
            for (int i = 0; i < kernel.NeuronCount; i++)
                list.Add(kernel.V[i]);
            return new WValue(list);
        }
        public override string ToString() => "<native fn wea_neuro_get_data>";
    }

    public class NeuroResetFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) return new WValue(false);
            kernel.Reset();
            return new WValue(true);
        }
        public override string ToString() => "<native fn wea_neuro_reset>";
    }

  
    public class NeuroEnableStdpFunc : IWCallable
    {
        public int Arity() => 6;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as CustomNeuroKernel;
            if (kernel == null) throw new Exception("wea_neuro_enable_stdp: Kernel gerekli.");

            double aPlus     = arguments[1].AsNumber();
            double aMinus    = arguments[2].AsNumber();
            double tauPlus   = arguments[3].AsNumber();
            double tauMinus  = arguments[4].AsNumber();
            double maxWeight = arguments[5].AsNumber();

            kernel.EnableStdp(aPlus, aMinus, tauPlus, tauMinus, maxWeight);
            return new WValue(true);
        }
        public override string ToString() => "<native fn wea_neuro_enable_stdp>";
    }
}
