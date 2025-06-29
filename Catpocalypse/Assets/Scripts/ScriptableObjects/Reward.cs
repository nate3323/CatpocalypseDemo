using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;



[CreateAssetMenu(fileName = "NewReward", menuName = "New Reward Asset")]
public class Reward : ScriptableObject
{
    public enum Types
    {
        Money,
        Scrap
    }



    [Tooltip("The type of reward this is.")]
    [SerializeField]
    private Types _Type;
    [Tooltip("The amount of this reward that is given to the player when the conditions are met. This field may be ignored for certain reward types.")]
    [SerializeField]
    private int _Amount;
    [Tooltip("This field lets you limit how many times a reward can be given to the player. Set it to 0 to have it unlimited. For example, you'd want to set this to 1 when you make a condition like \"SecondsElapsedSinceCurrentWaveStarted >= 10\". That way the reward can only happen once, thus preventing it from giving the reward to the player on every frame after that condition is met.")]
    [SerializeField, Min(0)]
    private int _Limit;
    [TextArea, Tooltip("This is an optional field for description or notes about this reward. This checkbox should always be enabled when you make a condition based on one of the time values. Otherwise conditions like \"SecondsSinceWaveStarted >= 10\" would give the player the reward on every frame after that value is greater than or equal to 10!")]
    [SerializeField]
    private string _Description;
    
    [Tooltip("This list defines the condition(s) that must be met for this reward to be given to the player.")]
    [SerializeField]
    private List<RewardCondition> _Conditions;


    private RewardsManager _RewardsManager;

    [NonSerialized]
    private int _TimesGiven;



    /// <summary>
    /// Evaluates the conditions required for this reward.
    /// </summary>
    /// <param name="manager">The parent RewardsManager that called this function.</param>
    /// <returns>True if the conditions for this reward have been met, or false otherwise.</returns>
    public bool EvaluateConditions(RewardsManager manager)
    {
        // If this reward's times given counter has reached its limit, then simply return false.
        // NOTE: If _Limit is set to 0, it means the reward can be given an unlimited number of times.
        if (_Limit > 0 && _TimesGiven >= _Limit)
        {
            return false;
        }


        if (manager != null)
            _RewardsManager = manager;
        else
            throw new ArgumentNullException(nameof(manager));


        float value1 = 0f;
        float value2 = 0f;
        foreach (RewardCondition condition in _Conditions)
        {
            // Get value 1.
            if (condition.Value1 != RewardsManager.ValueTypes.NUMBER_CustomValue)
                value1 = _RewardsManager.GetValue(condition.Value1);
            else
                value1 = condition.Value1CustomValue;


            // Get value 2.
            if (condition.Value2 != RewardsManager.ValueTypes.NUMBER_CustomValue)
                value2 = _RewardsManager.GetValue(condition.Value2);
            else
                value2 = condition.Value2CustomValue;


            // Test if the values meet the condition.
            if (!condition.Evaluate(value1, value2))
            {
                return false;
            }

        } // end foreach


        _TimesGiven++;

        return true;
    }



    public Types Type { get { return _Type; } }
    public float Amount { get { return _Amount; } }

}