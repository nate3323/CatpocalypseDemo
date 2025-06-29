using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScratchingPostUpgrades : UpgradeCard
{
    protected override void ChangeText()
    {
        _UpgradeTextBox.text = UpgradeText[PlayerDataManager.Instance.CurrentData.scratchUpgrades];
        _UpgradeCostTextBox.text = ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.scratchUpgrades].ToString();
        _LevelTextBox.text = (PlayerDataManager.Instance.CurrentData.scratchUpgrades + 1).ToString();
        _FlavorTextBox.text = _FlavorText[PlayerDataManager.Instance.CurrentData.scratchUpgrades];
    }
    public override void Upgrade()
    {
        if (PlayerDataManager.Instance.CurrentData.scrap >= ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.scratchUpgrades] 
            && PlayerDataManager.Instance.CurrentData.scratchUpgrades < ScrapUpgradeCost.Count)
        {
            PlayerDataManager.Instance.UpdateScrap(-ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.scratchUpgrades]);
            PlayerDataManager.Instance.UpdateScratchUpgrades(1);
            if (PlayerDataManager.Instance.CurrentData.scratchUpgrades == ScrapUpgradeCost.Count)
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
