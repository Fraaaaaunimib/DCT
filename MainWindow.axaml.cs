using System.Collections.Generic;
using Avalonia.Controls;

namespace DCT;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        List<List<double>> matrice = DCT2.matrice(10);
        for(int i = 0; i<10; i++)
        {
            for(int j = 0; j< 10; j++)
            {
                test.Text += matrice[i][j] + " ";
            }
            test.Text += "\n";
        }
    }
}