using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// This is a custom version of the Unity Button class, that adds OnMouseEnter and OnMouseExit events.
/// These events also respect the image's transparent areas. In other words, if the mouse is over a transparent part of
/// the image, that doesn't count as being over the button.
/// </summary>
public class CustomButton : Button
{
    public event EventHandler OnMouseEnter;
    public event EventHandler OnMouseExit;



    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        OnMouseEnter?.Invoke(this, EventArgs.Empty);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        OnMouseExit?.Invoke(this, EventArgs.Empty);
    }

}
