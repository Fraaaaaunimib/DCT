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
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace DCT;

public partial class MainWindow : Window
{

    private Bitmap? fileAperto = null;
    public MainWindow()
    {
        InitializeComponent();
        //this.Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        List<Risultato> lista = [];

        AvaPlot avaPlot1 = this.Find<AvaPlot>("MioGrafico");
        int[] dimensioni = {8, 16, 32, 64, 128, 256, 512, 1024};
        foreach(int n in dimensioni){
            lista.Add(Funzioni.calcoloDCT(n));
        }
        double[] yLog = lista.Select(r=>Math.Log10(Math.Max(r.tempo, 0.1))).ToArray();
        double[] yLogA = lista.Select(r=>Math.Log10(Math.Max(r.tempoA, 0.1))).ToArray();
        avaPlot1.Plot.XLabel("Dimensioni matrici");
        avaPlot1.Plot.YLabel("Tempo di esecuzione logaritmico");

        avaPlot1.Plot.Clear();

        var riga = avaPlot1.Plot.Add.Scatter(dimensioni, yLog);
        riga.Color = ScottPlot.Colors.Red;
        riga.LegendText="DCT2 personale";

        var rigaAccord = avaPlot1.Plot.Add.Scatter(dimensioni, yLogA);
        rigaAccord.Color = ScottPlot.Colors.Blue;
        rigaAccord.LegendText="DCT2 libreria Accord";
        avaPlot1.Plot.Axes.AutoScale();
        avaPlot1.Refresh();


    }
    
    private async void bottoneImmagine(object sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if(topLevel == null)
        {
            return;
        }
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Seleziona file",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Immagini bmp")
                {
                    Patterns = new[]
                    {
                        "*.bmp"
                    }
                }
            }
        });

        if (files.Count > 0){
            try {
            fileAperto = new Bitmap(await files[0].OpenReadAsync());
            immagineOriginale.Source = fileAperto;

            } catch (Exception ex)
            {
                testoMessaggi.Text = ex.ToString();

                DispatcherTimer.RunOnce(() =>
                {
                    testoMessaggi.Text = "Messaggi";
                }, TimeSpan.FromSeconds(3)
                );
            }

            double[,] matriceImmagine = Funzioni.convertiImmagineMatrice(fileAperto);
            matriceImmagine = Funzioni.convertiImmaginiBlocchi(matriceImmagine, (int)NumF.Value);
            Funzioni.stampaMatriceDebug(matriceImmagine);
        }
    }

    private void cambioValoreF(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (e.NewValue.HasValue)
        {
            NumD.Maximum = (decimal)((2*e.NewValue.Value)-2);
        }
    }

    
}