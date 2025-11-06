using BehaviorDesigner.Runtime;
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

        public SharedFloat DistanceToStop = 2f;
        
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
    
            
            if (Vector2.Distance(CruiserController.Instance.SpaceShipView.Position,
                    CruiserController.Instance.GetEnemySpaceship.Position) >= DistanceToStop.Value)
            {
                CruiserController.Instance.inputData.thrust = 0.5f;
                CruiserController.Instance.inputData.targetOrientation =
                    AimingHelpers.ComputeSteeringOrient(_spaceship, _otherSpaceship.Position, 1.2f);
            }
            else
            {
                CruiserController.Instance.inputData.thrust = 0;
                CruiserController.Instance.inputData.targetOrientation =
                    AimingHelpers.ComputeSteeringOrient(_spaceship, _otherSpaceship.Position, 1f);
            }
            
            CruiserController.Instance.inputData.shoot = AimingHelpers.CanHit(_spaceship,  CruiserController.Instance.GetEnemySpaceship.Position, CruiserController.Instance.GetEnemySpaceship.Velocity, 0.15f) &&
                                                         CruiserController.Instance.SpaceShipView.Energy >= 0.6f &&
                                                         CruiserController.Instance.GetEnemySpaceship.HitPenaltyCountdown == 0 &&
                                                         CruiserController.Instance.GetEnemySpaceship.StunPenaltyCountdown == 0;
            
            Debug.Log($"Target : {AimingHelpers.CanHit(_spaceship,  CruiserController.Instance.GetEnemySpaceship.Position, CruiserController.Instance.GetEnemySpaceship.Velocity, 0.15f)} | {CruiserController.Instance.SpaceShipView.Energy >= 0.6f}");
        }
    }

}
