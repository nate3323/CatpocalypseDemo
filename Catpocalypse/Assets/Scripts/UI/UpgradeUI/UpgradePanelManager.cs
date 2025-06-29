using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelManager : MonoBehaviour
{
    [SerializeField] private GameObject _TowerPanel;
    [SerializeField] private GameObject _DefensesPanel;
    [SerializeField] private GameObject _RobotPanel;

    private GameObject _currentUpgradePanel;
    private void Awake()
    {
        _currentUpgradePanel = _TowerPanel;
        _RobotPanel.SetActive(false);
        _DefensesPanel.SetActive(false);
    }

    public void EnableTowerPanel(bool enable)
    {
        _currentUpgradePanel.SetActive(!enable);
        _TowerPanel.SetActive(enable);
        _currentUpgradePanel = _TowerPanel;
    }

    public void EnableRobotPanel(bool enable)
    {
        _currentUpgradePanel.SetActive(!enable);
        _RobotPanel.SetActive(enable);
        _currentUpgradePanel = _RobotPanel;
    }

    public void EnableDefensesPanel(bool enable)
    {
        _currentUpgradePanel.SetActive(!enable);
        _DefensesPanel.SetActive(enable);
        _currentUpgradePanel = _DefensesPanel;
    }
}
