using System.Collections;
using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Place this script on a GameObject to have it always face the camera.
/// </summary>
public class AlwaysFaceCamera : MonoBehaviour
{
    [Tooltip("If enabled, the object will not be allowed to rotate on the Y axis.")]
    [SerializeField]
    private bool _LockAxisRotation_Y = true;

    [Tooltip("If LockAxisRotation_Y is enabled, this specifies the angle the y-axis will be locked to.")]
    [SerializeField]
    private float _YAxisLockToRotation = 180f;



    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform);

        if (_LockAxisRotation_Y)
        {
            Vector3 temp = transform.rotation.eulerAngles;
            temp.y = _YAxisLockToRotation; // This is because the icon is rotated 180 degrees. Otherwise transform.LookAt(camera) causes it to face the wrong direction. So I solved that by putting the icon on a child object that is rotated 180 degrees.
            transform.rotation = Quaternion.Euler(temp);
        }
    }
}
