using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NonAllergicUpgrades : UpgradeCard
{

    protected override void ChangeText()
    {
        _UpgradeTextBox.text = UpgradeText[PlayerDataManager.Instance.CurrentData.nAUpgrades];
        _UpgradeCostTextBox.text = ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.nAUpgrades].ToString();
        _LevelTextBox.text = (PlayerDataManager.Instance.CurrentData.nAUpgrades + 1).ToString();
        _FlavorTextBox.text = _FlavorText[PlayerDataManager.Instance.CurrentData.nAUpgrades];
    }
    public override void Upgrade()
    {
        if (PlayerDataManager.Instance.CurrentData.scrap >= ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.nAUpgrades]
            && PlayerDataManager.Instance.CurrentData.nAUpgrades < ScrapUpgradeCost.Count)
        {
            PlayerDataManager.Instance.UpdateScrap( -ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.nAUpgrades]);
            PlayerDataManager.Instance.UpdateNAUpgrades(1);
            if (PlayerDataManager.Instance.CurrentData.nAUpgrades == ScrapUpgradeCost.Count)
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
