using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PropertiesUI_LaserPointerTower : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown _TargetTypeDropdown;


    // Holds the list of cat types that the laser pointer tower can target.
    List<Type> _TargetableCatTypes;
    List<RedirectPathOption> _RedirectPathOptions;
    private int _SelectedPathIndex = 0;

    private LaserPointerTower _LaserPointerTower;



    /// <summary>
    /// Represents a single path redirection option. This is used by the Redirect Path property in the UI.
    /// </summary>
    private struct RedirectPathOption
    {
        public int NextNodeIndex; // The index of the first node on this path. This index is used to access the correct item in the JunctionNode.NextNodes list.
        public float DirectionAngle; // Used to sort the paths.
    }



    private void Awake()
    {
        _TargetableCatTypes = new List<Type>();
        _RedirectPathOptions = new List<RedirectPathOption>();
    }

    private void OnEnable()
    {
        if (TowerBase.SelectedTowerBase == null)
        {
            return;
        }

        if(TowerBase.SelectedTowerBase.tower == null)
        {
            return;
        }

        _LaserPointerTower = TowerBase.SelectedTowerBase.tower.GetComponent<LaserPointerTower>();
        if (_LaserPointerTower == null)
        {
            return;
        }


        // Get targetable cat types list.        
        GetTypesWithLaserTowerCanTargetAttribute(Assembly.GetExecutingAssembly());

        // Get the list of redirection paths that are available at the selected laser pointer tower.
        //GetRedirectPathOptions();


        PopulateUI();

        _LaserPointerTower.ArrowIsVisible = true;
    }

    private void OnDisable()
    {
        if (_LaserPointerTower != null)
            _LaserPointerTower.ArrowIsVisible = false;
    }

    private void PopulateUI()
    {
        _TargetTypeDropdown.options.Clear();

        // Add all the options into the target type dropdown list from the list of targetable cat types we found.
        foreach (Type t in _TargetableCatTypes)
            _TargetTypeDropdown.options.Add(new TMP_Dropdown.OptionData(t.Name));

        _TargetTypeDropdown.options.Add(new TMP_Dropdown.OptionData("All Cats"));

        SelectCurrentValuesInUI();
    }

    private void SelectCurrentValuesInUI()
    {
        if (_LaserPointerTower.TargetCatType == null)
            _TargetTypeDropdown.value = _TargetTypeDropdown.options.Count - 1; 
        else
            _TargetTypeDropdown.value = _TargetableCatTypes.IndexOf(_LaserPointerTower.TargetCatType);
        

        //_SelectedPathIndex = _LaserPointerTower.SelectedPathIndex;

        UpdateSelectedPathArrow();
    }

    private void UpdateSelectedPathArrow()
    {
        //_LaserPointerTower.SetArrowRotation(_RedirectPathOptions[_SelectedPathIndex].DirectionAngle);
    }

    private void GetTypesWithLaserTowerCanTargetAttribute(Assembly assembly)
    {
        _TargetableCatTypes.Clear();

        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(CatBase)) &&
                type.GetCustomAttributes(typeof(LaserTowerCanTargetAttribute), true).Length > 0)
            {
                _TargetableCatTypes.Add(type);
            }
        } // end foreach

    }

    private void GetRedirectPathOptions()
    {
        _RedirectPathOptions.Clear();

        WayPoint pathJunction = _LaserPointerTower.PathJunction;

        for (int i = 0; i < pathJunction.NextWayPoints.Count; i++)
        {
            WayPoint nextWayPoint = pathJunction.NextWayPoints[i];

            RedirectPathOption r = new RedirectPathOption();
            r.NextNodeIndex = i;
            r.DirectionAngle = Vector3.Angle(Vector3.forward, 
                                             nextWayPoint.transform.position - pathJunction.transform.position);

            // Snap to nearest 90 degree angle. This is because otherwise it points at an angle, since the waypoints are set
            // up that way to make the cats walk a bit more naturally.            
            r.DirectionAngle = Mathf.Round(r.DirectionAngle / 90) * 90; // This line gets the nearest multiple of 90 degrees.

            _RedirectPathOptions.Add(r);
        }


        // Lastly, sort the list by angle. The list needs to be sorted so the buttons cycle through the options in order.
        _RedirectPathOptions.Sort(RedirectPathOptionComparer);
    }


    private int RedirectPathOptionComparer(RedirectPathOption a, RedirectPathOption b)
    {
        return a.DirectionAngle.CompareTo(b.DirectionAngle);
    }



    // ====================================================================================================
    // UI Event Handlers
    // ====================================================================================================

    public void OnTargetTypeChanged()
    {
        if (_TargetTypeDropdown.options[_TargetTypeDropdown.value].text == "All Cats")
        {
            foreach (Type t in _TargetableCatTypes)
                _LaserPointerTower.TargetCatType = null;
        }
        else
        {
            _LaserPointerTower.TargetCatType = _TargetableCatTypes[_TargetTypeDropdown.value];
        }
    }

    public void OnPreviousRedirectPathButtonClicked()
    {
        _SelectedPathIndex--;
        if (_SelectedPathIndex < 0)
            _SelectedPathIndex = _RedirectPathOptions.Count - 1;

        _LaserPointerTower.SelectedPathIndex = _SelectedPathIndex;

        UpdateSelectedPathArrow();
    }

    public void OnNextRedirectPathButtonClicked()
    {
        _SelectedPathIndex++;
        if (_SelectedPathIndex > _RedirectPathOptions.Count - 1)
            _SelectedPathIndex = 0;

        _LaserPointerTower.SelectedPathIndex = _SelectedPathIndex;

        UpdateSelectedPathArrow();
    }
}
