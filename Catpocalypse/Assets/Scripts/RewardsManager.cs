using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



/// <summary>
/// This class manages rewards that each have conditions that must be met for them to be awarded to the player.
/// </summary>
/// <remarks>
/// 
/// IMPORTANT: The time options are not as useful for wave end rewards. The rewards in that list only have their conditions checked
///            when a wave is completed. So rewards with time-based conditions should go in the general rewards list, which is checked
///            every second by default. This can be changed in the RewardsManager inspector. The RewardsManager component can be
///            found on the WaveManager game object.
///            
/// NOTE:       When using the time values in conditions, you should generally set the Limit field on the reward to 1 so it can only be
///             awarded once. This is because otherwise a condition like "SecondsElapsedSinceLevelStarted >= 10" would give the reward to
///             the player on every frame after the condition becomes true, since it will be true on every frame from then on.
/// 
/// NOTE:       You can compare bool values to number values if you wish. If you do, it is important to realize that false has a value
///             of 0, while true has a value of 1.
///             
/// </remarks>
public class RewardsManager : MonoBehaviour
{
    public enum ValueTypes
    {
        BOOL_False = 0,
        BOOL_True,
        BOOL_PlayerSurvivedWave,
        BOOL_PlayerFailedWave,

        NUMBER_CatsDistractedInWave,
        NUMBER_CurrentWaveNumber,
        NUMBER_CustomValue,
        NUMBER_SecondsElapsedSinceCurrentWaveStarted,
        NUMBER_SecondsElapsedSinceLevelStarted,
        NUMBER_TotalCatsDistractedInLevel,
        NUMBER_TotalCatsInWave,
        NUMBER_TotalCatsReachedGoalInLevel,
        NUMBER_TotalWavesInLevel,
    }


    [Header("General Rewards")]


    [Tooltip("This field specifies how often (in seconds) that the RewardsManager will check the conditions of the general rewards.")]
    [SerializeField]    
    private float _generalRewardsCheckFreqency = 1.0f;

    [Tooltip("Rewards in this list are checked continuously (once every few seconds). This delay is controlled by the generalRewardsCheckFrequency setting.")]
    [SerializeField]
    private List<Reward> _generalRewards;

    [Space(10)]

    [Header("Wave End Rewards")]

    [Tooltip("Rewards in this list are only checked when a wave is completed.")]
    [SerializeField]
    private List <Reward> _waveEndRewards;


    private float _generalRewardsTimer;
    PlayerHealthManager _playerHealthManager;
    PlayerMoneyManager _playerMoneyManager;
    WaveManager _waveManager;
    [SerializeField]
    PlayerUpgradeData _playerUpgradeData;



    private void Awake()
    {
        _playerHealthManager = FindObjectOfType<PlayerHealthManager>();
        _playerMoneyManager = FindObjectOfType<PlayerMoneyManager>();
        _waveManager = FindObjectOfType<WaveManager>();


        _waveManager.WaveEnded += OnWaveEnded;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _generalRewardsTimer += Time.deltaTime;
        if (_generalRewardsTimer >= _generalRewardsCheckFreqency)
        {
            _generalRewardsTimer = 0.0f;
            CheckRewardsConditions(_generalRewards);
        }
    }

    private void OnDestroy()
    {
        _waveManager.WaveEnded -= OnWaveEnded;
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ValidateRewardsList(_generalRewards, "General");
            ValidateRewardsList(_waveEndRewards, "Wave End");
        }
    }

    private void ValidateRewardsList(List<Reward> rewardsList, string name)
    {
        // This loop iterates backwards because otherwise we'll mess up the reward indices when we remove items.
        // This way, the ones that get messed up by item removals are at the end of the list, which are ones we already checked so it doesn't matter.
        for (int i = rewardsList.Count - 1; i >= 0; i--)
        {
            Reward reward = rewardsList[i];
            if (reward == null)
            {
                Debug.LogWarning($"{name} rewards list: The reward at index [{i}] is null! Removing it from the list, but this only effects runtime so you'll need to remove it in edit mode to stop seeing this warning.");
                _generalRewards.RemoveAt(i);
            }
        }
    }

    private void CheckRewardsConditions(List<Reward> rewardsList)
    {
        for (int i = 0; i < rewardsList.Count; i++) 
        { 
            Reward reward = rewardsList[i];
            
            if (reward.EvaluateConditions(this))
            {
                GiveReward(reward.Type, reward.Amount);
            }
        }
    }

    public float GetValue(ValueTypes valueType)
    {
        // NOTE: We don't check for ValueTypes.CustomValue here, because this function is
        //       never called in that case.
        switch (valueType)
        {
            // Boolean values
            case ValueTypes.BOOL_False:
                return BoolToNumber(false);
            case ValueTypes.BOOL_True:
                return BoolToNumber(true);
            case ValueTypes.BOOL_PlayerFailedWave:
                return BoolToNumber(_playerHealthManager.IsPlayerDead);
            case ValueTypes.BOOL_PlayerSurvivedWave:
                return BoolToNumber(!_playerHealthManager.IsPlayerDead);


            // Numeric values
            case ValueTypes.NUMBER_CatsDistractedInWave:
                return _waveManager.NumCatsDistractedInWave;
            case ValueTypes.NUMBER_TotalCatsInWave:
                return _waveManager.TotalCatsInWave;

            case ValueTypes.NUMBER_TotalCatsDistractedInLevel:
                return _waveManager.TotalCatsDistractedInLevel;
            case ValueTypes.NUMBER_TotalCatsReachedGoalInLevel:
                return _waveManager.TotalCatsReachedGoalInLevel;
            
            case ValueTypes.NUMBER_CurrentWaveNumber:
                return _waveManager.WaveNumber;
            case ValueTypes.NUMBER_TotalWavesInLevel:
                return _waveManager.TotalWavesInLevel;

            case ValueTypes.NUMBER_SecondsElapsedSinceLevelStarted:
                return _waveManager.SecondsElapsedSinceLevelStarted;
            case ValueTypes.NUMBER_SecondsElapsedSinceCurrentWaveStarted:
                return _waveManager.SecondsElapsedSinceWaveStarted;


            default:
                throw new ArgumentException("The passed in value type has not been implemented into this switch statement yet!");

        } // end switch

    }

    private float BoolToNumber(bool value)
    {
        return value ? 1 : 0;
    }

    private bool NumberToBool(float value)
    {
        return value != 0 ? true : false;
    }

    public void GiveReward(Reward.Types rewardType, float amount)
    {
        switch (rewardType)
        {
            case Reward.Types.Money:
                _playerMoneyManager.AddMoney(amount);
                Debug.Log($"GAVE REWARD: {amount} money");
                break;


            default:
                throw new ArgumentException("The passed in reward type has not been implemented into this switch statement yet!");

        } // end switch
    }

    private void OnWaveEnded(object sender, EventArgs e)
    {
        CheckRewardsConditions(_waveEndRewards);
    }

}
