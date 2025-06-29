using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DefeatScreen : MonoBehaviour
{
    [Tooltip("The list of random defeat text messages that can appear on this screen.")]
    [SerializeField]
    private List<string> _DefeatTextMessages;

    [Tooltip("This is the text element where the random defeat message gets displayed.")]
    [SerializeField]
    private TextMeshProUGUI _RandomDefeatText;

    [SerializeField]
    private AudioSource _defeatSound;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Select a random display text every time this panel is opened.
        SelectRandomDisplayText();
        _defeatSound.Play();
    }

    private void SelectRandomDisplayText()
    {
        int index = Random.Range(0, _DefeatTextMessages.Count);

        _RandomDefeatText.text = _DefeatTextMessages[index];
    }

    public void OnRetryClicked()
    {
        GameObject[] bases = GameObject.FindGameObjectsWithTag("TowerBase");
        foreach (GameObject tb in bases)
        {
            if (tb.GetComponent<TowerBase>().hasTower == true)
            {
                tb.GetComponent<TowerBase>().DestroyTower();
            }
        }


        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        SceneLoader_Async.LoadSceneAsync("SceneManager.GetActiveScene().name");
    }

    public void OnLevelSelectClicked()
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
