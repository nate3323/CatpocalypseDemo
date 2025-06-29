using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;



[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LaserTowerCanTargetAttribute : Attribute
{
    public LaserTowerCanTargetAttribute(bool canTarget)
    {
        CanTarget = canTarget;
    }



    public bool CanTarget { get; private set; }
}
