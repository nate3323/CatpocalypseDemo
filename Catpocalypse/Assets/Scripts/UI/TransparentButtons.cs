using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// This class is a simple tool to make it so buttons whose images have transparent areas
/// will not register clicks on those transparent areas.
/// </summary>
/// <remarks>
/// USAGE:
/// ---------
/// 1. Place this script on a GameObject.
/// 2. In the inspector, add the buttons inquestion into this script's TransparentButtons list.
/// 3. Your done. The AlphaHitTestThreshold setting is fine with its default value in most cases.
/// </remarks>
public class TransparentButtons : MonoBehaviour
{
    [Tooltip("These are the buttons that need to ignore mouse clicks that occur on the transparent parts of their images.")]
    [SerializeField]
    public List<Image> _TransparentButtons;
    [Tooltip("This is the alpha level that must be present for a mouse click to cause a click event to be fired.")]
    public float _AlphaHitTestThreshold = 0.1f;



    void Awake()
    {
        for (int i = 0; i < _TransparentButtons.Count; i++)
        {
            _TransparentButtons[i].alphaHitTestMinimumThreshold = _AlphaHitTestThreshold;
        }
    }
}
