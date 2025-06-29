using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ScratchingPostTower : Tower
{

    

    

    [Header("Launcher Settings")]

    [Tooltip("The ScratchingPost prefab to launch")]
    [SerializeField]
    private GameObject _ScratchPost;

    [Tooltip("The IrresistibleScratchingPost prefab to launch")]
    [SerializeField]
    private GameObject _IrScratchPost;


    private bool _IsLaunching = false;

    private int _MaxPosts = 1;

    public int postCount = 0;
    private float IrCooldown = 8;
    private bool ISPReady = false;


    private new void Start()
    {
        base.Start();
        ApplyScrapUpgrades();
    }

    protected override void ApplyScrapUpgrades()
    {
        if (PlayerDataManager.Instance.CurrentData.scratchUpgrades > 1)
        {
            fireRate *= PlayerDataManager.Instance.Upgrades.ScratchingPostFireRateUpgrade;
            if (PlayerDataManager.Instance.CurrentData.scratchUpgrades > 4)
            {
                
            }
        }
    }

    public void Update()
    {
       
        if(targets.Count <= 0)
        {
            return;
        } else if(_IsLaunching)
        {
            return;
        }
        if (postCount < _MaxPosts)
        {
            postCount++;
            StartCoroutine(LaunchPost());
        }
    }

    private Vector3 FindClosestPostDestination()
    {
        if (targets[0])
        {
            //Targets the cat with the lowest current distraction/highest HP
            if(ISPReady)
            {
                float minDistractValue = 1000000000000000000; //The minimum distraction value, set to an arbitrary value to start
                Vector3 pos = new Vector3(0, 0, 0);
                for (int i = 0; i < targets.Count; i++)
                {
                    if(targets[i].GetComponent<CatBase>().Distraction < minDistractValue)
                    {
                        minDistractValue = targets[i].GetComponent<CatBase>().Distraction;
                        pos = targets[i].transform.position;
                    }
                }
                return pos;
            }
            return targets[0].transform.position;
        }
        return new Vector3(-100, -100, -100);
    }
    public override void Upgrade()
    {
        base.Upgrade();
        if(towerLevel == 1)
        {
            ISPReady = true;
        }
        //The other upgrades are applied in the scratching post script
       
    }


    private IEnumerator  LaunchPost()
    {
        _IsLaunching = true;
        Vector3 destination = FindClosestPostDestination();
        if (destination.y == -100)
        {
            _IsLaunching = false;
            yield return new WaitForEndOfFrame();
        }
        if (!ISPReady)
        {
            //TODO: Make the launch animation
            GameObject post = Instantiate(_ScratchPost, destination, Quaternion.identity);
            post.GetComponent<ScratchingPost>().parentTower = gameObject;
            
        }
        else
        {
            GameObject post = Instantiate(_IrScratchPost, destination, Quaternion.identity);
            post.GetComponent<IrresistableScratchingPost>().parentTower = gameObject;
           
            StartCoroutine(ISPCooldown());
        }
        _towerSound.Play();
        yield return new WaitForSeconds(towerStats.FireRate);
        _IsLaunching = false;
    }
    private IEnumerator ISPCooldown()
    {
        ISPReady = false;
        yield return new WaitForSeconds(IrCooldown);
        ISPReady = true;
    }


}
