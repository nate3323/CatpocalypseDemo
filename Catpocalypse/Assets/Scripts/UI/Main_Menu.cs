using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu : MonoBehaviour
{
    [SerializeField]
    private SaveLoadScreen _LoadScreen;

    [SerializeField]
    private GameObject _SettingsScreen;

    // Starts the game
    public void OnPlayButton()
    {
        SceneLoader_Async.LoadSceneAsync("LevelSelection");
    }

    public void OnLoadButton()
    {
        _LoadScreen.ShowLoadScreen();
    }

    // Opens options
    public void OnOptionsButton()
    {
        //SceneManager.LoadScene("Options");
        _SettingsScreen.SetActive(true);
    }

    // Closes the game
    public void OnExitButton()
    {
        Application.Quit();
    }
}