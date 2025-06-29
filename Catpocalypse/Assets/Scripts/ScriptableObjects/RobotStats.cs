using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RobotStats : ScriptableObject
{
    [SerializeField]
    private float _maxMovementSpeed;
    public float MaxMovementSpeed
    {
        get { return _maxMovementSpeed; }
        set { _maxMovementSpeed = value; }
    }
    [SerializeField]
    private float _fireRate;
    public float FireRate
    {
        get { return _fireRate; }
        set { _fireRate = value; }
    }
    [SerializeField]
    private float _distractionValue;
    public float DistractionValue
    {
        get { return _distractionValue; }
        set { _distractionValue = value; }
    }

    private float _launchSpeed = 10f;
    public float LaunchSpeed
    {
        get { return _launchSpeed; }
        set { _launchSpeed = value; }
    }
}
