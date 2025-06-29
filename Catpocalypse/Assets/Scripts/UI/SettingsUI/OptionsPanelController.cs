using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class OptionsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject _DisplayPanel;
    [SerializeField] private GameObject _AudioPanel;
    [SerializeField] private GameObject _ControlsPanel;
    [SerializeField] private GameObject _GameplayPanel;
    [SerializeField] private Toggle _StartToggle;

    private GameObject _ActivePanel;

    private void Start()
    {
        _DisplayPanel.SetActive(true);
        _StartToggle.Select();
        _ActivePanel = _DisplayPanel;
        _AudioPanel.SetActive(false);
        _ControlsPanel.SetActive(false);
        _GameplayPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ToggleDisplayPanel()
    {
        _ActivePanel.SetActive(false);
        _ActivePanel = _DisplayPanel;
        _ActivePanel.SetActive(true);
    }

    public void ToggleSoundPanel()
    {
        _ActivePanel.SetActive(false);
        _ActivePanel = _AudioPanel;
        _ActivePanel.SetActive(true);
    }

    public void ToggleControlsPanel()
    {
        _ActivePanel.SetActive(false);
        _ActivePanel = _ControlsPanel;
        _ActivePanel.SetActive(true);
    }

    public void ToggleGameplayPanel()
    {
        _ActivePanel.SetActive(false);
        _ActivePanel = _GameplayPanel;
        _ActivePanel.SetActive(true);
    }

    public void CloseButtonPressed()
    {
        gameObject.SetActive(false);
    }
}
