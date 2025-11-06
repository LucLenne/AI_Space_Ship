using BehaviorDesigner.Runtime.Tasks;
using DoNotModify;
using UnityEngine;

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
            Debug.Log("Shoot Mod");
            ShootEnemy();
            
            if (CruiserController.Instance.SpaceShipView.HasShot)
                return TaskStatus.Success;
            else
                return TaskStatus.Running;
        }
        void ShootEnemy()
        {
            CruiserController.Instance.inputData.targetOrientation = AimingHelpers.ComputeSteeringOrient(_spaceship, _otherSpaceship.Position);
            CruiserController.Instance.inputData.shoot = AimingHelpers.CanHit(_spaceship, _otherSpaceship.Position, angleToleranceShoot) &&
                                                         CruiserController.Instance.SpaceShipView.Energy >= 0.6f;
        }
    }

}
