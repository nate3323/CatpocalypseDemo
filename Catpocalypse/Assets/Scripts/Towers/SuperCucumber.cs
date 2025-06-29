using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCucumber : MonoBehaviour
{
    public GameObject target;
    public CucumberTower parentTower;
    [SerializeField,Tooltip("The smaller cucumbers that are spawned")]
    private GameObject cucumber;
    private List<GameObject> catsInRange;
    //true if the cucumber was spawned by a tier 5 cucumber tower
    //public bool _isSubCuc;
    // Start is called before the first frame update
    [SerializeField]
    private int _numberOfCucumbers = 5;

    [SerializeField, Tooltip("Sub Cucumber launch force")]
    private int _launchForce = 5;
    void Start()
    {
        catsInRange = new List<GameObject>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Cat")
        {
            SpawnCucumbers();
            //DistractCats();
            
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.tag == "Cat")
        {
            if (!catsInRange.Contains(collision.gameObject))
            {
                catsInRange.Add(collision.gameObject);
            }
           
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Cat")
        {
            catsInRange.Remove(other.gameObject);
        }
    }
    //private void DistractCats()
    //{
    //    foreach(GameObject cat in catsInRange)
    //    {
    //        if(cat != null)
    //        {
    //            cat.GetComponent<CatBase>().DistractCat(parentTower.GetComponent<Tower>().towerStats.DistractValue,parentTower.GetComponent<Tower>());
    //        }
            
    //    }
    //}
    void SpawnCucumbers()
    {
        Debug.Log("SpawnCucumbers called");
        for(int i = 0; i < _numberOfCucumbers; i++)
        {
            GameObject subCucumber = Instantiate(cucumber, new Vector3(gameObject.transform.position.x, transform.position.y, transform.position.z), Quaternion.identity, null);
            subCucumber.GetComponent<Rigidbody>().AddForce(new(Random.Range(-_launchForce, _launchForce), Random.Range(-_launchForce, _launchForce), Random.Range(-_launchForce, _launchForce)), ForceMode.Impulse);
            subCucumber.GetComponent<SubCucumber>().parentTower = parentTower;
        }
        //yield return null;
        Destroy(gameObject);
    }
}
