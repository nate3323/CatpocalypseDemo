using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TowerInfoPopupUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _TitleText;
    [SerializeField] private TextMeshProUGUI _DescriptionText;
    [SerializeField] private TextMeshProUGUI _CostText;
    [SerializeField] private TextMeshProUGUI _DamageText;
    [SerializeField] private TextMeshProUGUI _RangeText;
    [SerializeField] private TextMeshProUGUI _AOE_Text;
    [SerializeField] private TextMeshProUGUI _FireRateText;
    [SerializeField] private TextMeshProUGUI _SpecialText;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void UpdatePopupUI(TowerInfo towerInfo)
    {
        //_Icon.sprite = towerInfo.Icon;
        _TitleText.text = towerInfo.DisplayName;
        _DescriptionText.text = towerInfo.Description;
        _CostText.text = $"${towerInfo.Cost}";
        _DamageText.text = Utils.InsertSpacesBeforeCapitalLetters(Enum.GetName(typeof(Ratings), towerInfo.Damage));
        _RangeText.text = Utils.InsertSpacesBeforeCapitalLetters(Enum.GetName(typeof(Sizes), towerInfo.Range));
        _AOE_Text.text = Utils.InsertSpacesBeforeCapitalLetters(Enum.GetName(typeof(Sizes), towerInfo.AOE_Range));
        _FireRateText.text = Utils.InsertSpacesBeforeCapitalLetters(Enum.GetName(typeof(Ratings), towerInfo.FireRate));
        _SpecialText.text = towerInfo.Special;
    }

}
