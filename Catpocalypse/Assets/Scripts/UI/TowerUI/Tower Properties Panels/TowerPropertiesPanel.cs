using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class TowerPropertiesPanel : MonoBehaviour
{
    [SerializeField] PropertiesUI_LaserPointerTower _PropertiesUI_Laser;



    private void OnEnable()
    {
        InitUI();
    }

    private void OnDisable()
    {
        DisableAllPropertiesUIs();
    }

    private void InitUI()
    {
        // First, make sure all properties UIs are off, so we only end up with the correct one displayed.
        DisableAllPropertiesUIs();


        // The properties UI for the selectd tower has a script on it that will automatically update the UI when the panel is enabled.

        if (TowerBase.SelectedTowerBase == null)
            return;

        if (TowerBase.SelectedTowerBase.tower == null)
            return;


        bool showLaserPropertiesUI = TowerBase.SelectedTowerBase.tower.GetComponent<LaserPointerTower>() != null;
        _PropertiesUI_Laser.gameObject.SetActive(showLaserPropertiesUI);
    }

    private void DisableAllPropertiesUIs()
    {
        _PropertiesUI_Laser.gameObject.SetActive(false);
    }

    public void RefreshUI()
    {
        InitUI();
    }
}
