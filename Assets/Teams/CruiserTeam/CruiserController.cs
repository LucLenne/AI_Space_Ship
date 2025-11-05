using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using DoNotModify;
using UnityEngine;

namespace CruiserTeam
{
    public class CruiserController : BaseSpaceShipController
    {
        [SerializeField] private BehaviorTree tree;
        [SerializeField] private Clustering clusteringTool;
        
        [Space(10)]
        [SerializeField] private List<WayPointCluster> _clusters;

        private bool _passInit = false;
        
        public override void Initialize(SpaceShipView spaceship, GameData data)
        {
            // Verification before Initialization
            #region Verification
            if (tree == null || !TryGetComponent<BehaviorTree>(out tree))
            {
                Debug.LogError($"No BehaviorTree Component found", gameObject);
                return;
            }
            if (clusteringTool == null || !TryGetComponent<Clustering>(out clusteringTool))
            {
                Debug.LogError($"No Clustering Component found", gameObject);
                return;
            }
            
            _passInit = true;
            #endregion

            _clusters = clusteringTool.InitializeClustering(data);
        }

        public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
        {
            SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);
            float thrust = 1.0f;
            float targetOrient = AimingHelpers.ComputeSteeringOrient(spaceship, Target(data, spaceship));

            bool needShoot = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);

            //Debug.Log(targetOrient);
            
            return new InputData(thrust, targetOrient, needShoot, false, false);
        }

        Vector2 Target(GameData data, SpaceShipView spaceship)
        {
            int index = 0;
            float closerWayPoint = Mathf.Infinity;
            for (int i = 0; i <_clusters.Count; i++)
            {
                float actualDistance = Vector2.Distance(spaceship.Position, _clusters[i].averagePos);
                if (actualDistance < closerWayPoint && _clusters[i].nbCapturablePoints(spaceship.Owner) > 0)
                {
                    closerWayPoint = actualDistance;
                    index = i;
                }
            }
            
            return _clusters[index].OptimalTrajectory(spaceship)[0];
        }
    }
}
