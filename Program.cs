using System;
using System.Windows.Forms;

namespace WSharp
{
    static class Program
    {
     
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {

                Application.Run(new IDE());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uygulama Hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public static void ShowHelp()
        {
            string helpMsg = "--- WSharp Yardım ---\n\n" +
                             "wea_emit(değer)  -> Ekrana yazdırır.\n" +
                             "wea_unit x = 10  -> Değişken tanımlar.\n" +
                             "wea_if (...) {}  -> Koşul bloğu.\n" +
                             "wea_cycle (...) -> Döngü bloğu.\n" +
                             "wea_plot(x)      -> Grafik çizer.\n" +
                             "wea_neuro_...    -> Nöroloji kütüphanesi.";

            MessageBox.Show(helpMsg, "WSharp Scientific Edition", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
