using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;


/// <summary>
/// This class represents a clickable level tile on the level select screen.
/// The GameObject that this script is on needs to have a Kinematic (static) rigidbody, because
/// this causes it to combine the colliders of all the children. That way if you click on or
/// hover over any child object, the mouse events will fire.
/// </summary>
/// <remarks>
/// NOTE: The level icon and tooltip text are displayed via the overlay camera. This is how
///       I made it so they always render on top of everything else in the scene. That camera's
///       culling mask is set so it only renders objects on the LevelSelectScreenLevelTiles layer,
///       and the main camera's is set to render everything except that layer.
/// </remarks>
[RequireComponent(typeof(Rigidbody))]
public class ClickableLevelTile : MonoBehaviour
{
    [Tooltip("The name of the level the player will enter when they click on this tile. This name is what appears in the level tooltip when the player hovers over this level tile.")]
    [SerializeField]
    private string _levelName;

    [Tooltip("The name of the scene to load when the player clicks on this level tile.")]
    [SerializeField]
    private string _sceneToLoad;

    [Tooltip("The tooltip that will appear when the player hovers over this level tile.")]
    [SerializeField]
    private TextMeshPro _Tooltip;

    [Tooltip("This specifies whether or not the tooltip will appear when the player hovers over this level tile.")]
    [SerializeField]
    private bool _EnableToolTip = true;

    [Tooltip("If this option is enabled, the tooltip will be displayed all all times regardless of whether the player is hovering over this level tile or not.")]
    [SerializeField]
    private bool _AlwaysShowToolTip;



    private void Awake()
    {
        GetComponent<Rigidbody>().isKinematic = true;

        EnableToolTip(_AlwaysShowToolTip);
    }

    private void OnMouseUp()
    {
        // Prevents level selection when utilizing UI elements
        /**if (EventSystem.current.IsPointerOverGameObject())
        {
            StartCoroutine(WasteTimeSoMouseUpWillNotTrigger());
            return;
        }*/
        SceneLoader_Async.LoadSceneAsync(_sceneToLoad);
    }

    private void OnMouseEnter()
    {
        // Prevents level selection when utilizing UI elements
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (_EnableToolTip || _AlwaysShowToolTip)
            EnableToolTip(true);
        else
            EnableToolTip(false);
    }

    private void OnMouseExit()
    {
        // Prevents level selection when utilizing UI elements
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (_AlwaysShowToolTip)
            EnableToolTip(true);
        else
            EnableToolTip(false);
    }

    private void EnableToolTip(bool state)
    {
        if (state)
            _Tooltip.text = _levelName;

        _Tooltip.gameObject.gameObject.SetActive(state);
    }

    IEnumerator WasteTimeSoMouseUpWillNotTrigger()
    {
        yield return new WaitForEndOfFrame();
    }
}
