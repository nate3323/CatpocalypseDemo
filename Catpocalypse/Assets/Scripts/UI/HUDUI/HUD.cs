using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;


/// <summary>
/// This class manages the HUD display. Call the static methods to update the HUD GUI elements.
/// That way you can access the HUD from anywhere in our codebase.
/// 
/// This class is a singleton since we should only ever have one instance of it.
/// </summary>
public class HUD : MonoBehaviour
{
    private static HUD Instance;


    [Header("Health Bar Refs")]
    [SerializeField] private Image _PlayerHealthBar;
    [SerializeField] private TextMeshProUGUI _PlayerHealthBarLabel;

    // Temporarily commented out until further notice
    /**
    [Header("Cuteness Bar Refs")]
    [SerializeField] private Image _CutenessBar;
    [SerializeField] private TextMeshProUGUI _CutenessBarLabel;
    */

    [Header("Message Bar Refs")]
    [SerializeField] private GameObject _MessageBar;
    [SerializeField] private TextMeshProUGUI _MessageHeaderLabel;
    [SerializeField] private TextMeshProUGUI _MessageLabel;


    [Header("Wave Info Display Refs")]
    [SerializeField] private Button _StartWaveButton;
    [SerializeField] private TextMeshProUGUI _WaveNumberLabel;
    [SerializeField] private TextMeshProUGUI _CatsRemainingLabel;
    [SerializeField] private Image _CatsRemainingBar;

    [Header("Player Money Display Refs")]
    [SerializeField] private TextMeshProUGUI _PlayerMoneyLabel;

    [Header("Robot Display Refs")]
    [SerializeField] private Button _ToggleRobotButton;
    [SerializeField] private Image _RobotRechargeImage;
    [SerializeField] private TextMeshProUGUI _RobotPowerLevelLabel;

    [Header("Level End Panels")]
    [SerializeField] private GameObject _DefeatScreen;
    [SerializeField] private GameObject _VictoryScreen;

    [Header("Game Speed Controls")]
    [SerializeField] private Button _PauseButton;
    [SerializeField] private Button _NormalSpeedButton;
    

    private RobotController _RobotController;


    private void Awake()
    {
        
        _RobotController = FindAnyObjectByType<RobotController>();
        if (_RobotController != null)
            _RobotController.OnBatteryLevelChanged += UpdateRobotBatteryLevelDisplay;

        Instance = this;

        _PauseButton.gameObject.SetActive(false);
        _NormalSpeedButton.gameObject.SetActive(false);
        _MessageBar.SetActive(false);
    }

    private void Start()
    {
        /* This is just test code.
        UpdatePlayerHealthDisplay(64, 100);
        UpdateWaveNumberDisplay(16);
        UpdatePlayerMoneyDisplay(542);
        */
    }

    private void Update()
    {
        // This test function opens the victory or defeat screen when you press the space bar.
        //TestRandomWinLoseText(false);
    }

    /// <summary>
    /// This function allows you to quickly test the victory or defeat screens by pressing the spacebar.
    /// It should be called from the Update() method.
    /// Pressing the space bar a second time will close the screen it opened.
    /// If the parameter is true, the victory screen is opened. Otherwise the defeat screen is opened.
    /// </summary>
    /// <param name="testVictoryScreen"></param>
    private void TestRandomWinLoseText(bool testVictoryScreen = true)
    {
        GameObject screen = testVictoryScreen ? _VictoryScreen : _DefeatScreen;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (screen.activeSelf)
            {
                screen.SetActive(false);
            }
            else
            {
                if (testVictoryScreen)
                    HUD.RevealVictory();
                else
                    HUD.RevealDefeat();
            }

        }
    }

    private void UpdateMessageBar(string header, string message)
    {
        _MessageHeaderLabel.text = header;
        _MessageLabel.text = message;
    }

    private void OnDestroy()
    {
        if (_RobotController != null)
            _RobotController.OnBatteryLevelChanged -= UpdateRobotBatteryLevelDisplay;
        
        // Handles the instance of a player exiting the game while paused, then returning and the game is still paused.
        ShowPauseButton();
    }

    public static void UpdatePlayerHealthDisplay(float currentHP, float maxHP)
    {
        Instance.PlayerHealthBar.fillAmount = Mathf.Clamp01(currentHP / maxHP);
        Instance.PlayerHealthBarLabel.text = $"";
    }
    /** Disabled until further notice
    public static void UpdateCutenessDisplay(float currentCuteness, float maxCuteness)
    {
        Instance.CutenessBar.fillAmount = Mathf.Clamp01(currentCuteness / maxCuteness);
        Instance.CutenessBarLabel.text = $"";
    }*/

    public static void UpdateWaveInfoDisplay(float catsDefeated, float totalCatsInWave)
    {
        Instance._CatsRemainingBar.fillAmount = Mathf.Clamp01(catsDefeated / totalCatsInWave);
        Instance.CatsRemainingLabel.text = $"{(totalCatsInWave - catsDefeated)}";
    }

    public static void UpdateWaveNumberDisplay(int waveNumber)
    {
        Instance.WaveNumberLabel.text = $"{waveNumber}";
    }

    public static void UpdatePlayerMoneyDisplay(float playerMoney)
    {
        Instance.PlayerMoneyLabel.text = $"{playerMoney:N2}";
    }

    public static void UpdateRobotBatteryLevelDisplay(object sender, RobotBatteryEventArgs e)
    {
        if (Instance.RobotPowerLevelLabel != null)
            Instance._RobotRechargeImage.fillAmount = e.NewBatteryLevel;
            Instance.RobotPowerLevelLabel.text = $"{(e.NewBatteryLevel * 100):F0}%";
    }


    public static void HideWaveDisplay()
    {
        if (Instance == null)
            return;


        Instance.CatsRemainingLabel.gameObject.SetActive(false);
        
        // The button state should always be the opposite of the labels' state.
        Instance.StartWaveButton.gameObject.SetActive(true);

        // The pause button should only be available when a wave is running
        Instance._NormalSpeedButton.gameObject.SetActive(false);
        Instance._PauseButton.gameObject.SetActive(false);
    }

    public static void ShowWaveDisplay()
    {
        if (Instance == null)
            return;


        // Clear the text labels, so the player won't potentially see an old value before it updates.
        Instance.CatsRemainingLabel.text = "";

        Instance.WaveNumberLabel.gameObject.SetActive(true);
        Instance.CatsRemainingLabel.gameObject.SetActive(true);

        // The button state should always be the opposite of the labels' state.
        Instance._StartWaveButton.gameObject.SetActive(false);

        // The pause button should only be available when a wave is running
        ShowPauseButton();
    }

    public static void CloseMessageBar()
    {
        Instance._MessageBar.SetActive(false);
    }

    public static void RevealVictory()
    {
        Instance._StartWaveButton.gameObject.SetActive(false);
        Instance.WaveNumberLabel.gameObject.SetActive(false);
        Instance.CatsRemainingLabel.gameObject.SetActive(false);
        Instance._PauseButton.gameObject.SetActive(false);

        Instance._VictoryScreen.SetActive(true);
    }

    public static void RevealDefeat()
    {
        Instance._StartWaveButton.gameObject.SetActive(false);
        Instance.WaveNumberLabel.gameObject.SetActive(false);
        Instance.CatsRemainingLabel.gameObject.SetActive(false);
        Instance._PauseButton.gameObject.SetActive(false);

        Instance._DefeatScreen.SetActive(true);
    }

    public static void ShowPauseButton()
    {
        Instance._NormalSpeedButton.gameObject.SetActive(false);
        Instance._PauseButton.gameObject.SetActive(true);
        Time.timeScale = 1;
    }

    public static void ShowPlayButton()
    {
        Instance._NormalSpeedButton.gameObject.SetActive(true);
        Instance._PauseButton.gameObject.SetActive(false);
        Time.timeScale = 0;
    }

    public static void ShowMessage(string header, string message)
    {
        Instance.UpdateMessageBar(header, message);
        Instance.MessageBar.gameObject.SetActive(true);
        Instance._MessageHeaderLabel.gameObject.SetActive(true);
        Instance._MessageLabel.gameObject.SetActive(true);
    }


    public Image PlayerHealthBar { get { return _PlayerHealthBar; } }
    public TextMeshProUGUI PlayerHealthBarLabel { get { return _PlayerHealthBarLabel; } }

    /** Cuteness System Currently Disabled
    public Image CutenessBar { get { return _CutenessBar; } }
    public TextMeshProUGUI CutenessBarLabel { get { return _CutenessBarLabel; } }
    */

    public Button StartWaveButton { get { return _StartWaveButton; } }
    public TextMeshProUGUI WaveNumberLabel { get { return _WaveNumberLabel; } }
    public TextMeshProUGUI CatsRemainingLabel { get { return _CatsRemainingLabel; } }

    public TextMeshProUGUI PlayerMoneyLabel { get { return _PlayerMoneyLabel; } }

    public TextMeshProUGUI RobotPowerLevelLabel { get { return _RobotPowerLevelLabel; } }
    public Button ToggleRobotButton { get { return _ToggleRobotButton; } }
    public GameObject MessageBar { get { return _MessageBar; } }
}
