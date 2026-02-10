import sys
import json
import math

# ==========================================
# WNEURA ENGINE v1.0 (Izhikevich Model)
# ==========================================

def simulate_neuron(neuron_type, duration):
    # 1. Biyolojik Parametreler (Izhikevich, 2003)
    # a: toparlanma hızı, b: hassasiyet, c: reset değeri, d: reset sonrası artış
    if neuron_type == "RS": # Regular Spiking (Standart)
        a, b, c, d = 0.02, 0.2, -65.0, 8.0
    elif neuron_type == "FS": # Fast Spiking (Hızlı)
        a, b, c, d = 0.1, 0.2, -65.0, 2.0
    elif neuron_type == "IB": # Bursting (Patlamalı)
        a, b, c, d = 0.02, 0.2, -55.0, 4.0
    else: # Varsayılan (RS)
        a, b, c, d = 0.02, 0.2, -65.0, 8.0

    # 2. Başlangıç Değerleri
    v = -65.0  # Voltaj (mV)
    u = b * v  # Toparlanma değişkeni
    spikes = 0 # Ateşleme Sayacı
    
    # Simülasyon verisi
    dt = 1.0 # Zaman adımı (1 ms)
    steps = int(duration / dt)
    
    # 3. Simülasyon Döngüsü
    for t in range(steps):
        # Uyarı Akımı (Input Current - I)
        I = 10 if t > 10 else 0 

        # Diferansiyel Denklemler (Euler Metodu)
        v_next = v + (0.04 * v**2 + 5 * v + 140 - u + I)
        u_next = u + (a * (b * v - u))

        # Ateşleme Kontrolü (Spike)
        if v_next >= 30:
            v = c       # Resetle
            u = u + d   # Yükle
            spikes += 1
        else:
            v = v_next
            u = u_next

    return {"status": "success", "type": neuron_type, "spikes": spikes}

if __name__ == "__main__":
    try:
        # C#'tan gelen veriyi al
        if len(sys.argv) > 1:
            data = json.loads(sys.argv[1])
            ntype = data.get("type", "RS")
            ndur = float(data.get("duration", 1000))
        else:
            ntype, ndur = "RS", 1000

        # Hesapla
        result = simulate_neuron(ntype, ndur)
        
        # C#'a geri gönder
        print(json.dumps(result))
        
    except Exception as e:
        print(json.dumps({"status": "error", "msg": str(e)}))
