<div align="center">

#  WSharp (we#)
### Scientific Neurology & AI Simulation Platform

![Version](https://img.shields.io/badge/version-00.1_Beta-blue?style=for-the-badge)
![Platform](https://img.shields.io/badge/platform-.NET_10-purple?style=for-the-badge)
![Architecture](https://img.shields.io/badge/architecture-Headless_Hybrid-success?style=for-the-badge)
![Focus](https://img.shields.io/badge/focus-Neurology_&_AI-red?style=for-the-badge)

**"Simulating the complexity of biological brain development and decision-making processes."**

**WSharp**, biyolojik hesaplama (C#) ile yapay zeka ajanları (Python/Wneura) arasındaki boşluğu dolduran, **Headless Architecture** yapısına sahip yüksek performanslı bir bilimsel simülasyon dilidir.

[Mimari](#-system-architecture) • [Özellikler](#-key-features) • [Kurulum](#-installation) • [Kullanım](#-usage-examples)

</div>

---

## System Architecture (The Hybrid Core)

WSharp, C#'ın hızını Python ekosisteminin esnekliğiyle birleştiren hibrit bir yapı kullanır.

```mermaid
graph LR
    A[WSharp IDE] -->|wea_wneura_run| B(PythonBridge.cs)
    B -->|Spawns Process| C{Wneura Agents}
    C -->|PyTorch/CUDA| D[Brain Training]
    D -->|JSON Response| B
    B -->|Returns Data| A
    style A fill:#6a0dad,stroke:#333,stroke-width:2px,color:#fff
    style C fill:#3572A5,stroke:#333,stroke-width:2px,color:#fff

Özellik,Açıklama,Durum
 NeurologyLib,"Nernst, GHK ve Hodgkin-Huxley denklemleri için yerleşik fonksiyonlar.", Aktif
 PythonBridge,"WSharp içinden harici Python (Wneura) scriptlerini ""Headless"" çalıştırma.", Yeni
 AIFixer,Otomatik sözdizimi hatası tespiti ve kendi kendini onaran kod önerileri., Beta
 QuantumLib,Temel kuantum süperpozisyon ve dolanıklık simülasyonları., Deneysel
 Bio/Chem Libs,Kimyasal reaksiyonlar ve biyolojik bozunma simülasyonları., Aktif

 Installation & Setup
Gereksinimler (Prerequisites)
OS: Windows 10/11

Runtime: .NET 10.0 (Preview/RC)

Python: Python 3.9+ (Wneura entegrasyonu için şart)

Yapılandırma (Python Bağlantısı)
wea_wneura_run komutlarını kullanmak için köprüyü yapılandırmalısınız:

WSharp/PythonBridge.cs dosyasını açın.

PythonPath değişkenini bulun.

Python yolunuzu yapıştırın (veya otomatik algılama için dokunmayın).

// PythonBridge.cs içindeki örnek ayar
private static string PythonPath = @"PASTE_YOUR_PYTHON_PATH_HERE";

// Simülasyonu Başlat
wea_emit("Initializing Neural Link...")

// Wneura klasöründeki ajanı çalıştır
// Argümanlar: script_yolu, parametreler
wea_unit brain_data = wea_wneura_run("Wneura/agent.py", "--epochs 100")

// Python beyninden gelen JSON sonucunu ekrana bas
wea_emit("Training Complete. Results:")
wea_emit(brain_data)

// Parametreler: Geçirgenlik ve Konsantrasyonlar (K, Na, Cl)
wea_unit vm = wea_neuro_ghk_voltage(
    1.0, 0.04, 0.45,  // Permeability (Pk, Pna, Pcl)
    4.0, 140.0,       // K (out, in)
    145.0, 15.0,      // Na (out, in)
    110.0, 5.0        // Cl (out, in)
)

wea_emit("Membrane Potential (mV):")
wea_emit(vm)

 Roadmap & Development Routine
Geliştirme süreci katı bir disiplinle ilerler.

Rutin: Her Pazar, haftalık hata düzeltmeleri, optimizasyon ve kod incelemeleri yapılır.

Sonraki Adımlar:

[ ] Scientific Plotter ile Python verilerinin canlı çizimi.

[ ] ML tabanlı hata tahmini sunan gelişmiş AIFixer.

[ ] Bozunma simülasyonları için NuclearLib genişletmesi.

 Contributing & License
Bu proje Neuro-Symbolic AI tutkusuyla geliştirilen kişisel bir projedir. Katkılarınızı bekliyoruz!

MIT License altında dağıtılmaktadır.

<i>Developed with  by <b>Efeatagul</b> (weagw)</i>

</div>
