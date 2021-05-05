using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Collections;


[Serializable]
//struct Biome
//{
//    public int ID;
//    public string biomeName;
//    public int baseBlockID;
//}

public class MapManager : MonoBehaviour
{
    //-------------------<   SET IN EDITOR        >-------------------
    [SerializeField] private int chunkCountX;
    [SerializeField] private int chunkCountY;
    [SerializeField] private int genIterations;
    [Range(0, 100)] [SerializeField] int randomFillPercent;
    public String seed;
    public bool useSeed;


    [SerializeField] int xSamples;
    [SerializeField] int ySamples;
    [SerializeField] int tranzitionTreshhold;
    [Range(1, 16)] [SerializeField] int chunksToLoadAround=1;

    //-------------------<   GET FROM OUTSIDE     >-------------------
    [SerializeField] TileBase tempTile;
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] BlocksDB blockDB;
    [SerializeField] BiomesDB biomeDB;
    [SerializeField] GameObject objectToFollow;

    //-------------------<   PRIVATE              >-------------------
    Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    int[,] map;
    int[,] biomeMap;

    //-------------------<   STATIC               >-------------------
    public static int xChunkSize = 64;
    public static int yChunkSize = 64;
    Block[] blocksDatabase;
    Biome[] biomesDatabase;
    System.Random rng;
    int xBound;
    int yBound;
    BiomeGenerator biomeGenerator;

    private void SetVariables()
    {
        blocksDatabase = blockDB.database;
        biomesDatabase = biomeDB.database;
        xBound = xChunkSize * chunkCountX;
        yBound = yChunkSize * chunkCountY;

        if (useSeed) {
            SeedManager.setSeed(seed);
        }
        else SeedManager.setSeed();
        rng = SeedManager.rng;
        biomeGenerator = new BiomeGenerator(xBound, yBound, rng, xSamples, ySamples, tranzitionTreshhold);
    }
    private void Start()
    {
        SetVariables();
        CreateChunks();
        GenerateBiomeMap();
        GenerateMapShape();
        
        DrawStartChunks();
    }

   
    private void Update()
    {
        checkChunkChange();
    }

    Vector2Int previuChunk = new Vector2Int(-1, -1);
    int previusBiome = 0;
    private void checkChunkChange() {
        //Vector2Int currentChunk = new Vector2Int((int)(objectToFollow.transform.position.x / xChunkSize), (int)(objectToFollow.transform.position.y / yChunkSize));
        //if (previuChunk != currentChunk)
        //{
        //    DrawNewChunks(currentChunk);
        //    previuChunk = currentChunk;
            

        //}
        //if (biomeMap[(int)objectToFollow.transform.position.x, (int)objectToFollow.transform.position.y] != previusBiome) {

        //    StartCoroutine(ColorTransition(objectToFollow.GetComponent<Camera>().backgroundColor));
        //    previusBiome = biomeMap[(int)objectToFollow.transform.position.x, (int)objectToFollow.transform.position.y];

        //}
    }

    IEnumerator ColorTransition(Color newColor) {

        float ElapsedTime = 0.0f;
        float TotalTime = 0.4f;
        while (ElapsedTime < TotalTime) { 
            ElapsedTime += Time.deltaTime;
            objectToFollow.GetComponent<Camera>().backgroundColor = Color.Lerp( newColor, biomesDatabase[previusBiome].backgroundColor,(ElapsedTime / TotalTime));
            yield return null;
        }
        yield return new WaitForSeconds(1f);
    }

    private void DrawNewChunks(Vector2Int currentChunk)
    {
        
    }

    private void GenerateBiomeMap()
    {
        Biome[] toSpawn = { biomesDatabase[0], biomesDatabase[1], biomesDatabase[2], biomesDatabase[3] };
        biomeMap = biomeGenerator.GenerateBiomsFromRarity(toSpawn);
    }




    private void GenerateMapShape()     //Generates caves
    {

        map = new int[xBound, yBound];
        for (int x = 0; x < xBound; x++)
        {
            for (int y = 0; y < yBound; y++)
            {
                if (x == 0 || x == xBound - 1 || y == 0 || y == yBound - 1)
                    map[x, y] = 2;
                else
                {
                    map[x, y] = (rng.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
       
        for (int i = 0; i < genIterations; i++)
        {
            SmoothMap();
        }
        foreach (KeyValuePair<Vector2Int, Chunk> entry in chunks)
        {
            TranslateMapToChunk(entry.Value);
        }
    }

    void FlipMapRight() { 
        
    }

    void customMapStartValues()
    {
        for (int y = 0; y < yBound; y++)
        {
            for (int x = 0; x < xBound; x++)
            {
                if(x==xBound/2 || y == yBound / 2)
                {
                    map[x, y] = rng.Next(1, 3);
                }
            }
        }
    }


    void SmoothMap()        // this makes cave shape
    {

        for (int x = xBound-1; x >0 ; --x) 
        {
            for (int y = 0; y < yBound; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)   //get non air blocks around block
    {
        int wallCount = 0;
        for (int y = gridY - 1; y <= gridY + 1; y++)
        {
            for (int x = gridX - 1; x <= gridX + 1; x++)
            {
                if (x >= 0 && x < xBound && y >= 0 && y < yBound)
                    if (x != gridX || y != gridY)
                        if (map[x, y] != 0) wallCount+= map[x, y];

            }
        }
        return wallCount;
    }

    private void TranslateMapToChunk(Chunk chunk)       // sets chunk data to be the same as map data
    {
        int xStart = chunk.xPos * xChunkSize;
        int yStart = chunk.yPos * yChunkSize;


        for (int y = 0; y < yChunkSize; y++)
        {
            for (int x = 0; x < xChunkSize; x++)
            {
                chunk.data[x, y] = map[x + xStart, y + yStart];
            }
        }
    }

    

    private void DrawStartChunks()      //draw chunks on startup
    {
        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                DrawChunk(new Vector2Int(x, y));
            }
        }
    }

    private int DrawChunk(Vector2Int pos)   //draw specified chunk
    {
        Chunk chunk;
        chunks.TryGetValue(pos, out chunk);
        if (chunk == null) return 1;
        Tilemap tilemap = chunk.tilemap;
        tilemap.ClearAllTiles();
        for (int y = 0; y < yChunkSize; y++)
        {
            for (int x = 0; x < xChunkSize; x++)
            {
                if (chunk.data[x, y] > 0)
                {
                    //tilemap.SetTile(new Vector3Int(x, y, 0),tempTile);
                    tilemap.SetTile(new Vector3Int(x, y, 0), biomesDatabase[GetBiomeOnPos(x, y, pos)].baseBlock.baseTile);
                }


            }
        }
        return 0;
    }

    private int GetBiomeOnPos(int x, int y, Vector2Int pos)
    {
        int xStart = pos.x * xChunkSize;
        int yStart = pos.y * yChunkSize;
        return biomeMap[x + xStart, y + yStart];
    }

    private void CreateChunks()     //spawn chunk object with tilemap
    {
        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                GameObject o = Instantiate(chunkPrefab, new Vector3(x * xChunkSize, y * yChunkSize, 0), Quaternion.identity);
                o.transform.parent = this.transform;
                Chunk c = o.GetComponent<Chunk>();
                c.xPos = x;
                c.yPos = y;
                c.data = new int[xChunkSize, yChunkSize];
                c.tilemap = o.GetComponent<Tilemap>();
                o.transform.position = new Vector3(x * xChunkSize, y * yChunkSize, 0);
                chunks.Add(new Vector2Int(x, y), c);
            }
        }
    }
}

