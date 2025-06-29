using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DisplayOptionsManager : MonoBehaviour
{
    public TMP_Dropdown resolutions;
    public Toggle windowedToggle;

    void Start()
    {
        resolutions.SetValueWithoutNotify(PlayerDataManager.Instance.CurrentData._ResolutionSize);
        windowedToggle.SetIsOnWithoutNotify(PlayerDataManager.Instance.CurrentData.windowed);
        resolutions.onValueChanged.AddListener(OnResolutionChange);
        windowedToggle.onValueChanged.AddListener(OnWindowedChange);
    }

    private void OnResolutionChange(int resolution)
    {
        PlayerDataManager.Instance.UpdateResolutionSize(resolution);
        Screen.SetResolution(1920, 1080, PlayerDataManager.Instance.CurrentData.windowed);
    }

    private void OnWindowedChange(bool windowed)
    {
        PlayerDataManager.Instance.UpdateWindowed(windowed);
    }
}
