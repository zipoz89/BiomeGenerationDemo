using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SeedManager
{
    public static System.Random rng;

    public static void setSeed(string seed)
    {
        rng = new System.Random(seed.GetHashCode());

    }

    public static void setSeed()
    {
        rng = new System.Random();
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
