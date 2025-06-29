using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CucumberTower : Tower
{
    [SerializeField] GameObject cucumberPrefab;
    [SerializeField] GameObject superCucumberPrefab;
    [SerializeField]
    private Transform spawn;
    [SerializeField]
    private GameObject postProcess;
    [SerializeField]
    private Transform fireSpawn;


    [Header("Aiming Settings")]

    [Tooltip("This sets how far the tower should aim in front of a cat.")]
    [Min(0f)]
    [SerializeField]
    private float _DistanceInFrontOfTargetToAimFor = 5f;

    [Tooltip("How much gravity is applied to the cucumbers.")]
    [SerializeField]
    private float _Gravity = 9.81f;

    [Tooltip("The veritical launch angle of the cucumbers (in degrees).")]
    [Min(0)]
    [SerializeField]
    private float _LaunchAngle = 15f;

    [Tooltip("This controls how fast the tower rotates to aim.")]
    [Min(1f)]
    [SerializeField]
    private float _AimSpeed = 30f;

    [Tooltip("This controls how close the tower's aim must be to the target point (in degrees) before it will fire a cucumber.")]
    [SerializeField]
    private float _AimThreshold = 10f;


    private GameObject _CurrentTarget;
    private Vector3 _CurrentAimDirection;

    private float reloadTime = 2f;
    private float startingReloadTime = 2f;

    public bool buffCats;

    [SerializeField,Tooltip("How often the Super Cucumber ability activates")]
    private int superCucumberCooldown = 12;
    
    private bool canSCBeFired = false;
    private bool firingSC = false;
    private ParticleSystem _particle;
    private ParticleSystem _fire;
    
    private new void Awake()
    {
        base.Awake();

        _CurrentAimDirection = transform.forward;
    }

    // Start is called before the first frame update
    private new void Start()
    {
        base.Start();
        ApplyScrapUpgrades();
        StartCoroutine(Aim());
        _particle = spawn.gameObject.GetComponent<ParticleSystem>();
        _fire = fireSpawn.gameObject.GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Adds cats to the target list as they get in range
        if(other.gameObject.CompareTag("Cat"))
        {
            targets.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //Removes cats from the target list as they get out of range
        if (other.gameObject.CompareTag("Cat"))
        {
            targets.Remove(other.gameObject);
        }
    }

    protected override void ApplyScrapUpgrades()
    {
        if(PlayerDataManager.Instance.CurrentData.cucumberUpgrades > 0)
        {
            fireRate *= PlayerDataManager.Instance.Upgrades.CucumberFireRateUpgrade;
            if (PlayerDataManager.Instance.CurrentData.cucumberUpgrades > 1)
            {
                _AimSpeed *= PlayerDataManager.Instance.Upgrades.CucumberAimSpeedUpgrade;
                if (PlayerDataManager.Instance.CurrentData.cucumberUpgrades > 2)
                {
                    range.radius *= PlayerDataManager.Instance.Upgrades.CucumberRangeUpgrade;
                    if (PlayerDataManager.Instance.CurrentData.cucumberUpgrades > 3)
                    {
                        if (PlayerDataManager.Instance.CurrentData.cucumberUpgrades > 4)
                        {

                        }
                    }
                }
            }
        }
    }

    private Vector3 _TargetPoint;
    private Vector3 _Direction;
    private void Fire(GameObject target)
    {                     
        // Calculate distance between tower and target point.
        float distance = Vector3.Distance(transform.position, _TargetPoint);

        // Looking at the launch angle as a right triangle (right angle opposite the launcher)
        // we calculate the length of the opposite side of the triangle (aka vertical side, so its the height);
        Vector3 direction = _TargetPoint - transform.position;
        direction.y = distance * Mathf.Tan(_LaunchAngle * Mathf.Deg2Rad);

        _Direction = direction;



        // Calculate the launch velocity, and then combine it with the direction vector
        // to get the launch velocity as the proper 3D direction vector.
        float launchVelocity = CalculateLaunchVelocity(_TargetPoint, distance);

        // Spawn the cucumber.
        if (!firingSC)
        {

            GameObject proj = Instantiate(cucumberPrefab, spawn.transform.position, Quaternion.identity);
            Cucumber cucumber = proj.gameObject.GetComponent<Cucumber>();
            cucumber.target = target;
            cucumber.parentTower = this;
            proj.GetComponent<Rigidbody>().velocity = direction.normalized * launchVelocity;


        }
        //If firing a super cucumber
        else
        {
            Debug.Log("SuperCuc fired");
            GameObject proj = Instantiate(superCucumberPrefab, spawn.transform.position, Quaternion.identity);
            SuperCucumber cucumber = proj.gameObject.GetComponent<SuperCucumber>();
            cucumber.target = target;
            cucumber.parentTower = this;
            proj.GetComponent<Rigidbody>().velocity = direction.normalized * launchVelocity;
            StartCoroutine(SuperCucumber());
        }
        _towerSound.Play();
        _particle.Play();
        _fire.Play();
        //postProcess.GetComponent<Volume>()

        StartCoroutine(Reload());

    }
    

    /// <summary>
    /// This function makes the tower swivel toward the target.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Aim()
    {
        

        while (true)
        {
            GameObject target = SelectTarget();


            Vector3 enemyPos;
            if (target != null)
            {
                _TargetPoint = new Vector3(_TargetPoint.x, target.transform.position.y, _TargetPoint.z);
            }
            else
            {
                enemyPos = new Vector3(0f, 0f, 1000f); // There are no enemies outside of the enemy attack phase, so set a fake enemy position to the north.

                // Wait until the next frame.
                yield return null;
                continue;
            }


            Vector3 targetDirection = Vector3.Normalize(_TargetPoint - transform.position);
            

            // First get the horizontal rotation angle.
            float angleH = CalculateSignedAngle(_CurrentAimDirection, targetDirection, Vector3.up);
            float rotAmount = _AimSpeed * Time.deltaTime;

            if (angleH < 0)
                rotAmount *= -1;

            transform.Rotate(new Vector3(0, rotAmount, 0));
            _CurrentAimDirection = transform.forward;

            if (Mathf.Abs(angleH) <= _AimThreshold)
            {
                Fire(target);
                yield return Reload();
            }
            else
            {
                // Wait until next frame.
                yield return null;
            }
        } // end while(true)
    }

    //Overrides the Upgrade function in the tower script
    public override void Upgrade()
    {
        base.Upgrade();
        if(towerLevel == 1)
        {
            StartCoroutine(SuperCucumber());
            canSCBeFired = true;
        }
        else
        {
            reloadTime = reloadTime - (startingReloadTime * _towerUpgradesData.fireRateUpgradePercent);
            range.radius = range.radius + (towerStats.Range * _towerUpgradesData.rangeUpgradePercent);
        }



    }
    private Vector3 CalculateTargetPoint(GameObject target)
    {
        Vector3 targetPoint = target.transform.position;


        // Get the forward vector for the target and multiply by the distance we want to aim in front of the target.
        Vector3 forward = target.transform.forward;
        forward *= _DistanceInFrontOfTargetToAimFor;


        targetPoint += forward;

        return targetPoint;
    }

    private float CalculateLaunchVelocity(Vector3 targetPoint, float distanceFromTower)
    {
        float launchAngleInRadians = _LaunchAngle * Mathf.Deg2Rad;

        // Calculate the launch velocity we need (https://www.youtube.com/watch?v=_KLfj84SOh8)
        float numerator = _Gravity * Mathf.Pow(distanceFromTower, 2);
        float denominatorPart1 = 2f * Mathf.Pow(Mathf.Cos(launchAngleInRadians), 2);
        float denominatorPart2 = spawn.position.y + distanceFromTower * Mathf.Tan(launchAngleInRadians);
        float denominator = denominatorPart1 * denominatorPart2;


        
        // Use launch velocity equation: 
        float launchVelocity = Mathf.Sqrt(numerator / denominator);

        //Debug.Log($"LaunchVel: {launchVelocity}    Num: {numerator}    D1: {denominatorPart1}    D2: {denominatorPart2}    D: {denominator}");

        return launchVelocity;
    }

    private GameObject SelectTarget()
    {
        GameObject target = null;
        float smallestDistance = float.MaxValue;
        //If the Super Cucumber is not ready, target normally
        if (!canSCBeFired)
        {
            foreach (GameObject cat in targets)
            {
                if (cat != null)
                {
                    float distance = Vector3.Distance(transform.position, cat.transform.position);
                    if (distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        target = cat;
                    }
                }
            }
        }
        //If the Super Cucumber can be fired, target the cat closest to the goal
        else
        {
            firingSC = true;
            foreach (GameObject cat in GameObject.FindGameObjectsWithTag("Cat"))
            {
                if (cat != null)
                {
                    float distance = Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Goal").transform.position);
                    if (distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        target = cat;
                    }
                }
            }
        }
        


        // This null check prevents an exception being thrown below when we call CalculateTargetPoint().
        if (target == null)
        {
            return null;
        }

        // Find the point that is the specified distance ahead of the target.
        _TargetPoint = CalculateTargetPoint(target);

        return target;
    }
    IEnumerator Reload()
    {
        reloadTime = towerStats.FireRate;
        yield return new WaitForSeconds(reloadTime);
        //reloadTime = FireRate;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(spawn.transform.position, _TargetPoint);
    }

    /// <summary>
    /// Unlike the Vector2.Angle and Vector3.Angle functions, this one returns the angle as a number from -180 to +180 degrees.
    /// The previously mentioned Angle functions return the smallest angle between the two vectors as a value from 0-180 degrees.
    /// </summary>
    /// <param name="from">The starting vector.</param>
    /// <param name="to">The vector to measure to.</param>
    /// <param name="referenceAxis">The axis the rotation is around.</param>
    /// <returns>The angle between the passed in vectors (0 to 360 degrees).</returns>
    public float CalculateSignedAngle(Vector3 from, Vector3 to, Vector3 referenceAxis)
    {
        // Get the smallest angle between the two vectors (will be in the range 0 to 180 degrees);
        float angle = Vector3.Angle(from, to);

        // Change it to be in the range -180 to +180 degrees relative to the from vector.
        float sign = Mathf.Sign(Vector3.Dot(referenceAxis, Vector3.Cross(from, to)));
        return angle * sign;
    }
    //The Super Cucumber cooldown
    IEnumerator SuperCucumber()
    {
        firingSC = false;
        canSCBeFired = false;
        yield return new WaitForSeconds(superCucumberCooldown);
        canSCBeFired = true;

        
    }
}
