using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NPCNavigationController;


public class LaserPointerTower : Tower
{
    // This is the object that the tower will parent it's selected path indicator to. This is necessary, as otherwise the arrow gets messed up by the scale/rotation of the tower and its parents.
    public static Transform SelectedPathIndicatorsParent;

    // This is the same idea, but for laser end point objects that laser towers instantiate.
    public static Transform LaserEndPointsParent;



    [SerializeField]
    private GameObject laserPrefab; // The laser prefab to be copied
    [SerializeField]
    private GameObject laserEndPointPrefab; // The laser end point effect prefab
    [Tooltip("The max number of simulateous lasers this tower can have.")]
    [SerializeField, Min(1)]
    private int MaxLasers = 1; // The number of lasers a tower can instantiate
    
    private int currentMaxLasers = 1;
    [SerializeField]
    private GameObject arrowPrefab;

    [SerializeField] 
    private Transform laserSpawn; //The spawn point of the laser


    [Tooltip("This sets the radius that the tower will search for the nearest path junction within.")]
    [Min(1f)]
    [SerializeField]
    private float _PathJunctionDetectionRadius = 10f;
    
    [Header("Laser End Point")]
    [Tooltip("The laser sweeps back and forth across the path, and this value sets how wide the laser's sweep is.")]
    [Min(0f)]
    private float _LaserSweepWidth = 1f;
    [Tooltip("This sets how long (in seconds) it takes for the laser to go back and forth one time.")]
    [Min(0.1f)]
    private float _LaserSweepTime = 2f;
    [Tooltip("This sets how far the laser should target in front of a cat.")]
    [Min(0f)]
    private float _DistanceInFrontOfTargetToAimFor = 2f;

    private float rangeUpgradeMultiplier = .15f;
    // This holds a reference to the nearest node that has more than one possible next node.
    private WayPoint _PathJunction;

    // This is the arrow used to show which direction is currently selected while in the tower manipulation UI.
    private GameObject _Arrow;


    private List<LaserInfo> _Lasers;
    private int _ActiveLasersCount; // The number of lasers that are currently active
    private int level = 1;
    private bool suddenFlashUnlocked = false;
    /// <summary>
    /// This holds the index of the next way point cats should visit upon reaching the path junction.
    /// It depends upon where the Rally Point has been positioned.
    /// A value of -1 means the cat will randomly select a direction for itself, since the rally point is before the junction point, which means it does not specify which way to go.
    /// </summary>
    private int _CurrentJunctionDirectionIndex = -1;
    [SerializeField]
    private float _suddenFlashDuration = .5f;
    [SerializeField]
    private float _suddenFlashCooldown = 20f;



    private new void Awake()
    {
        base.Awake();

        _Lasers = new List<LaserInfo>();

        if (SelectedPathIndicatorsParent == null)
        {
            SelectedPathIndicatorsParent = new GameObject("Laser Pointer Towers Selected Path Indicators").transform;
            SelectedPathIndicatorsParent.transform.position = Vector3.zero;
        }

        if (LaserEndPointsParent == null)
        {
            LaserEndPointsParent = new GameObject("Laser End Point Objects").transform;
            LaserEndPointsParent.transform.position = Vector3.zero;
        }
    }

    private new void Start()
    {
        base.Start();
        ApplyScrapUpgrades();
        // Find the path junction that is near this tower.
        _PathJunction = FindAssociatedPathJunction();
        if (_PathJunction == null)
        {
            //throw new Exception("There is no path junction within range of this laser pointer tower!");
            return;
        }

        // Spawn the arrow that is used to show the selected path.
        _Arrow = Instantiate(arrowPrefab, _PathJunction.transform.position + (Vector3.up * 1f), Quaternion.identity, SelectedPathIndicatorsParent);
        _Arrow.gameObject.SetActive(false);
    }

    protected override void ApplyScrapUpgrades()
    {
        if (PlayerDataManager.Instance.CurrentData.laserUpgrades > 0)
        {
            if (PlayerDataManager.Instance.CurrentData.laserUpgrades > 1)
            {
                range.radius *= PlayerDataManager.Instance.Upgrades.LaserRangeUpgrade;
                if (PlayerDataManager.Instance.CurrentData.laserUpgrades > 2)
                {
                    distractValue *= PlayerDataManager.Instance.Upgrades.LaserDistractionUpgrade;
                    if (PlayerDataManager.Instance.CurrentData.laserUpgrades > 3)
                    {
                        if (PlayerDataManager.Instance.CurrentData.laserUpgrades > 4)
                        {

                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_Lasers.Count < currentMaxLasers)
        {
            StartCoroutine(SpawnLasers());
        }

        if(_ActiveLasersCount == 0)
        {
            _towerSound.Stop();
        }

        LaserControl();

        if (_ActiveLasersCount < currentMaxLasers)
            SelectTargets();

        
        if (_PathJunction != null)
            CheckIfActiveTargetsReachedJunction();
    }

    protected override void InitStateMachine()
    {
        // NOTE: This code looks the same as the base class for now, but there is a subtle difference.
        //       The condition for entering the activeState from the idleState is different.


        // Create tower states.
        TowerState_Active_Base activeState = new TowerState_Active_Base(this);
        TowerState_Disabled_Base disabledState = new TowerState_Disabled_Base(this);
        TowerState_Idle_Base idleState = new TowerState_Idle_Base(this);
        TowerState_Upgrading_Base upgradingState = new TowerState_Upgrading_Base(this);


        // Create and register transitions.
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        _stateMachine.AddTransitionFromState(idleState, new Transition(activeState, () => _ActiveLasersCount > 0));
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
    /// This function is called when we have fewer targets than numOfLasers.
    /// It will select more targets from targets and add them to _ActiveTargets if they
    /// have not passed the path junction associated with this tower yet.
    /// </summary>
    private void SelectTargets()
    {
        foreach (GameObject target in targets)
        {
            if(target != null)
            {
                CatBase cat = target.GetComponent<CatBase>();

                if (cat.NavController.NextWayPoint == null)
                {
                    continue;
                }

                if (_ActiveLasersCount >= currentMaxLasers)
                    break;


                // Create a TargetInfo for this cat.
                TargetInfo targetInfo = new TargetInfo();
                targetInfo.TargetCat = cat;

                // This bypasses the check below, since that code will always fail if the laser tower is not placed near a path junction.
                // If there is no nearby path junction, this code runs first, and immediately targets another cat as long as there is at
                // least one inactive laser available.
                if (_PathJunction == null)
                {
                    TargetCat(targetInfo);
                    continue;
                }
                else
                {
                    // Is this cat before the path junction point associated with this tower?
                    WayPointUtils.WayPointCompareResults result = WaveManager.Instance.WayPointUtils.CompareWayPointPositions(cat.NavController.NextWayPoint, _PathJunction);
                    
                    //Debug.Log($"Result: {result}    CatNextWaypoint: \"{cat.NextWayPoint.name}\"    JunctionWayPoint: \"{_PathJunction.name}\"");

                    if (result == WayPointUtils.WayPointCompareResults.A_IsBeforeB ||
                        result == WayPointUtils.WayPointCompareResults.A_And_B_AreSamePoint)
                    {
                        if (cat.NavController.NextWayPoint == PathJunction)
                            targetInfo.IsApproachingJunction = true;

                        TargetCat(targetInfo);

                    }
                }
            }

        } // end foreach

    }
    public override void Upgrade()
    {
        base.Upgrade();
        if (towerLevel == 1)
        {
            StartCoroutine(SuddenFlash());
        }
        else
        {
            range.radius = range.radius + (towerStats.Range * _towerUpgradesData.rangeUpgradePercent);
        }
        if (currentMaxLasers < MaxLasers)
        {
            currentMaxLasers++;
        }
        //level++;
    }
    IEnumerator SuddenFlash()
    {
        GameObject[] cats = targets.ToArray();
        if (targets.Count > 0)
        {
            Debug.LogWarning("Sudden flash called");
            for(int i = 0; i < cats.Length; i++) 
            {
                cats[i].GetComponent<CatBase>().stoppingEntities.Add(gameObject);
                StartCoroutine(SuddenFlashDuration(cats[i]));
            }
            
            
            yield return new WaitForSeconds(_suddenFlashCooldown);
        }
        yield return new WaitForSeconds(0);
        StartCoroutine(SuddenFlash());
        
    }
    IEnumerator SuddenFlashDuration(GameObject target)
    {
        yield return new WaitForSeconds(_suddenFlashDuration);
        target.GetComponent<CatBase>().stoppingEntities.Remove(gameObject);
    }
    private bool TargetCat(TargetInfo targetInfo)
    {
        int laserIndex = GetIndexOfFirstInactiveLaser();
        if (laserIndex >= 0 && !IsAlreadyTargeted(targetInfo.TargetCat))
        {
            ActivateLaser(laserIndex, targetInfo);
            return true;
        }

        return false;
    }

    private bool IsAlreadyTargeted(CatBase cat)
    {
        foreach (LaserInfo info in _Lasers)
        {
            if (info.TargetInfo != null && info.TargetInfo.TargetCat == cat)
                return true;
        }

        return false;
    }

    private void CheckIfActiveTargetsReachedJunction()
    {
        for (int i = 0; i < _ActiveLasersCount; i++) 
        { 
            TargetInfo info = _Lasers[i].TargetInfo;
            if (info == null)
                continue;

            WayPoint nextWaypoint = info.TargetCat.NavController.NextWayPoint;

            // If IsApproachingJunction is false but the cat's next point is the path junction associated
            // with this tower, then set that flag to true now.
            if (info.IsApproachingJunction == false && nextWaypoint == PathJunction)
                info.IsApproachingJunction = true;

            // If IsApproachingJunction is true but the cat's next point is NOT the path junction associated
            // with this tower, then it means the cat has reached the path junction waypoint and targeted
            // the next one. So in this case, we now change it's next waypoint to be the path this
            // tower is set to distract cats to.
            else if (info.IsApproachingJunction == true && nextWaypoint == PathJunction)
            {
                int index = _Lasers[i].NextWayPointIndex;
                // If the index is -1, then don't set the next index. This allows the cat to select it's own path in this case.
                // This may or may not mean an error has occurred. Check the Unity console for warnings/errors.
                if (index >= 0)
                    info.TargetCat.NavController.NextWayPoint = PathJunction.NextWayPoints[_Lasers[i].NextWayPointIndex];
            }
        }
    }

    /// <summary>
    /// Finds the first inactive laser.
    /// </summary>
    /// <returns>The first inactive laser.</returns>
    private int GetIndexOfFirstInactiveLaser()
    {
        for (int i = 0; i < _Lasers.Count; i++)
        {
            if (_Lasers[i].TargetInfo == null)
                return i;
        }


        // There are no inactive lasers at this time, so return -1 as an error code.
        return -1;
    }

    /// <summary>
    /// Checks targets found by the base class' OnTriggerEnter() method, and only adds them if
    /// they are the right type.
    /// </summary>
    /// <remarks>
    /// See the comments on this function in the Tower class for more info.
    /// </remarks>
    /// <param name="target">A potential target passed in by the base class.</param>
    protected override void OnNewTargetEnteredRange(GameObject target)
    {
        base.OnNewTargetEnteredRange(target);
    }

    protected override void OnTargetWentOutOfRange(GameObject target)
    {
        RemoveActiveTarget(target);
    }

    protected override void OnTargetHasDied(GameObject target)
    {
        RemoveActiveTarget(target);
    }
    private void RemoveActiveTarget(GameObject target)
    {
        CatBase cat = target.GetComponent<CatBase>();

        if (cat == null)
        {
            Debug.LogError("Target does not have a cat component!");
            return;
        }


        for (int i = 0; i < _ActiveLasersCount; i++)
        {
            if (_Lasers[i].TargetInfo != null && _Lasers[i].TargetInfo.TargetCat == cat)
            {
                DeactivateLaser(i);

                break;
            }
        } // end for i

    }

    private WayPoint FindAssociatedPathJunction()
    {
        WayPoint closestWayPoint = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in Physics.OverlapSphere(transform.position, _PathJunctionDetectionRadius, LayerMask.GetMask("WayPoints")))
        {
            //Debug.Log("Found way point in range: " + collider.name);
            WayPoint p = collider.GetComponent<WayPoint>();

            // Is this object a waypoint, and is it a path branch point?
            if (p != null && p.NextWayPoints.Count > 1)
            {
                float distance = Vector3.Distance(transform.position, p.transform.position); 
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWayPoint = p;
                }
            }
        } // end foreach


        //Debug.Log("Closest waypoint with branches: " + closestWayPoint.gameObject.name);
        return closestWayPoint;
    }

    private void LaserControl()
    {   
        for( int i = 0; i < _Lasers.Count; i++)
        {
            TargetInfo targetInfo = _Lasers[i].TargetInfo;

            // Base case for the tower. If any lasers still have target info, disable them.
            if (_ActiveLasersCount == 0 && targetInfo != null)
            {
                DeactivateLaser(i);
                return;
            }
            else if (targetInfo != null && targetInfo.TargetCat != null && _ActiveLasersCount > 0)
            {
                // If the cat has arrived at the waypoint closest to the rally point, then deactivate the laser so it will be available to target another cat.
                float distance = _ClosestWayPointToRP != null ? Vector3.Distance(targetInfo.TargetCat.transform.position, _ClosestWayPointToRP.transform.position)
                                                                : float.MaxValue;
                if (distance <= targetInfo.TargetCat.NavController.WayPointArrivedDistance)
                {
                    targets.Remove(targetInfo.TargetCat.gameObject);
                    DeactivateLaser(i);
                    _towerSound.Stop();
                    continue;
                }
                else
                {
                    // Changes the laser's length and _ActiveTargets the cat with it
                    Vector3[] linePositions = new Vector3[2];
                    linePositions[0] = _Lasers[i].Laser.transform.position;

                    Vector3 targetPoint = CalculateLaserEndPoint(i);

                    linePositions[1] = targetPoint;

                    _Lasers[i].LaserEndPoint.transform.position = targetPoint;

                    //Debug.Log(linePositions[0] + "    " + _Lasers[i].TargetInfo.TargetCat.name + " " + linePositions[1] + "  " + _Lasers[i].TargetInfo.TargetCat.position);

                    _Lasers[i].Laser.GetComponent<LineRenderer>().SetPositions(linePositions);


                    // Do damage to the cat. We multiply by Time.deltaTime so that the tower's distraction value is per-second.
                    _Lasers[i].TargetInfo.TargetCat.GetComponent<CatBase>().DistractCat(towerStats.DistractValue * Time.deltaTime, this);
                }

            }
            else if (targetInfo != null && targetInfo.TargetCat == null)
            {
                DeactivateLaser(i);
            }
            
            
        } // end for i       

    }

    private void ActivateLaser(int laserIndex, TargetInfo targetInfo)
    {
        _Lasers[laserIndex].Activate(targetInfo, _CurrentJunctionDirectionIndex);
        _Lasers[laserIndex].SweepTimer = _LaserSweepTime;
        _Lasers[laserIndex].SweepWidth = _LaserSweepWidth;
        _ActiveLasersCount++;
        _towerSound.Play();
    }

    private void DeactivateLaser(int laserIndex)
    {
        _Lasers[laserIndex].Deactivate();
        _ActiveLasersCount--;

    }

    private void DeactivateAllLasers()
    {
        for (int i = 0; i < _Lasers.Count; i++)
        {
            DeactivateLaser(i);
        }
       
    }

    private Vector3 CalculateLaserEndPoint(int index)
    {
        CatBase targetCat = _Lasers[index].TargetInfo.TargetCat;
        if (targetCat == null)
            return Vector3.zero;

        Vector3 targetPoint = targetCat.transform.position;


        // Get the forward vector for the target and multiply by the distance we want to aim in front of the target.
        Vector3 forward = targetCat.transform.forward.normalized;
        forward *= _DistanceInFrontOfTargetToAimFor;

        // Calculate the right/left vectors from the perspective of direction.
        Vector3 right = new Vector3(forward.z, 0, -forward.x).normalized;
        //Vector3 left = -right;

        // Multiply by the laser's SweepWidth so we get the desired sweep width.
        right *= _Lasers[index].SweepWidth;

        // Update the sweep timer for this laser.
        _Lasers[index].SweepTimer += Time.deltaTime;

        // Make the laser move back and forth.
        // First, calculate the angle to pass into the Sin function.
        float angle = 360 * (_Lasers[index].SweepTimer / _LaserSweepTime);
        if (angle >= 360)
            _Lasers[index].SweepTimer = 0f; // Reset this laser's sweep timer.

        // Convert the angle from degrees to radians, and then calculate the sine value.
        angle *= Mathf.Deg2Rad;
        float sinValue = Mathf.Sin(Mathf.Clamp(angle, 0f, 360f));

        // Calculate the position of the laser pointer relative to the center of the path.
        // This function is creating a sine wave that is aligned to the path basically.
        right *= sinValue;

        
        // Add the vectors we calculated to the target point to get the final target point in front of the cat.
        targetPoint += forward;
        targetPoint += right;

        return targetPoint;
    }

    // Spawns the laser and sets its position to the top of the tower
    IEnumerator SpawnLasers()
    {
        for (int i = _Lasers.Count; i < currentMaxLasers; i++)
        {
            LaserInfo newLaser = new LaserInfo();

            newLaser.Laser = Instantiate(laserPrefab, laserSpawn);
            newLaser.Laser.gameObject.GetComponent<AudioSource>().Stop();

            newLaser.LaserEndPoint = Instantiate(laserEndPointPrefab, LaserEndPointsParent);
            newLaser.LaserEndPoint.gameObject.SetActive(false);

            newLaser.SweepTimer = _LaserSweepTime;
            newLaser.SweepWidth = _LaserSweepWidth;

            _Lasers.Add(newLaser);

            yield return new WaitForSeconds(1f);
        }
    }


    /// <summary>
    /// Sets the rotation of the arrow that shows the selected path branch.
    /// </summary>
    /// <remarks>
    /// The rotation determines the direction the arrow points.
    /// </remarks>
    /// <param name="rotation">The rotation in degrees.</param>
    public void SetArrowRotation(float rotation)
    {
        Quaternion q = _Arrow.transform.rotation;
        q.eulerAngles = new Vector3(0f, -rotation, 0f);
        _Arrow.transform.rotation = q;
    }

    public override void EnableTargetDetection()
    {
        base.EnableTargetDetection();


        // Clear all current laser targets.
        DeactivateAllLasers();
    }

    public override void DisableTargetDetection()
    {
        base.DisableTargetDetection();

        // Clear all current laser targets.
        DeactivateAllLasers();
    }

    protected override void OnRallyPointChanged()
    {
        // Find the closest waypoint to the new rally point.
        WayPoint closestWayPoint = WaveManager.Instance.WayPointUtils.FindNearestWayPointTo(_RallyPoint);

        // Find out if that waypoint is before or after the path junction near this tower.
        WayPointUtils.WayPointCompareResults result = WaveManager.Instance.WayPointUtils.CompareWayPointPositions(closestWayPoint, _PathJunction);

        if (result == WayPointUtils.WayPointCompareResults.A_IsBeforeB || 
            result == WayPointUtils.WayPointCompareResults.A_And_B_AreSamePoint)
        {
            // The rally point is before the junction near this tower. Thus it does not specify which way to lead the cats.
            // So set this to -1 to let the cats choose their own path.
            _CurrentJunctionDirectionIndex = -1;
        }
        else if (result == WayPointUtils.WayPointCompareResults.A_IsAfterB)
        {
            for (int i = 0; i < _PathJunction.NextWayPoints.Count; i++)
            {
                // Find out if that waypoint is before or after the first waypoint in the first path branch from the path junction near this tower.
                WayPointUtils.WayPointCompareResults branchResult = WaveManager.Instance.WayPointUtils.CompareWayPointPositions(_PathJunction.NextWayPoints[i], closestWayPoint);

                // Is the nearest node in the first path branch?
                if (branchResult == WayPointUtils.WayPointCompareResults.A_IsBeforeB ||
                    branchResult == WayPointUtils.WayPointCompareResults.A_And_B_AreSamePoint)
                {
                    // The rally point is after the junction near this tower. Thus it specifies which way to lead the cats.
                    _CurrentJunctionDirectionIndex = i;

                    break;
                }

            } // end for i
        }
        else
        {
            Debug.LogWarning("Failed to determine how the cats should be directed based on the Rally Point! Setting _CurrentJunctionDirectionIndex to -1 to let the cats choose their own path.");
            _CurrentJunctionDirectionIndex = -1;
        }

    }


    /// <summary>
    /// This property controls whether or not the arrow that shows the selected path branch is visible or not.
    /// </summary>
    public bool ArrowIsVisible 
    {
        get { return _Arrow.activeSelf; }
        set 
        { 
            if (_Arrow != null)
                _Arrow.SetActive(value); 
        }
    }

    /// <summary>
    /// This specifies the index of the selected path. Specifically, it is the index of the first node on that
    /// path after _PathJunction. So basically, it is an index into _PathJunction.NextNodes to get the right next node.
    /// </summary>
    public int SelectedPathIndex
    {
        get; set;
    }

    public WayPoint SelectedPathNextNode { get { return _PathJunction.NextWayPoints[SelectedPathIndex]; } }
    

    public WayPoint PathJunction { get { return _PathJunction; } }



    private struct ExtraTargetInfo
    {
        public bool HasReachedJunction;
    }



    private class TargetInfo
    {
        public CatBase TargetCat;
        public bool IsApproachingJunction; // This will be true if the cat's next waypoint is set to the junction associated with this tower.
        public bool ReachedNextWayPoint;
    }


    /// <summary>
    /// Stores info related to a given laser.
    /// </summary>
    private class LaserInfo
    {
        public GameObject Laser; // The instantiated laser GameObject
        public GameObject LaserEndPoint; // The instantiated end point effects GameObject for the laser.
        public int NextWayPointIndex; // The index of the waypoint the cat should head to next after it reaches the path junction near this tower. This index is for accessing the appropriate next point from the _PathJunction waypoint's NextWayPoints list.
        public float SweepTimer; // Holds the elapsed time for the laser. This is used to make the laser sweep back and forth.
        public float SweepWidth; // How far back and forth the laser sweeps.
        public TargetInfo TargetInfo = null;



        public void Activate(TargetInfo targetInfo, int nextWayPointIndex)
        {
            Laser.SetActive(true);
            Laser.gameObject.GetComponent<AudioSource>().Play();
            LaserEndPoint.SetActive(true);
            NextWayPointIndex = nextWayPointIndex;
            SweepTimer = 2f;
            
            TargetInfo = targetInfo;
            TargetInfo.TargetCat.NavController.OnTargetReachedNextWayPoint += OnTargetReachedNextWayPoint;
        }

        public void Deactivate()
        {
            if (Laser != null)
            {
                Laser.gameObject?.GetComponent<AudioSource>().Stop();
                Laser.SetActive(false);
            }
            if (LaserEndPoint != null)
            {
                LaserEndPoint.SetActive(false);
            }

            SweepTimer = 0f;

            // Unsubscribe from the event if the cat still exists.
            if (TargetInfo != null && TargetInfo.TargetCat != null)
            {
                TargetInfo.TargetCat.NavController.OnTargetReachedNextWayPoint -= OnTargetReachedNextWayPoint;
            }

            TargetInfo = null;

        }

        private void OnTargetReachedNextWayPoint(object sender, ReachedNextWayPointEventArgs e)
        {
            TargetInfo.ReachedNextWayPoint = true;
        }
       

    }
}


