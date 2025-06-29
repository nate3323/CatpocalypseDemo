using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class RobotBatteryEventArgs : EventArgs
{
    private float _NewBatteryLevel;


    public RobotBatteryEventArgs(float newBatteryLevel)
    {
        _NewBatteryLevel = newBatteryLevel;
    }


    public float NewBatteryLevel
    {
        get { return _NewBatteryLevel; }
    }
}
