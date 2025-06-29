using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class VictoryScreen : MonoBehaviour
{
    [Tooltip("The list of random victory text messages that can appear on this screen.")]
    [SerializeField]
    private List<string> _VictoryTextMessages;

    [Tooltip("This is the text element where the random victory message gets displayed.")]
    [SerializeField]
    private TextMeshProUGUI _RandomVictoryText;

    [SerializeField]
    private AudioSource _victorySound;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Select a random display text every time this panel is opened.
        SelectRandomDisplayText();
        _victorySound.Play();
        PlayerDataManager.Instance.UpdateLevelsCompleted(1);
    }

    private void SelectRandomDisplayText()
    {
        int index = Random.Range(0, _VictoryTextMessages.Count);

        _RandomVictoryText.text = _VictoryTextMessages[index];
    }

    public void OnNextLevelClicked()
    {
        GameObject[] bases = GameObject.FindGameObjectsWithTag("TowerBase");
        foreach (GameObject tb in bases)
        {
            if (tb.GetComponent<TowerBase>().hasTower == true)
            {
                tb.GetComponent<TowerBase>().DestroyTower();
            }
        }

        //SceneManager.LoadScene("Level1");
        SceneLoader_Async.LoadSceneAsync("LevelSelection");
    }

    public void OnMainMenuClicked()
    {
        GameObject[] bases = GameObject.FindGameObjectsWithTag("TowerBase");
        foreach (GameObject tb in bases)
        {
            if (tb.GetComponent<TowerBase>().hasTower == true)
            {
                tb.GetComponent<TowerBase>().DestroyTower();
            }
        }

        //SceneManager.LoadScene("MainMenu");
        SceneLoader_Async.LoadSceneAsync("MainMenu");
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

}
