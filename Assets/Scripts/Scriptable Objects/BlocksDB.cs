using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BlockDB", menuName = "ScriptableObjects/DB/BlockDB")]
public class BlocksDB : ScriptableObject
{
    public Block[] database;

    [ContextMenu("Update IDs")]
    void UpdateId() {
        for (int i = 0; i < database.Length; i++)
        {
            database[i].ID = i;
        }
    }
}
