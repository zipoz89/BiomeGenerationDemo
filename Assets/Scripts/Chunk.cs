using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    public int xPos;
    public int yPos;
    public int[,] data;
    public Tilemap tilemap;
}
