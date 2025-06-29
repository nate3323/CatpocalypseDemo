using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[ExecuteInEditMode]
[Serializable]
public class RewardCondition
{
    [Tooltip("Sets the first value to use for this condition. If set to CustomValue, then it will use the value entered into Value1CustomValue.")]
    [SerializeField]
    private RewardsManager.ValueTypes _Value1;
    [Tooltip("This value is used for Value1 when it is set to \"CustomValue\".")]
    [SerializeField]
    private float _Value1CustomValue;

    [Space(10)]

    [Tooltip("Specifies how to compare Value1 to Value2.")]
    [SerializeField]
    private ConditionTypes _Condition;

    [Space(10)]

    [Tooltip("Sets the first value to use for this condition. If set to CustomValue, then it will use the value entered into Value2CustomValue.")]
    [SerializeField]
    private RewardsManager.ValueTypes _Value2;
    [Tooltip("This value is used for Value2 when it is set to \"CustomValue\".")]
    [SerializeField]
    private float _Value2CustomValue;

    [Space(10)]

    [Tooltip("Check this box if the expected outcome of this condition should be false instead of true.")]
    [SerializeField]
    private bool _EvaluatesToFalse;



    /// <summary>
    /// This function tests the two passed in values to see if they meet this condition.
    /// </summary>
    /// <param name="a">The first number to compare.</param>
    /// <param name="b">The second number to compare.</param>
    /// <returns>True or false after evaluating this condition with the two passed in values.</returns>
    public bool Evaluate(float a, float b)
    {
        bool result = false;

        switch (_Condition)
        {
            case ConditionTypes.NotEqual:
                result = a != b;
                break;
            case ConditionTypes.Equal:
                result = a == b;
                break;
            case ConditionTypes.LessThan:
                result = a < b;
                break;
            case ConditionTypes.LessThanOrEqual:
                result = a <= b;
                break;
            case ConditionTypes.GreaterThan:
                result = a > b;
                break;
            case ConditionTypes.GreaterThanOrEqual:
                result = a >= b;
                break;
            

            default:
                throw new Exception("The specified condition type has not been implemented into this switch statement yet!");

        } // end switch


        return !(_EvaluatesToFalse) ? result == true
                                    : result == false;
    }



    public enum ConditionTypes
    {
        NotEqual,
        Equal,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
    }



    public RewardsManager.ValueTypes Value1 { get { return _Value1; } }
    public float Value1CustomValue { get { return _Value1CustomValue; } }

    public ConditionTypes Type { get { return _Condition; } }

    public RewardsManager.ValueTypes Value2 { get { return _Value2; } }
    public float Value2CustomValue { get { return _Value2CustomValue; } }

    public bool EvaluatesToFalse { get { return _EvaluatesToFalse; } }

} // end class Condition



