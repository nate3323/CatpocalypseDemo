using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// This class represents a build tower button.
/// </summary>
/// <remarks>
/// I got this button's clickable area to have a custom shape defined by the alpha of the image by using the following quick video tutorial:
/// https://www.youtube.com/watch?v=m1lBHP5lxeY
/// 
/// NOTE: The image you set on the button MUST have Read/Write enabled in its import settings or you will get an error.
///       This is necessary for this script to make it so click events are not fired if you click on a transparent
///       part of the image. To set the image on a button, change it on the child GameObject called "Icon".       
/// 
/// </remarks>
public class BuildTowerButtonUI : MonoBehaviour
{
    [SerializeField] private TowerTypes _TowerType;

    [SerializeField] private Image _Image;


    private RectTransform _RectTransform;

    private CustomButton _Button;


    private void Awake()
    {
        _Button = GetComponent<CustomButton>();
        _RectTransform = GetComponent<RectTransform>();

        // This threshold determines where in the image click events are registered.
        // We set it very low so transparent areas of the button image will be ignored when clicked.
        _Image.alphaHitTestMinimumThreshold = 0.1f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public CustomButton Button { get { return _Button; } }
    public RectTransform RectTransform { get { return _RectTransform; } }
    public TowerTypes TowerType { get { return _TowerType; } }

}
