using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RobotUpgrades : UpgradeCard
{
    protected override void ChangeText()
    {
        _UpgradeTextBox.text = UpgradeText[PlayerDataManager.Instance.CurrentData.robotUpgrades];
        _UpgradeCostTextBox.text = ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.robotUpgrades].ToString();
        _LevelTextBox.text = PlayerDataManager.Instance.CurrentData.robotUpgrades.ToString();
        _FlavorTextBox.text = _FlavorText[PlayerDataManager.Instance.CurrentData.robotUpgrades];
    }

    public override void Upgrade()
    {
        if (PlayerDataManager.Instance.CurrentData.scrap >= ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.robotUpgrades] 
            && PlayerDataManager.Instance.CurrentData.robotUpgrades < ScrapUpgradeCost.Count)
        {
            PlayerDataManager.Instance.UpdateRobotUpgrades(1);
            PlayerDataManager.Instance.UpdateScrap(-ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.robotUpgrades]);
            if (PlayerDataManager.Instance.CurrentData.robotUpgrades == ScrapUpgradeCost.Count)
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
