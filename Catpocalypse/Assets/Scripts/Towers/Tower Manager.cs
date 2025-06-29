using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    public static GameObject instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        else
        {
            instance = gameObject;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
