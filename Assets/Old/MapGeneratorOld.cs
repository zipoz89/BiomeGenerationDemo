using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int boundX = 10;
    [SerializeField] private int boundY = 10;
    public string seed = "example";
    [SerializeField] private bool useSeed;
    [SerializeField] private int genIterations;
    [Range(0, 100)]
    public int randomFillPercent;

    int[,] map;

    [SerializeField] Tilemap tMap;
    [SerializeField] TileBase baseTail;

    System.Random rng;

    private void Start()
    {
        map = new int[boundX, boundY];
    }



    [ContextMenu("Genarete")] void generateTable() {
        if (useSeed)
            rng = new System.Random(seed.GetHashCode());
        else rng = new System.Random();
        for (int x = 0; x < boundX; x++)
        {
            for (int y = 0; y < boundY; y++)
            {
                if (x == 0 || x == boundX - 1 || y == 0 || y == boundY - 1)
                    map[x, y] = 1;
                else { 
                map[x, y] = (rng.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
        for (int i = 0; i < genIterations; i++)
        {
            SmoothMap();
        }
        drawTilemap();
    }


    void SmoothMap() {

        for (int x = 0; x < boundX; x++)
        {
            for (int y = 0; y < boundY; y++){
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX,int gridY) {
        int wallCount = 0;
        for (int y = gridY - 1; y <= gridY + 1; y++)
        {
            for (int x = gridX - 1; x <= gridX+1; x++){
                if (x >= 0 && x < boundX && y >= 0 && y < boundY)
                    if (x != gridX || y != gridY)
                        if (map[x, y] != 0) wallCount++;

            }
        }
        return wallCount;
    }

    [ContextMenu("Draw")] void drawTilemap()
    {
        tMap.ClearAllTiles();
        if (map!=null)
        for (int x = 0; x < boundX; x++)
        {
            for (int y = 0; y < boundY; y++)
            {
                    if (map[x, y] >0)
                    {
                        tMap.SetTile(new Vector3Int(x, y, 0), baseTail);
                        tMap.RefreshTile(new Vector3Int(x, y, 0));
                    }
                }
        }
        tMap.RefreshAllTiles();
    }
}
