using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StringWaverUpgrades : UpgradeCard
{
    protected override void ChangeText()
    {
        _UpgradeTextBox.text = UpgradeText[PlayerDataManager.Instance.CurrentData.stringUpgrades];
        _UpgradeCostTextBox.text = ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.stringUpgrades].ToString();
        _LevelTextBox.text = (PlayerDataManager.Instance.CurrentData.stringUpgrades + 1).ToString();
        _FlavorTextBox.text = _FlavorText[PlayerDataManager.Instance.CurrentData.stringUpgrades];
    }
    public override void Upgrade()
    {
        if (PlayerDataManager.Instance.CurrentData.scrap >= ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.stringUpgrades]
            && PlayerDataManager.Instance.CurrentData.stringUpgrades < ScrapUpgradeCost.Count)
        {
            PlayerDataManager.Instance.UpdateScrap(-ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.stringUpgrades]);
            PlayerDataManager.Instance.UpdateStringUpgrades(1);
            if (PlayerDataManager.Instance.CurrentData.stringUpgrades == ScrapUpgradeCost.Count)
            {
                MaxUpgradeReached();
            }
            else
            {
                ChangeText();
            }
        }
    }
}
