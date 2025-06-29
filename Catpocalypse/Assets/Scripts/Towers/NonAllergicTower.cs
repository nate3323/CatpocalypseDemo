using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonAllergicTower : Tower
{
    [SerializeField, Tooltip("The number of people the tower spawns"),Min(1)]
    private int maxNumOfPeople;
    private int peopleSpawned;
    [SerializeField, Tooltip("The speed of the spawned people")] private float personSpeed;
    [SerializeField, Tooltip("List of potential locations for the people to spawn")]
    private List<Transform> spawnPoints;
    [SerializeField, Tooltip("The Non-Allergic people that the tower spawns")]
    private GameObject person;
    private GameObject[] waypoints;
    private WayPoint closestWaypoint;
    private Transform spawnPoint;
    private List<GameObject> personList;
    public bool Enabled = true;
    PlayerCutenessManager cutenessManager;
    [SerializeField,Tooltip("How long it takes for the foodtime ability to cooldown")]
    private int foodTimeCooldown = 30;
    [SerializeField,Tooltip("How much Foodtime distracts cats")]

    private float foodTimeDistractValue = 40f;
    private bool foodTimeUnlocked = false;


    // Start is called before the first frame update
    private new void Start()
    {
        base.Start();

        ApplyScrapUpgrades();

        cutenessManager = GameObject.FindGameObjectWithTag("Goal").gameObject.GetComponent<PlayerCutenessManager>();
        //Disables the tower if it is built during the Non-Allergic Strike cuteness challenge
        if (cutenessManager.CurrentCutenessChallenge == PlayerCutenessManager.CutenessChallenges.NonAllergicStrike)
        {
            Enabled = false;
        }
        peopleSpawned = 0;
        if (FindNearestWayPoint(out WayPoint closestPoint))
        {
            closestWaypoint = closestPoint;
        }
        personList = new List<GameObject>();
        float dist = 100000;

        foreach(Transform spawn in spawnPoints)
        {

            if (Vector3.Distance(spawn.position, closestWaypoint.transform.position) < dist)
            {
                spawnPoint = spawn;
                dist = Vector3.Distance(spawn.position, closestWaypoint.transform.position);
            }
        }
        Debug.Log("I should be spawning");
        StartCoroutine(Spawner());
    }

    


    protected override void ApplyScrapUpgrades()
    {
        if (PlayerDataManager.Instance.CurrentData.nAUpgrades > 0)
        {
            // Placeholder for any future changes
            if (PlayerDataManager.Instance.CurrentData.nAUpgrades > 1)
            {
                // Placeholder
                if (PlayerDataManager.Instance.CurrentData.nAUpgrades > 2)
                {
                    personSpeed *= PlayerDataManager.Instance.Upgrades.NAMoveSpeedUpgrade;
                    if (PlayerDataManager.Instance.CurrentData.nAUpgrades > 3)
                    {
                        if (PlayerDataManager.Instance.CurrentData.nAUpgrades > 4)
                        {

                        }
                    }
                }
            }
        }
    }

    public override void Upgrade()
    {
        base.Upgrade();
        maxNumOfPeople++;
        if (towerLevel == 1)
        {
            foodTimeUnlocked = true;
            FoodTime();
        }
        else
        {
            range.radius = range.radius + (towerStats.Range * _towerUpgradesData.rangeUpgradePercent);
        }
       
    }
    void FoodTime()
    {
        GameObject[] cats = targets.ToArray();
        if(targets.Count > 0)
        {
            for(int i = 0;i<cats.Length;i++)
            {
                if(cats[i] != null)
                {
                    Debug.LogWarning("Cat distracted by Foodtime");
                    cats[i].GetComponent<CatBase>().DistractCat(foodTimeDistractValue, gameObject.GetComponent<Tower>());
                }
            }
            StartCoroutine(FoodTimeCooldown());
        }
        else
        {
            StartCoroutine(FoodTimeRecall());
        }
    }
    IEnumerator FoodTimeRecall()
    {
        yield return new WaitForSeconds(1);
        FoodTime();
    }
    IEnumerator FoodTimeCooldown()
    {
        yield return new WaitForSeconds(foodTimeCooldown);
        FoodTime();
    }

    public void DisableTower()
    {
        Enabled = false;
        peopleSpawned = 0;
        
       
    }
    IEnumerator Spawner()
    {
        yield return new WaitForSeconds(towerStats.FireRate);
        if (Enabled)
        {
            if (peopleSpawned >= maxNumOfPeople)
            {

                StopCoroutine(Spawner());
            }
            else
            {

                GameObject newPerson = Instantiate(person, _RallyPoint, Quaternion.identity, gameObject.transform);
                _towerSound.Play();
                //personList.Add(newPerson);

                peopleSpawned++;

            }
        }
        
        
        
        StartCoroutine(Spawner());
        
    }

    public int ActiveUnits {  get { return peopleSpawned; } }
}
