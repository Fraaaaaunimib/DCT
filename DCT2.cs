using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Diagnostics;
using Accord.Math;
using Avalonia.Media.Imaging;
using Avalonia;
using System.Runtime.InteropServices;

namespace DCT
{
public class DCT2
{
    public static List<List<double>> matrice(int n)
    {
        List<List<double>> matrice = new(n);
        Random random = new();
        
        for(int i = 0; i<n; i++)
        {
           matrice.Add([.. new double[n]]);
        }

        for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrice[i][j] = random.NextDouble() * 255.0;
                }
            }
        return matrice;
    }

    public static List<List<double>> D_matrix(int n)
        {
            List<List<double>> D = new(n);
            double dK = 0.0;
            for(int i = 0; i<n; i++)
            {
                D.Add([.. new double[n]]);
            }

            for(int i = 0; i < n; i++)
            {
                if(i == 0)
                {
                    dK = 1/Math.Sqrt(n);
                }
                else
                {
                    dK = Math.Sqrt(2.0/n);
                }

                for(int j = 0; j < n; j++)
                {
                    D[i][j] = dK*Math.Cos(i*Math.PI*((2.0*j + 1.0) / (2.0*n)));
                }
            }
            return D;

        }
    public static List<double> cVector(List<List<double>> D, List<double> f)
        {
            int n = f.Count;
            List<double> C = [.. new double[n]];
            for(int i = 0; i< n; i++)
            {
                for(int j = 0; j < n; j++)
                {
                    C[i] += D[i][j] * f[j];
                }
            }
            return C;
        }

    public static List<List<double>> dct(List<List<double>> matrice)
        {
            List<List<double>> d = D_matrix(matrice.Count);

            List<List<double>> temp = new List<List<double>>();
            List<List<double>> risultato = new List<List<double>>();

            for(int i = 0; i<matrice.Count; i++)
            {
                temp.Add([.. new double[matrice.Count]]);
                risultato.Add([.. new double[matrice.Count]]);
            }
        
            for(int i = 0; i<matrice.Count; i++)
            {
                temp[i] = cVector(d, matrice[i]);
            }

            for(int j = 0; j<matrice.Count; j++)
            {
                List<double> colonna = [];
                for(int i = 0; i<matrice.Count; i++)
                {
                    colonna.Add(temp[i][j]);
                }

                List<double> colonnaTrasformata = cVector(d, colonna);

                for(int i = 0; i<matrice.Count; i++)
                {
                    risultato[i][j] = colonnaTrasformata[i];
                }
            }
            return risultato;
        }



}

public class Funzioni
    {
        public static double[,] convertitore(List<List<double>> list)
        {
            double[,] risultato = new double[list.Count, list[0].Count];
            for(int i = 0; i < list.Count; i++)
            {
                for(int j = 0; j < list[0].Count; j++)
                {
                    risultato[i, j] = list[i][j];
                }
            }
            return risultato;
        }

        public static List<List<double>> convertitore(double[,] matrice)
        {
            
            List<List<double>> risultato = new List<List<double>>(matrice.GetLength(0));
            for(int i = 0; i < matrice.GetLength(0); i++)
            {
                List<double> rigaCorrente = new List<double>(matrice.GetLength(1));
                for(int j = 0; j < matrice.GetLength(1); j++)
                {
                    rigaCorrente.Add(matrice[i, j]);
                }
                risultato.Add(rigaCorrente);
            }
            return risultato;
        }

        public static Risultato calcoloDCT(int n)
        {
            List<List<double>> matrice = DCT2.matrice(n);
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

            return new Risultato(n, tempo, tempoA);
        }

        public static double[,] convertiImmagineMatrice(Bitmap immagine)
        {
            int larghezza = immagine.PixelSize.Width;
            int altezza = immagine.PixelSize.Height;

            double[,] matrice = new double[altezza, larghezza];
            int stride = 4*larghezza;
            unsafe {
                byte* arrayImmagine = (byte*)NativeMemory.Alloc((nuint)(altezza*stride));
                immagine.CopyPixels(new PixelRect(0, 0, larghezza, altezza), (nint)arrayImmagine, altezza*stride, stride);

                for(int i = 0; i < altezza; i++)
                {
                    for(int j = 0; j < larghezza; j++)
                    {
                        matrice[i,j]=(double)(arrayImmagine[(i*stride) + (j*4)]);
                        Console.WriteLine(" ");
                    }
                    Console.WriteLine("\n");
                }
                NativeMemory.Free(arrayImmagine);
            }
            return matrice; 

        }

        public static double[,] convertiImmaginiBlocchi(double[,] matrice, int F)
        {
            double[,] risultato = new double[matrice.GetLength(0), matrice.GetLength(1)];
            double[,] bloccoF = new double[F,F];

            for(int i = 0; i +F <=matrice.GetLength(0); i+=F)
            {
                for(int j = 0; j+F <= matrice.GetLength(1); j+=F)
                {
                    for(int k = 0; k < F; k++)
                    {
                        for(int l = 0; l < F; l++)
                        {
                            bloccoF[k,l] = matrice[k+i, l+j];
                        }
                    }
                    List<List<double>> conversione = convertitore(bloccoF);
                    conversione = DCT2.dct(conversione); 
                    bloccoF = convertitore(conversione);
                    for (int k = 0; k < F; k++)
                    {
                        for (int l = 0; l < F; l++)
                        {
                            risultato[k+i,l+j]=bloccoF[k,l];
                        }
                    }
                }
            }
            return risultato;
            
        }

        public static void stampaMatriceDebug(double[,] matrice)
        {
            for (int i = 0; i < matrice.GetLength(0); i++)
            {
                for (int j = 0; j < matrice.GetLength(1); j++)
                {
                    Debug.Write(matrice[i,j] + " ");
                }
                Debug.Write("\n");
            }
        }
    }



public class Risultato(int N, double tempo, double tempoA)
    {
        public int N { get; set; } = N;
        public double tempo { get; set; } = tempo;
        public double tempoA { get; set; } = tempoA;


    }
}
