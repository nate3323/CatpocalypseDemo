using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortTargets : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Cat"))
        {
            
            gameObject.transform.parent.gameObject.GetComponent<Fortifications>().targets.Add(other.gameObject);
            Debug.LogWarning(gameObject.transform.parent.gameObject.GetComponent<Fortifications>().targets[0]);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Cat"))
        {
            gameObject.transform.parent.gameObject.GetComponent<Fortifications>().targets.Remove(other.gameObject);
        }
    }
}
