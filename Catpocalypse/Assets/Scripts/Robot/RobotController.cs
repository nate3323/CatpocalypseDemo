using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;
using UnityEngine.SocialPlatforms;

public class RobotController : MonoBehaviour
{
    public event EventHandler<RobotBatteryEventArgs> OnBatteryLevelChanged;
    public event EventHandler<RobotStateChangedEventArgs> OnRobotStateChanged;


    public static RobotController Instance { get; private set; }

    public const string ROBOT_CAMERA_GAMEOBJECT_NAME = "Robot Virtual Camera";



    [Header("Movement Settings")]


    [Tooltip("How fast the tank turns in degrees per second.")]
    [SerializeField, Min(1f)]
    private float _MaxTurnSpeed = 30f;

    [Tooltip("The minimum user input value required to make the robot move.")]
    [SerializeField, Range(0f, 1f)]
    private float _UserInputThreshold = 0.1f;

    [Tooltip("How fast the robot decelerates (in meters per second) when the player stops pressing a forward/backward button.")]
    [SerializeField, Min(0f)]
    private float _MovementDecelerationRate = 1f;

    [Tooltip("How fast the steering returns to the neutral position (in degrees per second) when the player stops pressing a left/right button.")]
    [SerializeField, Min(0f)]
    private float _TurningDecelerationRate = 30f;

    [Tooltip("If set to true, the steering is inverted when the robot is moving backwards.")]
    [SerializeField]
    private bool _InvertSteeringWhenInReverse = true;

    [Tooltip("If set to true, the robot can be activated in between waves.")]
    [SerializeField]
    private bool _AllowActivationBetweenWaves = false;


    [Header("Battery Settings")]

    [Tooltip("This sets the robot's current maximum battery capacity.")]
    [SerializeField, Min(0f)]
    private float _BatteryMaxCapacity = 100f;

    [Tooltip("This sets how much battery power is used per second.")]
    [SerializeField, Min(0f)]
    private float _BatteryDrainPerSecond = 0.5f;

    [Tooltip("This sets how much the battery recharges per second.")]
    [SerializeField, Min(0f)]
    private float _BatteryRechargePerSecond = 1f;

    [Tooltip("When the battery charge drops to this percentage or below, the robot will shut down and begin recharging.")]
    [SerializeField, Min(0f)]
    private float _RobotShutdownThreshold = 0.15f;

    [Tooltip("This sets the minimum percentage the battery must be charged to before the robot can be activated again.")]
    [SerializeField, Min(0f)]
    private float _RobotMinActivationThreshold = 0.4f;


    [Header("Projectile Settings")]

    [Tooltip("This is the point where the robot's projectiles are launched from.")]
    [SerializeField]
    private Transform _ProjectileLaunchPoint;


    [Tooltip("This list specifies the prefabs for the projectiles that the robot can fire.")]
    [SerializeField]
    private List<RobotProjectile> _ProjectilePrefabs;



    private CinemachineVirtualCamera _RobotVirtualCamera;
    private PlayerInputManager _PlayerInputManager;
    private Rigidbody _Rigidbody;
    private WaveManager _WaveManager;

    [SerializeField]
    private SphereCollider _passiveDistractRadius;

    [SerializeField]
    private RobotStats _stats;

    private List<CatBase> _catsInRange = new List<CatBase>();
    [SerializeField]
    private float _passiveDistractRate;
    [SerializeField]
    private float _passiveDistractAmount;
    /// <summary>
    /// Tracks the current charge level of the battery.
    /// </summary>
    private float _CurrentBatteryCharge;


    private float _CurrentMovementSpeed;
    private float _CurrentTurnSpeed;

    private float _BatteryTimer;
    private float _ProjectileTimer;

    private float _MaxSpeedAdjustment;
    private float _LaunchAdjustment;
    private float _FireRateAdjustment;





    // ********************************************************
    // TODO: FIX MOVEMENT SO ROBOT DOESN'T JUST GO THROUGH THINGS!
    // ********************************************************
     

    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;


        _PlayerInputManager = FindObjectOfType<PlayerInputManager>();
        _Rigidbody = GetComponent<Rigidbody>();
        _RobotVirtualCamera = transform.Find(ROBOT_CAMERA_GAMEOBJECT_NAME).GetComponent<CinemachineVirtualCamera>();
        _WaveManager = FindObjectOfType<WaveManager>();

        // Set starting values for potential upgradable values
        _MaxSpeedAdjustment = 1;
        _LaunchAdjustment = 1;
        _FireRateAdjustment = 1;

        // Disable the robot's camera to force it to start with the main game camera.
        _RobotVirtualCamera.enabled = false;

        // Start the robot fully charged.
        _CurrentBatteryCharge = _BatteryMaxCapacity;

        // Deactivate the robot.
        DeactivateRobot();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Register the robot's camera with the camera manager.
        CameraManager.Instance.RegisterCamera(_RobotVirtualCamera, CameraTypes.RobotCamera);

        ApplyScrapUpgrades();

        // Fire the battery charge level changed event to make sure the HUD shows the correct starting value.
        OnBatteryLevelChanged?.Invoke(this, new RobotBatteryEventArgs(BatteryChargePercentage));
    }

    protected void ApplyScrapUpgrades()
    {
        if (PlayerDataManager.Instance.CurrentData.robotUpgrades > 0)
        {
            _MaxSpeedAdjustment *= PlayerDataManager.Instance.Upgrades.RobotSpeedUpgrade;
            if (PlayerDataManager.Instance.CurrentData.robotUpgrades > 1)
            {
                _LaunchAdjustment *= PlayerDataManager.Instance.Upgrades.RobotLaunchUpgrade;
                if (PlayerDataManager.Instance.CurrentData.robotUpgrades > 2)
                {
                    _FireRateAdjustment *= PlayerDataManager.Instance.Upgrades.RobotFireRateUpgrade;
                    if (PlayerDataManager.Instance.CurrentData.robotUpgrades > 3)
                    {
                        StartCoroutine(DistractCatsInRange());
                        if (PlayerDataManager.Instance.CurrentData.robotUpgrades > 4)
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
        if (PlayerInputManager.Robot_ToggleControl)
        {
            // Toggle the robot's active state.
            ToggleRobotButtonClicked();
        }


        // Run update code that should only execute in a certain state.
        if (IsActive)
            ActiveModeUpdate();
        else
            InactiveModeUpdate();


        // If the battery timer has reached one second, then charge or drain the battery depending on whether the robot is active.
        _BatteryTimer += Time.deltaTime;
        if (_BatteryTimer >= 1.0f)
        {
            // Reset the battery timer.
            _BatteryTimer = 0f;

            // Charge or drain the robot's battery depending on whether the robot is active or not.
            ApplyBatteryChargeOrDrain();
        }

        // Get user input and move the robot.
        GetUserInput();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Cat")
        {
            _catsInRange.Add(other.gameObject.GetComponent<CatBase>());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Cat")
        {
            _catsInRange.Remove(other.gameObject.GetComponent<CatBase>());
        }
    }

    /// <summary>
    /// This function is called by Update() when the robot is active.
    /// </summary>
    private void ActiveModeUpdate()
    {
        // Check if the robot is moving forward or backward.
        if (Mathf.Abs(_CurrentMovementSpeed) > 0f || Mathf.Abs(_CurrentTurnSpeed) > 0f)
            ApplyDeceleration();


        // Check the robot's battery.
        if (_CurrentBatteryCharge < _RobotShutdownThreshold)
        {
            // The charge has gotten too low, so the robot is shutting down.
            DeactivateRobot();
        }


        _ProjectileTimer += Time.deltaTime;
        if (PlayerInputManager.Robot_FireProjectile && _ProjectileTimer >= (_stats.FireRate *= _FireRateAdjustment))
        {
            // Reset the projectile cooldown timer.
            _ProjectileTimer = 0f;

            // Fire a new projectile.
            FireProjectile();
        }
    }

    private void InactiveModeUpdate()
    {

    }

    private void GetUserInput()
    {
        Vector2 movementInput = PlayerInputManager.Robot_Movement;

        // Do we have a non-zero input for forward/back axis?
        if (movementInput.y < -_UserInputThreshold || movementInput.y > _UserInputThreshold)
        {
            // We have some input, so move the robot.
            _CurrentMovementSpeed = movementInput.y * (_stats.MaxMovementSpeed * _MaxSpeedAdjustment);
        }

        // Do we have a non-zero input for the left/right axis?
        if (movementInput.x < -_UserInputThreshold || movementInput.x > _UserInputThreshold)
        {
            // We have some input, so turn the robot.
            _CurrentTurnSpeed = movementInput.x * _MaxTurnSpeed;
        }


        Quaternion q = transform.rotation;
        Vector3 angles = q.eulerAngles;
        angles.x = angles.z = 0f;

        if (_InvertSteeringWhenInReverse && _CurrentMovementSpeed < 0f)
            angles.y -= _CurrentTurnSpeed * Time.deltaTime;
        else
            angles.y += _CurrentTurnSpeed * Time.deltaTime;

        q.eulerAngles = angles;
        transform.rotation = q;

        // Multiply the forward direction by current speed to get a velocity.       
        Vector3 velocity = transform.forward * _CurrentMovementSpeed;
        // Add gravity.
        velocity.y = -0.5f;


        Vector3 position = transform.position;
        position += velocity * Time.deltaTime;
        transform.position = position;
        
    }

    /// <summary>
    /// This function applies a bit of deceleration to the robot, so it will slow down
    /// when the player stops pressing any movement buttons.
    /// </summary>
    private void ApplyDeceleration()
    {
        
        if (_CurrentMovementSpeed > 0f)
        {
            // The robot is moving forward, so we need to subtract to slow it down.
            _CurrentMovementSpeed = Mathf.Clamp(_CurrentMovementSpeed - (_MovementDecelerationRate * Time.deltaTime), 
                                                0f,
                                                _stats.MaxMovementSpeed);
        }
        else
        {
            // The robot is moving backwards, so we need to add to slow it down.
            _CurrentMovementSpeed = Mathf.Clamp(_CurrentMovementSpeed + (_MovementDecelerationRate * Time.deltaTime), 
                                                -_stats.MaxMovementSpeed,
                                                0f);
        }
        

        // If the current speed is almost zero, then just set it to zero.
        if (Mathf.Abs(_CurrentMovementSpeed) < 0.1f)
            _CurrentMovementSpeed = 0f;


        if (_CurrentTurnSpeed > 0f)
        {
            // The robot is turning right, so we need to add to slow it down.
            _CurrentTurnSpeed = Mathf.Clamp(_CurrentTurnSpeed - (_TurningDecelerationRate * Time.deltaTime), 
                                            0f, 
                                            _MaxTurnSpeed);
        }
        else
        {
            // The robot is turning left, so we need to add to slow it down.
            _CurrentTurnSpeed = Mathf.Clamp(_CurrentTurnSpeed + (_TurningDecelerationRate * Time.deltaTime), 
                                            -_MaxTurnSpeed,
                                            0f);
        }
    }

    /// <summary>
    /// This function is called once per frame. It charges or drains the battery depending on whether the robot is active.
    /// </summary>
    private void ApplyBatteryChargeOrDrain()
    {
        if (IsActive)
        {
            _CurrentBatteryCharge = Mathf.Clamp(_CurrentBatteryCharge - _BatteryDrainPerSecond,
                                                0f,
                                                _BatteryMaxCapacity);
        }
        else
        {
            _CurrentBatteryCharge = Mathf.Clamp(_CurrentBatteryCharge + _BatteryRechargePerSecond,
                                                0f,
                                                _BatteryMaxCapacity);
        }


        OnBatteryLevelChanged?.Invoke(this, new RobotBatteryEventArgs(BatteryChargePercentage));
    }

    /// <summary>
    /// Returns true if the robot is able to be activated right now.
    /// </summary>
    /// <returns></returns>
    public bool CanActivate()
    {
        if (!IsActive && // The robot is not already active
            Time.timeScale == 1.0f && // The game is not paused
            (_WaveManager.IsWaveInProgress || _AllowActivationBetweenWaves) && // A cat wave is in progress
            BatteryChargePercentage >= _RobotMinActivationThreshold) // The robot has enough battery power
        {
            return true;
        }


        return false;
    }

    private void FireProjectile()
    {
        // Select a random cat toy projectile
        int index = UnityEngine.Random.Range(0, _ProjectilePrefabs.Count);

        // Get a random projectile prefab.
        GameObject prefab = _ProjectilePrefabs[index].gameObject;
        
        // Create a new projectile and launch it.
        GameObject projectile = Instantiate(prefab, _ProjectileLaunchPoint.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody>().velocity = transform.forward * (projectile.GetComponent<RobotProjectile>().LaunchSpeed * _LaunchAdjustment);
    }

    /// <summary>
    /// This function is called when the player activates the robot.
    /// </summary>
    public void ActivateRobot()
    {
        if (!CanActivate())
            return;

        // Activate the robot.
        IsActive = true;

        // Deselect any selected towers.
        TowerBase.DeselectAllTowers();

        // Enable the robot's camera.
        _RobotVirtualCamera.enabled = true;

        // Set the currently selected game object to null. After entering robot mode, the UI button will still be selected.
        // This caused it to press the button again when I pressed the fire projectile button!
        EventSystem.current.SetSelectedGameObject(null);

        // Switch to the robot camera.
        CameraManager.Instance.SwitchToCamera(CameraTypes.RobotCamera);

        // Fire the state changed event.
        OnRobotStateChanged?.Invoke(this, new RobotStateChangedEventArgs(IsActive));
    }

    /// <summary>
    /// This function is called when the player deactivates the robot, or its battery gets too low.
    /// </summary>
    public void DeactivateRobot()
    {
        // Switch back to the main game camera.
        if (CameraManager.Instance != null)
            CameraManager.Instance.SwitchToCamera(CameraTypes.MainGameCamera);

        // Deactivate the robot.
        IsActive = false;

        // Enable the robot's camera.
        _RobotVirtualCamera.enabled = false;

        // Fire the state changed event.
        OnRobotStateChanged?.Invoke(this, new RobotStateChangedEventArgs(IsActive));
    }

    /// <summary>
    /// This function is called when the player clicks the robot button on the UI.
    /// </summary>
    public void ToggleRobotButtonClicked()
    {
        if (IsActive)
            DeactivateRobot();
        else
            ActivateRobot();
    }

    IEnumerator DistractCatsInRange()
    {
        yield return new WaitForSeconds(10);
    }

    /// <summary>
    /// Returns the current battery charge as a percentage.
    /// </summary>
    public float BatteryChargePercentage
    {
        get { return _CurrentBatteryCharge / _BatteryMaxCapacity; }
    }

    /// <summary>
    /// Returns true if the robot is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    public float BatteryMaxCapacity
    {
        get { return _BatteryMaxCapacity; }
        set { _BatteryMaxCapacity = value; }
    }

    public float BatteryRechargeRate
    {
        get { return _BatteryRechargePerSecond; }
        set { _BatteryRechargePerSecond = value; }
    }

    public float BatteryDrainPerSecond
    {
        get { return _BatteryDrainPerSecond; }
        set { _BatteryDrainPerSecond = value; }
    }
}
