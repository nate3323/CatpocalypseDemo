
using UnityEngine;


/// <summary>
/// This is the base class that all state machine states have at the base of their inheritance heirarchy.
/// </summary>
public abstract class CatState_Base : State_Base
{
    protected CatBase _parentCat;



    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="parentCat">The state needs a reference to its parent cat so it can call methods on it.</param>
    public CatState_Base(CatBase parentCat)
        : base(parentCat.gameObject)
    {
        _parentCat = parentCat;
    }


}

