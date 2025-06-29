using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// This class tracks how many cats are left across all spawners.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public event EventHandler WaveEnded;
    public event EventHandler LevelCleared;

    public static WaveManager Instance;

    private int _TotalWavesInLevel;

    private List<CatSpawner> _CatSpawners;
    private int _TotalCatsInWave;
    private int _CatsRemainingInWave;
    
    private int _CatsDistracted;
    private int _CatsReachedGoal;

    private int _TotalCatsDistracted;
    private int _TotalCatsReachedGoal;

    private float _SecondsSinceLevelStart;
    private float _SecondsSinceWaveStart;

    private int _WaveNumber = 1;
    private bool _WaveInProgress = false;

    private PlayerMoneyManager _PlayerMoneyManager;
    private PlayerCutenessManager _cutenessManager;

    private bool _scrapRewarded = false;

    [SerializeField] AudioSource _waveSound;
    [SerializeField] AudioClip _startClip;
    [SerializeField] AudioClip _endClip;
    [SerializeField] private int _ScrapReward;

    private WayPointUtils _WayPointUtils;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is already a WaveManager in this scene. Self destructing!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _WayPointUtils = new WayPointUtils();
    }

    private void Start()
    {
        _PlayerMoneyManager = FindObjectOfType<PlayerMoneyManager>();
        _cutenessManager = GameObject.FindGameObjectWithTag("Goal").GetComponent<PlayerCutenessManager>();
        CatBase.OnCatDied += OnCatDied;
        CatBase.OnCatReachGoal += OnCatReachGoal;
        _CatSpawners = new List<CatSpawner>();
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("CatSpawnPoint");

        foreach(GameObject spawner in spawners)
        {
            CatSpawner catSpawner = spawner.GetComponent<CatSpawner>();
            _CatSpawners.Add(spawner.GetComponent<CatSpawner>());
            if(spawner.GetComponent<CatSpawner>().NumberOfWaves > _TotalWavesInLevel)
            {
                _TotalWavesInLevel = spawner.GetComponent<CatSpawner>().NumberOfWaves;
            }
        }
        HUD.HideWaveDisplay();
        _WayPointUtils.Init();
    }

    private void Update()
    {
        _SecondsSinceLevelStart += Time.deltaTime;

        if (_WaveInProgress)
            _SecondsSinceWaveStart += Time.deltaTime;


        if (!_WaveInProgress && WaveNumber == _TotalWavesInLevel)
        {
            LevelCleared?.Invoke(this, EventArgs.Empty);
            if (!_scrapRewarded)
            {
                PlayerDataManager.Instance.UpdateScrap(_ScrapReward);
                _scrapRewarded = true;
            }
            HUD.RevealVictory();
        }
    }

    public void StartNextWave()
    {
        // Don't try to start a wave if one is already in progress.
        if (IsWaveInProgress)
            return;

        if (_cutenessManager.CurrentCutenessChallenge != PlayerCutenessManager.CutenessChallenges.None)
        {
            _cutenessManager.CutenessChallenge();
        }
        _WaveInProgress = true;
        _waveSound.clip = _startClip;
        _waveSound.volume = PlayerDataManager.Instance.CurrentData._SFXVolume;
        _waveSound.Play();
        Debug.Log("Start wave sound played");


        FindAllSpawners();

        foreach (CatSpawner spawner in _CatSpawners)
        {
            spawner.StartNextWave();
        }

        CalculateTotalCatsInWave();

        _CatsRemainingInWave = _TotalCatsInWave;
        _CatsDistracted = 0;
        _CatsReachedGoal = 0;

        HUD.ShowWaveDisplay();
        HUD.UpdateWaveInfoDisplay(TotalCatsInWave - _CatsRemainingInWave, TotalCatsInWave);
    }

    public void StopAllSpawning()
    {

        foreach (CatSpawner spawner in _CatSpawners)
        {
            spawner.StopSpawner();
        }
    }

    public void OnCatDied(object Sender, EventArgs e)
    {
        _CatsRemainingInWave--;
        _CatsDistracted++;
        _TotalCatsDistracted++;

        HUD.UpdateWaveInfoDisplay(TotalCatsInWave - _CatsRemainingInWave, TotalCatsInWave);

        if (_CatsRemainingInWave < 1)
        {
            HUD.HideWaveDisplay();
            _WaveInProgress = false;
            WaveEnded?.Invoke(this, EventArgs.Empty);
            //_waveSound.clip = _endClip;
            //_waveSound.volume = PlayerDataManager.Instance.CurrentData._SFXVolume;
            //_waveSound.Play();
            Debug.LogWarning("End wave sound played");
            if (_WaveNumber >= _TotalWavesInLevel && !FindObjectOfType<PlayerHealthManager>().IsPlayerDead)
            {
                HUD.RevealVictory();
            } else if (_WaveNumber <= _TotalWavesInLevel)
            {
                HUD.UpdateWaveNumberDisplay(++_WaveNumber);
            }


            //_winSound.
        }
    }
    public void OnCatReachGoal(object Sender, EventArgs e)
    {
        _CatsRemainingInWave--;
        _CatsReachedGoal++;
        _TotalCatsReachedGoal++;

        HUD.UpdateWaveInfoDisplay(TotalCatsInWave - _CatsRemainingInWave, TotalCatsInWave);

        if (_CatsRemainingInWave < 1)
        {
            HUD.HideWaveDisplay();
            _WaveInProgress = false;

            WaveEnded?.Invoke(this, EventArgs.Empty);

            _waveSound.clip = _endClip;
            _waveSound.volume = PlayerDataManager.Instance.CurrentData._SFXVolume;
            _waveSound.Play();
            Debug.LogWarning("End wave sound played");
            if (_WaveNumber >= _TotalWavesInLevel && !FindObjectOfType<PlayerHealthManager>().IsPlayerDead)
            {
                HUD.RevealVictory();
            }
            else if (_WaveNumber <= _TotalWavesInLevel)
            {
                HUD.UpdateWaveNumberDisplay(++_WaveNumber);
            }
        }
    }

    private void CalculateTotalCatsInWave()
    {
        _TotalCatsInWave = 0;
        foreach (CatSpawner spawner in _CatSpawners)
        {
            //Debug.Log($"Spawner: {spawner.CatsInCurrentWave}");
            _TotalCatsInWave += spawner.CatsInCurrentWave();
        }

        //Debug.Log($"Total: {_TotalCatsInWave}");
    }

    private void FindAllSpawners()
    {
        _CatSpawners = FindObjectsByType<CatSpawner>(FindObjectsSortMode.None).ToList();
    }


    private void OnWaveEnded(object sender, EventArgs e)
    {

    }

    private void OnDestroy()
    {
        CatBase.OnCatDied -= OnCatDied;
        CatBase.OnCatReachGoal -= OnCatReachGoal;
    }



    public int TotalWavesInLevel { get { return _TotalWavesInLevel; } }

    public int WaveNumber { get { return _WaveNumber; } }
    public bool IsWaveInProgress { get { return _WaveInProgress; } }

    public int NumCatsDistractedInWave { get { return _CatsDistracted; } }
    public int NumCatsReachedGoalInWave { get { return _CatsReachedGoal; } }
    public int TotalCatsInWave { get { return _TotalCatsInWave; } }
   
    public int TotalCatsDistractedInLevel { get { return _TotalCatsDistracted; } }
    public int TotalCatsReachedGoalInLevel { get { return _TotalCatsReachedGoal; } }

    public float SecondsElapsedSinceLevelStarted { get { return _SecondsSinceLevelStart; } }
    public float SecondsElapsedSinceWaveStarted { get { return _SecondsSinceWaveStart; } }

    public WayPointUtils WayPointUtils { get { return _WayPointUtils; } }
}

