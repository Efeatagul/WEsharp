# WSharp (We#) | Lingua Medica

[ **README** ](./README.md) | [ **LICENSE** ](./LICENSE) | [ **ROADMAP** ](./ROADMAP.md) | [ **CODE_OF_CONDUCT** ](./CODE_OF_CONDUCT.md) | [ **CONTRIBUTING** ](./CONTRIBUTING.md)

---

## Overview
WSharp is a high-performance, JIT-compiled Domain-Specific Language (DSL) architected for computational neurology and advanced biophysical simulations. It provides a specialized environment to bridge the gap between high-level Pythonic AI logic and bare-metal .NET/CUDA performance.

The project is designed for researchers and engineers who require massive-scale neural link simulations without the memory overhead of traditional interpreted languages.

## Core Capabilities
* **Zero-Allocation JIT Pipeline:** Compiles AST nodes directly into optimized native delegates, minimizing Garbage Collector (GC) pressure during intense simulation loops.
* **Fused CUDA Kernels:** Leverages ILGPU to execute complex biological ODEs (e.g., Izhikevich, Hodgkin-Huxley) in a single GPU pass.
* **Hybrid Interoperability:** Native C# execution core with a high-speed interop layer for Python-based AI agents and tensor processing.
* **Scientific Precision:** Built-in libraries for membrane potential calculations, stoichiometric balancing, and radioactive decay.

---

## Technical Specifications

### JIT Compilation Architecture
The engine utilizes a custom-built parser and interpreter that targets the .NET 10.0 runtime. By employing ValueTuple-based environment binding and tree-walk optimizations, WSharp achieves near-native execution speeds for iterative mathematical workloads.

### Computational Neurology Performance
The neurology module is optimized for SNN (Spiking Neural Networks). It supports O(1) trace-based STDP (Spike-Timing-Dependent Plasticity) updates across millions of synaptic connections.

### Benchmarks (NVIDIA RTX Series)
| Network Scale | Synaptic Connections | Simulation Step | Execution Time |
| :--- | :--- | :--- | :--- |
| 100,000 Neurons | 100,000,000 | 1,000 ms (Physics) | 924 ms |
| 100,000 Neurons | 100,000,000 | 1,000 ms (Full STDP) | 1.37 seconds |

---

## Quick Start Example

```
// Initialization of a cortical simulation
print "Initializing Neural Link..."

let neurons = 100000
let synapses = neurons * 100

// Define biophysical parameters for Izhikevich model
let v = -65.0
let u = 0.2 * v

// Trigger CUDA execution kernel
let brain = execute_cuda_simulation(v, u, synapses)

print "Simulation Complete."
```
Build and Requirements;
Runtime: .NET 10.0 SDK

GPU: NVIDIA CUDA-enabled GPU (Optional, for NeurologyLib acceleration)

OS: Windows 10/11 (Initial Release)
