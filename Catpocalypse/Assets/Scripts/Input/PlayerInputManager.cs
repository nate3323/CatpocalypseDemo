using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }


    public PauseMenu pauseMenuPanel;

    private PlayerInput _PlayerInputComponent;

    private InputAction _Robot_FireProjectileAction;
    private InputAction _Robot_MovementAction;
    private InputAction _Robot_ToggleControlAction;

    private InputAction _PanCameraAction; // This action pans the camera when in top-down view


    private RobotController _Robot;
    private int _X_Invert = 1;
    private int _Y_Invert = 1;


    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;

        _PlayerInputComponent = GetComponent<PlayerInput>();
        GetInputActions();

        _Robot = FindAnyObjectByType<RobotController>();

        IsInitialized = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerDataManager.Instance.CurrentData._MouseXInvert)
        {
            _X_Invert = -1;
        }
        if (PlayerDataManager.Instance.CurrentData._MouseYInvert)
        {
            _Y_Invert= -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInputValues();
        if (Input.GetKeyDown(KeyCode.P))
        {
            PressedPause();
        }
    }

    /// <summary>
    /// This function gets a reference to each InputAction in the PlayerInputActions asset.
    /// </summary>
    private void GetInputActions()
    {        
        _Robot_FireProjectileAction = _PlayerInputComponent.actions["Robot - Fire Projectile"];
        _Robot_MovementAction = _PlayerInputComponent.actions["Robot - Movement"];
        _Robot_ToggleControlAction = _PlayerInputComponent.actions["Robot - Toggle Control"];
        _PanCameraAction = _PlayerInputComponent.actions["Pan Camera"];
    }

    /// <summary>
    /// This function reads in the player input values for the current frame and updates the corresponding public properties on this class.
    /// </summary>
    private void UpdateInputValues()
    {
        // Is the robot present in this scene?
        if (_Robot != null)
        {
            // The robot is present, so update it's user input values appropriately based on whether it is activated or not.
            if (_Robot.IsActive)
            {
                float mouseSensitivity = PlayerDataManager.Instance.CurrentData._MouseSensitivity;
                // The robot is active, so set the PanCamera value to zero to disable movement of the main game camera while piloting the robot.
                Robot_FireProjectile = _Robot_FireProjectileAction.WasPerformedThisFrame();
                Vector2 currentCameraMovement = _Robot_MovementAction.ReadValue<Vector2>();
                Robot_Movement = new Vector2(currentCameraMovement.x, currentCameraMovement.y);
                Robot_ToggleControl = _Robot_ToggleControlAction.WasPerformedThisFrame();
                PanCamera = Vector2.zero;
            }
            else
            {
                //  The robot is not active, so clear the robot input values to disable the robot controls.
                Robot_FireProjectile = false;
                Robot_Movement = Vector2.zero;
                Robot_ToggleControl = _Robot_ToggleControlAction.WasPerformedThisFrame(); // We don't set this one to false here, as we want this action to still work when the robot is inactive.
                PanCamera = _PanCameraAction.ReadValue<Vector2>();
            }
        }
    }

    public void PressedPause()
    {
        pauseMenuPanel.gameObject.SetActive(true);
        pauseMenuPanel.OnPauseGame();
    }

    public void InvertX()
    {
        _X_Invert *= -1;
    }

    public void InvertY()
    {
        _Y_Invert *= -1;
    }


    public bool IsInitialized { get; private set; }



    // ====================================================================================================
    // Use the properties below to get player input values for the current frame.
    //
    // I made them static so they can easily be accessed from anywhere in the codebase without needing
    // a reference to this object.
    // ====================================================================================================
    
    public static bool Robot_FireProjectile { get; private set; }
    public static Vector2 Robot_Movement { get; private set; }
    public static bool Robot_ToggleControl { get; private set; }

    public static Vector2 PanCamera { get; private set; }

}
