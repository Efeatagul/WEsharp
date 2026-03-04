/* ======================================================================
 * WSHARP (We#) NEURO-ENGINE - WEAGW Ecosystem
 * Copyright (c) 2026 Efe Ata Gul. All rights reserved.
 * * This file is part of the WSharp project.
 * * OPEN SOURCE: Licensed under the GNU AGPL v3.0. You may use this 
 * file freely in open-source/academic projects provided you give 
 * clear attribution to "WSharp by Efe Ata Gul".
 * * COMMERCIAL: If you wish to use WSharp in closed-source, proprietary, 
 * or commercial products, you must purchase a WEAGW Commercial License.
 * ====================================================================== */
// ═══════════════════════════════════════════════════════════════
//  LivePlotEngine.cs — WPF Live Plot Window Manager
// ═══════════════════════════════════════════════════════════════
//
//  Creates WPF Window instances with ScottPlot.WPF's WpfPlot
//  control for real-time data visualization.
//
//  PUBLIC API (unchanged from WinForms version):
//    PlotLine(name, data)                 — line chart
//    PlotHeatmap(name, data)              — heatmap
//    PlotRaster(name, times, neuronIds)   — SNN raster plot
//    SetTitle(name, title)                — change window title
//    CloseWindow(name)                    — close one window
//    CloseAll()                           — close all windows
//
//  ⚠️  NO System.Windows.Forms references anywhere.
// ═══════════════════════════════════════════════════════════════

#nullable disable
using System;
using System.Collections.Generic;
using System.Windows;                   
using System.Windows.Controls;          
using System.Windows.Media;            
using ScottPlot;                        
using ScottPlot.WPF;                   
using ScottPlot.Plottables;

namespace WSharp
{
    
    public static class LivePlotEngine
    {
        private static readonly Dictionary<string, LivePlotWindow> _windows = new Dictionary<string, LivePlotWindow>();

       
        public static void PlotLine(string windowName, double[] data)
        {
            if (_windows.TryGetValue(windowName, out var win) && win.PlotType == "line" && win.Window.IsLoaded)
            {
                
                win.Window.Dispatcher.Invoke(() =>
                {
                    win.WpfPlot.Plot.Clear();

                    double[] xs = new double[data.Length];
                    double[] ys = new double[data.Length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        xs[i] = i;
                        ys[i] = data[i];
                    }

                    var scatter = win.WpfPlot.Plot.Add.Scatter(xs, ys);
                    scatter.Color = ScottPlot.Color.FromHex("#00D4FF");
                    scatter.LineWidth = 2;
                    scatter.MarkerSize = 0;

                    win.WpfPlot.Plot.Axes.AutoScale();
                    win.WpfPlot.Refresh();
                });
            }
            else
            {
                
                CreateLineWindow(windowName, data);
            }
        }

        private static void CreateLineWindow(string windowName, double[] data)
        {
            
            CloseWindow(windowName);

            var window = new Window
            {
                Title = $"WSharp Live — {windowName}",
                Width = 900,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 25))
            };

            var wpfPlot = new WpfPlot();
            window.Content = wpfPlot;

            
            wpfPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#14141A");
            wpfPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#1A1A24");
            wpfPlot.Plot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.FromHex("#888888");
            wpfPlot.Plot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHex("#888888");
            wpfPlot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#666666");
            wpfPlot.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#666666");
            wpfPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#2A2A35");

           
            double[] xs = new double[data.Length];
            double[] ys = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                xs[i] = i;
                ys[i] = data[i];
            }

            var scatter = wpfPlot.Plot.Add.Scatter(xs, ys);
            scatter.Color = ScottPlot.Color.FromHex("#00D4FF");
            scatter.LineWidth = 2;
            scatter.MarkerSize = 0;

            wpfPlot.Plot.Axes.AutoScale();
            wpfPlot.Refresh();

            var win = new LivePlotWindow
            {
                Window = window,
                WpfPlot = wpfPlot,
                PlotType = "line"
            };

            _windows[windowName] = win;

            window.Closed += (s, e) => { _windows.Remove(windowName); };
            window.Show();
        }

        
        public static void PlotHeatmap(string windowName, double[,] data)
        {
            if (_windows.TryGetValue(windowName, out var win) && win.PlotType == "heatmap" && win.Window.IsLoaded)
            {
               
                win.Window.Dispatcher.Invoke(() =>
                {
                    int rows = data.GetLength(0);
                    int cols = data.GetLength(1);
                    for (int r = 0; r < rows && r < win.HeatmapData.GetLength(0); r++)
                        for (int c = 0; c < cols && c < win.HeatmapData.GetLength(1); c++)
                            win.HeatmapData[r, c] = data[r, c];

                    win.Heatmap.Update();
                    win.WpfPlot.Refresh();
                });
            }
            else
            {
                CreateHeatmapWindow(windowName, data);
            }
        }

        private static void CreateHeatmapWindow(string windowName, double[,] data)
        {
            CloseWindow(windowName);

            var window = new Window
            {
                Title = $"WSharp Heatmap — {windowName}",
                Width = 700,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 25))
            };

            var wpfPlot = new WpfPlot();
            window.Content = wpfPlot;

           
            wpfPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#14141A");
            wpfPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#1A1A24");
            wpfPlot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#666666");
            wpfPlot.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#666666");

            
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            double[,] buffer = new double[rows, cols];
            Array.Copy(data, buffer, data.Length);

            var hmap = wpfPlot.Plot.Add.Heatmap(buffer);
            hmap.Colormap = new ScottPlot.Colormaps.Turbo();

            wpfPlot.Plot.Add.ColorBar(hmap);
            wpfPlot.Refresh();

            var win = new LivePlotWindow
            {
                Window = window,
                WpfPlot = wpfPlot,
                PlotType = "heatmap",
                HeatmapData = buffer,
                Heatmap = hmap
            };

            _windows[windowName] = win;
            window.Closed += (s, e) => { _windows.Remove(windowName); };
            window.Show();
        }

       
        public static void PlotRaster(string windowName, double[] times, double[] neuronIds)
        {
            if (_windows.TryGetValue(windowName, out var win) && win.PlotType == "raster" && win.Window.IsLoaded)
            {
                win.Window.Dispatcher.Invoke(() =>
                {
                    win.WpfPlot.Plot.Clear();

                    var scatter = win.WpfPlot.Plot.Add.Scatter(times, neuronIds);
                    scatter.Color = ScottPlot.Color.FromHex("#00FF00");
                    scatter.LineWidth = 0;
                    scatter.MarkerSize = 3;
                    scatter.MarkerShape = MarkerShape.FilledCircle;

                    win.WpfPlot.Plot.Axes.Bottom.Label.Text = "Time Step";
                    win.WpfPlot.Plot.Axes.Left.Label.Text   = "Neuron ID";
                    win.WpfPlot.Plot.Axes.AutoScale();
                    win.WpfPlot.Refresh();
                });
            }
            else
            {
                CreateRasterWindow(windowName, times, neuronIds);
            }
        }

        private static void CreateRasterWindow(string windowName, double[] times, double[] neuronIds)
        {
            CloseWindow(windowName);

            var window = new Window
            {
                Title = $"WSharp Raster — {windowName}",
                Width  = 1000,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 25))
            };

            var wpfPlot = new WpfPlot();
            window.Content = wpfPlot;

           
            wpfPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#14141A");
            wpfPlot.Plot.DataBackground.Color   = ScottPlot.Color.FromHex("#1A1A24");
            wpfPlot.Plot.Axes.Bottom.Label.ForeColor      = ScottPlot.Color.FromHex("#888888");
            wpfPlot.Plot.Axes.Left.Label.ForeColor        = ScottPlot.Color.FromHex("#888888");
            wpfPlot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Color.FromHex("#666666");
            wpfPlot.Plot.Axes.Left.TickLabelStyle.ForeColor   = ScottPlot.Color.FromHex("#666666");
            wpfPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#2A2A35");

            
            wpfPlot.Plot.Axes.Bottom.Label.Text = "Time Step";
            wpfPlot.Plot.Axes.Left.Label.Text   = "Neuron ID";

            
            var scatter = wpfPlot.Plot.Add.Scatter(times, neuronIds);
            scatter.Color       = ScottPlot.Color.FromHex("#00FF00");
            scatter.LineWidth   = 0;
            scatter.MarkerSize  = 3;
            scatter.MarkerShape = MarkerShape.FilledCircle;

            wpfPlot.Plot.Axes.AutoScale();
            wpfPlot.Refresh();

            var win = new LivePlotWindow
            {
                Window   = window,
                WpfPlot  = wpfPlot,
                PlotType = "raster"
            };

            _windows[windowName] = win;
            window.Closed += (s, e) => { _windows.Remove(windowName); };
            window.Show();
        }

       
        public static void SetTitle(string windowName, string title)
        {
            if (_windows.TryGetValue(windowName, out var win) && win.Window.IsLoaded)
            {
                win.Window.Dispatcher.Invoke(() =>
                {
                    win.Window.Title = title;
                    win.WpfPlot.Plot.Title(title);
                    win.WpfPlot.Refresh();
                });
            }
        }

        
        public static void CloseWindow(string name)
        {
            if (_windows.TryGetValue(name, out var win))
            {
                if (win.Window.IsLoaded)
                {
                    if (!win.Window.Dispatcher.CheckAccess())
                        win.Window.Dispatcher.Invoke(() => win.Window.Close());
                    else
                        win.Window.Close();
                }
                _windows.Remove(name);
            }
        }

        public static void CloseAll()
        {
            foreach (var kvp in new Dictionary<string, LivePlotWindow>(_windows))
            {
                if (kvp.Value.Window.IsLoaded)
                {
                    if (!kvp.Value.Window.Dispatcher.CheckAccess())
                        kvp.Value.Window.Dispatcher.Invoke(() => kvp.Value.Window.Close());
                    else
                        kvp.Value.Window.Close();
                }
            }
            _windows.Clear();
        }
    }

       internal class LivePlotWindow
    {
        public Window Window;
        public WpfPlot WpfPlot;
        public string PlotType; 

        
        public double[,] HeatmapData;
        public Heatmap Heatmap;
    }
}
