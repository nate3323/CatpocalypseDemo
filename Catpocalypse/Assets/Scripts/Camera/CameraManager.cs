using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Cinemachine;


public enum CameraTypes
{
    MainGameCamera = 0,
    RobotCamera,
}


/// <summary>
/// This class manages switching cameras.
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    private const string ROBOT_CAMERA_GAMEOBJECT_NAME = "Robot Virtual Camera";


    private Dictionary<CameraTypes, CinemachineVirtualCamera> _Cameras = new Dictionary<CameraTypes, CinemachineVirtualCamera>();



    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("An instance of CameraManager already exists! So this one destroyed itself.");
            Destroy(gameObject);
        }


        Instance = this;

        // Register the main camera.
        _Cameras.Add(CameraTypes.MainGameCamera, Camera.main.gameObject.GetComponentInChildren<CinemachineVirtualCamera>());
    }


    /// <summary>
    /// Registers a <see cref="CinemachineVirtualCamera"/> with this camera manager.
    /// </summary>
    /// <param name="newcamera">The new camera to register.</param>
    /// <returns>True if the camera was successfully registered.</returns>
    public bool RegisterCamera(CinemachineVirtualCamera newCamera, CameraTypes cameraType)
    {
        if (_Cameras.ContainsKey(cameraType) == false)
        {
            _Cameras.Add(cameraType, newCamera);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Switches the view to the specified camera.
    /// </summary>
    /// <param name="cameraType"></param>
    public void SwitchToCamera(CameraTypes cameraType)
    {
        // Switch to the specified camera.
        _Cameras[cameraType].MoveToTopOfPrioritySubqueue();
    }
}
