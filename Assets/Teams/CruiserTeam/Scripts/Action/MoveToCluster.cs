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
            GameData data = CruiserController.Instance.GameData;
            SpaceShipView spaceship = CruiserController.Instance.SpaceShipView;

            List<Vector2> path = BestCluster().OptimalTrajectory(spaceship);

            for (int i = 0; i < path.Count; i++)
            {
                Vector2 currentTarget = path[i];
                bool changed;

                do
                {
                    changed = false;

                    foreach (AsteroidView asteroid in data.Asteroids)
                    {
                        if (SegmentCircleIntersection(spaceship.Position, currentTarget, asteroid.Position, asteroid.Radius))
                        {
                            currentTarget = AvoidAsteroid(asteroid, spaceship, currentTarget);
                            changed = true; // Re-check, il peut encore taper un autre astéroïde
                        }
                    }

                } while (changed); // boucle tant qu’on trouve d'autres collisions

                path[i] = currentTarget;
                StartCoroutine(MoveTo(currentTarget, spaceship, data));
            }
            return TaskStatus.Success;
        }


        WayPointCluster BestCluster()
        {
            WayPointCluster closerCluster = new WayPointCluster();
            float minDistance = float.MaxValue;
            List<WayPointCluster> clusters = CruiserController.Instance.Clusters;
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
            return closerCluster;
        }

        IEnumerator MoveTo(Vector2 target, SpaceShipView spaceship, GameData data)
        {
            float radius = spaceship.Radius + data.WayPoints[0].Radius;
            while (Vector2.Distance(spaceship.Position, target) > radius)
            {
                float targetPos = AimingHelpers.ComputeSteeringOrient(spaceship, target);
                float thrust = ComputeThurst(spaceship, target);
                CruiserController.Instance.inputData = new InputData(thrust, targetPos, false, false, false);
                yield return null;
            }
            yield return TaskStatus.Running;
        }

        float ComputeThurst(SpaceShipView spaceship, Vector2 target)
        {
            float angle = Vector2.Angle(spaceship.Velocity, target - spaceship.Position);
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
            float avoidRadius = spaceship.Radius + asteroid.Radius;
            float dist = avoidRadius * 1.5f;

            Vector2 shipPos = spaceship.Position;
            Vector2 dir = (target - shipPos).normalized;

            // Deux points de contournement possibles
            Vector2 perp = new Vector2(dir.y, -dir.x) * dist;

            Vector2 c1 = asteroid.Position + perp;
            Vector2 c2 = asteroid.Position - perp;

            // On choisit celui qui rapproche le plus de la target finale
            return (c1 - target).sqrMagnitude < (c2 - target).sqrMagnitude ? c1 : c2;
        }
    }
}

