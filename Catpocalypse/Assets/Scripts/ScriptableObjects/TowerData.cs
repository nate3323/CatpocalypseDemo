using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class TowerData : ScriptableObject
{
    [SerializeField]
    protected float distractValue;
    public float DistractValue { set { distractValue = value; } get { return distractValue; } }
    [SerializeField]
    protected float fireRate;
    public float FireRate
    {
        set
        {
            fireRate = value;
        }
        get
        {
            return fireRate;
        }
    }
    [SerializeField]
    protected float buildCost;
    public float BuildCost 
    { 
        get 
        { 
            return buildCost; 
        }
        set
        {
            buildCost = value;
        }
    }
    [SerializeField]
    protected float upgradeCost;
    public float UpgradeCost
    {
        get
        {
            return upgradeCost;
        }
        set
        {
            upgradeCost = value;
        }
    }
    [SerializeField]
    protected float range;
    public float Range
    {
        set
        {
            range = value;
        }
        get
        {
            return range;
        }
    }

}
