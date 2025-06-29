using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


/// <summary>
/// This class runs the build tower UI.
/// </summary>
[RequireComponent(typeof(TowerInfoCollection))]
public class TowerSelectorUI : MonoBehaviour
{
    [SerializeField]
    private TowerInfoPopupUI _TowerInfoPopupUI;

    [SerializeField]
    private Image _RingImage;

    [Tooltip("This is the parent object of all the tower select buttons.")]
    [SerializeField] GameObject _ButtonsParent;

    [Tooltip("This field adjusts the radius used to position the buttons on the ring.")]
    [Min(0)]
    [SerializeField] int _ButtonsRadius = 80;


    [SerializeField]
    private GameObject notEnoughFundsScreen;
    [SerializeField]
    private PlayerMoneyManager playerMoneyManager;
    [SerializeField]
    private TextMeshProUGUI cutenessMeterMaxedText;


    public bool inUse;
    private GameObject towerSpawner;

    private TowerInfoCollection _TowerInfoCollection;

    private string _BuildFailedHeader;
    private string _BuildFailedMessage;


    private void Awake()
    {
        notEnoughFundsScreen.SetActive(false);

        inUse = false;


        // This makes it so the click event is not fired if you click on a transparent part of the ring images, such as in the corners outside the ring.
        _RingImage.alphaHitTestMinimumThreshold = 0.9f;    


        _TowerInfoPopupUI.gameObject.SetActive(false);

        _TowerInfoCollection = GetComponent<TowerInfoCollection>();

    }

    private void Start()
    {
        PositionButtonsAroundRing();

        ConnectTowerButtonMouseOverEvents();
        _BuildFailedHeader = "Build Failed!";
        _BuildFailedMessage = "We don't have enough funds to build that tower!";
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


        UpdatePosition();

        if (Mouse.current.leftButton.isPressed)
        {
            CheckIfClickedOutsideUI();
        }
    }

    /// <summary>
    /// This function updates the position of the tower selector UI on the screen. This keeps it centered on the selected
    /// tower base when the user zooms in/out.
    /// </summary>
    private void UpdatePosition()
    {
        RectTransform rectTrans = GetComponent<RectTransform>();

        TowerBase selected = TowerBase.SelectedTowerBase;
        if (selected == null)
            return;

        Vector2 position = Camera.main.WorldToScreenPoint(selected.transform.position);
        //Debug.Log($"World: {selected.transform.position}    Screen: {position}");

        GetComponent<RectTransform>().position = position;

        //TODO: If Position is to the left or right of screen, ensure the ring "faces" the correct side of the screen.
    }

    private void PositionButtonsAroundRing()
    {
        // Get the buttom most point on the ring to start at.
        Vector2 bottomPoint = new Vector2(0, -_ButtonsRadius);

        int buttonCount = _ButtonsParent.transform.childCount;
        float angleBetweenButtons = 120f / buttonCount;

        Quaternion rotation = Quaternion.identity;
        for (int i = 0; i < buttonCount; i++)
        {
            Transform curButton = _ButtonsParent.transform.GetChild(i);

            rotation.eulerAngles = new Vector3(0, 0, 45 + (i * angleBetweenButtons));
            curButton.position = transform.position + (rotation * bottomPoint);
        }
    }

    private void ConnectTowerButtonMouseOverEvents()
    {
        // I had to create a separate component and add it to each button as you can see, since Button does not have a MouseOver event for some odd reason.
        // Well it does, but you have to override OnMouseEnter in a class that's inherits from Button.

        int buttonCount = _ButtonsParent.transform.childCount;
        for (int i = 0; i < buttonCount; i++)
        {
            BuildTowerButtonUI curButton = _ButtonsParent.transform.GetChild(i).GetComponent<BuildTowerButtonUI>();

            // Hook up the MouseEnter event
            curButton.Button.OnMouseEnter += OnMouseEnteredAnyTowerButton;

            // Hook up the MouseExit event
            curButton.Button.OnMouseExit += OnMouseExitedAnyTowerButton;

        }

    }

    private void OnMouseEnteredAnyTowerButton(object sender, EventArgs e)
    {
        BuildTowerButtonUI clickedBuildButtonUI = (sender as CustomButton).GetComponent<BuildTowerButtonUI>();
        if (clickedBuildButtonUI.name == "Close Button")
            return;


        RectTransform rectTransform = _TowerInfoPopupUI.GetComponent<RectTransform>();

        // Calculate the position of the tower info popup.
        Vector3 popupPosition = rectTransform.anchoredPosition;
        popupPosition = clickedBuildButtonUI.GetComponent<RectTransform>().anchoredPosition;
        Vector2 offset = new Vector2(-clickedBuildButtonUI.RectTransform.rect.width / 2, 0f);
        rectTransform.anchoredPosition = popupPosition - (Vector3)offset;


        // Get the corresponding tower info so we can use it to fill in the popup.
        TowerInfo towerInfo = _TowerInfoCollection.GetTowerInfo(clickedBuildButtonUI.TowerType);
        if (towerInfo == null)
        {
            Debug.LogError($"There is no TowerInfo scriptable object created for tower type \"{Enum.GetName(typeof(TowerTypes), clickedBuildButtonUI.TowerType)}\"");
            return;
        }

        _TowerInfoPopupUI.UpdatePopupUI(towerInfo);
        _TowerInfoPopupUI.gameObject.SetActive(true);
    }

    private void OnMouseExitedAnyTowerButton(object sender, EventArgs e)
    {
        _TowerInfoPopupUI.gameObject.SetActive(false);
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
        if (playerMoneyManager.SpendMoney(towerSpawner.GetComponent<TowerSpawn>().MoneyToSpend(selection)))
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

            HUD.ShowMessage(_BuildFailedHeader, _BuildFailedMessage);
        }

    }

    private void CloseUI()
    {
        _TowerInfoPopupUI.gameObject.SetActive(false); // Close the tower info popup if it is currently displayed.

        cutenessMeterMaxedText.gameObject.SetActive(false);
        gameObject.SetActive(false);

        inUse = false;
    }

    public void CheckIfClickedOutsideUI()
    {
        // If the user clicked a spot that is not on the GUI, then close the tower UI.
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // Check if the mouse position is outside the ring radius. If so, close the towe UI. We don't want it to close if you
            // click in the transparent area in the middle of the UI, only if you click outside it and not on UI.
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (Vector2.Distance(_RingImage.transform.position, mousePos) > _ButtonsRadius + 40f) // We add extra here since the button radius is a little bit inside the ring.
            {
                OnClose();
            }
        }
    }

}
