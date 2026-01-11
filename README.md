#  WSharp (we#) - Scientific Programming Language

> **"Code the Universe."**
>
> **WSharp (we#)** is a powerful, interpreted programming language built on the .NET ecosystem, designed specifically for **scientific computing, biological simulations, and complex physics engines.** It bridges the gap between easy-to-read syntax and hardcore mathematical modeling.

![Version](https://img.shields.io/badge/version-v00.1_Scientific-blue)
![Build](https://img.shields.io/badge/build-passing-brightgreen)
![Platform](https://img.shields.io/badge/platform-Windows_.NET-purple)
![License](https://img.shields.io/badge/license-MIT-orange)

---

## Why WSharp?

Standard languages require heavy libraries and complex setups to perform scientific calculations. **WSharp comes with the laws of the universe built-in.**

Whether you are simulating a **neuron firing in the brain**, calculating **quantum tunneling probabilities**, or creating a **2D physics game**, WSharp handles the math natively.

### Key Features
* ** Custom Core:** Built from scratch with a dedicated Lexer, Parser, and Interpreter.
* ** Visual Engine:** Integrated GDI+ graphics library for real-time plotting and drawing.
* ** Computational Neuroscience:** Native support for Izhikevich neuron models and STDP learning rules.
* ** Quantum & Nuclear Physics:** Built-in functions for high-energy physics and quantum mechanics.
* ** Bio-Chemistry:** DNA transcription and chemical stoichiometry tools out of the box.

---

##  Scientific Modules (The Engine)

WSharp includes specialized libraries that make scientific coding accessible:

| Module | Description | Key Functions |
| :--- | :--- | :--- |
| **`NeurologyLib`** | **Computational Neuroscience** | `wea_neuro_izhi_v`, `wea_neuro_stdp`, `wea_neuro_ghk_flux` |
| **`QuantumLib`** | **Quantum Mechanics** | `wea_quant_tunneling`, `wea_quant_uncertainty_p`, `wea_quant_prob` |
| **`NuclearLib`** | **Nuclear Physics** | `wea_nuc_energy` ($E=mc^2$), `wea_nuc_decay`, `wea_nuc_binding_energy` |
| **`PhysicsLib`** | **Classical Mechanics** | `wea_phy_force`, `wea_phy_gravity`, `wea_phy_ke` |
| **`BiologyLib`** | **Genetics & Biology** | `wea_bio_dna_rna`, `wea_bio_codon_count`, `wea_bio_sa_vol_ratio` |
| **`ChemistryLib`** | **Chemistry** | `wea_chem_gas_p` ($PV=nRT$), `wea_chem_ph`, `wea_chem_molarity` |
| **`MathLib`** | **Advanced Math** | `wea_math_lerp`, `wea_math_clamp`, `wea_math_atan2` |

---

## Code Examples

### 1. Simulating a Neuron (Izhikevich Model)
```we#
// Simulating a spiking neuron
wea_unit v = -65
wea_unit u = -13
wea_emit("Starting Neuron Simulation...")

wea_cycle (v < 30) {
    // Calculate next voltage step using built-in Neuro physics
    v = wea_neuro_izhi_v(v, u, 10, 1)
    u = wea_neuro_izhi_u(v, u, 0.02, 0.2, 1)

Update History (Changelog)
v00.1 - The Scientific Edition (Current)
Major Overhaul aimed at scientific computing.

[NEW] NeurologyLib: Added support for Hodgkin-Huxley & Izhikevich models, Nernst/GHK equations, and Hebbian Learning.

[NEW] QuantumLib: Added functions for Heisenberg Uncertainty, De Broglie wavelength, and Tunneling.

[NEW] NuclearLib: Added Radioactive decay, Half-life, and Mass-Energy calculations.

[NEW] Bio/Chem/Phys Libs: Expanded the core with fundamental natural science laws.

[OPTIMIZATION] Refactored Interpreter.cs to handle dynamic library loading.

[FIX] Removed Environment.cs conflict; unified Scope management in Types.cs.

v00.1 Beta - The Graphical Update
[NEW] DrawLib: Introduced GDI+ windowing system (wea_view_init, wea_draw_circle).

[NEW] MathLib: Added Lerp, Clamp, and basic Trigonometry.

[FIX] Fixed infinite loop issues in wea_cycle.

v00.0 Alpha - The Foundation
[CORE] Initial release of the custom Lexer and Parser.

[CORE] Basic variable declaration (wea_unit) and printing (wea_emit).

[CORE] Basic arithmetic operations (+, -, *, /).
    
    wea_emit("Membrane Potential: " + v + " mV")
    wea_wait(50)
}
wea_emit("NEURON FIRED!")

Installation & Build
Clone the Repository:

Bash

git clone [https://github.com/YourUsername/WSharp.git](https://github.com/YourUsername/WSharp.git)
Open in Visual Studio: Open we# 00.1 beta.csproj in Visual Studio 2022.

Build: Press Ctrl + Shift + B. Ensure you have the .NET Desktop Runtime installed.

Run: Start WSharp.exe to enter the interactive shell or drag-and-drop a .we file.

 Roadmap
[ ] Multi-Language Syntax: Native keywords for Turkish (TR), Spanish (ES), and German (DE).

[ ] 3D Rendering: Basic 3D shapes and camera control.

[ ] Neural Network API: High-level functions to create simple AI perceptrons.

License
This project is licensed under the MIT License - see the LICENSE file for details.
