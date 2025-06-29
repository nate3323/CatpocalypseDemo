using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This is the Moving State providing functionality for Cats that are moving
/// </summary>
public class CatState_Stopped : CatState_Base
{
    public CatState_Stopped(CatBase parent)
        : base(parent)
    {

    }

    public override void OnEnter()
    {
        _parent.gameObject.GetComponent<NavMeshAgent>().speed = 0;
    }

    public override void OnExit()
    {
        _parent.gameObject.GetComponent<NavMeshAgent>().speed = _parent.GetComponent<CatBase>().speed;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

}