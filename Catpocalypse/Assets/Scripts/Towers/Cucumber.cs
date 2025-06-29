using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cucumber : MonoBehaviour
{
    [Tooltip("The cucumber will disapppear after this many seconds if it does not distract any cats.")]
    [SerializeField] private float _MaxLifeTime = 5f;


    List<GameObject> cats = new List<GameObject>();
    
    public GameObject target;
    public Tower parentTower;
    [SerializeField]
    private AudioSource _landingSound;

    // If the cucumber does not distract a cat in this amount of time, it will disappear.
    private float _SpawnTime;

    void Awake()
    {
        _SpawnTime = Time.time;
    }

    private void Update()
    {
        if(target != null)
        {
            
            float distance = Vector3.Distance(transform.position, target.transform.position);
            
            if(distance < 4)
            {
                if(parentTower != null)
                {
                    Distract();
                }
                
                
            }
        }
        else if (target == null)
        {
            Destroy(gameObject);
        }
     
        
        if (Time.time - _SpawnTime >= _MaxLifeTime) 
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            cats.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            cats.Remove(other.gameObject);
        }
    }
    private void Distract()
    {
        CatBase cat = target.GetComponent<CatBase>();
        if (!parentTower.gameObject.GetComponent<CucumberTower>().buffCats)
        {
            cat.DistractCat(parentTower.GetDistractionValue(), parentTower);
        }
        else if (parentTower.gameObject.GetComponent<CucumberTower>().buffCats && //If the buff cats challenge is active
                 cat.spedUp) //If the cat is not already sped up
        {
            cat.spedUp = true;
            target.GetComponent<NavMeshAgent>().speed *= 1.25f;
            
        }
        
        Destroy(gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 0)
        {
            _landingSound.Play();
        }
    }

}
