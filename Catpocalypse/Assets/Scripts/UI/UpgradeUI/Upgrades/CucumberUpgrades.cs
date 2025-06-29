using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CucumberUpgrades : UpgradeCard
{


    protected override void ChangeText()
    {
        _UpgradeTextBox.text = UpgradeText[PlayerDataManager.Instance.CurrentData.cucumberUpgrades];
        _UpgradeCostTextBox.text = ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.cucumberUpgrades].ToString();
        _LevelTextBox.text = (PlayerDataManager.Instance.CurrentData.cucumberUpgrades + 1).ToString();
        _FlavorTextBox.text = _FlavorText[PlayerDataManager.Instance.CurrentData.cucumberUpgrades];
    }
    public override void Upgrade()
    {
        if(PlayerDataManager.Instance.CurrentData.scrap >= ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.cucumberUpgrades]
            && PlayerDataManager.Instance.CurrentData.cucumberUpgrades < ScrapUpgradeCost.Count - 1)
        {
            PlayerDataManager.Instance.UpdateScrap(-ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.cucumberUpgrades]);
            PlayerDataManager.Instance.UpdateCucumberUpgrades(1);
            if(PlayerDataManager.Instance.CurrentData.cucumberUpgrades == ScrapUpgradeCost.Count)
            {
                MaxUpgradeReached();
            } else
            {
                ChangeText();
            }
            SignalUpgrade();
        }
    }
}

