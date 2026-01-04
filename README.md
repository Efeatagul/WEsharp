WE-Sharp (WSharp) Engine
Master-Build 2.0 | Sunday Optimization Edition

WE-Sharp is a high-performance scripting language engine built on the C# infrastructure, featuring a unique syntax and a modular library architecture.

Core Features
Unique Syntax: Intuitive command set optimized with the wea_ prefix.

Modular Architecture: Easily extendable via the ILibrary interface—add new capabilities in seconds.

Advanced Error Handling: Robust flow control using custom wea_eman and wea_fail blocks.

Built-in Cores:

Logic (Math): Geometric and probabilistic calculations with degree-to-radian support.

Chrono (Time): High-precision time tracking and flow control.

Audio: Frequency-based sound synthesizer for musical notes and system alerts.

Interface (UI): Dynamic terminal theming (Ghost, Void, Neon, Magma modes).

 Code Example
A simple loop and UI styling in WSharp:

Kod snippet'i

wea_ui_style("wea_neon") # Set terminal to neon mode
wea_unit counter = 0

wea_cycle (counter < 5) {
    wea_emit "Step Index: " + counter
    wea_audio_tone("C#", 200) # Play a musical note at each step
    counter = counter + 1
}

wea_audio_alert("wea_info")
wea_emit "Operation Completed Successfully!"
 Project Structure
The engine is built on four primary pillars:

Lexer (The Scanner): Breaks source code into atomic tokens.

Parser (The Brain): Converts tokens into a hierarchical Abstract Syntax Tree (AST).

Interpreter (The Heart): Executes logic and manages memory/variables.

Libraries (The Tools): Modular extensions for Math, Audio, and UI.

 Getting Started
Open the project solution (.sln) in Visual Studio.

Press Ctrl + Shift + B to build the engine.

Run your .wea scripts through the generated executable.

 Sunday Routine Logs
This project follows a strict Sunday Routine for maintenance:

Weekly bug fixes and stability audits.

Performance optimization of the Core Brain.

Non-copyrighted name changes and terminology alignment.

Developed by: [Efe Ata Gül]

Version: 2.0.0-Master

License: MIT
