using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerManipulationUI : MonoBehaviour
{
    [SerializeField]
    private GameObject destroyTowerUI;

    [Tooltip("The GameObject that displays the properties of the selected tower on the left of the tower manipulation bar.")]
    [SerializeField] TowerPropertiesPanel _TowerPropertiesPanel;

    [SerializeField]
    private Button destroyBtn;
    [SerializeField]
    private Button upgradeBtn;
    [SerializeField]
    private PlayerMoneyManager playerMoneyManager;

    [Tooltip("The edit rally point button.")]
    [SerializeField]
    private Button _RallyPointButton;

    [Tooltip("The color to tint the edit rally point button while in rally point edit mode.")]
    [SerializeField]
    private Color _RallyPointButtonTint;
    
    private TowerBase currentSelectedBase;

    public bool inUse;

    public bool IsInEditRallyPointMode;

    private float _TimeSinceLastRallyPointSet;

    private string _FailedUpgradeHeader;
    private string _NotEnoughFundsMessage;
    private string _TowerMaxLevelMessage;

    public void Start()
    {
        if (Time.time < 1)
        {
            inUse = false;
            this.gameObject.SetActive(false);
        }
        _FailedUpgradeHeader = "Upgrade Failed!";
        _NotEnoughFundsMessage = "Not Enough Funds!";
        _TowerMaxLevelMessage = "Tower is already max level!";
    }

    private void Update()
    {
        if(!inUse)
        {
            this.gameObject.SetActive(false);
            return;
        } else
        {
            UpdatePosition();
        }

        _TimeSinceLastRallyPointSet += Time.deltaTime;

        if (Mouse.current.leftButton.isPressed)
        {
            if (currentSelectedBase.hasTower && IsInEditRallyPointMode)
            {
                // Do a ray cast to find the point in the world the user clicked on.
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    // Is this position within range of the tower and on a path?
                    if (ValidateNewRallyPoint(hit))
                    {
                        currentSelectedBase.tower.SetRallyPoint(new Vector3(hit.point.x, 0f, hit.point.z));
                        currentSelectedBase.ShowRallyPoint();

                        _TimeSinceLastRallyPointSet = 0f;
                    }
                }
                else
                {
                    Debug.LogError("The mouse click somehow hit nothing!");
                }
            }
            // I had to add this forced delay to stop it from closing the tower manipulation UI on the next frame after
            // editing a rally point. At that point, IsInRallyPointMode is no longer true, so the above if fails and
            // so it would drop down here and close the UI. This delay timer prevents that.
            else if (_TimeSinceLastRallyPointSet >= 1f)
            {              
                // If the mouse clicked outside of this UI, then close it.
                CheckIfClickedOutsideUI();
            }
        }
        else
        {
            // The left mouse button is not being pressed.
        }
    }

    private void UpdatePosition()
    {
        RectTransform rectTrans = GetComponent<RectTransform>();

        TowerBase selected = TowerBase.SelectedTowerBase;
        if (selected == null)
            return;

        Vector2 position = Camera.main.WorldToScreenPoint(selected.transform.position);
        position[1] -= 60;
        //Debug.Log($"World: {selected.transform.position}    Screen: {position}");

        GetComponent<RectTransform>().position = position ;
    }

    void OnEnable()
    {
        _TimeSinceLastRallyPointSet = 0f;    
    }

    public void SetCurrentSelectedBase(TowerBase current)
    {
        if (currentSelectedBase != current)
            ExitEditRallyPointMode();


        currentSelectedBase = current;

        RefreshUI();


        if (currentSelectedBase != null && gameObject.activeSelf)
        {
            currentSelectedBase.ShowRangeOutline();
            currentSelectedBase.ShowRallyPoint();
        }
    }

    public void OnRallyPointButtonClicked()
    {
        if (!IsInEditRallyPointMode)
            EnterEditRallyPointMode();
        else
            ExitEditRallyPointMode();
    }

    public void OnDestroyButtonClicked()
    {
        ExitEditRallyPointMode();

        currentSelectedBase.DestroyTower();
        playerMoneyManager.SpendMoney((-1) * currentSelectedBase.refundVal);
        this.gameObject.SetActive(false);
    }

    public void OnUpgradeButtonClicked()
    {
        ExitEditRallyPointMode();
        if (currentSelectedBase.tower.towerLevel < currentSelectedBase.tower.maxLevel)
        {
            if(playerMoneyManager.SpendMoney(currentSelectedBase.tower.GetUpgradeCost()))
            {
                currentSelectedBase.tower.Upgrade();
            } else
            {
                HUD.ShowMessage(_FailedUpgradeHeader, _NotEnoughFundsMessage);
            }   
        } 
        else
        {
            HUD.ShowMessage(_FailedUpgradeHeader, _TowerMaxLevelMessage);
        }
        this.gameObject.SetActive(false);
    }

    public void OnCloseButtonClicked()
    {
        ExitEditRallyPointMode();

        if (currentSelectedBase != null)
        {
            currentSelectedBase.HideRangeOutline();
            currentSelectedBase.HideRallyPoint();
        }

        this.gameObject.SetActive(false);
    }

    public void RefreshUI()
    {
        _TowerPropertiesPanel.RefreshUI();
    }

    public void CheckIfClickedOutsideUI()
    {
        // If the user clicked a spot that is not on the this GUI, then close the tower manipulation UI.
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            OnCloseButtonClicked();
        }
    }

    public void EnterEditRallyPointMode()
    {
        IsInEditRallyPointMode = true;

        SetRallyButtonTint(_RallyPointButtonTint);
    }

    public void ExitEditRallyPointMode()
    {
        IsInEditRallyPointMode = false;

        // Deselect the edit rally point button. If we leave it selected, the hover effect doesn't work.
        EventSystem.current.SetSelectedGameObject(null);

        SetRallyButtonTint(Color.white);
    }

    private void SetRallyButtonTint(Color color)
    {
        ColorBlock colors = _RallyPointButton.colors;

        // If we are removing the tint, then set the highlight color back to its default value by copying it from another button.
        if (color == Color.white)
            colors.highlightedColor = upgradeBtn.colors.highlightedColor;
        else
            colors.highlightedColor = color;


        colors.normalColor = color;
        colors.selectedColor = color;

        _RallyPointButton.colors = colors;
    }

    /// <summary>
    /// Checks if the user clicked in a valid spot for the new rally point.
    /// </summary>
    private bool ValidateNewRallyPoint(RaycastHit hit)
    {
        float distance = Vector3.Distance(currentSelectedBase.transform.position, hit.point);


        // Is this point on a path?
        int layerMask = 1 << hit.collider.gameObject.layer;
        if ((layerMask & LayerMask.GetMask("Pathing")) == 0)
            return false;


        // Is this point within range of the tower?
        if (distance <= currentSelectedBase.tower.RangeRadius)
            return true;
        else
            return false;
    }

    /// <summary>
    /// This function recursively moves up the object hierarchy from the passed in transform.
    /// </summary>
    /// <param name="possibleChild">The object to check to see if it is a child of this UI.</param>
    /// <returns>True if the object is a child of this UI or one of its children.</returns>
    private bool IsChild(Transform possibleChild)
    {
        if (possibleChild.parent == null)
            return false;

        if (possibleChild.parent == transform)
            return true;
        else
            return IsChild(possibleChild.parent);
    }
}
