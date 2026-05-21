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
using System.Threading.Tasks;
using System.Threading;

namespace DCT;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _cts = null;

    private Bitmap? fileAperto = null;

    private bool? DCT = false;
    public MainWindow()
    {
        InitializeComponent();
        _bottoneStop.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.No);
        NumF.IsEnabled = false;
        NumD.IsEnabled = false;
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
    
    private async void disabilitaUI()
    {
        _bottoneImmagine.IsEnabled = false;
        NumF.IsEnabled = false;
        NumD.IsEnabled = false;
    }

    private async void abilitaUI()
    {
        _bottoneImmagine.IsEnabled = true;
        NumF.IsEnabled = true;
        NumD.IsEnabled = true;
    }

    private async void bottoneStop(object sender, RoutedEventArgs e)
    {
        if (_cts == null && fileAperto == null) {
            testoMessaggi.Text = "Nessuna DCT in esecuzione al momento";
            return;
        } else if (_cts != null){
            _cts.Cancel();
            DCT = false;
            abilitaUI();
        iconaBottone.Data = (Avalonia.Media.Geometry)this.FindResource("IconPlay");
        } else if (fileAperto != null)
        {
            DCT = true;
            valoriDCT();
    }
    }

    private async void proporzioneImmagine(object sender, RoutedEventArgs e)
    {
        try {
        if(sender is RadioButton bottone)
        {
            if ((bool)bottone.IsChecked)
            {
                if(bottone.Content.ToString() == "Stretch")
                {
                    immagineOriginale.Stretch = Avalonia.Media.Stretch.Fill;
                    immagineDCT.Stretch = Avalonia.Media.Stretch.Fill;
                }
                else if(bottone.Content.ToString() == "Fill")
                {
                    immagineOriginale.Stretch = Avalonia.Media.Stretch.Uniform;
                    immagineDCT.Stretch = Avalonia.Media.Stretch.Uniform;
                }
            }
        }
        } catch
        {
            
        }
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
                    testoMessaggi.Text = "Le informazioni verranno visualizzate qui";
                }, TimeSpan.FromSeconds(10)
                );
            }
            valoriDCT();
        }
    }



    public async void valoriDCT()
    {
        disabilitaUI();
        if (fileAperto == null) return;
        _bottoneStop.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
        iconaBottone.Data = (Avalonia.Media.Geometry)this.FindResource("IconStop");

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        int F = (int)NumF.Value;
        int d = (int)NumD.Value;
        testoMessaggi.Text = "Processando immagine con DCT";
        double[,] matriceImmagine = Funzioni.convertiImmagineMatrice(fileAperto);
        try {
        await Task.Run(() =>
        {
            matriceImmagine = Funzioni.convertiImmaginiBlocchi(matriceImmagine, F, d, token);
        }, token);
        testoMessaggi.Text = "Finito";
        } catch
        {
            testoMessaggi.Text = "DCT interrotta";
        } finally
        {
            _cts.Dispose();
            _cts = null;
        }
        immagineDCT.Source = Funzioni.conversioneMatriceBitmap(matriceImmagine);
        iconaBottone.Data = (Avalonia.Media.Geometry)this.FindResource("IconPlay");
        NumF.Maximum = (decimal)immagineDCT.Source.Size.Width;
        abilitaUI();
    }

    private void cambioValoreF(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (e.NewValue.HasValue)
        {
            NumD.Maximum = (decimal)((2*e.NewValue.Value)-2);
        }
    }

    private async void nascondiPannello(object sender, RoutedEventArgs e)
    {
        var margine = boxBottone.Margin;
        boxBottone.Margin = new Avalonia.Thickness((margine.Left - boxBottone.Width)+150, margine.Top, margine.Right, margine.Bottom);
    }
    
}