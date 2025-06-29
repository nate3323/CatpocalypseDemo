using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


/// <summary>
/// This class automatically loads in all TowerInfo assets from the Resources folder,
/// which contain the data on each tower. It also auto-updates certain fields like cost,
/// from the prefabs.
/// </summary>
public class TowerInfoCollection : MonoBehaviour
{
    [SerializeField]
    private GameObject laserPointerTowerPrefab;
    
    [SerializeField]
    private GameObject scratchingPostTowerPrefab;

    [SerializeField]
    private GameObject cucumberTowerPrefab;

    [SerializeField]
    private GameObject stringWaverTowerPrefab;

    [SerializeField]
    private GameObject yarnBallTowerPrefab;

    [SerializeField]
    private GameObject naTowerPrefab;


    private List<TowerInfo> _TowerInfoList = new List<TowerInfo>();
    


    private void Awake()
    {
        // Load all of the TowerInfo scriptable objects in the project, even if they
        // aren't inside the TowerPopupsInfo folder.
        _TowerInfoList = Resources.LoadAll<TowerInfo>("").ToList();

        SyncTowerInfosWithPrefabs();
    }

    /// <summary>
    /// This function copies necessary values from the tower prefabs, so the UI stays up-to-date even
    /// when the tower prefabs are modified.
    /// 
    /// NOTE: We might want to change this later so all info is pulled from the
    ///       TowerInfo scriptable objects rather than having some stored on the prefabs.
    /// </summary>
    private void SyncTowerInfosWithPrefabs()
    {
        TowerInfo laserTowerInfo = GetTowerInfo(TowerTypes.LaserPointer);
        Tower laserPointerTower = laserPointerTowerPrefab.GetComponent<Tower>();
        laserTowerInfo.Cost = laserPointerTower.BuildCost;       

        TowerInfo scratchingPostTowerInfo = GetTowerInfo(TowerTypes.ScratchingPost);
        Tower scratchingPostTower = scratchingPostTowerPrefab.GetComponent<Tower>();
        scratchingPostTowerInfo.Cost = scratchingPostTower.towerStats.BuildCost;

        TowerInfo cucumberTowerInfo = GetTowerInfo(TowerTypes.CucumberThrower);
        Tower cucumberTower = cucumberTowerPrefab.GetComponent<Tower>();
        cucumberTowerInfo.Cost = cucumberTower.towerStats.BuildCost;

        TowerInfo stringWaverTowerInfo = GetTowerInfo(TowerTypes.StringWaver);
        Tower stringWaverTower = stringWaverTowerPrefab.GetComponent<Tower>();
        stringWaverTowerInfo.Cost = stringWaverTower.towerStats.BuildCost;

        TowerInfo yarnBallTowerInfo = GetTowerInfo(TowerTypes.YarnBall);
        Tower yarnBallTower = yarnBallTowerPrefab.GetComponent<Tower>();
        yarnBallTowerInfo.Cost = yarnBallTower.towerStats.BuildCost;

        TowerInfo NATowerInfo = GetTowerInfo(TowerTypes.NonAllergic);
        Tower NATower = naTowerPrefab.GetComponent<Tower>();
        NATowerInfo.Cost = NATower.towerStats.BuildCost;
    }

    public TowerInfo GetTowerInfo(TowerTypes towerType)
    {
        for (int i = 0; i < _TowerInfoList.Count; i++)
        {
            if (_TowerInfoList[i].TowerType == towerType)
                return _TowerInfoList[i];
        }

        return null;
    }

    public TowerInfo GetTowerInfo(int index)
    {
        return _TowerInfoList[index];
    }



    public int Count { get { return _TowerInfoList.Count; } }
}
