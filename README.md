
<div align="center">

#  WSharp Studio: Scientific Programming Environment

![Version](https://img.shields.io/badge/version-0.1_Alpha-blue?style=for-the-badge&logo=visual-studio)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=for-the-badge)
![Focus](https://img.shields.io/badge/focus-Neuroscience_%26_Physics-purple?style=for-the-badge)
![License](https://img.shields.io/badge/license-MIT-orange?style=for-the-badge)

<br>

**WSharp**, bilimsel simülasyonlar, nörolojik modelleme ve fizik hesaplamaları için geliştirilmiş, **kendi IDE'sine sahip** yüksek seviyeli bir programlama dilidir. Sadece kod yazmak için değil; veriyi görselleştirmek ve canlı simüle etmek için tasarlanmıştır.

[Kurulum](#-kurulum-ve-çalıştırma) • [Belgeler](#-kütüphane-ve-modüller) • [Özellikler](#-yeni-özellikler-v01-update)

</div>

---

##  Yeni Özellikler (v0.1 Update)

WSharp artık sadece bir konsol uygulaması değil, tam teşekküllü bir **Geliştirme Ortamı (IDE)**.

| Özellik | Açıklama |
| :--- | :--- |
| ** 6-Panel Grid UI** | Dosya Gezgini, Kod Editörü, Terminal, Grafik Paneli, Değişken İzleyici ve AI Chat tek ekranda. |
| ** Scientific Plotter** | `wea_plot(x)` komutu ile verileri anlık olarak grafikleştirme (Sinüs dalgaları, Spike trenleri vb.). |
| ** Neurology Engine** | Hodgkin-Huxley, Nernst ve GHK denklemlerini içeren gelişmiş nöro-biyoloji motoru. |
| ** IntelliSense** | Kod yazarken otomatik tamamlama ve sözdizimi renklendirme (Syntax Highlighting). |
| ** Variable Watcher** | Hafızadaki değişkenleri (Voltaj, Zaman, İyon Konsantrasyonu) canlı izleme paneli. |
| ** Local AI Chat** | İnternetsiz çalışan, dilin dokümantasyonunu bilen yerel asistan. |

---

## 🧪 Kütüphane ve Modüller

WSharp, bilimsel hesaplamalar için özelleşmiş **10+ yerleşik kütüphane** ile gelir.

| Kütüphane | Fonksiyon Öneki | Açıklama ve Örnek Fonksiyonlar |
| :--- | :--- | :--- |
| **Neurology** | `wea_neuro_` | **(YENİ)** `hh_alpha_m`, `ghk_voltage`, `nernst`, `syn_nmda` |
| **Plotting** | `wea_plot` | **(YENİ)** Veri görselleştirme ve osiloskop benzeri çizim. |
| **Math** | `wea_math_` | `sin`, `cos`, `sqrt`, `pow`, `abs`, `round` |
| **Physics** | `wea_phys_` | `force`, `kinetic_energy`, `gravitational_force` |
| **Quantum** | `wea_quant_` | `superposition`, `entanglement_check` |
| **Biology** | `wea_bio_` | `dna_transcription`, `enzyme_rate` |
| **Nuclear** | `wea_nuc_` | `decay_rate`, `binding_energy` |
| **Chemistry** | `wea_chem_` | `molar_mass`, `ph_calc`, `ideal_gas` |
| **Standard** | `wea_` | `emit` (yazdır), `read` (oku), `wait` (bekle), `time` |

---

##  Kod Örnekleri

### 1. Nörolojik Simülasyon (Hodgkin-Huxley Gate)
Bir nöronun sodyum kanalının voltaja bağlı açılma olasılığını hesaplar ve grafik çizer.

```javascript
// Membran Dinlenim Voltajı
wea_unit V = -65 

wea_emit("Simülasyon Başlıyor...")

// Voltajı -65mV'den +20mV'ye kadar artır
wea_cycle (V < 20) {
    
    // Sodyum kapısının açılma hızı (Alpha M)
    wea_unit alpha = wea_neuro_hh_alpha_m(V)
    
    // Sodyum kapısının kapanma hızı (Beta M)
    wea_unit beta = wea_neuro_hh_beta_m(V)
    
    // Denge durumu (Açıklık Oranı)
    wea_unit open_prob = alpha / (alpha + beta)
    
    wea_emit("Voltaj: " + V + "mV -> Açıklık: %" + (open_prob * 100))
    
    // Grafiğe Çiz (Scientific Plotter Sekmesinde Görünür)
    wea_plot(V, open_prob * 100)
    
    V = V + 5
    wea_wait(50)
}
2. Kuantum Süperpozisyon Testi
JavaScript

wea_unit state = wea_quant_superposition(0.707, 0.707) // |0> ve |1> durumu
wea_emit("Quantum State Probability: " + state)

wea_if (state > 0.5) {
    wea_emit("Collapse: State |1>")
}
 Kurulum ve Çalıştırma
Repoyu klonlayın:

Bash

git clone [https://github.com/KULLANICIADIN/WSharp.git](https://github.com/KULLANICIADIN/WSharp.git)
WSharp.sln dosyasını Visual Studio 2022 ile açın.

Start (F5) tuşuna basın.

Açılan WSharp Studio penceresinde sol üstten File -> New diyerek kodlamaya başlayın!

Kodu çalıştırmak için sağ üstteki yeşil RUN butonuna basın.

Roadmap (Gelecek Planları)
[ ] Wneura Entegrasyonu: Yapay Sinir Ağlarını (ANN) WSharp içinde eğitmek.

[ ] 3D Protein Katlama: BiologyLib için görsel 3D modelleme.

[ ] Export to Python: WSharp kodunu Python scriptine çevirme.

<div align="center"> <i>Developed with  by <b>Efeatagul</b> for Science & Code.</i> </div>
