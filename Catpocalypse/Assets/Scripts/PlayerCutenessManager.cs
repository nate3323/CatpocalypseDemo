using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using Random = UnityEngine.Random;


public class PlayerCutenessManager : MonoBehaviour
{
    public static PlayerCutenessManager Instance;


    [Header("Cuteness Bar Settings")]

    [SerializeField] private float _MaxCuteness = 100f;

    [SerializeField]
    private TextMeshProUGUI cutenessMeterMaxedText;

    [SerializeField]
    private TextMeshProUGUI cutenessChallengeText;

    [Header("Cuteness Challenges Parameters")]

    [Tooltip("This sets how much less effective (as a percentage) distraction is on the cats.")]
    [Range(0f, 1f)]
    [SerializeField] private float _CatsDistractionThresholdDebuffPercent = 0.5f;

    [ 
    Tooltip("The percent debuff to the fire rate of the tower"),
    Range(0f, 1f)]
    public float _TowerFireRateDebuffPercent;

    private CutenessChallenges _CurrentCutenessChallenge = CutenessChallenges.None;

    private float _Cuteness = 0f;

    public TowerTypes _TowerType;

    public CatTypes _CatType;

    public float _cutenessChallengeCatBuffPercent = 1.25f;


    /// <summary>
    /// This enum defines the challenges caused by maxing out the cuteness meter.
    /// </summary>
    public enum CutenessChallenges
    {
        None, // This is used when there is no active cuteness challenge
        CatsGetHarderToDistract, //Applies a debuff to tower damage
        BuffCatType, //Buffs a type of cat
        DebuffTowerType, //Debuffs a type of tower
        NonAllergicStrike, //Disables the Non-Allergic Towers
        CucumberTowerBuffsCats, //The Cucumber Tower buffs cats instead of distracting them
    }


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is already a PlayerCutenessManager in this scene. Self destructing!");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cutenessMeterMaxedText.gameObject.SetActive(false);
    }

    private void Start()
    {
        WaveManager.Instance.WaveEnded += OnWaveEnded;

        //Currently disabled HUD.UpdateCutenessDisplay(_Cuteness, _MaxCuteness);
    }
    private void Update()
    {

    }

    public void AddCuteness(int amount)
    {
        float adjustedAmount = amount;
        if(PlayerDataManager.Instance.CurrentData.fortificationUpgrades > 1)
        {
            adjustedAmount *= PlayerDataManager.Instance.Upgrades.CutenessResistanceUpgrade;
        }
        _Cuteness = Mathf.Clamp(_Cuteness + adjustedAmount, 0, _MaxCuteness);
        //Currently Disabled HUD.UpdateCutenessDisplay(_Cuteness, _MaxCuteness);
    }

    private void OnWaveEnded(object sender, EventArgs e)
    {
        if(CurrentCutenessChallenge != CutenessChallenges.None)
        {
            EndCutenessChallenge();
            // Reset this in case one was active in the previous wave.
            _CurrentCutenessChallenge = CutenessChallenges.None;
        }
        
        /** All  cuteness features currently disabled
        // Did the bar get maxed during the previous wave?
        if (_Cuteness >= _MaxCuteness)
        {            
            _Cuteness = 0f;
            HUD.UpdateCutenessDisplay(_Cuteness, _MaxCuteness);

            cutenessMeterMaxedText.gameObject.SetActive(true);

            new WaitForSeconds(4);

            cutenessMeterMaxedText.gameObject.SetActive(false);

            // Randomly select a cuteness challenge for the next wave.
            SelectCutenessChallenge();
        } */
    }
        
    private void SelectCutenessChallenge()
    {
        var challenges = Enum.GetValues(typeof(CutenessChallenges));

        // We're starting the random range at 1, so we don't randomly select 0 (the "None" option).
        int index = Random.Range(1, challenges.Length - 1);

        _CurrentCutenessChallenge = (CutenessChallenges) challenges.GetValue(index);
       
    }



    public float Cuteness { get { return _Cuteness; } }
    public CutenessChallenges CurrentCutenessChallenge { get { return _CurrentCutenessChallenge; } }

    public float CuteChallenge_CatsGetHarderToDistract_DebuffPercent { get { return _CatsDistractionThresholdDebuffPercent; } }
    //Executes the cuteness challenge
    public void CutenessChallenge()
    {
        if (CurrentCutenessChallenge == CutenessChallenges.CatsGetHarderToDistract)
        {
            foreach(Tower tower in FindObjectsOfType<Tower>())
            {
                tower.towerStats.DistractValue *= CuteChallenge_CatsGetHarderToDistract_DebuffPercent;
            }
            cutenessChallengeText.text = "All towers do less damage";
        }
        if (CurrentCutenessChallenge == CutenessChallenges.DebuffTowerType)
        {
            var towerTypes = Enum.GetValues(typeof(TowerTypes));
            //Gets the index, excluding the laser pointer and non-allergic towers
            int index = Random.Range(1,towerTypes.Length-2);
            _TowerType = (TowerTypes) towerTypes.GetValue(index);
            foreach (Tower tower in FindObjectsOfType<Tower>())
            {
                //If the tower is the type that is getting debuffed
                if(tower.TowerTypeTag == _TowerType)
                {
                    tower.towerStats.FireRate *= 1+ _TowerFireRateDebuffPercent;
                }
                cutenessChallengeText.text = _TowerType+" towers fire slower";
            }
        }
        if(CurrentCutenessChallenge == CutenessChallenges.NonAllergicStrike)
        {
            
            foreach (Tower tower in FindObjectsOfType<Tower>())
            { 
                if(tower.TowerTypeTag == TowerTypes.NonAllergic)
                {
                    tower.gameObject.GetComponent<NonAllergicTower>().DisableTower();
                }
            }
            foreach (NonAllergicPerson person in FindObjectsOfType<NonAllergicPerson>())
            {
                Destroy(person.gameObject);
            }
            cutenessChallengeText.text = "Non-Allergic towers are disabled";
        }
        if(CurrentCutenessChallenge == CutenessChallenges.CucumberTowerBuffsCats)
        {
            foreach (Tower tower in FindObjectsOfType<Tower>())
            {
                if (tower.TowerTypeTag == TowerTypes.CucumberThrower)
                {
                    tower.gameObject.GetComponent<CucumberTower>().buffCats = true;
                }
            }
            cutenessChallengeText.text = "Cucumber Thrower towers buff cats instead of distracting them";
        }
        if(CurrentCutenessChallenge == CutenessChallenges.BuffCatType)
        {
            var catTypes = Enum.GetValues(typeof(CatTypes));
            int index = Random.Range(0, 2);
            _CatType = (CatTypes) catTypes.GetValue(index);
            cutenessChallengeText.text = _CatType +  " Cats have a buff this wave";
        }
    }
    //Ends the cuteness challenge when the wave ends
    private void EndCutenessChallenge()
    {
        if (CurrentCutenessChallenge == CutenessChallenges.CatsGetHarderToDistract)
        {
            foreach (Tower tower in FindObjectsOfType<Tower>())
            {
                tower.towerStats.DistractValue /= CuteChallenge_CatsGetHarderToDistract_DebuffPercent;
            }
        }
        if (CurrentCutenessChallenge == CutenessChallenges.DebuffTowerType)
        {
            foreach (Tower tower in FindObjectsOfType<Tower>())
            {
                //If the tower is the type that got debuffed
                if (tower.TowerTypeTag == _TowerType)
                {
                    tower.towerStats.FireRate /= 1+_TowerFireRateDebuffPercent;
                }
            }
        }
        if(CurrentCutenessChallenge == CutenessChallenges.NonAllergicStrike)
        {
            foreach (Tower tower in FindObjectsOfType<Tower>())
            {
                if (tower.TowerTypeTag == TowerTypes.NonAllergic)
                {
                    tower.gameObject.GetComponent<NonAllergicTower>().Enabled = true;
                }
            }
        }
        if (CurrentCutenessChallenge == CutenessChallenges.CucumberTowerBuffsCats)
        {
            foreach (Tower tower in FindObjectsOfType<Tower>())
            {
                if (tower.TowerTypeTag == TowerTypes.CucumberThrower)
                {
                    tower.gameObject.GetComponent<CucumberTower>().buffCats = false;
                }
            }
            
        }

        cutenessChallengeText.text = "";
    }
}
