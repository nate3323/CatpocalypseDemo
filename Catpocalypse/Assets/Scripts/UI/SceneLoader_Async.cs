using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEditor;
using TMPro;

using Random = UnityEngine.Random;


public class SceneLoader_Async : MonoBehaviour
{
    public static SceneLoader_Async Instance;


    [Header("UI References")]

    [SerializeField] private Slider _ProgressBar;
    [SerializeField] private TextMeshProUGUI _ProgressBarText;
    [SerializeField] private TextMeshProUGUI _LoadingScreenMessageText;
    [SerializeField] private TextMeshProUGUI _PressAnyKeyText;


    [Header("Settings")]

    [Tooltip("If enabled, the loading screen will stay open until the user presses any key. If UseLoadingScreenCloseDelay is also enabled, then the loading screen will close either when the user presses any button or when the delay expires, so whichever happens first.")]
    [SerializeField] private bool _WaitForButtonPressToCloseLoadingScreen = true;
    
    [Space(10)]
    
    [Tooltip("If enabled, the loading screen will wait have a delay before closing after the next scene finishes loading. The length of this delay is specifed by the LoadingScreenCloseDelay setting.")]
    [SerializeField] private bool _UseLoadingScreenCloseDelay;
    [Tooltip("If UseLoadingScreenCloseDelay is enabled, this sets how long to wait after the next scene finishes loading before the loading screen should close.")]
    [SerializeField] private float _LoadingScreenCloseDelay = 5f;

    [Space(10)]

    [Tooltip("This list specifies all of the random messages that can appear on the loading screen.")]
    [SerializeField] private List<string> _LoadingScreenMessages;


    private static bool IsLoadingScreenActive;

    private static bool ReceivedInput;

    IDisposable _UserInputListener;



    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("A SceneLoader_Async already exists! Self destructing.");
            Destroy(gameObject);
            return;
        }


        Instance = this;

        // This will only be displayed if _WaitForButtonPressToCloseLoadingScreen is set to true.
        _PressAnyKeyText.gameObject.SetActive(false);

        // Listen for user input
        _UserInputListener = InputSystem.onAnyButtonPress.Call(OnAnyInput);

        // Tell Unity to not destroy this game object when a new scene is loaded.
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!IsLoadingScreenActive)
        {
            // Disable the loading screen.
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        _UserInputListener?.Dispose();
    }

    public static void LoadSceneAsync(string sceneToLoad)
    {
        if (Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Loading Screen");
            GameObject loadingScreen = Instantiate(prefab, Vector3.zero, Quaternion.identity);
           
            loadingScreen.GetComponent<SceneLoader_Async>().LoadScene_Async(sceneToLoad);
        }
        else
        {
            Instance.LoadScene_Async(sceneToLoad);
        }
    }


    public void LoadScene_Async(string sceneToLoad)
    {
        if (IsLoadingScreenActive)
        {
            Debug.LogError($"Cannot load the scene \"{sceneToLoad}\", because the loading screen is already loading another scene!");
            return;
        }

        // Select a randome message.
        int index = Random.Range(0, _LoadingScreenMessages.Count);

        // Display the randomly selected message.
        _LoadingScreenMessageText.text = _LoadingScreenMessages[index];

        // Enable the loading screen GameObject.
        gameObject.SetActive(true);
        
        // Start loading the specified scene.
        StartCoroutine(LoadScene(sceneToLoad));        
    }

    private IEnumerator LoadScene(string sceneToLoad)
    {
        IsLoadingScreenActive = true;

        _ProgressBar.value = 0f;
        _ProgressBarText.text = "0%";

        // Start loading the specified scene asynchonrously.
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Wait while the scene loads and show progress.
        while (!loadOperation.isDone)
        {
            float progress = loadOperation.progress / 1.0f;
            _ProgressBar.value = progress;
            _ProgressBarText.text = progress.ToString("P0");
            yield return null;  
        }


        // I added these two lines just because the progress won't necessarily by at exactly 100% after the above loop.
        _ProgressBar.value = 1f;
        _ProgressBarText.text = "100%";

        // If the option is enabled, then keep the loading screen up until the user presses any button.
        if (_WaitForButtonPressToCloseLoadingScreen || _UseLoadingScreenCloseDelay)
        {
            ReceivedInput = false;
            float timer = 0f;

            // If _WaitForButtonPressToCloseLoadingScreen is enabled, then wait until the user presses any button before closing the loading screen.
            // If _UseLoadingScreenCloseDelay is enabled, the wait for the specified delay time before closing the loading screen.
            // If both options are enabled, the loading screen will close either when the user presses any button or when the delay expires, so whichever happens first.
            while (true)                   
            {
                // TODO : Create a fade-in, fade-out text stating press any button to continue

                // If _WaitForButtonPressToCloseLoadingScreen is enabled, then wait until the user presses any button before closing the loading screen.
                if (_WaitForButtonPressToCloseLoadingScreen && ReceivedInput)
                {
                    break;
                }

                // If _UseLoadingScreenCloseDelay is enabled, the wait for the specified delay time before closing the loading screen.
                timer += Time.deltaTime;
                if (_UseLoadingScreenCloseDelay && timer > _LoadingScreenCloseDelay)
                {
                    break;
                }


                // Wait one frame.
                yield return null; 

            } // end while

            ReceivedInput = false;
        }


        // Close the loading screen.
        IsLoadingScreenActive = false;
        gameObject.SetActive(false);
    }

    private void OnAnyInput(InputControl control)
    {
        ReceivedInput = true;
    }
}
