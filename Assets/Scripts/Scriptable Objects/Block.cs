using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Block", menuName = "ScriptableObjects/Blocks/Block")]
public class Block : ScriptableObject
{
    public string blockName;
    public int ID;
    public TileBase baseTile;
}
