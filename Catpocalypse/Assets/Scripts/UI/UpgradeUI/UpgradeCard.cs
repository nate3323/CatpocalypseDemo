using System.Collections.Generic;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class UpgradeCard : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _UpgradeTextBox;
    [SerializeField] protected TextMeshProUGUI _FlavorTextBox;
    [SerializeField] protected TextMeshProUGUI _UpgradeCostTextBox;
    [SerializeField] protected TextMeshProUGUI _LevelTextBox;
    [SerializeField] protected List<string> UpgradeText;
    [SerializeField] protected List<int> ScrapUpgradeCost;
    [SerializeField] protected List<string> _FlavorText;

    public static event EventHandler OnUpgrade;

    public void Start()
    {
        ChangeText();
    }

    protected void SignalUpgrade()
    {
        OnUpgrade?.Invoke(this, EventArgs.Empty);
        Debug.Log("Upgraded a tower, scrap should show " + PlayerDataManager.Instance.CurrentData.scrap);
    }

    protected abstract void ChangeText();

    public abstract void Upgrade();

    protected void MaxUpgradeReached()
    {
        _FlavorTextBox.gameObject.SetActive(false);
        _UpgradeCostTextBox.gameObject.SetActive(false);
        _UpgradeTextBox.text = "Maximum Upgrades Reached";
    }

}