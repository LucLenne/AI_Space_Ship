using BehaviorDesigner.Runtime.Tasks;
using DoNotModify;
namespace CruiserTeam
{
    [TaskCategory("CruiserTeam")]
    public class TargetEnemy : Action
    {
        private GameData _data;
        private SpaceShipView _spaceship;
        private SpaceShipView _otherSpaceship;
        public float angleToleranceShoot;
        public override void OnStart()
        {
            _data = CruiserController.Instance.GameData;
            _spaceship = CruiserController.Instance.SpaceShipView;
            _otherSpaceship = CruiserController.Instance.GameData.GetSpaceShipForOwner(1 - _spaceship.Owner);

        }
        public override TaskStatus OnUpdate()
        {
            ShootEnemy();
            return TaskStatus.Running;
        }
        void ShootEnemy()
        {
            AimingHelpers.ComputeSteeringOrient(_spaceship, _otherSpaceship.Position);
            AimingHelpers.CanHit(_spaceship, _otherSpaceship.Position, angleToleranceShoot);
        }
    }

}
