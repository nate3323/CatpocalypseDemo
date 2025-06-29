using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(StateMachine))]
public class Tower : MonoBehaviour
{
    [Tooltip("This field gives us an easy way to find the TowerInfo object that corresponds to this tower.")]
    [SerializeField] TowerTypes towerTypeTag;

    [SerializeField,Min(1)]
    protected float buildCost;
    
    [Tooltip("This is the percentage of the cost that is refunded when the player destroys the tower.")]
    [Range(0f, 1f)]
    [SerializeField]
    protected float refundPercentage = 0.85f;

    [Tooltip("This SphereCollider defines the range of the tower.")]
    [SerializeField]
    protected SphereCollider range;
    [SerializeField]
    protected float radius;
    [SerializeField, Min(1)]
    protected float distractValue;
    [SerializeField]
    protected int numberOfTargets;

    protected Vector3 targetDirection;
    public List<GameObject> targets;

    // This property is currently only used by the laser pointer tower, but I put it
    // here in Tower in case any other tower needs it later.
    protected Type _TargetCatType = null; // null means target all types of cats. This was previously set to: typeof(NormalCat); to target only normal cats.


    protected StateMachine _stateMachine;
    public int towerLevel = 0;
    [SerializeField] private float upgradeCost;
    public bool _cutenessChallengeActive = false;
    PlayerCutenessManager _cutenessManager;

    [SerializeField]
    protected float fireRate;

    // This just caches the default rally point position for this tower.
    protected Vector3 _DefaultRallyPoint;

    // The current rally point for this tower.
    protected Vector3 _RallyPoint;

    // Holds a list of all WayPoints that are within this tower's range.
    protected List<WayPoint> _WayPointsInRange = new List<WayPoint>();

    // The closest waypoint to the Rally Point. Will be null if no WayPoints are within the tower's range.
    protected WayPoint _ClosestWayPointToRP;

    [SerializeField]
    public TowerData towerStats;
    [SerializeField]
    public AudioSource _towerSound;

    [SerializeField]
    public int maxLevel = 4;

    [SerializeField]
    public TowerUpgradesData _towerUpgradesData;

    protected void Awake()
    {
        range.radius = radius;
        RangeRadius = range.radius;
        
        FindDefaultRallyPoint();
        upgradeCost = towerStats.UpgradeCost;
    }

    protected void Start()
    {
        _cutenessManager = GameObject.FindGameObjectWithTag("Goal").gameObject.GetComponent<PlayerCutenessManager>();

        //Applies the debuff if the tower was built during the cuteness challenge
        if(_cutenessManager.CurrentCutenessChallenge == PlayerCutenessManager.CutenessChallenges.CatsGetHarderToDistract)
        {
            towerStats.DistractValue *= _cutenessManager.CuteChallenge_CatsGetHarderToDistract_DebuffPercent;
        }
        if(_cutenessManager.CurrentCutenessChallenge == PlayerCutenessManager.CutenessChallenges.DebuffTowerType)
        {
            if(towerTypeTag == _cutenessManager._TowerType)
            {
                towerStats.FireRate *= 1 + _cutenessManager._TowerFireRateDebuffPercent;
            }
        }
        if(_cutenessManager.CurrentCutenessChallenge == PlayerCutenessManager.CutenessChallenges.CucumberTowerBuffsCats &&
            towerTypeTag == TowerTypes.CucumberThrower)
        {
            gameObject.GetComponent<CucumberTower>().buffCats = true;
        }
    }
    
    protected void OnEnable()
    {
        // Update the RadiusRange property with the original range before we adjust it.
        RangeRadius = range.radius;

        // This corrects the problem with our prefabs. For example, the laser tower
        // has a scale of 500. It's collider has a radius of 6. This effectively means
        // the true size of the collider is radius = 30,000. This adjusts the collider
        // radius by simply dividing it by the gameObject's scale. It doesn't matter
        // whether we use x, y, or z here since it is a sphere.
        //_Collider = GetComponent<SphereCollider>();
        range.radius = range.radius / transform.localScale.x;


        _cutenessManager = GameObject.FindGameObjectWithTag("Goal").GetComponent<PlayerCutenessManager>();
        if (_stateMachine == null)
        {
            _stateMachine = GetComponent<StateMachine>();
            if (_stateMachine == null)
                throw new Exception($"The tower \"{gameObject.name}\" does not have a state machine component!");

            InitStateMachine();
        }


        FindDefaultRallyPoint();


        EnableTargetDetection();        
    }

    private void OnDisable()
    {
        DisableTargetDetection();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Cat"))
        {
            OnNewTargetEnteredRange(collider.gameObject);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Cat"))
        {
            OnTargetWentOutOfRange(collider.gameObject);

            targets.Remove(collider.gameObject);
        }
    }

    /// <summary>
    /// This function is overriden by subclasses to allow them to setup the state machine with their own states.
    /// </summary>
    protected virtual void InitStateMachine()
    {
        // Create tower states.
        TowerState_Active_Base activeState = new TowerState_Active_Base(this);
        TowerState_Disabled_Base disabledState = new TowerState_Disabled_Base(this);
        TowerState_Idle_Base idleState = new TowerState_Idle_Base(this);
        TowerState_Upgrading_Base upgradingState = new TowerState_Upgrading_Base(this);


        // Create and register transitions.
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        _stateMachine.AddTransitionFromState(idleState, new Transition(activeState, () => targets.Count > 0));
        _stateMachine.AddTransitionFromState(disabledState, new Transition(idleState, () => IsTargetDetectionEnabled));

        _stateMachine.AddTransitionFromAnyState(new Transition(disabledState, () => !IsTargetDetectionEnabled));
        _stateMachine.AddTransitionFromAnyState(new Transition(idleState, () => IsTargetDetectionEnabled));

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


        // Tell state machine to write in the debug console every time it exits or enters a state.
        //_stateMachine.EnableDebugLogging = true;

        // Set the starting state.
        _stateMachine.SetState(idleState);
    }

    /// <summary>
    /// This function exists so that it can be overriden in subclasses. 
    /// </summary>
    /// <remarks>
    /// The purpose of is function is to allow a given tower type to have its own filters on targets.
    /// Specifically I added this function since the Laser Pointer Tower needs to only care about cats
    /// of a certain type. So it overrides this function to insert it's own functionality.
    /// 
    /// NOTE: Here in the tower base class this function simply adds the target to the list,
    ///       as the base class doesn't need to do any filtering of targets that are in range.
    ///       This defines the default behavior for towers that do not override this function.
    /// </remarks>
    /// <param name="target">The target game object to verify and add to the list.</param>
    protected virtual void OnNewTargetEnteredRange(GameObject target)
    {
        if (TargetCatType == null) // All cats
        {
            targets.Add(target);
        }
        else // A specific cat type is selected, so target only that cat type.
        {
            CatBase cat = target.GetComponent<CatBase>();
            if (cat != null && cat.GetType() == TargetCatType)
            {
                targets.Add(target);
            }
        }

    }

    /// <summary>
    /// This function is just an event hander that subclasses can override to be notified when
    /// a target moves out of range. So it's like NewTargetEnteredRange(), but the opposite.
    /// </summary>
    /// <remarks>
    /// 
    /// NOTE: Subclasses DO NOT need to remove the target from targets.
    ///       This class does that right after it calls this event handler.
    ///       For example, laser tower has an ActiveTargets list, so it must remove
    ///       said target from that list in its override of this method.
    /// </remarks>
    /// <param name="target"></param>
    protected virtual void OnTargetWentOutOfRange(GameObject target)
    {

    }

    /// <summary>
    /// This function is an event handler that subclasses can override to be
    /// notified when a target has "died".
    /// </summary>
    /// <param name="target"></param>
    protected virtual void OnTargetHasDied(GameObject target)
    {

    }

    protected virtual void ApplyScrapUpgrades()
    {

    }

    public virtual void Upgrade()
    {
        towerLevel++;
    }
    void OnMouseEnter()
    {
        gameObject.GetComponentInParent<TowerBase>().hoveredOver = true;
        gameObject.GetComponent<Renderer>().material = gameObject.GetComponentInParent<TowerBase>().towerHovered;
    }

    void OnMouseExit()
    {
        gameObject.GetComponentInParent<TowerBase>().hoveredOver = false;
        gameObject.GetComponent<Renderer>().material = gameObject.GetComponentInParent<TowerBase>().towerNotHovered;
    }

    private void OnCatDied(object sender, EventArgs e)
    {
        OnTargetHasDied((GameObject) sender);

        targets.Remove(sender as GameObject);
    }

    public void OnDestroy()
    {
        Destroy(this);
    }
    public float GetDistractionValue()
    {
        return distractValue;
    }

    /// <summary>
    /// Finds the default rally point, and initializes the current rally point to be that default.
    /// </summary>
    /// <returns></returns>
    private bool FindDefaultRallyPoint()
    {
        // Attempt to find the nearest WayPoint within the tower's range.
        if (FindNearestWayPoint(out WayPoint wayPoint))
        {           
            // Set the default rally point to the location of the nearest WayPoint.
            _DefaultRallyPoint = wayPoint.transform.position;

            // Set the current rally point equal to the default.
            _RallyPoint = _DefaultRallyPoint;


            return true;
        }


        return false;
    }

    /// <summary>
    /// Finds the nearest WayPoint within the tower's range, and fills in the _WayPointsInRange list in the same loop.
    /// </summary>
    /// <param name="wayPoint">This out parameter returns the nearest WayPoint within the tower's range, or null if none was found.</param>
    /// <returns>True if the nearest WayPoint was found, or false if none were in range.</returns>
    protected bool FindNearestWayPoint(out WayPoint wayPoint)
    {
        _WayPointsInRange.Clear();
        wayPoint = null;

        // Find all WayPoints that are within this tower's range.
        RaycastHit[] hits = Physics.SphereCastAll(transform.parent.parent.position,
                                                  RangeRadius,
                                                  Vector3.forward,
                                                  0.1f,
                                                  LayerMask.GetMask("WayPoints"),
                                                  QueryTriggerInteraction.Collide);

        // Did we find at least one WayPoint within range of this tower?
        if (hits.Length < 1)
        {
            Debug.LogError($"Tower \"{gameObject.name}\" could not set a default rally point, because no WayPoints were found within the tower's range!", gameObject);
            return false;
        }

        // Iterate through the WayPoints we found to determine the nearest one.
        float closestDistance = float.MaxValue;
        WayPoint closestWayPoint = null;
        foreach (RaycastHit hit in hits)
        {
            WayPoint p = hit.collider.GetComponent<WayPoint>();
            if (p != null)
            {
                _WayPointsInRange.Add(p);

                float distance = Vector3.Distance(transform.parent.parent.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    if (p != null)
                    {
                        closestDistance = distance;
                        closestWayPoint = p;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"WayPoint \"{hit.collider.name}\" does not have a WayPoint component. Skipping it.", gameObject);
                continue;
            }

        } // end foreach


        // Return the nearest WayPoint via the out parameter.
        wayPoint = closestWayPoint;

        // return the nearest WayPoint.
        return true;
    }

    public float GetBuildCost()
    {
        return buildCost;
    }
    public float GetUpgradeCost()
    {
        return upgradeCost;
    }
    public float SetUpgradeCost(float newCost)
    {
        upgradeCost = newCost;
        return upgradeCost;
    }

    public float GetRefundPercentage()
    {
        return refundPercentage;
    }

    public virtual void EnableTargetDetection()
    {
        range.enabled = true;
        targets.Clear();
    }

    public virtual void DisableTargetDetection()
    {
        range.enabled = false;
        targets.Clear();
    }

    public Vector3 GetRallyPoint()
    {
        return _RallyPoint;
    }

    public void SetRallyPoint(Vector3 newRallyPoint)
    {
        bool changed = true;
        if (_RallyPoint == newRallyPoint)
            changed = false;


        if (changed)
        {
            _RallyPoint = newRallyPoint;

            _ClosestWayPointToRP = WaveManager.Instance.WayPointUtils.FindNearestWayPointTo(_RallyPoint);

            OnRallyPointChanged();
        }
    }


    /// <summary>
    /// This function is called whenever the tower's rally point gets changed.
    /// Subclasses can override this function to do any extra logic they need to happen whenever the rally point changes.
    /// </summary>
    protected virtual void OnRallyPointChanged()
    {

    }


    public float BuildCost { get { return buildCost; } }
    public float DistractValue { set { distractValue = value; } get { return distractValue; } }
    public float FireRate { set { fireRate = value; } get { return fireRate; } }
    public bool IsTargetDetectionEnabled { get { return range.enabled; } }

    public TowerBase ParentTowerBase { get; set; }


    /// <summary>
    /// The radius of the tower's range.
    /// </summary>
    /// <remarks>
    /// This is dividing by the scale, because the radius is automatically adjusted based on the scale in 
    /// </remarks>
    public float RangeRadius { get; private set; }

    public Type TargetCatType
    {
        get { return _TargetCatType; }
        set 
        {
            /*
            if (!typeof(CatBase).IsAssignableFrom(value))
            {
                throw new Exception($"The passed in cat type is not a subclass of CatBase! The tower in question is \"{gameObject.name}\" of type {this.GetType()}");
            }
            */

            _TargetCatType = value; 
        }
    }

    public TowerTypes TowerTypeTag { get { return towerTypeTag; } }
}
