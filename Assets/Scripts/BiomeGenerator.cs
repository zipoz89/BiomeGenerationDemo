using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BiomGenerationMode
{
    checker,
    distribution,
    premade,
}


public class BiomeGenerator
{
    int[,] biomeMap;
    bool[,] biomeAvailability;
    int xBound;
    int yBound;
    int xSamples;
    int ySamples;
    int tranzitionTreshhold;
    System.Random rng;
    public BiomeGenerator(int xBound,int yBound, System.Random rng, int xSamples, int ySamples, int tranzitionTreshhold)
    {
        this.xBound = xBound;
        this.yBound = yBound;
        this.rng = rng;
        this.xSamples = xSamples;
        this.ySamples = ySamples;
        this.tranzitionTreshhold = tranzitionTreshhold; ;
    }



    void resetMap() {
        this.biomeMap = new int[xBound, yBound];
        
    }

    public int[,] GenerateOneBiome()     //generate biomes
    {
        resetMap();
        for (int y = 0; y < biomeMap.GetLength(1); y++)
        {
            for (int x = 0; x < biomeMap.GetLength(0); x++)
            {
                biomeMap[x, y] = 1;

            }
        }
        return biomeMap;
    }
    public int[,] GenerateBiomsFromRarity(Biome[] toSpawn) {
        resetMap();
        this.biomeAvailability = new bool[xSamples, ySamples];
        int raritySum = 0;

        foreach (Biome n in toSpawn)
        {
            raritySum += n.rarity;
            //Debug.Log(n.biomaName+ " rarity: "+ n.rarity);
        }
        float biomeParcel = (float)(xSamples * ySamples) / raritySum;
        int[] biomeCount = new int[toSpawn.Length];


        int highestRarity = 0;
        int highestRarityI = 0;
        int sumOfReservedChunks = 0;
        for (int i = 0; i < biomeCount.Length; i++)
        {
            if (toSpawn[i].rarity > highestRarity)
            {
                highestRarity = toSpawn[i].rarity;
                highestRarityI = i;
            }
            biomeCount[i] = (int)(toSpawn[i].rarity * biomeParcel);
            sumOfReservedChunks += biomeCount[i];

        }
        biomeCount[highestRarityI] += ((xSamples * ySamples) - sumOfReservedChunks); //mozna by zrobic ranodomowo to

        List<int> biomeSpawnOrder = new List<int>();
        for (int i = 0; i < biomeCount.Length; i++)
        {
            for (int j = 0; j < biomeCount[i]; j++)
            {
                biomeSpawnOrder.Add(toSpawn[i].ID);
            }
        }

        SeedManager.Shuffle<int>(biomeSpawnOrder);

        //string s = "";
        //foreach (int t in biomeSpawnOrder)
        //    s += t.ToString();
        //Debug.Log(s);


        int[,] biomeCenterMap = ListToBiomeCenters(biomeSpawnOrder);


        int xSampleSizeInBlocks = xBound / xSamples;
        int ySampleSizeInBlocks = yBound / ySamples;

        Vector2Int[,] centers = new Vector2Int[xSamples,ySamples];

        Vector2Int randomizedPos;
        for (int y = 0; y < ySamples; y++)
        {
            for (int x = 0; x < xSamples; x++)
            {
                randomizedPos = new Vector2Int(
                    rng.Next(x* xSampleSizeInBlocks, x * xSampleSizeInBlocks+ xSampleSizeInBlocks),
                    rng.Next(y * ySampleSizeInBlocks, y * ySampleSizeInBlocks + ySampleSizeInBlocks));
                centers[x, y] = randomizedPos;
                //GameObject o = new GameObject();
                //o.transform.position = new Vector3(randomizedPos.x, randomizedPos.y, 0);
                //o.transform.name = biomeCenterMap[x, y].ToString();
            }
        }
        MapToVoronoiDiagram(centers, biomeCenterMap);
        //GenerateOneBiome();
        return biomeMap;
    }

    private int[,] ListToBiomeCenters(List<int> biomeSpawnOrder)
    {
        int[,] biomeCenterMap = new int[xSamples,ySamples];
        int i = 0;
        for (int y = 0; y < ySamples; y++)
        {
            for (int x = 0; x < xSamples; x++)
            {
                biomeCenterMap[x, y] = biomeSpawnOrder[i];
                i++;
            }
        }
        return biomeCenterMap;
    }

    private void MapToVoronoiDiagram(Vector2Int[,] centers, int[,] biomeCenterMap) {

        float maxPossibleDistance = Mathf.Sqrt((xBound / xSamples)* (xBound / xSamples) + (yBound / ySamples)* (yBound / ySamples));

        for (int y = 0; y < yBound; y++)
        {
            for (int x = 0; x < xBound; x++)
            {
                int xSampleCord = x/ (xBound / xSamples);
                int ySampleCord = y / (yBound / ySamples);

                int closestBiomeId = 0;
                float closestBiomeDst = float.MaxValue;
                for (int yt = ySampleCord - 1; yt <= ySampleCord + 1; yt++){
                    for (int xt = xSampleCord - 1; xt <= xSampleCord + 1; xt++){
                        if (xt >= 0 && xt < xSamples && yt >= 0 && yt < ySamples) {
                            int x2 = centers[xt, yt].x;
                            int y2 = centers[xt, yt].y;
                            float dst = DstBtwnPoints(x, y, x2, y2);


                            if (dst - tranzitionTreshhold / 2 < closestBiomeDst && dst + tranzitionTreshhold / 2 > closestBiomeDst)
                            {
                                if (rng.Next(0, 10) < 3)
                                {
                                    closestBiomeDst = dst;
                                    closestBiomeId = biomeCenterMap[xt, yt];
                                }

                            }
                            if (dst <= closestBiomeDst) {
                                closestBiomeDst = dst;
                                closestBiomeId = biomeCenterMap[xt, yt];
                            }
                            
                        }
                    }
                }
                biomeMap[x, y] = closestBiomeId;


                //Debug.Log(xSampleCord + ";"+ ySampleCord);
            }
        }

    }

    private float DstBtwnPoints(int x1,int y1, int x2, int y2) {
        return Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }


}
