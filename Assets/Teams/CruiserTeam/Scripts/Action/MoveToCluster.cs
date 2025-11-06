using BehaviorDesigner.Runtime;
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
        public AnimationCurve thrustAnglePower;
        private Coroutine followRoutine;
        private List<TargetPath> path;

        [BehaviorDesigner.Runtime.Tasks.Tooltip("The distance before the Spaceship ignores the current target (gives smoother trajectory, 0.5 by default).")]
        public SharedFloat ignoreTargetRadius = 0.5f;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The overshoot angle at which the spaceship will turn when a target is given (1.2 by default).")]
        public SharedFloat aimingHelperOvershoot = 1.2f;


        public override void OnStart()
        {
            followRoutine = StartCoroutine(FollowPath());
        }

        public override TaskStatus OnUpdate()
        {
            if (followRoutine != null)
            {
                return TaskStatus.Running;
            }
            else
            {
                return TaskStatus.Success;
            }
        }
        IEnumerator FollowPath()
        {
            //Init Value
            GameData data = CruiserController.Instance.GameData;
            SpaceShipView spaceship = CruiserController.Instance.SpaceShipView;
            path = BestCluster().OptimalTrajectory(spaceship);
            for (int i = 0; i < path.Count; i++)
            {
                TargetPath currentTarget = path[i];
                yield return StartCoroutine(MoveTo(i, spaceship, data));
            }
            followRoutine = null;
            yield return null;
        }

        WayPointCluster BestCluster()
        {
            WayPointCluster closerCluster = new WayPointCluster();
            float minDistance = float.MaxValue;
            List<WayPointCluster> clusters = CruiserController.Instance.Clusters;
            foreach (WayPointCluster cluster in clusters)
            {
                float currentDistance = Vector2.Distance(cluster.averagePos, CruiserController.Instance.SpaceShipView.Position);
                if (minDistance > currentDistance && cluster.nbCapturablePoints(CruiserController.Instance.SpaceShipView.Owner) > 0)
                {
                    minDistance = currentDistance;
                    closerCluster = cluster;
                }
            }
            return closerCluster;
        }

        IEnumerator MoveTo(int indexTargetPath, SpaceShipView spaceship, GameData data)
        {
            bool hasReached = false;

            while (!hasReached)
            {
                Vector2 currentTarget = path[indexTargetPath].position;

                // Passage au prochain waypoint
                if (indexTargetPath < path.Count - 1 &&
                    Vector2.Distance(currentTarget, spaceship.Position) <= ignoreTargetRadius.Value)
                {
                    currentTarget = path[indexTargetPath + 1].position;
                }

                float targetOrient = AimingHelpers.ComputeSteeringOrient(spaceship, currentTarget, aimingHelperOvershoot.Value);
                float thrust = ComputeThurst(spaceship, currentTarget);

                bool shouldLayMine = false;
                if (Vector2.Distance(spaceship.Position, path[indexTargetPath].position) <= ignoreTargetRadius.Value)
                {
                    hasReached = true;
                    if (spaceship.Energy >= 0.99f)
                    {
                        if (indexTargetPath == 0 || indexTargetPath == path.Count - 1)
                            shouldLayMine = true;
                    }
                }

                CruiserController.Instance.inputData =
                    new InputData(thrust, targetOrient, false, shouldLayMine, false);

                yield return null;
            }
        }

        float ComputeThurst(SpaceShipView spaceship, Vector2 target)
        {
            float angle = Vector2.Angle(spaceship.LookAt, target - spaceship.Position);
            return thrustAnglePower.Evaluate(angle);
        }
    }
}

