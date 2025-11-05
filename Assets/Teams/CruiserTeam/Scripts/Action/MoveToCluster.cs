using BehaviorDesigner.Runtime.Tasks;
using CruiserTeam;
using DoNotModify;
using UnityEngine;

namespace Cruiser
{
    [TaskCategory("CruiserTeam")]
    public class MoveToCluster : Action
    {
        public override TaskStatus OnUpdate()
        {
            WayPointCluster closerCluster = new WayPointCluster();
            float minDistance = float.MaxValue;
            foreach (WayPointCluster cluster in CruiserController.Instance.Clusters)
            {
                float currentDistance = Vector2.Distance(cluster.averagePos, CruiserController.Instance.SpaceShipView.Position);
                if (minDistance > currentDistance)
                {
                    minDistance = currentDistance;
                    closerCluster = cluster;
                }
            }
            WayPointView closerPoint;
            minDistance = float.MaxValue;
            foreach (WayPointView point in closerCluster.wayPoints)
            {
                float currentDistance = Vector2.Distance(point.Position, CruiserController.Instance.SpaceShipView.Position);
                if (currentDistance < minDistance)
                {
                    closerPoint = point;
                    minDistance = currentDistance;
                }
            }
            return TaskStatus.Success;
        }
    }
}

