using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    private List<PlayerData> _PlayerData;
    private PlayerData _trackedData;
    private PlayerUpgradeData _playerUpgradeData;
    private int maxSlots = 3;
    private int _CurrentData;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }
        // Tell Unity to not destroy this game object when a new scene is loaded.
        DontDestroyOnLoad(gameObject);
        _playerUpgradeData = (PlayerUpgradeData) Resources.Load("PlayerUpgrades");
        LoadData();
        _CurrentData = 0;
        if (_PlayerData.Count > 0)
        {
            _trackedData = _PlayerData[0];
            int currentSaveSlot = _CurrentData;
            foreach (PlayerData playerData in _PlayerData)
            {
                if (playerData.date > _trackedData.date)
                {
                    _trackedData.date = playerData.date;
                    _CurrentData = currentSaveSlot;
                }
                currentSaveSlot++;
            }
        }
        else
        {
            _trackedData = new PlayerData(_playerUpgradeData);
        }
    }

    private void Start()
    {
        
    }

    private void LoadData()
    {
        _PlayerData = new List<PlayerData>(maxSlots);
        for (int i = 0; i < maxSlots; i++)
        {
            if (!LoadGame(i))
            {
                _PlayerData.Add(new PlayerData(_playerUpgradeData));
            }
        }
    }

    public IReadOnlyList<PlayerData> ViewPlayerData()
    {
        return _PlayerData.ToList().AsReadOnly();
    }

    public void SaveGame(int i)
    {
        UpdateTimePlayed();
        UpdateDatePlayed();
        _PlayerData[i] = _trackedData;
        string saveFilePath = BuildSaveFilePath(i);
        string savePlayerData = JsonUtility.ToJson(_PlayerData[i]);
        File.WriteAllText(saveFilePath, savePlayerData);

        Debug.Log("Save file created at: " + saveFilePath);
    }

    public bool LoadGame(int i)
    {
        string saveFilePath = BuildSaveFilePath(i);
        if (File.Exists(saveFilePath))
        {
            string loadPlayerData = File.ReadAllText(saveFilePath);
            PlayerData loadedData = JsonUtility.FromJson<PlayerData>(loadPlayerData);
            _PlayerData.Add(loadedData);
            _CurrentData = i;
            Debug.Log("Load game complete! \nLevels Completed: " + _PlayerData[i].levelsCompleted);
            return true;
        }
        else
        {
            Debug.Log("There are no save files to load!");
            return false;
        }
            
   

    }

    public bool DeleteSaveFile()
    {
        string saveFilePath = BuildSaveFilePath(_CurrentData);
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);

            Debug.Log("Save file deleted!");
            return true;
        }
        else
            Debug.Log("There is nothing to delete!");
        return false;
    }

    private string BuildSaveFilePath(int i)
    {
        return Application.persistentDataPath + "/PlayerData" + i + ".json";
    }

    public void SetName(string name)
    {
        _trackedData.name = name;
    }

    public void UpdateScrap(int amount)
    {
        _trackedData.scrap += amount;
    }
    public void UpdateLevelsCompleted(int amount)
    {
        _trackedData.levelsCompleted += amount;
    }
    public void UpdateRobotUpgrades(int amount)
    {
        _trackedData.robotUpgrades += amount;
    }
    public void UpdateFortificationUpgrades(int amount)
    {
        _trackedData.fortificationUpgrades += amount;
    }
    public void UpdateLaserUpgrades(int amount)
    {
        _trackedData.laserUpgrades += amount;
    }
    public void UpdateScratchUpgrades(int amount)
    {
        _trackedData.scratchUpgrades += amount;
    }
    public void UpdateNAUpgrades(int amount)
    {
        _trackedData.nAUpgrades += amount;
    }
    public void UpdateYarnUpgrades(int amount)
    {
        _trackedData.yarnUpgrades += amount;
    }
    public void UpdateStringUpgrades(int amount)
    {
        _trackedData.stringUpgrades += amount;
    }
    public void UpdateCucumberUpgrades(int amount)
    {
        _trackedData.cucumberUpgrades += amount;
    }

    public void UpdateRewardUpgrade(int amount)
    {
        _trackedData.catRewardUpgrades += amount;
    }

    private void UpdateTimePlayed()
    {
        _trackedData.time = Time.realtimeSinceStartup;
    }

    private void UpdateDatePlayed()
    {
        _trackedData.date = DateTime.Now;
    }

    public void UpdateMasterVolume(float amount)
    {
        _trackedData._MasterVolume = amount;
    }

    public void UpdateMusicVolume(float amount)
    {
        _trackedData._MusicVolume = amount;
    }
    public void UpdateSFXVolume(float amount)
    {
        _trackedData._SFXVolume = amount;
    }

    public void UpdateResolutionSize(int amount)
    {
        _trackedData._ResolutionSize = amount;
    }

    public void UpdateWindowed(bool value)
    {
        _trackedData.windowed = value;
    }

    public void UpdateXInversion(bool value)
    {
        _trackedData._MouseXInvert = value;
    }

    public void UpdateYInversion(bool value)
    {
        _trackedData._MouseYInvert = value;
    }

    public void UpdateMouseSensitivity(float amount)
    {
        _trackedData._MouseSensitivity = amount;
    }

    public PlayerData CurrentData { get { return _trackedData; } }
    public PlayerUpgradeData Upgrades { get { return _playerUpgradeData; } }

}



public class PlayerData
{
    public PlayerData(PlayerUpgradeData data)
    {
        name = "";
        scrap = 300;
        levelsCompleted = 0;
        robotUpgrades = 0;
        fortificationUpgrades = 0;
        laserUpgrades = 0;
        scratchUpgrades = 0;
        nAUpgrades = 0;
        yarnUpgrades = 0;
        stringUpgrades = 0;
        cucumberUpgrades = 0;
        catRewardUpgrades = 0;
        time = 0;
        date = DateTime.Now;
        _MasterVolume = 1f;
        _MusicVolume = 1f;
        _SFXVolume = 1f;
        _ResolutionSize = 0;
        _MouseXInvert = false;
        _MouseYInvert = false;
        _MouseSensitivity = 0.5f;
}

    public string name;
    public int scrap;
    public int levelsCompleted;
    public int robotUpgrades;
    public int fortificationUpgrades;
    public int laserUpgrades;
    public int scratchUpgrades;
    public int nAUpgrades;
    public int yarnUpgrades;
    public int stringUpgrades;
    public int cucumberUpgrades;
    public int catRewardUpgrades;
    public float time;
    public DateTime date;

    //Settings
    public float _MasterVolume;
    public float _MusicVolume;
    public float _SFXVolume;
    public int _ResolutionSize;
    public bool windowed;
    public bool _MouseXInvert;
    public bool _MouseYInvert;
    public float _MouseSensitivity;
}