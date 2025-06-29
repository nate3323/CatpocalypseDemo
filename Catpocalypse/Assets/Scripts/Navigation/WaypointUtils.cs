using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



/// <summary>
/// This class allows us to traverse the waypoint network and find out if a given waypoint comes before or after another one.
/// The LaserTower uses this to find out where each potential target is in relation to the associated path junction.
/// </summary>
public class WayPointUtils
    {
    private Dictionary<WayPoint, WayPointInfo> _WayPointInfoLookup; // A lookup table that gives you a list of the waypoints that reference the passed in way point.
    public Dictionary<Endpoints, WalkPathInfo> _WayPointPaths;
    private List<WayPoint> _AllWayPointsInScene; // Holds all waypoints in the scene.
    private List<WayPoint> _StartWayPoints; // Holds way points that are not referenced by any otherway points.
    private List<WayPoint> _EndWayPoints; // Holds waypoints that do not have any next way points set.
    private List<WayPoint> _OrphanedPoints; // Holds waypoints that are not referenced by any otherway points, and do not have any next waypoints set either.

    private bool _IsInitialized;

    public enum WayPointCompareResults
    {
        NoPathConnectsA_To_B = -100,

        A_And_B_AreBothNull = -50,

        B_IsNull = -49,
        A_IsNull = -48,

        // The enum values above this comment are essentially error codes.

        A_IsBeforeB = -1,
        A_And_B_AreSamePoint = 0,
        A_IsAfterB = 1,
    }

    public void Init()
    {
        _WayPointInfoLookup = new Dictionary<WayPoint, WayPointInfo>();
        _WayPointPaths = new Dictionary<Endpoints, WalkPathInfo>();

        _StartWayPoints = new List<WayPoint>();
        _EndWayPoints = new List<WayPoint>();
        _OrphanedPoints = new List<WayPoint>();

        _WayPointPaths.Clear();
        _WayPointInfoLookup.Clear();
        _StartWayPoints.Clear();
        _EndWayPoints.Clear();
        _OrphanedPoints.Clear();


        _AllWayPointsInScene = GameObject.FindObjectsOfType<WayPoint>().ToList();

        FindAllWayPointReferences();


        // Find start and end waypoints.
        foreach (WayPoint p in _AllWayPointsInScene)
        {
            int refCount = 0;


            if (_WayPointInfoLookup.TryGetValue(p, out WayPointInfo wpInfo))
            {
                refCount = wpInfo.References.Count;
            }
            else
            {
                refCount = 0;
                Debug.LogError($"Failed to find dictionary entry for waypoint {p.name}!");
            }


            int nextCount = p.NextWayPoints.Count;
            int prevCount = p.PrevWayPoints.Count;


            // Does p link to at least one other waypoint and is not referenced by any other way point?
            if (nextCount > 0 && prevCount <= 0)
            {
                _StartWayPoints.Add(p);
            }
            // Does p link to 0 other waypoints and have at least one other waypoint that links into it?
            else if (nextCount <= 0 && prevCount > 0)
            {
                _EndWayPoints.Add(p);
            }
            // Does p link to 0 other waypoints and is not referenced by any other way point?
            else if (nextCount == 0 && prevCount == 0) { 
                _OrphanedPoints.Add(p);
            }
        } // end foreach
        //Debug.Log($"Waypoints Breakdown:  |  Total: {_AllWayPointsInScene.Count}  |  Start Points: {_StartWayPoints.Count}  |  End Points: {_EndWayPoints.Count}  |  Orphaned Points: {_OrphanedPoints.Count}");

        FindAllShortestPaths();
        //Debug.Log($"Total paths found: { _WayPointPaths.Count}");
        _IsInitialized = true;
    }

    /// <summary>
    /// Compiles a list for each waypoint that contains all waypoints that link into it.
    /// </summary>
    private void FindAllWayPointReferences()
    {
        // Find all the waypoints that reference each way point.
        for (int i = 0; i < _AllWayPointsInScene.Count; i++)
        {

            WayPoint p = _AllWayPointsInScene[i];

            // If this waypoint doesn't have an entry in the dictionary yet, then make one.
            if (!_WayPointInfoLookup.ContainsKey(p))
                _WayPointInfoLookup.Add(p, new WayPointInfo());


            for (int j = 0; j < p.NextWayPoints.Count; j++)
            {
                WayPoint nextP = p.NextWayPoints[j];

                WayPointInfo wpInfo;


                // If there is an accidental null entry, remove it.
                if (nextP == null)
                    p.NextWayPoints.RemoveAt(j);


                // Check if waypoint nextP is already in the dictionary or not.
                _WayPointInfoLookup.TryGetValue(nextP, out wpInfo);
                if (wpInfo == null)
                {
                    wpInfo = new WayPointInfo();
                    _WayPointInfoLookup.Add(nextP, wpInfo);
                }
                else
                {
                    // Waypoint nextP is already in the dictionary, so simply add the new reference to the list.
                    wpInfo = _WayPointInfoLookup[nextP];
                }


                if (!wpInfo.References.Contains(p))
                    wpInfo.References.Add(p);
                else
                    p.NextWayPoints.RemoveAt(i); // Waypoint p contains more than one reference to nextP, so just remove this one.

            } // end foreach WayPoint nextP

        } // end foreach WayPoint p

    }

    /// <summary>
    /// Finds the nearest WayPoint to the specified position.
    /// </summary>
    /// <param name="position">The position to find the closest WayPoint to.</param>
    /// <returns>The closest WayPoint to the specified position, or null if there are no WayPoints in the scene.</returns>
    public WayPoint FindNearestWayPointTo(Vector3 position)
    {

        if (_AllWayPointsInScene.Count <= 0)
            return null;


        float smallestDistance = float.MaxValue;
        WayPoint nearestWayPoint = null;
        for (int i = 0; i < _AllWayPointsInScene.Count; i++)
        {
            WayPoint curWayPoint = _AllWayPointsInScene[i];

            float dist = Vector3.Distance(position, curWayPoint.transform.position);
            if (dist < smallestDistance)
            {
                smallestDistance = dist;
                nearestWayPoint = curWayPoint;
            }

        } // end for i


        return nearestWayPoint;
    }

    public WayPoint FindClosestEndPoint(WayPoint currentLocation)
    {
        WayPoint result = null;
        float currentDistance = float.MaxValue;
        foreach (WayPoint waypoint in _EndWayPoints) {
            Endpoints endpoints = new Endpoints(currentLocation, waypoint);
            if(_WayPointPaths.TryGetValue(endpoints, out WalkPathInfo path)){
                if(currentDistance > path.DistanceTraveled)
                {
                    result = waypoint;
                    currentDistance = path.DistanceTraveled;
                }
            } else
            {
                Debug.LogError("No path between " + endpoints.Start.ToString() + " and " + endpoints.End.ToString());
            }
        }
        if (result == null)
        {
            Debug.LogError("No WayPoint found");
        }
        return result;
    }
    private void FindAllShortestPaths()
    {
        Queue<WalkPathInfo> queue = new Queue<WalkPathInfo>();
        foreach (WayPoint wayPoint in _AllWayPointsInScene)
        {
            WalkPathInfo wayPathInfo = new WalkPathInfo();
            wayPathInfo.StartPoint = wayPoint;
            wayPathInfo.VisitedPoints.Add(wayPoint);
            queue.Enqueue(wayPathInfo);
        }
        while(queue.Count > 0)
        {
            WalkPathInfo walkPath = queue.Dequeue();
            WayPoint currentWayPoint = walkPath.VisitedPoints[walkPath.VisitedPoints.Count - 1];

            //Walk Both Forwards and Backwards
            foreach(WayPoint wayPoint in currentWayPoint.NextWayPoints)
            {
                if (!walkPath.VisitedPoints.Contains(wayPoint))
                {
                    WalkPathInfo clonedPath = walkPath.Clone();
                    clonedPath.VisitedPoints.Add(wayPoint);
                    clonedPath.DistanceTraveled += Vector3.Distance(currentWayPoint.transform.position, wayPoint.transform.position);
                    Endpoints endpoints = new Endpoints(walkPath.StartPoint, wayPoint);
                    if(_WayPointPaths.TryGetValue(endpoints, out WalkPathInfo existingPath))
                    {
                        if(existingPath.DistanceTraveled > clonedPath.DistanceTraveled)
                        {
                            _WayPointPaths[endpoints] =clonedPath;
                        }
                    }
                    else
                    {
                        _WayPointPaths.Add(endpoints, clonedPath);
                    }
                    if (!_EndWayPoints.Contains(wayPoint))
                    {
                        queue.Enqueue(clonedPath);
                    }
                }
            }
            foreach(WayPoint wayPoint in currentWayPoint.PrevWayPoints)
            {
                if (!walkPath.VisitedPoints.Contains(wayPoint))
                {
                    WalkPathInfo clonedPath = walkPath.Clone();
                    clonedPath.VisitedPoints.Add(wayPoint);
                    clonedPath.DistanceTraveled += Vector3.Distance(currentWayPoint.transform.position, wayPoint.transform.position);
                    Endpoints endpoints = new Endpoints(walkPath.StartPoint, wayPoint);
                    if (_WayPointPaths.TryGetValue(endpoints, out WalkPathInfo existingPath))
                    {
                        if (existingPath.DistanceTraveled > clonedPath.DistanceTraveled)
                        {
                            _WayPointPaths.Add(endpoints, clonedPath);
                        }
                    }
                    else
                    {
                        _WayPointPaths.Add(endpoints, clonedPath);
                    }
                    if (!_StartWayPoints.Contains(wayPoint))
                    {
                        queue.Enqueue(clonedPath);
                    }
                }
            }
            
        }
    }

    public int GetWayPointReferenceCount(WayPoint p)
    {
        return _WayPointInfoLookup[p].References.Count;
    }

    /// <summary>
    /// This function checks the dictionary of paths to see if a path exists between Waypoint a
    /// and Waypoint b. If that path does not exist, but a path from Waypoint b to Waypoint a exists, 
    /// it returns the inverted path from Waypoint b to Waypoint a. If no path at all, it throws an error.
    /// </summary>
    /// <param name="a">The start Waypoint a to Waypoint b</returns>
    public List<WayPoint> GetPath(WayPoint a, WayPoint b)
    {
        if(a == null)
        {
            Debug.LogError("Start WayPoint does not exist");
        }
        if(b== null)
        {
            Debug.LogError("End WayPoint does not exist");
        }
        Endpoints endpoints = new Endpoints(a, b);
        WalkPathInfo pathing = _WayPointPaths[endpoints];
        if (_WayPointPaths.TryGetValue(endpoints, out WalkPathInfo path)){
            return path.VisitedPoints;
        }
        endpoints = new Endpoints(b, a);
        if (_WayPointPaths.TryGetValue(endpoints, out path))
        {
            List<WayPoint> wayPointList = path.VisitedPoints;
            wayPointList.Reverse();
            return wayPointList;
        }
        Debug.LogError($"No path found between {a.ToString()} and {b.ToString()}");
        return null;
    }

    public bool IsJunctionWayPoint(WayPoint p)
    {
        return p.NextWayPoints.Count > 1;
    }

    public bool IsStartingWayPoint(WayPoint p)
    {
        return _StartWayPoints.Contains(p);
    }

    public bool IsEndingWayPoint(WayPoint p)
    {
        return _EndWayPoints.Contains(p);
    }

    public bool IsOrphanedWayPoint(WayPoint p)
    {
        return _OrphanedPoints.Contains(p);
    }

    public class WalkPathInfo : IEquatable<WalkPathInfo>
    {
        public WayPoint StartPoint; // Specifies which waypoint the WalkWayPointPath() function will start at.

        // References to the two waypoints we are comparing position-wise as we traverse the path.
        public WayPoint A;
        public WayPoint B;

        public float DistanceTraveled; // Distance traveled so far (how many waypoints we've passed so far)

        public float DistanceToA; // Distance from start to A (how many waypoints we passed to reach it)
        public float DistanceToB; // Distance from start to B (how many waypoints we passed to reach it)

        public List<WayPoint> VisitedPoints = new List<WayPoint>(); // Holds waypoints that have already been visited by the CompareWayPointPositions() function.

        // These track whether or not we visited the two relevant waypoints.
        public bool VisitedA;
        public bool VisitedB;



        /// <summary>
        /// Creates a clone of this WalkPathInfo.
        /// </summary>
        /// <param name="source">The WalkPathInfo object to create a clone of.</param>
        /// <returns>A clone of the passed in WalkPathInfo object.</returns>
        public WalkPathInfo Clone()
        {
            WalkPathInfo clone = new WalkPathInfo();

            clone.StartPoint = StartPoint;
            clone.A = A;
            clone.B = B;
            clone.DistanceTraveled = DistanceTraveled;
            clone.DistanceToA = DistanceToA;
            clone.DistanceToB = DistanceToB;
            clone.VisitedA = VisitedA;
            clone.VisitedB = VisitedB;
            
            clone.VisitedPoints = new List<WayPoint>();
            clone.VisitedPoints.AddRange(VisitedPoints);

            return clone;
        }

        public void DEBUG_PrintWalkPathInfo()
        {
            string separator = new string('=', 256);

            Debug.Log(separator);

            Debug.Log($"StartPoint: \"{StartPoint.name}\"    WaypointA: \"{A.name}\"    WaypointB: \"{B.name}\"");
            Debug.Log($"TotalDistanceTraveled: {DistanceTraveled}    DistanceFromStartToA: {DistanceToA}    DistanceFromStartToB: {DistanceToB}");
            Debug.Log($"Visited Waypoint A: {VisitedA}    Visited Waypoint B: {VisitedB}");

            Debug.Log(separator);
        }

        public bool Equals(WalkPathInfo other)
        {
            if (other == null)
            {
                Debug.Log("WalkPathInfo is null");
                return false;
            }
            if (!this.StartPoint.Equals(other.StartPoint)) return false;
            if(!this.DistanceTraveled.Equals(other.DistanceTraveled)) return false;
            if(!this.VisitedPoints.Equals(other.VisitedPoints)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class WayPointInfo
    {
        // Holds a list of all WayPoints that reference this one.
        public List<WayPoint> References = new List<WayPoint>();
    }

    public class Endpoints
    {
        public Endpoints(WayPoint start, WayPoint end)
        {
            Start = start;
            End = end;
        }
        public WayPoint Start;
        public WayPoint End;

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != this.GetType())
            {
                Debug.LogError("Wrong Object Type");
                return false;
            }
            Endpoints compareEndpoints = (Endpoints) other;
            if(this.Start.Equals(compareEndpoints.Start) && this.End.Equals(compareEndpoints.End)) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns> -1 if waypoint a is before waypoint b, 1 if waypoint a is after waypoint b, and 0 if a and b are the same node, and -100 if there is no path connecting the two waypoints.</returns>
    public WayPointCompareResults CompareWayPointPositions(WayPoint a, WayPoint b)
    {
        // First check if a, b, or both are null, or if they are the same waypoint.
        // We don't need to bother with the loops below in any of these cases.
        if (a == null && b == null)
            return WayPointCompareResults.A_And_B_AreBothNull;
        else if (a == null && b != null)
            return WayPointCompareResults.A_IsNull;
        else if (a != null && b == null)
            return WayPointCompareResults.B_IsNull;
        else if (a == b)
            return WayPointCompareResults.A_And_B_AreSamePoint;
        return WayPointCompareResults.NoPathConnectsA_To_B;
        /**
        List<WalkPathInfo> results = new List<WalkPathInfo>();


        if (!_IsInitialized)
            //Init();


        // Start walking through the waypoints from each starting point.
        foreach (WayPoint startPoint in _StartWayPoints)
        {
            WalkPathInfo walkPathInfo = new WalkPathInfo()
            {
                StartPoint = startPoint,
                A = a,
                B = b,
            };


            // Call the recursive path walking function to walk the path and record info along the way.
            results.AddRange(WalkWaypointPath(walkPathInfo));

        } // end foreach startPoint


        // Now we need to check the results.
        foreach (WalkPathInfo result in results)
        {
            //result.DEBUG_PrintWalkPathInfo();

            // Did this particular path cross both waypoints A and B before it terminated?
            if (result.VisitedA && result.VisitedB)
            {
                if (result.DistanceToA < result.DistanceToB)
                    return WayPointCompareResults.A_IsBeforeB;
                else if (result.DistanceToA > result.DistanceToB)
                    return WayPointCompareResults.A_IsAfterB;
            }
        } // end foreach.


        // None of the paths traversed visited both waypoints A and B, so return 0 to indicate that we didn't see any paths connecting them.
        return WayPointCompareResults.NoPathConnectsA_To_B;
    */
    }
    /**
    /// <summary>
    /// This recursive function starts at the specified start point, and then walks the path
    /// until it reaches a dead end or a junction. If it reaches a junction, it will call itself
    /// for each path branch. It will also return if it encounters a waypoint that has already
    /// been visited to prevent any scenario where it could get stuck going in circles forever.
    /// </summary>
    /// <param name="walkPathInfo">An object containing data that needs to persist between the recursive calls of this function.</param>
    /// <returns>A list of WalkPathInfos (one for each possible path encountered).</returns>
    private List<WalkPathInfo> WalkWaypointPath(WalkPathInfo walkPathInfo)
    {
        List<WalkPathInfo> results = new List<WalkPathInfo>();

        WayPoint currentPoint = walkPathInfo.StartPoint;

        while (true)
        {
            walkPathInfo.DistanceTraveled++;

            // If this point has already been visited, then simply return.
            if (walkPathInfo.VisitedPoints.Contains(currentPoint))
            {
                results.Add(walkPathInfo);
                break;
            }

            walkPathInfo.VisitedPoints.Add(currentPoint);

            // Is the current waypoint A or B?
            if (currentPoint == walkPathInfo.A)
            {
                walkPathInfo.VisitedA = true;
                walkPathInfo.DistanceToA = walkPathInfo.DistanceTraveled;
            }
            else if (currentPoint == walkPathInfo.B)
            {
                walkPathInfo.VisitedB = true;
                walkPathInfo.DistanceToB = walkPathInfo.DistanceTraveled;
            }


            // If the current waypoint is a deadend, or if both waypoints A and B have
            // been visited, then break out of this loop.
            if (currentPoint.NextWayPoints.Count <= 0 ||
                (walkPathInfo.VisitedA && walkPathInfo.VisitedB))
            {
                results.Add(walkPathInfo);
                break;
            }


            // If the current point is a junction point, then call this function once for each
            // path branch, and then break out of this loop.
            if (currentPoint.NextWayPoints.Count > 1)
            {
                foreach (WayPoint nextPoint in currentPoint.NextWayPoints)
                {
                    WalkPathInfo clone = walkPathInfo.Clone();
                    clone.StartPoint = nextPoint;

                    results.AddRange(WalkWaypointPath(clone));
                } // end foreach

                break;
            }


            // If this waypoint has multiple next waypoints, that was already handled above.
            // The same is true if it has none. So here we just need to move our pointer to
            // the next waypoint in line.
            currentPoint = currentPoint.NextWayPoints[0];

        } // end while


        // Debug.Log($"Results Count: {results.Count}");

        return results;
    }
    */
}
