using System.Collections;
using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Add this component to a GameObject to make it always face the camera.
/// </summary>
public class FaceCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}
