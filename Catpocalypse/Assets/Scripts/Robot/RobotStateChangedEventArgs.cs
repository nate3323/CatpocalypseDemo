using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class RobotStateChangedEventArgs : EventArgs
{
    private bool _NewIsActiveState;


    public RobotStateChangedEventArgs(bool newIsActiveState)
    {
        _NewIsActiveState = newIsActiveState;
    }


    public bool NewIsActiveState
    {
        get { return _NewIsActiveState; }
    }
}
