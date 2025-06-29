using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardsUpgrades : UpgradeCard
{ 
    protected override void ChangeText()
    {
        _UpgradeTextBox.text = UpgradeText[PlayerDataManager.Instance.CurrentData.catRewardUpgrades];
        _UpgradeCostTextBox.text = ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.catRewardUpgrades].ToString();
        _LevelTextBox.text = (PlayerDataManager.Instance.CurrentData.catRewardUpgrades + 1).ToString();
        _FlavorTextBox.text = _FlavorText[PlayerDataManager.Instance.CurrentData.catRewardUpgrades];
    }
    public override void Upgrade()
    {
        if (PlayerDataManager.Instance.CurrentData.scrap >= ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.catRewardUpgrades] 
            && PlayerDataManager.Instance.CurrentData.catRewardUpgrades < PlayerDataManager.Instance.Upgrades.MaxRewardUpgrades)
        {
            PlayerDataManager.Instance.UpdateScrap(-ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.catRewardUpgrades]);
            PlayerDataManager.Instance.UpdateRewardUpgrade(1);
            if (PlayerDataManager.Instance.CurrentData.catRewardUpgrades == ScrapUpgradeCost.Count)
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
