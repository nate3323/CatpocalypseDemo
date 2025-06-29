using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "NewCatSpawnInfo", menuName = "New Cat Spawn Info Asset")]
public class CatSpawnInfo : ScriptableObject
{
    [Tooltip("Set what type of Cat to spawn. Multiple of the same CatType can be used in a wave.")]
    public CatTypes CatType;

    [Tooltip("Set the amount of this cat to spawn")]
    public int NumberToSpawn;

}