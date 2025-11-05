using BehaviorDesigner.Runtime;
using DoNotModify;
using System.Collections.Generic;
using UnityEngine;

namespace CruiserTeam
{
    [System.Serializable]
    public struct WayPointCluster
    {
        public List<WayPoint> WayPoints;
        public Vector2 averagePos
        {
            get
            {
                if (averagePos == null)
                    return Vector2.zero;
                else
                    return averagePos;
            }
        }
        public float weight;

        public float centerDistance { get; }
        public void computeCenterDistance()
        {
            return;
        }
    }

    public class CruiserController : BaseSpaceShipController
    {
        [SerializeField] private BehaviorTree tree;

        [SerializeField] private List<WayPointCluster> _clusters;
        private GameData _gameData;
        private SpaceShipView _spaceShipView;
        public static CruiserController Instance;

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
            if (tree == null && !TryGetComponent<BehaviorTree>(out tree))
            {
                Debug.LogError($"No BehaviorTree found", gameObject);
                return;
            }
        }

        public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
        {
            GameData = data;
            SpaceShipView = spaceship;
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
            float closerWayPoint = Mathf.Infinity;
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
