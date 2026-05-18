using System;
using System.Collections.Generic;

namespace DCT
{
public class DCT2
{
    public static List<List<double>> matrice(int n)
    {
        List<List<double>> matrice = new(n);
        Random random = new Random();
        
        for(int i = 0; i<n; i++)
        {
            matrice.Add(new List<double>());
            for(int j = 0; j<n; j++)
            {
                    matrice[i].Add(0.0);
            }
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
}
}