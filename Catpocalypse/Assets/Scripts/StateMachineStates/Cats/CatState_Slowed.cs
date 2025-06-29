using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This is the Slowed State providing functionality for enemies that are slowed
/// </summary>
public class CatState_Slowed : CatState_Base
{
    public CatState_Slowed(CatBase parent)
        : base(parent)
    {

    }

    public override void OnEnter()
    {
        UpdateSlowedModifier();
    }

    public override void OnExit()
    {
        _parent.gameObject.GetComponent<NavMeshAgent>().speed = _parent.GetComponent<CatBase>().speed;
    }

    public override void OnUpdate()
    {
        UpdateSlowedModifier();
    }

    private void UpdateSlowedModifier()
    {
        CatBase cat = _parent.GetComponent<CatBase>();
        float modifier = 1;
        foreach (GameObject obj in cat.slowingEntities)
        {
            if (obj != null)
            {
                ScratchingPost post = obj.GetComponent<ScratchingPost>();
                if (post != null)
                {
                    if (post.speedDebuff > modifier)
                    {
                        modifier = post.speedDebuff;
                    }
                }
                if(obj.GetComponent<StringWaverTower>() != null)
                {
                    modifier = obj.GetComponent<StringWaverTower>()._speedDebuff;
                }
                
            }
        }
        _parent.GetComponent<NavMeshAgent>().speed = cat.speed / modifier;
    }

}