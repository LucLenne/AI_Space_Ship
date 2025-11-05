using DoNotModify;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace CruiserTeam
{
    [System.Serializable]
    public struct WayPointCluster
    {
        public WayPointCluster(List<WayPointView> a_waypoints)
        {
            wayPoints = a_waypoints;

            if (wayPoints.Count <= 0)
            {
                _averagePos = Vector2.zero;
                centerDistance = 0;
                weight = 0;
            }
            else
            {
                Vector2 average = Vector2.zero;
                foreach (WayPointView current in wayPoints)
                {
                    average += current.Position;
                }

                _averagePos = average / wayPoints.Count;
                centerDistance = _averagePos.magnitude;
                weight = 0;
            }
        }

        public List<WayPointView> wayPoints;

        public float weight;
        public float centerDistance { get; private set; }
        private Vector2 _averagePos;
        public Vector2 averagePos
        {
            get
            {
                if (_averagePos == null)
                    return Vector2.zero;
                else
                    return _averagePos;
            }
        }

        // Returns the number of points that aren't the owner's one
        public int nbCapturablePoints(int a_owner)
        {
            int count = 0;

            foreach (WayPointView current in wayPoints)
            {
                if (current.Owner != a_owner)
                    count++;
            }

            return count;
        }
        
        /// <summary>
        /// Will Compute the optimal trajectory to go and traverse this cluster
        /// </summary>
        /// <param name="a_spaceShip"></param>
        /// <returns>List of points to go to</returns>
        public List<Vector2> OptimalTrajectory(SpaceShipView a_spaceShip)
        {
            List<Vector2> trajectory = new List<Vector2>();

            if (wayPoints == null || wayPoints.Count <= 0)
                return trajectory;
            else if (wayPoints.Count == 1)
            {
                trajectory.Add(wayPoints[0].Position - (wayPoints[0].Radius * (a_spaceShip.Position - wayPoints[0].Position).normalized));
                return trajectory;
            }

            int closest = 0;
            for (int i = 1; i < wayPoints.Count; i++)
            {
                if (Vector2.Distance(wayPoints[i].Position, a_spaceShip.Position) < Vector2.Distance(wayPoints[closest].Position, a_spaceShip.Position))
                    closest = i;
            }
            ;

            List<int> indexTaken = new List<int>();
            indexTaken.Add(closest);

            int tempo = 0;
            while (indexTaken.Count < wayPoints.Count && tempo < 100)
            {
                int nextIndex = -1;
                float distance = Mathf.Infinity;
                
                for (int i = 0; i < wayPoints.Count; i++)
                {
                    if (wayPoints[i].Owner != a_spaceShip.Owner &&
                        !indexTaken.Contains(i) &&
                        Vector2.Distance(wayPoints[i].Position, wayPoints[indexTaken[^1]].Position) < distance)
                    {
                        distance = Vector2.Distance(wayPoints[i].Position, wayPoints[indexTaken[^1]].Position);
                        nextIndex = i;
                    }
                }

                if (nextIndex <= -1)
                    break;
                
                indexTaken.Add(nextIndex);
                tempo++;
            }
            if (tempo >= 100)
                Debug.LogError("Optimal Trajectory 'While' error");

            if (indexTaken.Count <= 1)
            {
                foreach (int index in indexTaken)
                {
                    trajectory.Add(wayPoints[index].Position);
                }
            }
            else
            {
                for (int i = 0; i < indexTaken.Count; i++)
                {
                    if (i == 0)
                    {
                        float distance = Vector2.Distance(a_spaceShip.Position, wayPoints[indexTaken[i]].Position) + 
                                         Vector2.Distance(wayPoints[indexTaken[i]].Position, wayPoints[indexTaken[i + 1]].Position);
                        float progress = Vector2.Distance(a_spaceShip.Position, wayPoints[indexTaken[i]].Position) / distance;
                        
                        Vector2 lerpPos = Vector2.Lerp(a_spaceShip.Position, wayPoints[indexTaken[i + 1]].Position, progress);
                        Vector2 dirVector = (lerpPos - wayPoints[indexTaken[i]].Position).normalized;
                        
                        trajectory.Add(wayPoints[indexTaken[i]].Position + (dirVector * wayPoints[indexTaken[i]].Radius));
                    }
                    else if (i == indexTaken.Count - 1)
                    {
                        trajectory.Add(wayPoints[indexTaken[i]].Position);
                    }
                    else
                    {
                        float distance = Vector2.Distance(trajectory[i - 1], wayPoints[indexTaken[i]].Position) + 
                                         Vector2.Distance(wayPoints[indexTaken[i]].Position, wayPoints[indexTaken[i + 1]].Position);
                        float progress = Vector2.Distance(trajectory[i - 1], wayPoints[indexTaken[i]].Position) / distance;
                        
                        Vector2 lerpPos = Vector2.Lerp(trajectory[i - 1], wayPoints[indexTaken[i + 1]].Position, progress);
                        Vector2 dirVector = (lerpPos - wayPoints[indexTaken[i]].Position).normalized;
                        
                        trajectory.Add(wayPoints[indexTaken[i]].Position + (dirVector * wayPoints[indexTaken[i]].Radius));
                    }
                }
            }
            
            return trajectory;
        }
    }

    public class ClusterBinaryTree
    {
        public ClusterBinaryTree left = null;
        public ClusterBinaryTree right = null;

        public List<WayPointView> wayPoints = new List<WayPointView>();
    }

    public class Clustering : MonoBehaviour
    {
        private enum Direction
        {
            Center,
            UpLeft,
            DownLeft,
            UpRight,
            DownRight
        }

        [Tooltip("The radius of centered cluster in units.")]
        [SerializeField] private float _centerRadius;
        [Tooltip("Limit of Waypoints per Cluster.")]
        [SerializeField] private int _limitOfWaypoints = 2;


        [Space(10)]
        [SerializeField] private bool _ShowClusterDebug;
        [SerializeField] private List<WayPointCluster> _DebugClusterList = new List<WayPointCluster>();
        [SerializeField] private List<Color> _clusterColors = new List<Color>();

        private bool _endClustering = false;

        private void OnValidate()
        {
            _limitOfWaypoints = Mathf.Clamp(_limitOfWaypoints, 2, int.MaxValue);
        }

        public List<WayPointCluster> InitializeClustering(GameData a_data)
        {
            // Creating Binary tree of each zone
            List<ClusterBinaryTree> binaryTrees = new List<ClusterBinaryTree>();

            binaryTrees.Add(GetPointInZone(Direction.Center, a_data.WayPoints));
            binaryTrees.Add(GetPointInZone(Direction.UpRight, a_data.WayPoints));
            binaryTrees.Add(GetPointInZone(Direction.UpLeft, a_data.WayPoints));
            binaryTrees.Add(GetPointInZone(Direction.DownRight, a_data.WayPoints));
            binaryTrees.Add(GetPointInZone(Direction.DownLeft, a_data.WayPoints));

            // Clustering Waypoints in each Binary trees
            List<List<WayPointView>> clustersWayPoints = new List<List<WayPointView>>();

            for (int i = 0; i < binaryTrees.Count; i++)
            {
                binaryTrees[i] = ClusteringPointsInNode(binaryTrees[i]);
                RecoverBinaryNode(binaryTrees[i], ref clustersWayPoints);
            }

            // Reallocate every Cluster of type List<WayPointView> as type WayPointCluster, to be returned
            List<WayPointCluster> clusterList = new List<WayPointCluster>();
            for (int i = 0; i < clustersWayPoints.Count; i++)
            {
                clusterList.Add(new WayPointCluster(clustersWayPoints[i]));
            }

            _DebugClusterList = clusterList;

            _endClustering = true;
            return clusterList;
        }

        /// <summary>
        /// Recovers each and every sorted nodes as Clusters
        /// </summary>
        private void RecoverBinaryNode(ClusterBinaryTree node, ref List<List<WayPointView>> a_result)
        {
            if (node != null)
            {
                if (node.wayPoints != null && node.wayPoints.Count > 0)
                    a_result.Add(node.wayPoints);

                RecoverBinaryNode(node.left, ref a_result);
                RecoverBinaryNode(node.right, ref a_result);
            }
        }

        /// <summary>
        /// Get point depending on which zone it is situated on
        /// </summary>
        /// <returns></returns>
        private ClusterBinaryTree GetPointInZone(Direction a_dir, List<WayPointView> a_data)
        {
            ClusterBinaryTree node = new ClusterBinaryTree();

            switch (a_dir)
            {
                case Direction.Center:
                    foreach (WayPointView current in a_data)
                    {
                        if (current.Position.magnitude <= _centerRadius)
                            node.wayPoints.Add(current);
                    }
                    break;
                case Direction.UpLeft:
                    foreach (WayPointView current in a_data)
                    {
                        if (current.Position.magnitude > _centerRadius &&
                            current.Position.x <= 0 && current.Position.y > 0)
                            node.wayPoints.Add(current);
                    }
                    break;
                case Direction.DownLeft:
                    foreach (WayPointView current in a_data)
                    {
                        if (current.Position.magnitude > _centerRadius &&
                            current.Position.x < 0 && current.Position.y <= 0)
                            node.wayPoints.Add(current);
                    }
                    break;
                case Direction.UpRight:
                    foreach (WayPointView current in a_data)
                    {
                        if (current.Position.magnitude > _centerRadius &&
                            current.Position.x > 0 && current.Position.y >= 0)
                            node.wayPoints.Add(current);
                    }
                    break;
                case Direction.DownRight:
                    foreach (WayPointView current in a_data)
                    {
                        if (current.Position.magnitude > _centerRadius &&
                            current.Position.x >= 0 && current.Position.y < 0)
                            node.wayPoints.Add(current);
                    }
                    break;
            }

            return node;
        }

        /// <summary>
        /// Divide the given Cluster with the maximum numbers of Waypoint by Cluster
        /// </summary>
        /// <returns></returns>
        private ClusterBinaryTree ClusteringPointsInNode(ClusterBinaryTree a_node)
        {
            ClusteringPointsInNode(ref a_node);
            return a_node;
        }
        /// <summary>
        /// Divide the given Cluster with the maximum numbers of Waypoint by Cluster
        /// Recursive Method
        /// </summary>
        /// <returns></returns>
        private void ClusteringPointsInNode(ref ClusterBinaryTree a_node)
        {
            // Is there less point than the limit
            if (a_node.wayPoints.Count <= _limitOfWaypoints)
                return;

            // Create Branches
            a_node.right = new ClusterBinaryTree();
            a_node.left = new ClusterBinaryTree();

            // Check the center of attraction of all points
            Vector2 weightCenter = Vector2.zero;
            foreach (WayPointView current in a_node.wayPoints)
            {
                weightCenter += current.Position;
            }
            weightCenter = weightCenter / a_node.wayPoints.Count;

            // // Check the furthest point from the center of attraction
            Vector2 furthestPoint = weightCenter; // SHOULD ALWAYS BE EQUAL TO A POINT
            for (int i = 1; i < a_node.wayPoints.Count; i++)
            {
                if ((a_node.wayPoints[i].Position - weightCenter).magnitude >= (furthestPoint - weightCenter).magnitude)
                    furthestPoint = a_node.wayPoints[i].Position;
            }

            // Check every point depending if there are closer to the center of attraction or further
            foreach (WayPointView current in a_node.wayPoints)
            {
                float distanceA = Vector2.Distance(current.Position, weightCenter);
                float distanceB = Vector2.Distance(current.Position, furthestPoint);

                if (distanceA < distanceB)
                    a_node.right.wayPoints.Add(current);
                else
                    a_node.left.wayPoints.Add(current);
            }


            // Clear info on this node and checking lower nodes
            a_node.wayPoints.Clear();
            ClusteringPointsInNode(ref a_node.right);
            ClusteringPointsInNode(ref a_node.left);
        }

        #region Debug
        private void OnDrawGizmos()
        {
            if (_clusterColors == null || _clusterColors.Count < _DebugClusterList.Count)
            {
                if (_clusterColors == null)
                    _clusterColors = new List<Color>();

                for (int i = _clusterColors.Count; i < _DebugClusterList.Count; i++)
                {
                    _clusterColors.Add(new Color(Random.value, Random.value, Random.value, 0.7f));
                }
            }

            if (_ShowClusterDebug)
            {
                Gizmos.color = Color.yellow;

                Gizmos.DrawLine(new Vector3(_centerRadius, 0, -1), new Vector3(100, 0, -1));
                Gizmos.DrawLine(new Vector3(-_centerRadius, 0, -1), new Vector3(-100, 0, -1));
                Gizmos.DrawLine(new Vector3(0, _centerRadius, -1), new Vector3(0, 100, -1));
                Gizmos.DrawLine(new Vector3(0, -_centerRadius, -1), new Vector3(0, -100, -1));
                Gizmos.DrawWireSphere(Vector3.zero, _centerRadius);

                for (int i = 0; i < _DebugClusterList.Count; i++)
                {
                    Gizmos.color = _clusterColors[i];

                    foreach (WayPointView current in _DebugClusterList[i].wayPoints)
                    {
                        Gizmos.DrawSphere(new Vector3(current.Position.x, current.Position.y, -1), current.Radius + 0.1f);
                    }
                }
            }

        }
        #endregion
    }
}
