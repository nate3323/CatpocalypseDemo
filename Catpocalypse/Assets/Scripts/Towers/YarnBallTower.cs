using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YarnBallTower : Tower
{
    [SerializeField] private GameObject yarnBallPrefab;
    [SerializeField] private GameObject throwPointPrefab;
    [SerializeField] private GameObject ballShotgunPrefab;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwRange = 5f;
    private bool canThrow = true;
    private float sizeMultiplier = 1;
    public bool upgraded = false;
    private float yarnFirerate;
    private float startingRange;
    private new void Start()
    {
        base.Start();
        startingRange = range.radius;
        ApplyScrapUpgrades();
        yarnFirerate = towerStats.FireRate;
        // Start the projectile throwing coroutine
        StartCoroutine(ThrowProjectiles());
    }

    protected override void ApplyScrapUpgrades()
    {
        if (PlayerDataManager.Instance.CurrentData.yarnUpgrades > 0)
        {
            fireRate *= PlayerDataManager.Instance.Upgrades.YarnThrowerFireRateUpgrade;
            if (PlayerDataManager.Instance.CurrentData.yarnUpgrades > 1)
            {

                if (PlayerDataManager.Instance.CurrentData.yarnUpgrades > 2)
                {

                    if (PlayerDataManager.Instance.CurrentData.yarnUpgrades > 3)
                    {
                        if (PlayerDataManager.Instance.CurrentData.yarnUpgrades > 4)
                        {

                        }
                    }
                }
            }
        }
    }

    IEnumerator DistractCat(GameObject cat)
    {

        if (targets.Contains(cat) && cat != null)
        {

            cat.GetComponent<CatBase>().DistractCat(distractValue, this.gameObject.GetComponent<Tower>());
            yield return new WaitForSeconds(5f);
            StartCoroutine(DistractCat(cat));
        }

    }

    IEnumerator ThrowProjectiles()
    {
        while (true)
        {
            // Check if canThrow is true before throwing the projectile
            if (canThrow)
            {
                if (IsTargetInRange())
                {
                    ThrowProjectile();
                    canThrow = false; // Set to false after throwing, prevent further throws until reset

                    // Wait for a specified time before allowing another throw
                    yield return new WaitForSeconds(yarnFirerate); // Adjust the delay as needed

                    canThrow = true; // Set back to true to allow another throw
                }
            }

            yield return null;
        }
    }

    bool IsTargetInRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, throwRange);

        foreach (Collider collider in colliders)
        {
            // Check if the collider belongs to the "cat" layer
            if (collider.gameObject.layer == LayerMask.NameToLayer("Cat"))
            {
                return true;
            }
        }

        return false;
    }

    void ThrowProjectile()
    {
        if (throwPointPrefab != null)
        {
            // Instantiate the throwPoint prefab
            GameObject throwPointObject = Instantiate(throwPointPrefab, transform.position, transform.rotation);
            Transform throwPoint = throwPointObject.transform;
            _towerSound.Play();
            GameObject projectile = Instantiate(yarnBallPrefab, throwPoint.position, throwPoint.rotation);
            projectile.GetComponent<YarnBall>().parentTower = gameObject.GetComponent<Tower>();
            projectile.transform.localScale *= sizeMultiplier;
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Get the target GameObject with the "cat" layer
                GameObject target = FindTargetByLayer("Cat");

                if (target != null)
                {
                    // Calculate the direction to the target
                    Vector3 direction = (target.transform.position - throwPoint.position).normalized;

                    // Apply force in the direction of the target
                    rb.AddForce(direction * throwForce, ForceMode.Impulse);
                    _towerSound.Play();
                }
            }
        }
            
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            targets.Add(other.gameObject);
            
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
        {
            targets.Remove(other.gameObject);
            
        }
    }
    public override void Upgrade()
    {
        if (towerLevel == 1)
        {
            base.Upgrade();
            upgraded = true;
        }
        else
        {
            yarnFirerate = yarnFirerate +(towerStats.FireRate * _towerUpgradesData.fireRateUpgradePercent);
            range.radius = range.radius + (startingRange * _towerUpgradesData.rangeUpgradePercent);
        }
       
    }
    GameObject FindTargetByLayer(string Cat)
    {
        // Find the target by layer name
        GameObject[] targets = GameObject.FindGameObjectsWithTag(Cat);

        if (targets.Length > 0)
        {
            return targets[0]; // Assuming there is only one target
        }

        return null;
    }
   
    
}