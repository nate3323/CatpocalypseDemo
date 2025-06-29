using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ScratchingPost : MonoBehaviour
{
    [Header("Scratching Post Settings")]

    [Tooltip("The amount a nearby cat's speed is slowed by")]
    [SerializeField]
    [Min(0f)]
    public float speedDebuff = 1.8f;

    [Tooltip("The duration the Scratching Post will last")]
    [SerializeField]
    [Min(0f)]
    private float _Duration;

    [Tooltip("The time between Duration ticks")]
    [SerializeField]
    [Min(0f)]
    private float _DurationTickTime;

    [Tooltip("The durability of the Scratching Post")]
    [SerializeField]
    [Min(0f)]
    private float _Durability;


    [Tooltip("The amount of Durability removed per cat per tick")]
    [SerializeField]
    [Min(0f)]
    private float _DurabilityRemovedByCat;

    [SerializeField, Tooltip("The sphere collider of the Scratching Post")] private SphereCollider range;
    private float startingRange;

    private List<GameObject> _Cats;

    private bool _Destroying = false;

    public GameObject parentTower;

    public void Start()
    {
        startingRange = range.radius;
        _Cats = new List<GameObject>();
        if(PlayerDataManager.Instance.CurrentData.scratchUpgrades > 0)
        {
            range.radius *= PlayerDataManager.Instance.Upgrades.ScratchingPostRangeUpgrade;
            if(PlayerDataManager.Instance.CurrentData.scratchUpgrades > 2)
            {
                _Durability *= PlayerDataManager.Instance.Upgrades.ScratchingPostDurabilityUpgrade;
            }
        }
        StartCoroutine(DurationCountDown(_Duration));
        ApplyUpgrades();
    }

    public void Update()
    {
        if(_Destroying)
        {
            return;
        }
        if (_Durability <= 0)
        {
            _Destroying = true;
            DestroyPost();
        }
        if (_Cats.Count > 0)
        {
            _Durability -= _Cats.Count * _DurabilityRemovedByCat * Time.deltaTime;

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Cat")
        {
            _Cats.Add(other.gameObject);
            other.GetComponent<CatBase>().slowingEntities.Add(gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Cat")
        {
            _Cats.Remove(other.gameObject);
            other.GetComponent<CatBase>().slowingEntities.Remove(gameObject);
        }
    }
    public void ApplyUpgrades()
    {
        if (parentTower.GetComponent<ScratchingPostTower>().towerLevel >1)
        {
            for (int i = 1; i < parentTower.GetComponent<ScratchingPostTower>().towerLevel; i++)
            {
                _Durability++;
                range.radius = range.radius + (startingRange * parentTower.GetComponent<ScratchingPostTower>()._towerUpgradesData.rangeUpgradePercent);
            }
        }
        
    }
    private void RemoveDurability()
    {
        _Durability -= _DurabilityRemovedByCat;
        if (_Durability <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void DestroyPost()
    {
        //TODO: OnDestroy, OnTriggerEnter, and OnTriggerExit needs to account for if there is another tower affecting the Cat speed and set
        //      it to that speed without setting IsSlowed to false. To do this, we might need to set IsSlowed to an int value (or smaller if we want)
        //      and have it act as a counter instead. From there, the Cat needs to find the greatest slowing effect and set its' speed to that.
        //      This accounts for multiple slow towers of varying effect to occur.

        foreach (GameObject obj in _Cats)
        {
            if (obj != null)
            {
                obj.GetComponent<CatBase>().slowingEntities.Remove(gameObject);
            }
        }
        parentTower.GetComponent<ScratchingPostTower>().postCount--;
        Destroy(gameObject);
    }

    private IEnumerator DurationCountDown(float currentTimeLeft)
    {
        if (currentTimeLeft == 0)
        {
            DestroyPost();
            yield return new WaitForEndOfFrame();
        }
        else
        {
            GameObject[] currentCats = _Cats.ToArray();
            DistractCats(currentCats);
            yield return new WaitForSeconds(_DurationTickTime);
            StartCoroutine(DurationCountDown(--currentTimeLeft));
        }
    }

    private void DistractCats(GameObject[] cats)
    {
        
        if (cats.Length > 0)
        {
            foreach (GameObject obj in cats)
            {
                if (obj != null)
                {
                    CatBase cat = obj.GetComponent<CatBase>();
                    cat.DistractCat(
                        parentTower.GetComponent<ScratchingPostTower>().towerStats.DistractValue,
                        parentTower.GetComponent<ScratchingPostTower>()
                        );
                    RemoveDurability();
                }
                else
                {
                    _Cats.Remove(obj);
                }
            }
        }
        
        
    }
}