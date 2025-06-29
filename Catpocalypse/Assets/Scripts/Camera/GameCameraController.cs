using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


/// <summary>
/// This class controls the main game camera, responding to user input to move it around as the user desires.
/// </summary>
public class GameCameraController : MonoBehaviour
{
    public enum MouseButtons { Left, Middle, Right };


    [Header("Camera Movement Settings")]
    [Tooltip("The movement speed of the camera when you click and drag.")]
    [SerializeField] private float _CameraMouseDragMoveSpeed = 8f;

    [Tooltip("The movement speed of the camera in meters per second when you pan using WASD, arrowkeys, or left stick on a gamepad.")]
    [SerializeField] private float _CameraMoveSpeed = 10f;

    // See the comments in the InitCamera() function below for more on this object.
    [SerializeField] private Transform _CameraTargetObject;

    [Tooltip("This specifies the max distance the camera can be from the origin on each axis. It prevents it from moving more than the specified distance from (0,0,0) on any axis.")]
    [SerializeField] private Vector3 _CameraMoveLimits;

    [Tooltip("This property specifies which mouse button is used to drag the camera around.")]
    [SerializeField] private MouseButtons _DragMovementButton = MouseButtons.Right;

    [Tooltip("This property specifies whether or not the mouse move amount is inverted before being used to move the camera.")]
    [SerializeField] private bool _InvertMouseInputForDrags = true;

    [Tooltip("This specifies the object the camera will be focused on at the start of the level. If it is null, then it will start focused on (0,0,0).")]
    [SerializeField] private GameObject _CameraStartTarget;


    [Header("Zoom Settings")]
    [Tooltip("The minimum zoom distance (in units/meters).")]
    [SerializeField] private float _MinZoomDistance = 4f;
    [Tooltip("The maximum zoom distance (in units/meters).")]
    [SerializeField] private float _MaxZoomeDistance = 64f;
    [Tooltip("How much (in units/meters) that the camera moves forward/back per mouse scroll wheel movement.")]
    [SerializeField] private float _ZoomRate = 2f;
    [Tooltip("Whether or not to invert the mouse scroll wheel input.")]
    [SerializeField] private bool _InvertScrollwheelInput = true;


    private CinemachineVirtualCamera _VirtualCamera;
    private float _ZoomDistance;
    private float _DefaultZoomDistance;



    private void Awake()
    {
        _CameraTargetObject.gameObject.SetActive(true);
        _VirtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (_CameraStartTarget != null)
        {
            _CameraTargetObject.transform.position = _CameraStartTarget.transform.position;
        }
        InitCamera();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // If the user moved the scroll wheel, then adjust zoom level.
        if (Mouse.current.scroll.value != Vector2.zero)
            AdjustZoomLevel(Mouse.current.scroll.value);


        // Check if the user pressed the WASD or arrow keys, or dragged the mouse with RMB down.
        // If so, move the camera accordingly.
        if (PlayerInputManager.PanCamera != Vector2.zero)
            MoveCamera(PlayerInputManager.PanCamera * _CameraMoveSpeed);

        HandleMouseDrags();
    }


    private void HandleMouseDrags()
    {
        if ((_DragMovementButton == MouseButtons.Left && Mouse.current.leftButton.isPressed) ||
            (_DragMovementButton == MouseButtons.Middle && Mouse.current.middleButton.isPressed) ||
            (_DragMovementButton == MouseButtons.Right && Mouse.current.rightButton.isPressed))
        {
            // Get the mouse move amount.
            Vector2 delta = Mouse.current.delta.value;

            MoveCamera((_InvertMouseInputForDrags ? -delta : delta) * _CameraMouseDragMoveSpeed);
        }
    }

    private void MoveCamera(Vector2 moveInput)
    {
        // Calculate the move distance on both the X and Z axis. We just set Y to 0 so the camera
        // stays at the height it is at.
        Vector3 moveDistance = new Vector3(moveInput.x, 0f, moveInput.y);

        // We also multiply by Time.deltaTime so that the camera will move the correct amount
        // regardless of the current frame rate.
        moveDistance *= Time.deltaTime;

        // Calculate the new position of the camera target object and clamp it within our camera movement limits.
        Vector3 newPosition = ClampPosition(_CameraTargetObject.transform.position + moveDistance);

        // Move the camera target object.
        _CameraTargetObject.transform.position = newPosition;
    }

    private void InitCamera()
    {
        // Here I am setting the camera to both look at and follow the camera target object.
        // The camera target object is just a sphere I made in the scene. It is invisible.
        // When you use the pan camera controls, they move this object around, causing the
        // camera to move. I used Cinemachine so we have nicer camera movement/transitions.

        _VirtualCamera.LookAt = _CameraTargetObject;
        _VirtualCamera.Follow = _CameraTargetObject;


        _ZoomDistance = Vector3.Distance(Vector3.zero, GetCameraOffset());
        _DefaultZoomDistance = _ZoomDistance;
    }

    /// <summary>
    /// This function clamps the position of the camera target object so it cannot move outside the range specified
    /// by _CameraMoveLimits on each axis. If it tries to, it will just stay at the edge.
    /// </summary>
    /// <param name="position">The position to clamp.</param>
    /// <returns>The clamped position.</returns>
    private Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3(Mathf.Clamp(position.x, -_CameraMoveLimits.x + _CameraStartTarget.transform.position.x, _CameraMoveLimits.x + _CameraStartTarget.transform.position.x),
                           Mathf.Clamp(position.y, -_CameraMoveLimits.y + _CameraStartTarget.transform.position.y, _CameraMoveLimits.y + _CameraStartTarget.transform.position.y),
                           Mathf.Clamp(position.z, -_CameraMoveLimits.z + _CameraStartTarget.transform.position.z, _CameraMoveLimits.z + _CameraStartTarget.transform.position.z));
    }

    private Vector3 GetCameraOffset()
    {
        return _VirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    }

    private void AdjustZoomLevel(Vector2 mouseScrollValue)
    {
        if (_InvertScrollwheelInput)
            mouseScrollValue = -mouseScrollValue;


        // Calculate change in camera zoom (distance from target).
        float newDistance = _ZoomDistance + ((mouseScrollValue.y / 120) * _ZoomRate);
        newDistance = Mathf.Clamp(newDistance, _MinZoomDistance, _MaxZoomeDistance);

        // Calculate the scale needed to change the zoom to the appropriate level (it's just the new zoom distance calculated as a percentage of the current zoom distance).
        float scale = newDistance / _ZoomDistance;


        // Prevent the zoom distance from going outside the range specified in the inspector.
        _ZoomDistance = newDistance;


        _VirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset *= scale;
    }

   
}
