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
            float targetOrient = spaceship.Orientation + 90.0f;
            bool needShoot = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);

            return new InputData(thrust, targetOrient, needShoot, false, false);
        }
    }

}
