using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour
{
    public Slider MouseSensitivitySlider;
    public Toggle InvertYToggle;
    public Toggle InvertXToggle;


    // Start is called before the first frame update
    void Start()
    {
        MouseSensitivitySlider.value = PlayerDataManager.Instance.CurrentData._MouseSensitivity;

        MouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChange);
        InvertXToggle.onValueChanged.AddListener(OnXInvert);
        InvertYToggle.onValueChanged.AddListener(OnYInvert);
        
    }

    private void OnMouseSensitivityChange(float value)
    {
        PlayerDataManager.Instance.CurrentData._MouseSensitivity = value;
    }

    private void OnXInvert(bool value)
    {
        PlayerDataManager.Instance.CurrentData._MouseXInvert = value;
    }

    private void OnYInvert(bool value)
    {
        PlayerDataManager.Instance.CurrentData._MouseYInvert = value;
    }
}

