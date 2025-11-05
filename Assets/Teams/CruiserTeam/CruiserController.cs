using BehaviorDesigner.Runtime;
using DoNotModify;
using System.Collections.Generic;
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
        private GameData _gameData;
        private SpaceShipView _spaceShipView;
        public static CruiserController Instance;

        public InputData inputData;

        public SpaceShipView SpaceShipView { get => _spaceShipView; private set => _spaceShipView = value; }
        public GameData GameData { get => _gameData; private set => _gameData = value; }
        public List<WayPointCluster> Clusters { get => _clusters; set => _clusters = value; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }


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
            GameData = data;
            SpaceShipView = spaceship;
            SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);
            //AimingHelpers.ComputeSteeringOrient(spaceship, Target(data, spaceship));

            bool needShoot = AimingHelpers.CanHit(spaceship, otherSpaceship.Position, otherSpaceship.Velocity, 0.15f);
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
