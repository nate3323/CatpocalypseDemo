using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class CatSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("A list of Wave scriptable Objects that have a list of cats to spawn")]
    [SerializeField]
    private List<Wave> _Waves;

    [Tooltip("The time between cats to spawn")]
    [SerializeField]
    [Min(1f)]
    private float _TimeBetweenSpawns;

    [Header("Game Object References")]
    [SerializeField, Tooltip("One possible spawn point for cats")]
    private Transform _SpawnPoint1;

    [SerializeField] private GameObject _NormalCat;
    [SerializeField] private GameObject _HeavyCat;
    [SerializeField] private GameObject _LightCat;
    [SerializeField] private GameObject _NormalBoxCat;
    [SerializeField] private GameObject _HeavyBoxCat;
    [SerializeField] private PlayerCutenessManager _CutenessManager;

    private int _CurrentWave;
    private Coroutine _CurrentCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        _CurrentWave = 0;
    }

    public void StartNextWave()
    {       
         _CurrentCoroutine =  StartCoroutine(Spawner(_CurrentWave++));
    }
    public void StopSpawner()
    {
        if (_CurrentCoroutine != null)
        {
            StopCoroutine(_CurrentCoroutine);
        }
        
    }
    IEnumerator Spawner(int currentWave)
    {
        int currentCatType = 0;
        CatTypes type = _Waves[currentWave].cats[currentCatType].CatType;
        int catsOfCurrentType = _Waves[currentWave].cats[currentCatType].NumberToSpawn;
        int totalCats = CatsInCurrentWave();
        GameObject cat = null;
        for (int i = 0; i < totalCats; i++)
        {
            if( catsOfCurrentType == 0 ) {
                type = _Waves[currentWave].cats[currentCatType].CatType;
                catsOfCurrentType = _Waves[currentWave].cats[currentCatType].NumberToSpawn;
            }
            switch (type)
            {
                case CatTypes.Light:
                    cat = Instantiate(_LightCat, _SpawnPoint1);
                    break;
                case CatTypes.Normal:
                    cat = Instantiate(_NormalCat, _SpawnPoint1);
                    break;
                case CatTypes.Heavy:
                    cat = Instantiate(_HeavyCat, _SpawnPoint1);
                    break;
                case CatTypes.LightBox:
                    break;
                case CatTypes.NormalBox:
                    cat = Instantiate(_NormalBoxCat, _SpawnPoint1);
                    break;
                case CatTypes.HeavyBox:
                    cat = Instantiate(_HeavyBoxCat, _SpawnPoint1);
                    break;

            }
            if(_CutenessManager.CurrentCutenessChallenge == PlayerCutenessManager.CutenessChallenges.BuffCatType && //If the buff cats cuteness challenge is active
                cat.GetComponent<CatBase>()._catType == _CutenessManager._CatType)                                  //If the cat is the type that is getting buffed
            {
                //Increases the distraction threshold
                cat.GetComponent<CatBase>().DistractionThreshold *= _CutenessManager._cutenessChallengeCatBuffPercent;
            }
            _CutenessManager.AddCuteness(cat.GetComponent<CatBase>().Cuteness);
            catsOfCurrentType--;
            if(catsOfCurrentType == 0)
            {
                currentCatType++;
            }
            yield return new WaitForSeconds(_TimeBetweenSpawns);
        }

    }


    public int CatsInCurrentWave()
    {
        Wave current = _Waves[_CurrentWave - 1];
        int tally = 0;
        foreach(CatSpawnInfo cats in current.cats)
        {
            tally += cats.NumberToSpawn;
        }
        return tally;
    }

    public int NumberOfWaves { get { return _Waves.Count; } }
}
