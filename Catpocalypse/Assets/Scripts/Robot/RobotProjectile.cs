using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// This class represents a projectile fired by the robot.
/// </summary>
public class RobotProjectile : MonoBehaviour
{

    [Tooltip("Sets the launch speed of this projectile in meters per seecond.")]
    [SerializeField, Min(0f)]
    public float _LaunchSpeed = 10f;

    [Tooltip("How long the projectile will exist before it disappears.")]
    [SerializeField, Min(1f)]
    protected float _Lifetime = 10f;

    [SerializeField]
    private RobotStats _stats;
    /// <summary>
    /// Tracks how long the projectile has existed for.
    /// </summary>
    private float _Timer;



    // Start is called before the first frame update
    void Start()
    {
        _LaunchSpeed = _stats.LaunchSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        _Timer += Time.deltaTime;
        if (_Timer >= _Lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Cat")
        {
            CatBase target = collision.gameObject.GetComponent<CatBase>();
            if (PlayerDataManager.Instance.CurrentData.robotUpgrades > 4)
            {
                StartCoroutine(Stun(target));
            }
            if (target != null)
                Distract(target);

        }
    }

    /// <summary>
    /// This function is called to distract the cat the projectile hit.
    /// </summary>
    /// <param name="target"></param>
    private void Distract(CatBase target)
    {
        target.DistractCat(_stats.DistractionValue, null);

        Destroy(gameObject);
    }
    IEnumerator Stun(CatBase target)
    {
        target.stoppingEntities.Add(gameObject);
        yield return new WaitForSeconds(.5f);
        target.stoppingEntities.Remove(gameObject);

    }
    

    



    

    public float LaunchSpeed
    {
        get { return _LaunchSpeed; }
        set { _LaunchSpeed = value; }
    }

    public float LifeTime
    {
        get { return _Lifetime; }
        set { _Lifetime = value; }
    }

}
