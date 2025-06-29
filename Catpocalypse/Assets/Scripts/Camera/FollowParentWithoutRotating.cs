using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowParentWithoutRotating : MonoBehaviour
{
    Quaternion _StartingRotation;



    private void Awake()
    {
        // Save the current rotation.
        _StartingRotation = transform.rotation;    
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Force the object to not rotate when the parent does.
        transform.rotation = _StartingRotation;
    }
}
