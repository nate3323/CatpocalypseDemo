using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "New Wave Asset")]
public class Wave : ScriptableObject
{

    public List<CatSpawnInfo> cats;

}