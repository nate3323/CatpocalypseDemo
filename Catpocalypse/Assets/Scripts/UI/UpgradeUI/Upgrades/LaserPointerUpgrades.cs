using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LaserPointerUpgrades : UpgradeCard
{
    [SerializeField]
    private float _rangeUpgrade = 1.2f;
    [SerializeField]
    private float _distractionUpgrade = 1.2f;

    protected override void ChangeText()
    {
        _UpgradeTextBox.text = UpgradeText[PlayerDataManager.Instance.CurrentData.laserUpgrades];
        _UpgradeCostTextBox.text = ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.laserUpgrades].ToString();
        _LevelTextBox.text = (PlayerDataManager.Instance.CurrentData.laserUpgrades + 1).ToString();
        _FlavorTextBox.text = _FlavorText[PlayerDataManager.Instance.CurrentData.laserUpgrades];
    }
    public override void Upgrade()
    {
        if (PlayerDataManager.Instance.CurrentData.scrap >= ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.laserUpgrades] 
            && PlayerDataManager.Instance.CurrentData.laserUpgrades < ScrapUpgradeCost.Count)
        {
            PlayerDataManager.Instance.UpdateLaserUpgrades(1);
            PlayerDataManager.Instance.UpdateScrap(-ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.laserUpgrades]);
            if (PlayerDataManager.Instance.CurrentData.laserUpgrades == ScrapUpgradeCost.Count)
            {
                MaxUpgradeReached();
            }
            else
            {
                ChangeText();
            }
            SignalUpgrade();
        }
    }
}
