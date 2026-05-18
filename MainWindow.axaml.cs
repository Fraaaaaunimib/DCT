using System.Collections.Generic;
using Avalonia.Controls;
using Accord.Math;
using System.Diagnostics;
using System;
using System.Linq;
using Accord;
using System.Drawing;
using ScottPlot;
using Accord.IO;
using ScottPlot.Avalonia;

namespace DCT;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        List<Risultato> lista = [];

        AvaPlot avaPlot1 = this.Find<AvaPlot>("MioGrafico");
        int[] dimensioni = {8, 16, 32, 64, 128, 256, 512, 1024};
        foreach(int n in dimensioni){
            lista.Add(Funzioni.calcoloDCT(n));
        }
        double[] yLog = lista.Select(r=>Math.Log10(Math.Max(r.tempo, 0.1))).ToArray();
        double[] yLogA = lista.Select(r=>Math.Log10(Math.Max(r.tempoA, 0.1))).ToArray();

        avaPlot1.Plot.Clear();

        var riga = avaPlot1.Plot.Add.Scatter(dimensioni, yLog);
        riga.Color = ScottPlot.Colors.Red;

        var rigaAccord = avaPlot1.Plot.Add.Scatter(dimensioni, yLogA);
        rigaAccord.Color = ScottPlot.Colors.Blue;

        avaPlot1.Plot.Axes.AutoScale();
        avaPlot1.Refresh();
    }
}