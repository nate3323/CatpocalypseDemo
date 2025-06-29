using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// This class automatically loads in all CatInfo assets from the Resources folder,
/// which contain the data on each cat.
/// </summary>
public class CatInfoCollection : MonoBehaviour
{
    private List<CatInfo> _CatInfoList = new List<CatInfo>();
    


    private void Awake()
    {
        // Load all of the TowerInfo scriptable objects in the project, even if they
        // aren't inside the TowerPopupsInfo folder.
        _CatInfoList = Resources.LoadAll<CatInfo>("").ToList();

        SyncCatInfosWithPrefabs();
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
    private void SyncCatInfosWithPrefabs()
    {
    }

    public CatInfo GetCatInfo(CatTypes catType)
    {
        for (int i = 0; i < _CatInfoList.Count; i++)
        {
            if (_CatInfoList[i].CatType == catType)
                return _CatInfoList[i];
        }

        return null;
    }

    public CatInfo GetCatInfo(int index)
    {
        return _CatInfoList[index];
    }



    public int Count { get { return _CatInfoList.Count; } }
}
