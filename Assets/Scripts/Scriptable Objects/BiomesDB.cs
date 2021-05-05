using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BiomesDB", menuName = "ScriptableObjects/DB/BiomesDB")]
public class BiomesDB : ScriptableObject
{
    public Biome[] database;

    [ContextMenu("Update IDs")]
    void UpdateId()
    {
        for (int i = 0; i < database.Length; i++)
        {
            database[i].ID = i;
        }
    }
}
