using UnityEngine;
using System.Collections.Generic;
using DoNotModify;

namespace CruiserTeam
{
    [System.Serializable]
    public struct WayPointCluster
    {
        public List<WayPointView> wayPoints;

        public float weight;
    
        public float centerDistance { get; private set; }

        public Vector2 averagePos
        {
            get
            {
                if (averagePos == null)
                    return Vector2.zero;
                else
                    return averagePos;
            }
        }

        public void computeCenterDistance(Vector2 a_center)
        {
            if (wayPoints.Count <= 0)
                return;
        
            Vector2 averagePos = Vector2.zero;
            foreach (WayPointView current in wayPoints)
            {
                averagePos += current.Position;
            }
        
            averagePos = averagePos / wayPoints.Count;
            centerDistance = (averagePos - a_center).magnitude;
        }
    }
    
    public class ClusterBinaryTree
    {
        public ClusterBinaryTree left { get; set; }
        public ClusterBinaryTree right { get; set; }
            
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
        [SerializeField] private int _limitOfWaypoints;

        public List<WayPointCluster> InitializeClustering(GameData a_data)
        {
            List<ClusterBinaryTree> cluster = new List<ClusterBinaryTree>();

            cluster.Add(GetPointInZone(Direction.Center, a_data.WayPoints));
            cluster.Add(GetPointInZone(Direction.UpRight, a_data.WayPoints));
            cluster.Add(GetPointInZone(Direction.UpLeft, a_data.WayPoints));
            cluster.Add(GetPointInZone(Direction.DownRight, a_data.WayPoints));
            cluster.Add(GetPointInZone(Direction.DownLeft, a_data.WayPoints));
            
            List<List<WayPointView>> clusters = new List<List<WayPointView>>();
            
        }
        
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

        private ClusterBinaryTree GetPointInZone(Direction a_dir, List<WayPointView> a_data)
        {
            ClusterBinaryTree node = new ClusterBinaryTree();
            
            switch (a_dir)
            {
                case Direction.Center :
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
                            current.Position.x < 0 && current.Position.y >= 0)
                            node.wayPoints.Add(current);
                    }
                    break;
                case Direction.DownLeft:
                    foreach (WayPointView current in a_data)
                    {
                        if (current.Position.magnitude > _centerRadius &&
                            current.Position.x < 0 && current.Position.y < 0)
                            node.wayPoints.Add(current);
                    }
                    break;
                case Direction.UpRight:
                    foreach (WayPointView current in a_data)
                    {
                        if (current.Position.magnitude > _centerRadius &&
                            current.Position.x >= 0 && current.Position.y >= 0)
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
    }
}
