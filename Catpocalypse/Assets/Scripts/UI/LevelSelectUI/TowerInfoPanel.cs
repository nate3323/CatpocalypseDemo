using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TowerInfoPanel : MonoBehaviour
{

    [Header("Info Panel Settings")]

    [SerializeField] TextMeshProUGUI _Text_DisplayName;
    [SerializeField] TextMeshProUGUI _Text_Cost;
    [SerializeField] TextMeshProUGUI _Text_Damage;
    [SerializeField] TextMeshProUGUI _Text_Range;
    [SerializeField] TextMeshProUGUI _Text_AOE_Range;
    [SerializeField] TextMeshProUGUI _Text_FireRate;
    [SerializeField] TextMeshProUGUI _Text_Cooldown;
    [SerializeField] TextMeshProUGUI _Text_UpgradeCost;
    [SerializeField] TextMeshProUGUI _Text_Upgrade;
    [SerializeField] TextMeshProUGUI _Text_Special;
    [SerializeField] TextMeshProUGUI _Text_Description;
    [SerializeField] Image _Image_Icon;

    [Space(10)]

    [Tooltip("This list controls which towers are displayed in this window. The tower info displayed by this panel is pulled from the scriptable objects in this list.")]
    [SerializeField] List<TowerInfo> _TowerInfoList;
    [SerializeField] ToggleGroup group;

    private TowerInfoCollection _TowerInfoCollection;

    private void Awake()
    {
        _TowerInfoCollection = GetComponent<TowerInfoCollection>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetUI();
        gameObject.SetActive(false);
    }

    private void OnSelectedTowerChanged(TowerTypes tower)
    {
        UpdateUI(_TowerInfoCollection.GetTowerInfo(tower));
    }

    public void ButtonClicked_Close()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// This function updates the UI whenever the selected tower is changed.
    /// </summary>
    /// <param name="info">The TowerInfo object for the newly selected tower. It contains all the info about that tower.</param>
    private void UpdateUI(TowerInfo info)
    {
        _Text_DisplayName.text = info.DisplayName;
        _Text_Cost.text = $"${info.Cost}";
        _Text_Damage.text = Utils.InsertSpacesBeforeCapitalLetters(info.Damage.ToString());
        _Text_Range.text = Utils.InsertSpacesBeforeCapitalLetters(info.Range.ToString());
        _Text_AOE_Range.text = Utils.InsertSpacesBeforeCapitalLetters(info.AOE_Range.ToString());
        _Text_FireRate.text = Utils.InsertSpacesBeforeCapitalLetters(info.FireRate.ToString());
        _Text_Cooldown.text = Utils.InsertSpacesBeforeCapitalLetters(info.CoolDown.ToString());
        _Text_UpgradeCost.text = Utils.InsertSpacesBeforeCapitalLetters(info.UpgradeCost.ToString());
        _Text_Upgrade.text = info.Upgrade;
        _Text_Special.text = info.Special.ToString();
        _Text_Description.text = info.Description.ToString();
        _Image_Icon.sprite = info.Icon;
    }

    public void ResetUI()
    {
        UpdateUI(_TowerInfoCollection.GetTowerInfo(0));
    }

    /**
     * This section  is the listed methods that toggles reference when updating the Tower Information UI
     */
    public void OnCucumberToggle() { OnSelectedTowerChanged(TowerTypes.CucumberThrower); }
    public void OnLaserToggle() { OnSelectedTowerChanged(TowerTypes.LaserPointer); }
    public void OnNonAllergicToggle() { OnSelectedTowerChanged(TowerTypes.NonAllergic); }
    public void OnScratchingPostToggle() { OnSelectedTowerChanged(TowerTypes.ScratchingPost); }
    public void OnStringWaverToggle() { OnSelectedTowerChanged(TowerTypes.StringWaver); }
    public void OnYarnThrowerToggle() { OnSelectedTowerChanged(TowerTypes.YarnBall); }
}
