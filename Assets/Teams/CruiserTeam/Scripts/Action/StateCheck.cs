using CruiserTeam;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DoNotModify;
using UnityEngine;

namespace Cruiser
{
    [TaskCategory("CruiserTeam")]
    public class StateCheck : Action
    {
        private CruiserController controller = null;
        
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Behavior State : 0 = Passive | 1 = Aggressive")]
        public SharedInt currentState = 0;
        
        //Condition 1
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Condition 1 : The point gap between our and the enemy Spaceship")]
        public SharedInt pointGap = -2;
        
        // Condition 2
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Condition 2 : The energy left in the enemy spaceship")]
        public SharedFloat enemyEnergyLeft = 0.4f;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Condition 2 : The distance gap between our and the enemy spaceship")]
        public SharedFloat withinDistance = 2f;
        
        
        public override void OnStart()
        {
            controller = CruiserController.Instance;
        }

        public override TaskStatus OnUpdate()
        {
            if ((controller.SpaceShipView.Score - controller.GetEnemySpaceship.Score) <=
                pointGap.Value || /*Condition 1*/
                (controller.GetEnemySpaceship.Energy <= enemyEnergyLeft.Value &&
                 Vector2.Distance(controller.SpaceShipView.Position, controller.GetEnemySpaceship.Position) <=
                 withinDistance.Value)) /*Condition 2*/
            {
                currentState.Value = 1;
            }
            else
                currentState.Value = 0;

            return TaskStatus.Success;
        }
    }
}
