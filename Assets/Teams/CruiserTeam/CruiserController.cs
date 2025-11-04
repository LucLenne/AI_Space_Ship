using BehaviorDesigner.Runtime;
using DoNotModify;
using UnityEngine;

namespace CruiserTeam
{

    public class CruiserController : BaseSpaceShipController
    {
        [SerializeField] private BehaviorTree tree;
        public override void Initialize(SpaceShipView spaceship, GameData data)
        {
        }

        public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
        {
            SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);
            float thrust = 1.0f;
            float targetOrient = AimingHelpers.ComputeSteeringOrient(spaceship, Target(data, spaceship));

            bool needShoot = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);
            Debug.Log(targetOrient);
            return new InputData(thrust, targetOrient, needShoot, false, false);
        }

        Vector2 Target(GameData data, SpaceShipView spaceship)
        {
            int index = 0;
            float closerWayPoint = Vector2.Distance(spaceship.Position, data.WayPoints[0].Position);
            for (int i = 0; i < data.WayPoints.Count; i++)
            {
                float actualDistance = Vector2.Distance(spaceship.Position, data.WayPoints[i].Position);
                if (actualDistance < closerWayPoint && data.WayPoints[i].Owner != spaceship.Owner)
                {
                    closerWayPoint = actualDistance;
                    index = i;
                }
            }
            return data.WayPoints[index].Position;
        }
    }
}
