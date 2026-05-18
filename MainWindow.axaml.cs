using System.Collections.Generic;
using Avalonia.Controls;
using Accord.Math;
using System.Diagnostics;
using System;

namespace DCT;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        List<List<double>> matrice = DCT2.matrice(10);
        Stopwatch cronometro = Stopwatch.StartNew();
        List<List<double>> matrice2 = DCT2.dct(matrice);
        cronometro.Stop();
        double tempo = cronometro.Elapsed.TotalMilliseconds;
        
        double[,] matriceAccord = Funzioni.convertitore(matrice);

        cronometro = Stopwatch.StartNew();
        CosineTransform.DCT(matriceAccord);
        cronometro.Stop();
        double tempoA = cronometro.Elapsed.TotalMilliseconds;

        Console.WriteLine($"tempo: {tempo}   tempoA: {tempoA}" );        

/*
        for(int i = 0; i<10; i++)
        {
            for(int j = 0; j< 10; j++)
            {
                test.Text += matrice[i][j] + " ";
                test2.Text += matrice2[i][j] + " ";
                test3.Text += matriceAccord[i,j] + " ";
            }
            test.Text += "\n";
            test2.Text += "\n";
            test3.Text += "\n";
            
        }*/

        
        MioGrafico.Plot.Clear();

        


    }
}