using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fortifications : MonoBehaviour
{
    [SerializeField]
    private PlayerUpgradeData upgradeData;
    public List<GameObject> targets; 
    public int distactionValue = 5;
    [SerializeField]
    private GameObject spawnPoint;

    [SerializeField]
    private GameObject person;

    [SerializeField,Tooltip("How often the Tier 5 upgrade does damage")]
    private int damageInterval = 1;
    [SerializeField,Tooltip("How much damage the Tier 5 upgrade does")]
    private int tierFiveDistractValue = 4;
    // Start is called before the first frame update
    void Start()
    {
        targets = new List<GameObject>();
        if (PlayerDataManager.Instance.CurrentData.fortificationUpgrades > 1) 
        { 
            FortificationTierTwo(); 
        }
        if(PlayerDataManager.Instance.CurrentData.fortificationUpgrades > 4)
        {
            
            FortificationTierFive();
        }

    }
    private void FortificationTierTwo()
    {
        GameObject newPerson = Instantiate(person,spawnPoint.transform.position,Quaternion.identity,gameObject.transform);
        newPerson.transform.localScale = new Vector3(.2f,.2f,.2f);
    }
    private void FortificationTierFive()
    {
        StartCoroutine(TierFive());
    }
    IEnumerator TierFive()
    {
        List<GameObject> currenttargets = new List<GameObject>();
        foreach (GameObject item in targets)
        {
            currenttargets.Add(item);
        }
        //currenttargets = targets;
        if(currenttargets.Count > 0)
        {
            foreach (GameObject target in currenttargets)
            {
                if(target != null)
                {
                    target.GetComponent<CatBase>().FortificationCatDistraction(tierFiveDistractValue, this);
                }
            }
        }
        currenttargets.Clear();
        yield return new WaitForSeconds(damageInterval);
        StartCoroutine(TierFive());
    }

}


