using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavigationController: MonoBehaviour
{
    [Min(0f)]
    [Tooltip("This sets how close the cat must get to the next WayPoint to consider itself to have arrived there. This causes it to then target the next WayPoint (or a randomly selected one if the current WayPoint has multiple next points set in the Inspector.")]
    [SerializeField] protected float _WayPointArrivedDistance = 1f;

    public NavMeshAgent agent;
    protected float _distanceFromNextWayPoint = 0f;
    protected WayPoint _nextWayPoint;
    private WayPoint _goalPoint;
    private Queue<WayPoint> currentPath;

    public event EventHandler<ReachedNextWayPointEventArgs> OnTargetReachedNextWayPoint;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentPath = new Queue<WayPoint>();
        FindNearestWayPoint();
        _goalPoint = WaveManager.Instance.WayPointUtils.FindClosestEndPoint(_nextWayPoint);
        List<WayPoint> waypointList = WaveManager.Instance.WayPointUtils.GetPath(_nextWayPoint, _goalPoint);
        if(waypointList == null)
        {
            Debug.Log("Utils don't work");
        }
        if (waypointList.Count <= 0)
        {
            Debug.Log("Utils don't work, list doesn't build correctly");
        }
        foreach (WayPoint p in waypointList)
        {
            currentPath.Enqueue(p);
        }
        agent.SetDestination(_nextWayPoint.transform.position);
    }

    private void Update()
    {
        if(agent.speed <= 0f)
        {
            return;
        }
        if (_nextWayPoint != null)
        {
            _distanceFromNextWayPoint = Vector3.Distance(transform.position, _nextWayPoint.transform.position);

            if (HasReachedDestination())
            {
                OnTargetReachedNextWayPoint?.Invoke(this,
                    new ReachedNextWayPointEventArgs
                    {
                        Npc = gameObject
                    });
                if (currentPath.Count > 0)
                {
                    _nextWayPoint = currentPath.Dequeue();
                    agent.SetDestination(_nextWayPoint.transform.position);
                }
                else
                {
                    _nextWayPoint = null;
                }
            }
        }
        else
        {
            if(currentPath.Count > 0)
            {
                _nextWayPoint = currentPath.Dequeue();
                agent.SetDestination(_nextWayPoint.transform.position);
            } 
            else
            {
                _distanceFromNextWayPoint = 0f;
            }      
        }
    }

    protected void FindNearestWayPoint()
    {
        float minDistance = float.MaxValue;
        WayPoint nearestWayPoint = null;

        foreach (WayPoint wayPoint in FindObjectsByType<WayPoint>(FindObjectsSortMode.None))
        {
            float distance = Vector3.Distance(transform.position, wayPoint.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestWayPoint = wayPoint;
            }
        }


        _nextWayPoint = nearestWayPoint;

    }

    public bool HasReachedDestination()
    {
        return _distanceFromNextWayPoint <= _WayPointArrivedDistance &&
               agent.pathStatus <= NavMeshPathStatus.PathComplete;
    }

    public void SetTargetLocation(Vector3 location)
    {
        WayPoint target = WaveManager.Instance.WayPointUtils.FindNearestWayPointTo(location);
        foreach(WayPoint waypoint in WaveManager.Instance.WayPointUtils.GetPath(_nextWayPoint, target))
        {
            currentPath.Enqueue(waypoint);
        }
    }

    public WayPoint NextWayPoint { get { return _nextWayPoint; } set { _nextWayPoint = value; } }
    public float WayPointArrivedDistance { get { return _WayPointArrivedDistance; } }

    public class ReachedNextWayPointEventArgs : EventArgs
    {
        public GameObject Npc;
    }
}