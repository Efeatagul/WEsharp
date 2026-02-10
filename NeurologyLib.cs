using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace WSharp
{
    /// <summary>
    /// WSharp Nöroloji Motoru (CPU Optimized)
    /// Izhikevich nöron modeli ile yüksek performanslı simülasyon
    /// </summary>
    public class NeuroKernel
    {
        // --- Yapılandırma ---
        public int NeuronCount { get; private set; }
        public float Dt { get; set; } = 0.05f;

        // --- State Arrays (Cache-friendly memory layout) ---
        public float[] V;      // Membrane Potential
        public float[] U;      // Recovery Variable (Izhikevich)
        public float[] I_ext;  // External Input
        
        // --- Parameters (Heterojen nöron tipleri için) ---
        public float[] A;
        public float[] B;
        public float[] C;
        public float[] D;

        // --- Spike Events ---
        public bool[] Spikes;
        public List<int> SpikeIndices;

        // --- Synaptic Connections (CSR Format) ---
        private int[] _synapseTargets;
        private float[] _synapseWeights;
        private int[] _synapseIndptr;

        public NeuroKernel(int neuronCount)
        {
            NeuronCount = neuronCount;
            InitializeMemory();
        }

        private void InitializeMemory()
        {
            V = new float[NeuronCount];
            U = new float[NeuronCount];
            I_ext = new float[NeuronCount];
            
            A = new float[NeuronCount];
            B = new float[NeuronCount];
            C = new float[NeuronCount];
            D = new float[NeuronCount];
            
            Spikes = new bool[NeuronCount];
            SpikeIndices = new List<int>(NeuronCount / 10);

            // Default: Regular Spiking neurons
            Array.Fill(V, -65.0f);
            Array.Fill(A, 0.02f);
            Array.Fill(B, 0.2f);
            Array.Fill(C, -65.0f);
            Array.Fill(D, 8.0f);
        }

        /// <summary>
        /// Rastgele ağ oluşturur (test için)
        /// </summary>
        public void BuildRandomNetwork(int connectionsPerNeuron)
        {
            int totalSynapses = NeuronCount * connectionsPerNeuron;
            _synapseTargets = new int[totalSynapses];
            _synapseWeights = new float[totalSynapses];
            _synapseIndptr = new int[NeuronCount + 1];

            var rand = new Random();
            int cursor = 0;

            for (int i = 0; i < NeuronCount; i++)
            {
                _synapseIndptr[i] = cursor;
                for (int j = 0; j < connectionsPerNeuron; j++)
                {
                    _synapseTargets[cursor] = rand.Next(0, NeuronCount);
                    _synapseWeights[cursor] = (float)rand.NextDouble() * 10.0f;
                    cursor++;
                }
            }
            _synapseIndptr[NeuronCount] = cursor;
        }

        /// <summary>
        /// [PHYSICS KERNEL] Multi-threaded Izhikevich integration
        /// </summary>
        public void Step()
        {
            SpikeIndices.Clear();

            // 1. Physics computation (parallel)
            Parallel.For(0, NeuronCount, i =>
            {
                float v = V[i];
                float u = U[i];
                float i_in = I_ext[i];

                // Izhikevich model derivatives
                float dv = (0.04f * v * v + 5.0f * v + 140.0f - u + i_in);
                float du = (A[i] * (B[i] * v - u));

                V[i] += dv * Dt;
                U[i] += du * Dt;

                // Spike detection
                Spikes[i] = V[i] >= 30.0f;
            });

            // 2. Spike processing and synaptic transmission
            for (int i = 0; i < NeuronCount; i++)
            {
                if (Spikes[i])
                {
                    SpikeIndices.Add(i);
                    
                    // Reset mechanism
                    V[i] = C[i];
                    U[i] += D[i];

                    // Synaptic transmission
                    if (_synapseTargets != null)
                    {
                        int start = _synapseIndptr[i];
                        int end = _synapseIndptr[i + 1];

                        for (int k = start; k < end; k++)
                        {
                            int target = _synapseTargets[k];
                            float weight = _synapseWeights[k];
                            I_ext[target] += weight;
                        }
                    }
                }
                else
                {
                    // Current decay
                    I_ext[i] *= 0.9f;
                }
            }
        }

        /// <summary>
        /// WSharp API: Nörona akım enjekte et
        /// </summary>
        public void SetInput(int neuronIndex, float current)
        {
            if (neuronIndex >= 0 && neuronIndex < NeuronCount)
                I_ext[neuronIndex] = current;
        }

        public float GetVoltage(int neuronIndex)
        {
            return (neuronIndex >= 0 && neuronIndex < NeuronCount) ? V[neuronIndex] : 0f;
        }

        public int GetSpikeCount()
        {
            return SpikeIndices.Count;
        }

        /// <summary>
        /// Nöron tipini ayarla (RS, FS, IB, CH)
        /// </summary>
        public void SetNeuronType(int index, string type)
        {
            if (index < 0 || index >= NeuronCount) return;

            switch (type.ToUpper())
            {
                case "RS": // Regular Spiking
                    A[index] = 0.02f; B[index] = 0.2f; C[index] = -65f; D[index] = 8f;
                    break;
                case "FS": // Fast Spiking
                    A[index] = 0.1f; B[index] = 0.2f; C[index] = -65f; D[index] = 2f;
                    break;
                case "IB": // Intrinsically Bursting
                    A[index] = 0.02f; B[index] = 0.2f; C[index] = -55f; D[index] = 4f;
                    break;
                case "CH": // Chattering
                    A[index] = 0.02f; B[index] = 0.2f; C[index] = -50f; D[index] = 2f;
                    break;
            }
        }
    }

    // --- WSharp Wrapper Functions ---
    public class NeuroCreateFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            int neuronCount = (int)arguments[0].AsNumber();
            var kernel = new NeuroKernel(neuronCount);
            return new WValue(kernel);
        }
        public override string ToString() => "<native fn neuro_create>";
    }

    public class NeuroBuildNetworkFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as NeuroKernel;
            int connections = (int)arguments[1].AsNumber();
            kernel?.BuildRandomNetwork(connections);
            return new WValue(true);
        }
        public override string ToString() => "<native fn neuro_build_network>";
    }

    public class NeuroStepFunc : IWCallable
    {
        public int Arity() => 1;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as NeuroKernel;
            kernel?.Step();
            return new WValue(kernel?.GetSpikeCount() ?? 0);
        }
        public override string ToString() => "<native fn neuro_step>";
    }

    public class NeuroSetInputFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as NeuroKernel;
            int index = (int)arguments[1].AsNumber();
            float current = (float)arguments[2].AsNumber();
            kernel?.SetInput(index, current);
            return new WValue(true);
        }
        public override string ToString() => "<native fn neuro_set_input>";
    }

    public class NeuroGetVoltageFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            var kernel = arguments[0].Value as NeuroKernel;
            int index = (int)arguments[1].AsNumber();
            return new WValue(kernel?.GetVoltage(index) ?? 0f);
        }
        public override string ToString() => "<native fn neuro_get_voltage>";
    }
}
