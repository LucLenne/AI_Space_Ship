using BehaviorDesigner.Runtime.Tasks;
using CruiserTeam;
using DoNotModify;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cruiser
{
    [TaskCategory("CruiserTeam")]
    public class MoveToCluster : Action
    {
        private InputData inputData;
        public AnimationCurve thrustAnglePower;
        public override TaskStatus OnUpdate()
        {
            WayPointCluster closerCluster = new WayPointCluster();
            float minDistance = float.MaxValue;
            List<WayPointCluster> clusters = CruiserController.Instance.Clusters.;
            WayPointCluster clusterDone = new WayPointCluster();
            foreach (WayPointCluster cluster in clusters)
            {
                float currentDistance = Vector2.Distance(cluster.averagePos, CruiserController.Instance.SpaceShipView.Position);
                if (minDistance > currentDistance)
                {
                    minDistance = currentDistance;
                    closerCluster = cluster;
                }
            }
            //CruiserController.Instance.inputData = ShipToCluster(closerCluster);

            return TaskStatus.Success;
        }

        IEnumerator MoveTo(Vector2 target, SpaceShipView spaceship, GameData data)
        {
            float radius = spaceship.Radius + data.WayPoints[0].Radius;
            while (Vector2.Distance(spaceship.Position, target) > radius)
            {
                float targetPos = AimingHelpers.ComputeSteeringOrient(spaceship, target);
                float thrust = ComputeThurst(spaceship, target);
                inputData = new InputData(thrust, targetPos, false, false, false);
                yield return null;
            }
            yield return TaskStatus.Success;
        }

        float ComputeThurst(SpaceShipView spaceship, Vector2 target)
        {
            float angle = Vector2.Angle(spaceship.Velocity, target - spaceship.Position);
            return thrustAnglePower.Evaluate(angle);
        }
    }
}

