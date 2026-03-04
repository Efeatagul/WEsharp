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
#nullable disable
using System;
using System.Collections.Generic;

namespace WSharp
{
    
    public class PlotLiveFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string type = arguments[1].AsString().ToLower();

            if (type == "heatmap")
            {
               
                var outerList = arguments[0].AsList();
                int rows = outerList.Count;
                if (rows == 0) throw new Exception("Heatmap icin bos olmayan 2D matris gerekli.");

              
                var firstRow = new WValue(outerList[0]).AsList();
                int cols = firstRow.Count;

                double[,] matrix = new double[rows, cols];
                for (int r = 0; r < rows; r++)
                {
                    var row = new WValue(outerList[r]).AsList();
                    for (int c = 0; c < row.Count && c < cols; c++)
                    {
                        matrix[r, c] = Convert.ToDouble(row[c]);
                    }
                }

                LivePlotEngine.PlotHeatmap("Heatmap", matrix);
            }
            else
            {
                var list = arguments[0].AsList();
                double[] data = new double[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    data[i] = Convert.ToDouble(list[i]);
                }

                LivePlotEngine.PlotLine("Signal", data);
            }

            
            System.Threading.Thread.Sleep(16); 

            return new WValue(null);
        }
        public override string ToString() => "<native fn plot_live>";
    }

    
    public class PlotLiveNamedFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string windowName = arguments[0].AsString();
            string type = arguments[2].AsString().ToLower();

            if (type == "heatmap")
            {
                var outerList = arguments[1].AsList();
                int rows = outerList.Count;
                if (rows == 0) throw new Exception("Heatmap icin bos olmayan 2D matris gerekli.");

                var firstRow = new WValue(outerList[0]).AsList();
                int cols = firstRow.Count;

                double[,] matrix = new double[rows, cols];
                for (int r = 0; r < rows; r++)
                {
                    var row = new WValue(outerList[r]).AsList();
                    for (int c = 0; c < row.Count && c < cols; c++)
                    {
                        matrix[r, c] = Convert.ToDouble(row[c]);
                    }
                }

                LivePlotEngine.PlotHeatmap(windowName, matrix);
            }
            else
            {
                var list = arguments[1].AsList();
                double[] data = new double[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    data[i] = Convert.ToDouble(list[i]);
                }

                LivePlotEngine.PlotLine(windowName, data);
            }

            System.Threading.Thread.Sleep(16);
            return new WValue(null);
        }
        public override string ToString() => "<native fn plot_live_named>";
    }

   
    public class PlotTitleFunc : IWCallable
    {
        public int Arity() => 2;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string windowName = arguments[0].AsString();
            string title = arguments[1].AsString();
            LivePlotEngine.SetTitle(windowName, title);
            return new WValue(null);
        }
        public override string ToString() => "<native fn plot_title>";
    }

    
    public class PlotCloseFunc : IWCallable
    {
        public int Arity() => 0;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            LivePlotEngine.CloseAll();
            return new WValue(null);
        }
        public override string ToString() => "<native fn plot_close>";
    }

    
    public class PlotRasterFunc : IWCallable
    {
        public int Arity() => 3;
        public WValue Call(Interpreter interpreter, List<WValue> arguments)
        {
            string windowName = arguments[0].AsString();

            var timesList = arguments[1].AsList();
            var idsList   = arguments[2].AsList();

            double[] times = new double[timesList.Count];
            double[] ids   = new double[idsList.Count];

            for (int i = 0; i < timesList.Count; i++)
                times[i] = Convert.ToDouble(timesList[i]);
            for (int i = 0; i < idsList.Count; i++)
                ids[i] = Convert.ToDouble(idsList[i]);

            LivePlotEngine.PlotRaster(windowName, times, ids);

            System.Threading.Thread.Sleep(16);
            return new WValue(null);
        }
        public override string ToString() => "<native fn plot_raster>";
    }
}
