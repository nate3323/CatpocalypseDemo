using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FortificationUpgrades : UpgradeCard
{
    [SerializeField,Tooltip("The upgrade for the hairball removal speed")]
    private float _hairballRemovalSpeedUpgrade = .5f;

    [SerializeField, Tooltip("How much the player health is upgraded by")]
    private int _healthUpgrade = 2;

    protected override void ChangeText()
    {
        _UpgradeTextBox.text = UpgradeText[PlayerDataManager.Instance.CurrentData.fortificationUpgrades];
        _UpgradeCostTextBox.text = ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.fortificationUpgrades].ToString();
        _LevelTextBox.text = (PlayerDataManager.Instance.CurrentData.fortificationUpgrades + 1).ToString();
        _FlavorTextBox.text = _FlavorText[PlayerDataManager.Instance.CurrentData.fortificationUpgrades];
    }
    public override void Upgrade()
    {
        if (PlayerDataManager.Instance.CurrentData.scrap >= ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.fortificationUpgrades] 
            && PlayerDataManager.Instance.CurrentData.fortificationUpgrades < ScrapUpgradeCost.Count)
        {
            PlayerDataManager.Instance.UpdateFortificationUpgrades(1);
            PlayerDataManager.Instance.UpdateScrap(-ScrapUpgradeCost[PlayerDataManager.Instance.CurrentData.fortificationUpgrades]);
            if (PlayerDataManager.Instance.CurrentData.fortificationUpgrades == ScrapUpgradeCost.Count)
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
