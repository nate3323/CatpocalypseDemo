using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class YarnThrowerUpgrades : UpgradeCard
{

    protected override void ChangeText()
    {
        _UpgradeTextBox.text = UpgradeText[PlayerDataManager.Instance.CurrentData.yarnUpgrades];
        _UpgradeCostTextBox.text = ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.yarnUpgrades].ToString();
        _LevelTextBox.text = (PlayerDataManager.Instance.CurrentData.yarnUpgrades + 1).ToString();
        _FlavorTextBox.text = _FlavorText[PlayerDataManager.Instance.CurrentData.yarnUpgrades];
    }
    public override void Upgrade()
    {
        if (PlayerDataManager.Instance.CurrentData.scrap >= ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.yarnUpgrades] 
            && PlayerDataManager.Instance.CurrentData.yarnUpgrades < ScrapUpgradeCost.Count)
        {
            PlayerDataManager.Instance.UpdateScrap(-ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.yarnUpgrades]);
            PlayerDataManager.Instance.UpdateYarnUpgrades(1);
            if (PlayerDataManager.Instance.CurrentData.yarnUpgrades == ScrapUpgradeCost.Count)
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
