using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WneuraBridge _bridge;

        public MainWindow()
        {
            InitializeComponent();
            _bridge = new WneuraBridge();
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RunButton.IsEnabled = false;
                OutputText.Text = "Initializing WNEURA Simulation...\nSending parameters to Python Engine...";

                // 1. Create Dummy Configuration (Test Data)
                var simulationConfig = new
                {
                    initial_states = new[] { 0.5, 1.0, 0.5, 0.2 }, // D, R, S, C
                    parameters = new
                    {
                        stress_load = 0.5,
                        c_clearance = 0.1,
                        s_prod = 1.0,
                        s_decay = 0.2,
                        stress_impact = 0.5,
                        d_prod = 1.0,
                        d_decay = 0.2,
                        r_recovery = 0.1,
                        d_tox = 0.1,
                        c_tox = 0.1
                    },
                    days = 100
                };

                // Serialize to JSON
                string jsonInput = JsonSerializer.Serialize(simulationConfig);

                // 2. Call the Engine via Bridge (Async to keep UI responsive)
                // We wrap the synchronous Process call in Task.Run
                string result = await Task.Run(() => _bridge.RunSimulation(jsonInput));

                // 3. Display Result
                try
                {
                    // Pretty-print if it's valid JSON
                    using (var jDoc = JsonDocument.Parse(result))
                    {
                         string prettyJson = JsonSerializer.Serialize(jDoc.RootElement, new JsonSerializerOptions { WriteIndented = true });
                         OutputText.Text = prettyJson;
                    }
                }
                catch (JsonException)
                {
                    // Fallback for raw text (e.g., error messages that aren't JSON)
                    OutputText.Text = result;
                }
            }
            catch (Exception ex)
            {
                OutputText.Text = $"CRITICAL ERROR:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            }
            finally
            {
                RunButton.IsEnabled = true;
            }
        }
    }
}
