using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SaveLoadScreen : MonoBehaviour
{

    /// <summary>
    /// TODO: Need to implement customizable save slots
    /// </summary>

    [SerializeField] private GameObject _SaveButton;
    [SerializeField] private GameObject _LoadButton;
    [SerializeField] private GameObject _InputPanel;
    [SerializeField] private TMP_InputField _NameInputField;
    [SerializeField] private GameObject[] _SaveFileButtons;

    private int _CurrentSaveSelected;

    public void Start()
    {
        _InputPanel.gameObject.SetActive(false);
        gameObject.SetActive(false);
        _CurrentSaveSelected = -1;
        IReadOnlyList<PlayerData> data = PlayerDataManager.Instance.ViewPlayerData();
        //Debug.Log(data.Count);
        for(int i = 0; i < _SaveFileButtons.Length; i++)
        {
            if(!data[i].name.Equals(""))
            {
                _SaveFileButtons[i].GetComponent<SaveSlot>().UpdateSaveLabel(data[i]);
            }
        }
    }

    public void OnSaveButton()
    {
        if(_CurrentSaveSelected == -1)
        {
            Debug.Log("No Save Slot selected.");
            return;
        }
        _InputPanel.gameObject.SetActive(true);
        _NameInputField.ActivateInputField();
    }

    public void OnSubmit()
    {
        if (_NameInputField.text.Equals(""))
        {
            PlayerDataManager.Instance.SetName("Save " + _CurrentSaveSelected);
        } else
        {
            PlayerDataManager.Instance.SetName(_NameInputField.text);
        }
        _NameInputField.DeactivateInputField();
        _InputPanel.gameObject.SetActive(false);
        PlayerDataManager.Instance.SaveGame(_CurrentSaveSelected);
        _SaveFileButtons[_CurrentSaveSelected].GetComponent<SaveSlot>().UpdateSaveLabel(PlayerDataManager.Instance.CurrentData);
    }

    public void OnSaveFileButton(string saveFile)
    {
        int newSave = -1;
        if(!int.TryParse(saveFile, out newSave))
        {
            Debug.Log("There is something wrong with the save file selection system.");
            return;
        }
        if(newSave == _CurrentSaveSelected)
        {
            _CurrentSaveSelected = -1;
        } else
        {
            _CurrentSaveSelected = newSave;
        }
        

    }

    public void OnLoadButton()
    {
        if(_CurrentSaveSelected == -1)
        {
            Debug.Log("No Save Slot selected.");
        } else
        {
            if (PlayerDataManager.Instance.LoadGame(_CurrentSaveSelected))
            {
                gameObject.SetActive(false);
                SceneLoader_Async.LoadSceneAsync("LevelSelection");
            }
            
        }
    }

    public void OnExitButton()
    {
        this.gameObject.SetActive(false);
    }

    public void ShowSaveScreen()
    {
        gameObject.SetActive(true);
        _LoadButton.SetActive(false);
        _SaveButton.SetActive(true);
    }

    public void ShowLoadScreen()
    {
        gameObject.SetActive(true);
        _LoadButton.SetActive(true);
        _SaveButton.SetActive(false);
    }
}