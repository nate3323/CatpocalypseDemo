using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;


public class CatInfoPanel : MonoBehaviour
{
    [Header("General")]

    [Tooltip("This is a reference to the parent of this panel. It is only used so that when you close this panel, it can reopen the main level select panel.")]
    [SerializeField] GameObject _ParentPanel;


    [Header("Info Pane Settings")]

    [SerializeField] TextMeshProUGUI _Text_DisplayName;
    [SerializeField] TextMeshProUGUI _Text_Size;
    [SerializeField] TextMeshProUGUI _Text_Speed;
    [SerializeField] TextMeshProUGUI _Text_SpawnRate;
    [SerializeField] TextMeshProUGUI _Text_HP;
    [SerializeField] TextMeshProUGUI _Text_PayOnDistraction;
    [SerializeField] TextMeshProUGUI _Text_Special;
    [SerializeField] TextMeshProUGUI _Text_Description;


    [Header("Cat Selection Pane Settings")]
    
    [SerializeField] TMP_Dropdown _Dropdown_CatSelection;

    [Space(10)]


    private CatInfoCollection _CatInfoCollection;



    private void Awake()
    {
        _CatInfoCollection = GetComponent<CatInfoCollection>();
    }

    // Start is called before the first frame update
    void Start()
    {
        PopulateCollectionDropdown();
        ResetUI();
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PopulateCollectionDropdown()
    {
        _Dropdown_CatSelection.options.Clear();


        // Spawn a button for each tower type in the list.
        for (int i = 0; i < _CatInfoCollection.Count; i++)
        {
            CatInfo info = _CatInfoCollection.GetCatInfo(i);
            _Dropdown_CatSelection.options.Add(new TMP_Dropdown.OptionData(info.DisplayName));
        }
    }

    public void OnSelectedCatChanged()
    {
        UpdateUI(_CatInfoCollection.GetCatInfo(_Dropdown_CatSelection.value));
    }

    public void ButtonClicked_Close()
    {
        // This is commented out since the level select dialog is no longer being used, but I left it in the scene for now.
        //_ParentPanel?.gameObject.SetActive(true);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// This function updates the UI whenever the selected cat is changed.
    /// </summary>
    /// <param name="info">The CatInfo object for the newly selected cat. It contains all the info about that cat.</param>
    private void UpdateUI(CatInfo info)
    {
        _Text_DisplayName.text = info.DisplayName;
        _Text_Size.text = Utils.InsertSpacesBeforeCapitalLetters(info.Size.ToString());
        _Text_Speed.text = Utils.InsertSpacesBeforeCapitalLetters(info.Speed.ToString());
        _Text_SpawnRate.text = Utils.InsertSpacesBeforeCapitalLetters(info.SpawnRate.ToString());
        _Text_HP.text = Utils.InsertSpacesBeforeCapitalLetters(info.HP.ToString());
        _Text_PayOnDistraction.text = Utils.InsertSpacesBeforeCapitalLetters(info.PayOnDistraction.ToString());
        _Text_Special.text = info.Special.ToString();
        _Text_Description.text = info.Description.ToString();
    }

    public void ResetUI()
    {
        _Dropdown_CatSelection.value = 0;
        _Dropdown_CatSelection.RefreshShownValue();

        UpdateUI(_CatInfoCollection.GetCatInfo(0));
    }
}
