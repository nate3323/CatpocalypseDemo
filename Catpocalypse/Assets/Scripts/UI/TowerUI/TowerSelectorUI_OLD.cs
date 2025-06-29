using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(TowerInfoCollection))]
public class TowerSelectorUI_OLD : MonoBehaviour
{
    [SerializeField]
    private GameObject buildTowerUI;

    [SerializeField]
    private TowerInfoPopupUI _TowerInfoPopupUI;


    [Header("Button References")]
    [SerializeField]
    private Button laserPointerTowerBtn;
    [SerializeField]
    private Button scratchingPostTowerBtn;
    [SerializeField]
    private Button cucumberThrowerTowerBtn;
    [SerializeField]
    private Button stringWaverTowerBtn;
    [SerializeField]
    private Button yarnBallTowerBtn;
    [SerializeField]
    private Button nonAllergicTowerBtn;
    [SerializeField]
    private Button closeBtn;
    [SerializeField]
    private GameObject notEnoughFundsScreen;
    [SerializeField]
    private PlayerMoneyManager playerMoneyManager;
    [SerializeField]
    private TextMeshProUGUI cutenessMeterMaxedText;


    public bool inUse;
    private GameObject towerSpawner;

    private TowerInfoCollection _TowerInfoCollection;


    private void Awake()
    {
        notEnoughFundsScreen.SetActive(false);

        inUse = false;


        _TowerInfoPopupUI.gameObject.SetActive(false);

        ConnectTowerButtonMouseOverEvents();
    }

    private void Start()
    {
        _TowerInfoCollection = GetComponent<TowerInfoCollection>();
    }

    private void Update()
    {
        // Close the tower selector UI if no tower base is selected.
        // This is because if the user clicks on a tower button while no tower base is selected, it causes a null reference exception.
        if (TowerBase.SelectedTowerBase == null && gameObject.activeSelf)
        {
            inUse = false;
            gameObject.SetActive(false);
        }
    }

    private void ConnectTowerButtonMouseOverEvents()
    {
        // I had to create a separate component and add it to each button as you can see, since Button does not have a MouseOver event for some odd reason.
        // Well it does, but you have to override OnMouseEnter in a class that's inherits from Button.

        // Hook up the MouseEnter events
        laserPointerTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseEnter += OnMouseEnteredAnyTowerButton;
        scratchingPostTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseEnter += OnMouseEnteredAnyTowerButton;
        cucumberThrowerTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseEnter += OnMouseEnteredAnyTowerButton;
        stringWaverTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseEnter += OnMouseEnteredAnyTowerButton;
        yarnBallTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseEnter += OnMouseEnteredAnyTowerButton;
        nonAllergicTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseEnter += OnMouseEnteredAnyTowerButton;


        // Hook up the MouseExit events
        laserPointerTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseExit += OnMouseExitedAnyTowerButton;
        scratchingPostTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseExit += OnMouseExitedAnyTowerButton;
        cucumberThrowerTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseExit += OnMouseExitedAnyTowerButton;
        stringWaverTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseExit += OnMouseExitedAnyTowerButton;
        yarnBallTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseExit += OnMouseExitedAnyTowerButton;
        nonAllergicTowerBtn.GetComponent<TowerSelectButtonMouseOver>().OnMouseExit += OnMouseExitedAnyTowerButton;
    }

    private void OnMouseEnteredAnyTowerButton(object sender, TowerSelectButtonMouseOver.MouseOverEventArgs e)
    {
        TowerSelectButtonMouseOver mouseOverComponent = (sender as TowerSelectButtonMouseOver);

        RectTransform rectTransform = _TowerInfoPopupUI.GetComponent<RectTransform>();

        Vector3 popupPosition = rectTransform.anchoredPosition;        
        popupPosition.x = mouseOverComponent.GetComponent<RectTransform>().anchoredPosition.x;
        popupPosition.x -= (mouseOverComponent.GetWidth() / 2);
        rectTransform.anchoredPosition = popupPosition;


        TowerInfo towerInfo = _TowerInfoCollection.GetTowerInfo(e.TowerType);
        if (towerInfo == null)
        {
            Debug.LogError($"There is no TowerInfo scriptable object created for tower type \"{Enum.GetName(typeof(TowerTypes), e.TowerType)}\"");
            return;
        }

        _TowerInfoPopupUI.UpdatePopupUI(towerInfo);
        _TowerInfoPopupUI.gameObject.SetActive(true);
    }

    private void OnMouseExitedAnyTowerButton(object sender, TowerSelectButtonMouseOver.MouseOverEventArgs e)
    {
        _TowerInfoPopupUI.gameObject.SetActive(false);

        TowerSelectButtonMouseOver mouseOverComponent = (sender as TowerSelectButtonMouseOver);
    }

    public void SetCurrentSelectedSpawn(GameObject current)
    {
        towerSpawner = current;
    }

    public void OnLaserPointerTowerSelect()
    {
        OnBuildSelect(0);
    }

    public void OnScratchingPostTowerSelect()
    {
        OnBuildSelect(1);
    }

    public void OnCucumberThrowerTowerSelect()
    {
        OnBuildSelect(2);
    }

    public void OnStringWaverTowerSelect()
    {
        OnBuildSelect(3);
    }

    public void OnYarnBallTowerSelect()
    {
        OnBuildSelect(4);
    }
    public void OnNATowerSelect()
    {
        OnBuildSelect(5);
    }

    public void OnClose()
    {
        towerSpawner.transform.parent.GetComponent<TowerBase>().Deselect();

        towerSpawner = null;

        CloseUI();
    }

    private void OnBuildSelect(int selection)
    {
        //Debug.Log("Active: " + gameObject.activeSelf);
        if(playerMoneyManager.SpendMoney(towerSpawner.GetComponent<TowerSpawn>().MoneyToSpend(selection))) 
        {
            towerSpawner.GetComponent<TowerSpawn>().BuildTower(selection);
            CloseUI();
        } 
        else
        {
            // I put this here to fix a bizarre glitch where occasionally I'm getting an error that the coroutine couldn't be started
            // because the TowerSelectUI is not active. I cannot find any reason for that to be happening, so I did this instead.
            if (gameObject.activeSelf == false)
                gameObject.SetActive(true);

            StartCoroutine(RevealNotEnoughFundsScreen());
        }
        
    }

    private void CloseUI()
    {
        _TowerInfoPopupUI.gameObject.SetActive(false); // Close the tower info popup if it is currently displayed.

        cutenessMeterMaxedText.gameObject.SetActive(false);
        gameObject.SetActive(false);

        inUse = false;
    }

    private IEnumerator RevealNotEnoughFundsScreen()
    {
        notEnoughFundsScreen.SetActive(true);
        yield return new WaitForSeconds(1f);
        notEnoughFundsScreen.SetActive(false);
    }

}
