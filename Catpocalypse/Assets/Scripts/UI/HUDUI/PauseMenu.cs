using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    [SerializeField] SaveLoadScreen _Panel_SaveLoadScreen;
    [SerializeField] GameObject _Panel_Settings;
    private float _SavedGameSpeed;

    public void Awake()
    {
        this.gameObject.SetActive(false);
        _SavedGameSpeed = 0f;
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void OnPauseGame()
    {
        _SavedGameSpeed= Time.timeScale;
        Time.timeScale = 0f;
        this.gameObject.SetActive(true);
    }

    public void ButtonClicked_SaveScreen()
    {
        if (_Panel_SaveLoadScreen.gameObject.activeSelf)
        {
            return;
        }

        // Display the SaveLoadScreen panel.
        _Panel_SaveLoadScreen.ShowSaveScreen();

    }

    public void ButtonClicked_LoadScreen()
    {
        if (_Panel_SaveLoadScreen.gameObject.activeSelf)
        {
            return;
        }

        // Display the SaveLoadScreen panel.
        _Panel_SaveLoadScreen.ShowLoadScreen();

    }

    public void ButtonClicked_Settings()
    {
        if (_Panel_Settings.activeSelf)
        {
            return;
        }

        // Display the SaveLoadScreen panel.
        _Panel_Settings.SetActive(true);

    }

    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void OnResumeGame()
    {
        Time.timeScale = _SavedGameSpeed;
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Returns the player to the main menu.
    /// </summary>
    public void OnMainMenu()
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

    public void OnLevelSelect()
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
        SceneLoader_Async.LoadSceneAsync("LevelSelection");

    }
}
