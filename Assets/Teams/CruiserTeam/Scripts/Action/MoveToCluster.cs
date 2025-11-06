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
        private bool laydownMine = false;

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
            Debug.Log("Move Mod");

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
            yield break;
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
            float radius = spaceship.Radius;

            while (!hasReached)
            {
                Vector2 target;
                if (indexTargetPath < path.Count - 1 && Vector2.Distance(path[indexTargetPath].position, spaceship.Position) <= ignoreTargetRadius.Value)
                {
                    target = path[indexTargetPath + 1].position;
                }
                else
                {
                    target = path[indexTargetPath].position;
                }


                float targetPos = AimingHelpers.ComputeSteeringOrient(spaceship, target, aimingHelperOvershoot.Value);
                float thrust = ComputeThurst(spaceship, target);

                CruiserController.Instance.inputData =
                    new InputData(thrust, targetPos, false, laydownMine, false);

                if (laydownMine)
                    laydownMine = false;

                // Detection : si le vaisseau est passe dans le cercle
                if (path[indexTargetPath].wayPoint.Owner == spaceship.Owner)
                {
                    hasReached = true;
                    if (indexTargetPath == path.Count - 1 || indexTargetPath == 0)
                    {
                        laydownMine = true;
                    }
                }
                yield break;
            }

            float ComputeThurst(SpaceShipView spaceship, Vector2 target)
            {
                float angle = Vector2.Angle(spaceship.LookAt, target - spaceship.Position);
                return thrustAnglePower.Evaluate(angle);
            }
        }
    }
}

