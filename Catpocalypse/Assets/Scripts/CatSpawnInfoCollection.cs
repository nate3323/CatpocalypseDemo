using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// This class automatically loads in all CatSpawnInfo assets from the Resources folder,
/// which contain the data on each CatSpawn scriptable object.
/// </summary>
public class CatSpawnInfoCollection : MonoBehaviour
{
    public List<CatSpawnInfo> _CatSpawnInfoList = new List<CatSpawnInfo>();



    private void Awake()
    {
        // Load all of the TowerInfo scriptable objects in the project, even if they
        // aren't inside the TowerPopupsInfo folder.
        _CatSpawnInfoList = Resources.LoadAll<CatSpawnInfo>("").ToList();

        SyncCatSpawnInfosWithPrefabs();
    }

    /// <summary>
    /// This function copies necessary values from the cat prefabs, so the UI stays up-to-date even
    /// when the cat prefabs are modified.
    /// 
    /// This function is just like TowerInfoCollection.SyncTowerInfoWithPrefabs(), but not currently
    /// being used. I just kept this here in case it is needed later.
    /// 
    /// NOTE: We might want to change this later by making it so all info is pulled from the
    ///       CatInfo scriptable objects rather than having some stored on the prefabs.
    /// </summary>
    private void SyncCatSpawnInfosWithPrefabs()
    {
    }

    public CatSpawnInfo GetCatInfo(CatTypes catType)
    {
        for (int i = 0; i < _CatSpawnInfoList.Count; i++)
        {
            if (_CatSpawnInfoList[i].CatType == catType)
                return _CatSpawnInfoList[i];
        }

        return null;
    }

    public List<CatSpawnInfo> GetCatSpawnInfo()
    {
        return _CatSpawnInfoList;
    }



    public int Count { get { return _CatSpawnInfoList.Count; } }
}