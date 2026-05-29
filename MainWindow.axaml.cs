using System.Collections.Generic;
using Avalonia.Controls;
//using Accord.Math;
//using System.Diagnostics;
using System;
//using System.Linq;
//using Accord;
//using System.Drawing;
//using ScottPlot;
//using Accord.IO;
//using ScottPlot.Avalonia;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Threading.Tasks;
using System.Threading;
//using System.IO;

namespace DCT;

public partial class MainWindow : Window  
{
    private CancellationTokenSource? _cts = null;

    private Bitmap? fileAperto = null;

    private bool? DCT = false;

    private bool isSyncScroll = false;
    public MainWindow()
    {
        InitializeComponent();
        _bottoneStop.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.No);
        NumF.IsEnabled = false;
        NumD.IsEnabled = false;

        this.AddHandler(Avalonia.Input.Gestures.PointerTouchPadGestureMagnifyEvent, Immagini_Zoom, Avalonia.Interactivity.RoutingStrategies.Bubble, true);
       // this.Loaded += MainWindow_Loaded; 
        //Per il primo punto del pdf fa partire il confronte della dct fatta in casa con quello della libreria Accord
    }

    
   /* private async void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
    */
    
    //Disabilita gli elementi dell'interfaccia utente, quando la DCT è in esecuzione
    private async void disabilitaUI()
    {
        _bottoneImmagine.IsEnabled = false;
        NumF.IsEnabled = false;
        NumD.IsEnabled = false;
    }
    //Abilita gli elementi dell'interfaccia utente, quando la DCT non è in esecuzione
    private async void abilitaUI()
    {
        _bottoneImmagine.IsEnabled = true;
        NumF.IsEnabled = true;
        NumD.IsEnabled = true;
    }

    //Bottone di inizio/interruzione della DCT
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
        ToolTip.SetTip(_bottoneStop, "Inizia la DCT (CTRL+D)");  
        } else if (fileAperto != null)
        {
            DCT = true;
            valoriDCT();
    }
    }

    //Scelta modalità di visualizzazione dell'immagine
    //Fill: mantiene la proporzione originale dell'immagine
    //Stretch: riempe il contenitore con l'immagine
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

    //Selezione un'immagine dal disco di tipo .bmp
    private async void bottoneImmagine(object sender, RoutedEventArgs e)
    {
        caricaImmagine();
    }

    private async void caricaImmagine()
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
                if(fileAperto != null)
                {
                    if(immagineDCT.Source is IDisposable DCTBitmap)
                    {
                        DCTBitmap.Dispose();
                    }
                    if(immagineOriginale.Source is IDisposable OriginalBitmap)
                    {
                        OriginalBitmap.Dispose();
                    }
                    immagineOriginale.Source = null;
                    immagineDCT.Source = null;
                    fileAperto.Dispose();
                    fileAperto = null;
                }
            fileAperto = new Bitmap(await files[0].OpenReadAsync());

            NumF.Maximum = (decimal)Math.Min(fileAperto.Size.Width, fileAperto.Size.Height);
            immagineOriginale.Source = fileAperto;
            this.Title = "DCT - " + files[0].Path;

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


    //Esecuzione DCT
    private async void valoriDCT()
    {
        disabilitaUI();
        if (fileAperto == null) return;
        _bottoneStop.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
        iconaBottone.Data = (Avalonia.Media.Geometry)this.FindResource("IconStop");
        ToolTip.SetTip(_bottoneStop, "Ferma la DCT");

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

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        if(immagineDCT.Source is IDisposable vecchiaBitmap)
        {
            vecchiaBitmap.Dispose();
            immagineDCT.Source = null;
        }
        immagineDCT.Source = Funzioni.conversioneMatriceBitmap(matriceImmagine);
        iconaBottone.Data = (Avalonia.Media.Geometry)this.FindResource("IconPlay");
        ToolTip.SetTip(_bottoneStop, "Inizia la DCT (CTRL+D)");
        NumF.Maximum = (decimal)immagineDCT.Source.Size.Width;
        abilitaUI();
    }

    //Cambio valore di F e d
    private void cambioValoreF(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if(sender is NumericUpDown valore)
        {
            if (e.NewValue.HasValue){
                if(valore == NumF)
                    {
                        NumD.Maximum =(2*e.NewValue.Value)-2;
                    }           

                if(NumF.Value > NumF.Maximum)
                    {
                        NumF.Value = NumF.Maximum;
                    }

                if (NumD.Value > NumD.Maximum)
                    {
                        NumD.Value = NumD.Maximum;
                    }
        }
        else if (!e.NewValue.HasValue)
            {
                valore.Value = 1;
            }
        }
        
    }

    private void Immagini_Zoom(object sender, Avalonia.Input.PointerDeltaEventArgs e)
{
        double vecchioZoom = ZoomSlider.Value;
        double zoomFactor = 1.0 + (e.Delta.Y * 0.5); 
        double nuovoZoom = Math.Clamp(vecchioZoom * zoomFactor, ZoomSlider.Minimum, ZoomSlider.Maximum);

        if (vecchioZoom == nuovoZoom) return;

        ScrollViewer activeScroll = scrollDCT.IsPointerOver ? scrollDCT : scrollOriginale;
        Avalonia.Point posView = e.GetPosition(activeScroll);

        ZoomSlider.Value = nuovoZoom;

        e.Handled = true;
}

    private void ScrollOriginale_posizione(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (isSyncScroll) return;
        isSyncScroll = true;
        scrollDCT.Offset = scrollOriginale.Offset;
        isSyncScroll = false;
    }

    private void ScrollDCT_posizione(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (isSyncScroll) return;
        isSyncScroll = true;
        scrollOriginale.Offset = scrollDCT.Offset;
        isSyncScroll = false;
    }

    //Test matrice e prima riga della matrice come richiesto dal pdf
    private async void calcoloTest(object sender, RoutedEventArgs e)
    {
        List<List<double>> matrice = [[231, 32, 233, 161, 24, 71, 140, 245], [247, 40, 248, 245, 124, 204, 36, 107],
        [234, 202, 245, 167, 9, 217, 239, 173], [193, 190, 100, 167, 43, 180, 8, 70], [11, 24, 210, 177, 81, 243, 8, 112],
        [97, 195, 203, 47, 125, 114, 165, 181], [193, 70, 174, 167, 41, 30, 127, 245], [87, 149, 57, 192, 65, 129, 178, 228]];
        List<List<double>> risultato = DCT2.dct(matrice);
        Funzioni.stampaMatriceDebug(Funzioni.convertitore(risultato));

        Console.WriteLine("\n");
        List<double> array = [231, 32, 233, 161, 24, 71, 140, 245];
        matrice = DCT2.D_matrix(8);

        List<double> vector = DCT2.cVector(matrice, array);
        foreach (double valore in vector)
        {
            Console.Write($"{valore:e2} ");
        }
        Console.WriteLine();
    }
    
}
