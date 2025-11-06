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
        public bool wipedMovement;
        private Coroutine followRoutine;
        private List<TargetPath> path;
        public override void OnStart()
        {
            followRoutine = StartCoroutine(FollowPath());
        }

        public override TaskStatus OnUpdate()
        {
            if (wipedMovement)
                return TaskStatus.Failure;

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

                bool changed;
                do
                {
                    changed = false;
                    foreach (AsteroidView asteroid in data.Asteroids)
                    {
                        if (SegmentCircleIntersection(spaceship.Position, currentTarget.position, asteroid.Position, asteroid.Radius))
                        {
                            currentTarget.position = AvoidAsteroid(asteroid, spaceship, currentTarget.position);
                            changed = true;
                        }
                    }
                }
                while (changed);

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
                if (indexTargetPath < path.Count - 1 && Vector2.Distance(path[indexTargetPath].position, spaceship.Position) <= 0.5f)
                {
                    target = path[indexTargetPath + 1].position;
                }
                else
                {
                    target = path[indexTargetPath].position;
                }


                float targetPos = AimingHelpers.ComputeSteeringOrient(spaceship, target);
                float thrust = ComputeThurst(spaceship, target);

                CruiserController.Instance.inputData =
                    new InputData(thrust, targetPos, false, false, false);



                // Détection : si le vaisseau est passé dans le cercle
                if (path[indexTargetPath].wayPoint.Owner == spaceship.Owner)
                    hasReached = true;

                yield return null;
            }

            // on quitte la coroutine
            yield break;
        }

        float ComputeThurst(SpaceShipView spaceship, Vector2 target)
        {
            float angle = Vector2.Angle(spaceship.LookAt, target - spaceship.Position);
            return thrustAnglePower.Evaluate(angle);
        }

        bool SegmentCircleIntersection(Vector2 A, Vector2 B, Vector2 C, float R)
        {
            Vector2 AB = B - A;
            float t = Vector2.Dot(C - A, AB) / Vector2.Dot(AB, AB);
            t = Mathf.Clamp01(t);
            Vector2 closest = A + t * AB;

            return (closest - C).sqrMagnitude <= R * R;
        }

        Vector2 AvoidAsteroid(AsteroidView asteroid, SpaceShipView spaceship, Vector2 target)
        {
            Vector2 toAst = asteroid.Position - spaceship.Position;
            float avoidRadius = spaceship.Radius * 1.5f + asteroid.Radius;

            // Distance actuelle
            float d = toAst.magnitude;

            // Si déjà trop proche : esquive d’urgence
            if (d < avoidRadius * 1.2f)
            {
                Vector2 escapeDir = (spaceship.Position - asteroid.Position).normalized;
                return spaceship.Position + escapeDir * avoidRadius * 2f;
            }

            // Angle tangent
            float angleOffset = Mathf.Acos(avoidRadius / d);

            // Direction générale
            float baseAngle = Mathf.Atan2(toAst.y, toAst.x);

            // Deux options
            Vector2 p1 = asteroid.Position + new Vector2(
                Mathf.Cos(baseAngle + angleOffset),
                Mathf.Sin(baseAngle + angleOffset)) * avoidRadius * 1.2f;

            Vector2 p2 = asteroid.Position + new Vector2(
                Mathf.Cos(baseAngle - angleOffset),
                Mathf.Sin(baseAngle - angleOffset)) * avoidRadius * 1.2f;

            // Choisir celui qui va le plus vers la target
            return Vector2.Distance(p1, target) < Vector2.Distance(p2, target) ? p1 : p2;
        }
    }
}

