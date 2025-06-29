using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
[CreateAssetMenu]
public class TowerUpgradesData : ScriptableObject
{
    [SerializeField,Max(1)]
    public float rangeUpgradePercent;

    [SerializeField]
    public float fireRateUpgradePercent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
