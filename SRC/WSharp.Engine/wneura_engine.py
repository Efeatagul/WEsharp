# ======================================================================
# WSHARP (We#) NEURO-ENGINE - WEAGW Ecosystem
# Copyright (c) 2026 Efe Ata Gul. All rights reserved.
# 
# This file is part of the WSharp project.
# 
# OPEN SOURCE: Licensed under the GNU AGPL v3.0. You may use this 
# file freely in open-source/academic projects provided you give 
# clear attribution to "WSharp by Efe Ata Gul".
# 
# COMMERCIAL: If you wish to use WSharp in closed-source, proprietary, 
# or commercial products, you must purchase a WEAGW Commercial License.
# ======================================================================
#!/usr/bin/env python3
# ═══════════════════════════════════════════════════════════════
#  WNEURA ENGINE v2.0 — Persistent Daemon (stdin/stdout JSON)
# ═══════════════════════════════════════════════════════════════
#
#  ARCHITECTURE:
#  ─────────────
#  This script runs as a LONG-LIVED DAEMON process. It does NOT
#  exit after processing a single command. Instead it loops
#  forever reading one JSON payload per line from stdin, processes
#  it, and prints one JSON response line to stdout.
#
#  PROTOCOL:
#    1. C# writes one JSON object per line to this process's stdin
#    2. This script reads the line, parses JSON, dispatches command
#    3. This script prints one JSON response line to stdout
#    4. This script flushes stdout so C# can read immediately
#    5. Loop back to step 1
#
#  The process only terminates when:
#    • stdin is closed (C# app exits or calls Dispose)
#    • An unrecoverable system error occurs
#
#  SUPPORTED COMMANDS:
#    { "command": "simulate",   "type": "RS", "duration": 1000 }
#    { "command": "analyze",    "data": [1.0, 2.0, ...] }
#    { "command": "plot_save",  "x": [...], "y": [...], "filename": "out" }
#    { "command": "ping" }   → health check, returns {"status":"pong"}
#
# ═══════════════════════════════════════════════════════════════

import sys
import json
import math


def cmd_simulate(params):
    """Izhikevich neuron model simulation."""
    neuron_type = params.get("type", "RS")
    duration = float(params.get("duration", 1000))

   
    if neuron_type == "RS":     
        a, b, c, d = 0.02, 0.2, -65.0, 8.0
    elif neuron_type == "FS":   
        a, b, c, d = 0.1, 0.2, -65.0, 2.0
    elif neuron_type == "IB":    
        a, b, c, d = 0.02, 0.2, -55.0, 4.0
    else:                        
        a, b, c, d = 0.02, 0.2, -65.0, 8.0

    
    v = -65.0   
    u = b * v   
    spikes = 0  

    dt = 1.0
    steps = int(duration / dt)

   
    for t in range(steps):
        I = 10 if t > 10 else 0 

        v_next = v + (0.04 * v**2 + 5 * v + 140 - u + I)
        u_next = u + (a * (b * v - u))

        if v_next >= 30:
            v = c
            u = u + d
            spikes += 1
        else:
            v = v_next
            u = u_next

    return {"status": "success", "type": neuron_type, "spikes": spikes}


def cmd_analyze(params):
    """Statistical analysis using pandas describe()."""
    import pandas as pd
    data = params["data"]
    df = pd.DataFrame(data, columns=["value"])
    desc = df["value"].describe()
    result = {}
    for key, val in desc.items():
        result[key] = round(float(val), 6)
    return {"status": "success", "result": result}


def cmd_plot_save(params):
    """Save x/y data as a PNG plot using matplotlib."""
    import matplotlib
    matplotlib.use("Agg") 
    import matplotlib.pyplot as plt
    import os

    x_data = params["x"]
    y_data = params["y"]
    filename = params["filename"]

    fig, ax = plt.subplots(figsize=(10, 6))
    ax.plot(x_data, y_data, color="#4FC3F7", linewidth=2, marker="o", markersize=4)
    ax.set_xlabel("X")
    ax.set_ylabel("Y")
    ax.set_title("WSharp Data Plot")
    ax.grid(True, alpha=0.3)
    fig.tight_layout()

    if not filename.lower().endswith(".png"):
        filename = filename + ".png"

    fig.savefig(filename, dpi=150, bbox_inches="tight")
    plt.close(fig)

    abs_path = os.path.abspath(filename)
    return {"status": "success", "file": abs_path}



HANDLERS = {
    "simulate":   cmd_simulate,
    "analyze":    cmd_analyze,
    "plot_save":  cmd_plot_save,
}


def dispatch(payload):
    """Route a JSON payload to the correct handler."""
    command = payload.get("command", "")

    if command == "ping":
        return {"status": "pong"}

    handler = HANDLERS.get(command)
    if handler is None:
        return {"status": "error", "msg": f"Unknown command: {command}"}

    return handler(payload)



if __name__ == "__main__":
   

    while True:
        try:
            
            line = sys.stdin.readline()

         
            if not line:
                break

           
            line = line.strip()
            if not line:
                continue

          
            payload = json.loads(line)

            
            response = dispatch(payload)

            print(json.dumps(response), flush=True)

        except json.JSONDecodeError as e:
            
            error = {"status": "error", "msg": f"Invalid JSON: {str(e)}"}
            print(json.dumps(error), flush=True)

        except Exception as e:
           
            error = {"status": "error", "msg": str(e)}
            print(json.dumps(error), flush=True)