using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;



public class TowerBase : MonoBehaviour
{
    public static TowerBase SelectedTowerBase;

    // This event fires when any tower base in the level gets selected.
    public static event EventHandler OnAnyTowerBaseWasSelected;


    public bool usable;  
    
    public TowerSelectorUI towerSelectorUI;
    public TowerManipulationUI towerManipulationUI;
    public Material towerHovered;
    public Material towerNotHovered;
    public Material towerSelected;
    [SerializeField]
    private GameObject towerSpawn;
    public bool hasTower;
    public bool hoveredOver;
    [SerializeField]
    public LayerMask layer;
    public float refundVal;
    public bool IsSelected = false;

    [Tooltip("This is the vertical position that the rally point GameObject will be at. This propery just gives us an easy way to move all rally point flags up/down as needed by just editing the TowerBase prefab in the prefabs folder.")]
    [SerializeField]
    private float _RallyPointVerticalOffset = 0.5f;


    private Tower _Tower;
    private RobotController _Robot;

    // This is just a capsule with a shader graph on it that I made.
    // It's already a child object, so the OnEnable() method just finds it and sets this variable.
    // However, it is also its own prefab, so the parameters on the outline material can be changed once and affect all towers in one go!
    protected GameObject _TowerRangeOutline;

    // The RallyPoint GameObject.
    protected GameObject _RallyPointGameObject;

    private void Awake()
    {
        hasTower = false;
        hoveredOver = false;
        tower = null;
        refundVal = 0;

        _Robot = FindObjectOfType<RobotController>();

        OnAnyTowerBaseWasSelected += OnAnyTowerBaseSelected;


        // But first, get a reference to it if we don't have one yet.
        Transform t = transform.Find("TowerRangeOutline");
        if (t != null)
        {
            _TowerRangeOutline = t.gameObject;

            // Hide the outline for now.
            HideRangeOutline();
        }
        else
        {
            Debug.LogError($"Tower base \"{gameObject.name}\" does not have a \"TowerRangeOutline\" child object for displaying the tower's range!", gameObject);
        }


        Transform t2 = transform.Find("RallyPoint");
        if (t2 != null)
        {
            _RallyPointGameObject = t2.gameObject;

            // Hide the rally point for now.
            HideRallyPoint();
        }
        else
        {
            Debug.LogError($"Tower base \"{gameObject.name}\" does not have a \"RallyPoint\" child object for displaying the tower's rally point!", gameObject);
        }
    }

    void OnMouseEnter()
    {
        // Check if the mouse is over a UI element or the robot is active. If so, then we should ignore the hover.
        if (EventSystem.current.IsPointerOverGameObject() || 
            (_Robot != null && _Robot.IsActive))
        {
            return;
        }


        hoveredOver = true;

        // Don't set the hover color if the tower is selected.
        if (!IsSelected)
            gameObject.GetComponent<Renderer>().material = towerHovered;
    }


    void OnMouseExit()
    {
        // NOTE: I did not add the EventSystem check here like I did in OnMouseEnter() and OnMouseUpAsButton().
        //       This is because we don't need it here. If we put it here, it will cause the issue that you 
        //       could move the mouse off of the tower base, and it will then stay highlighted errouneously
        //       if the mouse stays on a UI element while moving off of the tower base.



        hoveredOver = false;
        
        // Don't restore normal material unless the tower is not selected.
        if (!IsSelected)
            gameObject.GetComponent<Renderer>().material = towerNotHovered;
    }

    void OnMouseUpAsButton()
    {
        // Check if the mouse is over a UI element, or the robot is active. If so, then we should ignore the click.
        if (EventSystem.current.IsPointerOverGameObject() ||
            (_Robot != null && _Robot.IsActive))
        {
            return;
        }


        gameObject.GetComponent<Renderer>().material = towerSelected;
        IsSelected = true;

        SelectedTowerBase = this;
        OnAnyTowerBaseWasSelected?.Invoke(gameObject, EventArgs.Empty);

        if (enabled)
        {
            if (!hasTower)
            {
                if (towerSelectorUI.gameObject.activeSelf)
                {
                    towerSelectorUI.SetCurrentSelectedSpawn(towerSpawn);
                }
                else
                {
                    ShowTowerSelectorUI(true);
                    ShowTowerManiulationUI(false);
                    towerSelectorUI.SetCurrentSelectedSpawn(towerSpawn);
                }
                    
            } 
            else 
            {
                if (towerManipulationUI.gameObject.activeSelf)
                {
                    towerManipulationUI.SetCurrentSelectedBase(this);
                }
                else
                {
                    ShowTowerManiulationUI(true);
                    ShowTowerSelectorUI(false);
                        
                    towerManipulationUI.SetCurrentSelectedBase(this);
                }
                    
            }
            
        }
    }

    private void ShowTowerSelectorUI(bool state)
    {
        towerSelectorUI.gameObject.SetActive(state);
        towerSelectorUI.GetComponent<TowerSelectorUI>().inUse = state;
    }

    private void ShowTowerManiulationUI(bool state)
    {
        towerManipulationUI.gameObject.SetActive(state);
        towerManipulationUI.inUse = state;
    }

    public void DestroyTower()
    {
        if (SelectedTowerBase == this)
            SelectedTowerBase = null;


        this.hasTower = false;
        Destroy(tower.gameObject);
        tower = null;

    }

    public void OnDestroy()
    {
        OnAnyTowerBaseWasSelected -= OnAnyTowerBaseSelected;
    }

    public void Deselect()
    {
        IsSelected = false;
        HideRangeOutline();
        HideRallyPoint();

        gameObject.GetComponent<Renderer>().material = towerNotHovered;
        
        if (SelectedTowerBase == this)
            SelectedTowerBase = null;
    }

    public void OnAnyTowerBaseSelected(object towerBase, EventArgs e)
    {
        GameObject selected = towerBase as GameObject;


        // If the tower clicked on was not this one, then deselect this one.
        if (selected != this.gameObject)
            Deselect();
    }

    /// <summary>
    /// This function deselects all towers.
    /// It is used, for example, when we enter the robot control mode.
    /// </summary>
    public static void DeselectAllTowers()
    {
        if (SelectedTowerBase != null)
            SelectedTowerBase.Deselect();


        TowerBase[] towers = FindObjectsOfType<TowerBase>();
        for (int i = 0; i < towers.Length; i++)
        {
            towers[i].Deselect();
        }
    }

    public void ShowRangeOutline()
    {
        if (tower == null)
            return;


        // Set the scale of the range outline object appropriately now.
        // We are dividing the radius by the tower base's scale, because it is set to 4. As a result, when we set the scale of the outline, it ends of 4x too big. This counteracts that.
        // Then we multiply by 2f since this value is a radius.
        float scale = (tower.RangeRadius / transform.localScale.x) * 2f;
        // The y scale here is set to 50f to make the capsule's vertical dimension much larger than the other two. This way it doesn't accidentally highlight the top of the tower if it intersects with it.
        _TowerRangeOutline.transform.localScale = new Vector3(scale, 1000f, scale);


        _TowerRangeOutline.SetActive(true);
    }

    public void HideRangeOutline()
    {
        _TowerRangeOutline.SetActive(false);
    }

    public void ShowRallyPoint()
    {
        if (tower == null)
            return;


        // Calculate vector from tower to rally point.
        Vector3 direction = tower.GetRallyPoint() - transform.position;
        direction.y = _RallyPointVerticalOffset;

        // Move the rally point visual to the correct location.
        _RallyPointGameObject.transform.position = transform.position + direction;

        // Make sure the flag has no rotation at all. This way it always faces toward the camera. This won't matter if we replace it with a model, or maybe a symbol laid flat on the ground later.
        _RallyPointGameObject.transform.rotation = Quaternion.identity;

        // Make the rally point object visible.
        _RallyPointGameObject.SetActive(true);
    }

    public void HideRallyPoint()
    {
        _RallyPointGameObject.SetActive(false);
    }


    
    public Tower tower
    {
        get { return _Tower; }
        set
        {
            if (_Tower != null)
                _Tower.ParentTowerBase = null;

            _Tower = value;
            if (_Tower != null)
                _Tower.ParentTowerBase = this;
        }
    }

}
